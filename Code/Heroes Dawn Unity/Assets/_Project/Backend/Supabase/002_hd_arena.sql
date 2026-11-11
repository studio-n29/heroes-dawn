-- Bloc D (1.0.0) : arène des tournois JcJ.
-- ┌───────────────────────────────────────────────────────────────────────────────────────────────┐
-- │ SCRIPT NON APPLIQUÉ. À relire, puis à appliquer uniquement sur accord explicite de l'équipe.  │
-- └───────────────────────────────────────────────────────────────────────────────────────────────┘
-- Les tables du jeu portent le préfixe hd_ : les tables hq_* (N29 HQ) appartiennent à un autre outil et ne sont jamais touchées.
-- Aucune clé service_role n'est utilisée ni requise : le client n'a que la clé anon et le jeton du joueur.
--
-- Tournoi : identifiant d'édition « <tournoi>:<année de début> » (ex. christmas:2026), calculé par le client à partir du calendrier.
-- Jour du plafond : jour UTC du serveur (le client affiche son jour local ; l'écart d'un fuseau est accepté, le serveur fait foi).
-- Protection contre les clients modifiés : limitée aux plafonds (10 duels par jour et par tournoi, points bornés) ; les combats ne sont pas vérifiés.

-- ---------------------------------------------------------------------------------------------------------------------
-- 1. Captures de défense : l'équipe de chaque joueur, lisible par les autres joueurs connectés (adversaires hors ligne).
-- ---------------------------------------------------------------------------------------------------------------------
create table if not exists public.hd_arena_defenses (
  user_id uuid primary key references auth.users (id) on delete cascade,
  snapshot jsonb not null,
  rating int not null default 0,
  updated_at timestamptz not null default now(),
  constraint hd_arena_defenses_size check (pg_column_size(snapshot) <= 65536),
  constraint hd_arena_defenses_rating check (rating >= 0 and rating <= 100000)
);

create index if not exists hd_arena_defenses_rating_idx on public.hd_arena_defenses (rating);

-- ---------------------------------------------------------------------------------------------------------------------
-- 2. Scores : un par joueur et par édition. Aucune écriture directe : seules les fonctions ci-dessous les modifient.
-- ---------------------------------------------------------------------------------------------------------------------
create table if not exists public.hd_arena_scores (
  tournament_id text not null,
  user_id uuid not null references auth.users (id) on delete cascade,
  points int not null default 0,
  reached_at timestamptz not null default now(),
  claimed_tiers int[] not null default '{}',
  primary key (tournament_id, user_id),
  constraint hd_arena_scores_points check (points >= 0 and points <= 100000),
  constraint hd_arena_scores_tournament check (tournament_id ~ '^[a-z_]{1,40}:[0-9]{4}$')
);

-- Classement : points décroissants, puis date d'atteinte du score (le plus tôt gagne).
create index if not exists hd_arena_scores_board_idx on public.hd_arena_scores (tournament_id, points desc, reached_at);

-- ---------------------------------------------------------------------------------------------------------------------
-- 3. Journal des duels : sert au plafond quotidien et à l'audit. host_id est le joueur qui verse le résultat ;
--    guest_id l'adversaire quand il est connu (nul pour une équipe de repli).
-- ---------------------------------------------------------------------------------------------------------------------
create table if not exists public.hd_arena_matches (
  id uuid primary key default gen_random_uuid(),
  tournament_id text not null,
  match_ref text not null,
  host_id uuid not null references auth.users (id) on delete cascade,
  guest_id uuid references auth.users (id) on delete set null,
  mode text not null,
  result text not null,
  points int not null default 0,
  counted boolean not null default true,
  created_at timestamptz not null default now(),
  constraint hd_arena_matches_mode check (mode in ('offline', 'online')),
  constraint hd_arena_matches_result check (result in ('win', 'loss', 'quit', 'cancelled')),
  constraint hd_arena_matches_ref unique (tournament_id, host_id, match_ref)
);

create index if not exists hd_arena_matches_day_idx on public.hd_arena_matches (host_id, tournament_id, created_at);

-- ---------------------------------------------------------------------------------------------------------------------
-- 4. Sécurité par ligne
-- ---------------------------------------------------------------------------------------------------------------------
alter table public.hd_arena_defenses enable row level security;
alter table public.hd_arena_scores enable row level security;
alter table public.hd_arena_matches enable row level security;

-- Captures : lisibles par tout joueur connecté (adversaires), écrites par leur propriétaire seulement.
create policy hd_arena_defenses_select on public.hd_arena_defenses
  for select to authenticated using (true);
create policy hd_arena_defenses_insert on public.hd_arena_defenses
  for insert to authenticated with check (auth.uid() = user_id);
create policy hd_arena_defenses_update on public.hd_arena_defenses
  for update to authenticated using (auth.uid() = user_id) with check (auth.uid() = user_id);

-- Scores : classement lisible par tous les joueurs connectés ; aucune politique d'écriture (insert, update, delete refusés au client).
create policy hd_arena_scores_select on public.hd_arena_scores
  for select to authenticated using (true);

-- Journal : chacun ne lit que ses propres duels ; aucune écriture directe.
create policy hd_arena_matches_select on public.hd_arena_matches
  for select to authenticated using (auth.uid() = host_id);

-- La date de la capture est posée par le serveur.
create or replace function public.hd_arena_defenses_touch()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  new.updated_at := now();
  return new;
end
$$;

drop trigger if exists hd_arena_defenses_touch on public.hd_arena_defenses;
create trigger hd_arena_defenses_touch
  before insert or update on public.hd_arena_defenses
  for each row execute function public.hd_arena_defenses_touch();

-- ---------------------------------------------------------------------------------------------------------------------
-- 5. Fonctions (security definer, search_path vide : tout est qualifié)
-- ---------------------------------------------------------------------------------------------------------------------

-- Verse l'issue d'un duel : applique les points, le plafond de 10 duels par jour et par tournoi, la borne quotidienne et le départage.
-- Idempotente : le même p_match pour le même joueur ne compte qu'une fois. Renvoie les points totaux du joueur.
create or replace function public.hd_submit_duel(
  p_tournament text,
  p_match text,
  p_mode text,
  p_result text,
  p_opponent uuid default null
)
returns int
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_user uuid := auth.uid();
  v_points int;
  v_delta int := 0;
  v_counted boolean;
  v_today int;
  v_today_points int;
  c_daily_cap constant int := 10;
  c_daily_points constant int := 200;   -- 10 victoires en ligne au maximum
begin
  if v_user is null then
    raise exception 'not_authenticated' using errcode = '28000';
  end if;
  if p_tournament !~ '^[a-z_]{1,40}:[0-9]{4}$' or p_mode not in ('offline', 'online')
     or p_result not in ('win', 'loss', 'quit', 'cancelled') or length(p_match) not between 1 and 64 then
    raise exception 'invalid_arguments' using errcode = '22023';
  end if;
  if p_opponent = v_user then
    raise exception 'invalid_arguments' using errcode = '22023';
  end if;

  -- Rejeu du même duel : on ne recompte pas.
  if exists (select 1 from public.hd_arena_matches m
             where m.tournament_id = p_tournament and m.host_id = v_user and m.match_ref = p_match) then
    select s.points into v_points from public.hd_arena_scores s where s.tournament_id = p_tournament and s.user_id = v_user;
    return coalesce(v_points, 0);
  end if;

  -- Un duel annulé ou un départ hors ligne ne compte pas dans le plafond.
  v_counted := not (p_result = 'cancelled' or (p_result = 'quit' and p_mode = 'offline'));

  select s.points into v_points from public.hd_arena_scores s
    where s.tournament_id = p_tournament and s.user_id = v_user for update;
  v_points := coalesce(v_points, 0);

  select count(*) filter (where m.counted), coalesce(sum(m.points) filter (where m.points > 0), 0)
    into v_today, v_today_points
    from public.hd_arena_matches m
    where m.host_id = v_user and m.tournament_id = p_tournament
      and m.created_at >= date_trunc('day', now() at time zone 'utc') at time zone 'utc';

  if v_counted and v_today >= c_daily_cap then
    raise exception 'daily_cap' using errcode = 'P0001';
  end if;

  if p_result = 'win' then
    v_delta := case p_mode when 'online' then 20 else 10 end;
    v_delta := least(v_delta, greatest(0, c_daily_points - v_today_points));   -- borne quotidienne des points
  elsif p_result = 'quit' and p_mode = 'online' then
    v_delta := -least(10, v_points);                                           -- jamais sous 0
  end if;

  insert into public.hd_arena_scores as s (tournament_id, user_id, points, reached_at)
    values (p_tournament, v_user, greatest(0, v_points + v_delta), now())
    on conflict (tournament_id, user_id) do update
      set points = greatest(0, s.points + v_delta),
          reached_at = case when v_delta <> 0 then now() else s.reached_at end;

  insert into public.hd_arena_matches (tournament_id, match_ref, host_id, guest_id, mode, result, points, counted)
    values (p_tournament, p_match, v_user, p_opponent, p_mode, p_result, v_delta, v_counted);

  return greatest(0, v_points + v_delta);
end
$$;

-- Classement d'une édition (top p_top, au plus 100) avec le nom des joueurs lu dans leur capture.
create or replace function public.hd_arena_leaderboard(p_tournament text, p_top int default 100)
returns table (rank bigint, user_id uuid, name text, points int, is_player boolean)
language sql
stable
security definer
set search_path = ''
as $$
  select row_number() over (order by s.points desc, s.reached_at, s.user_id) as rank,
         s.user_id,
         coalesce(d.snapshot ->> 'Name', '') as name,
         s.points,
         s.user_id = auth.uid() as is_player
    from public.hd_arena_scores s
    left join public.hd_arena_defenses d on d.user_id = s.user_id
   where s.tournament_id = p_tournament and s.points > 0
   order by s.points desc, s.reached_at, s.user_id
   limit least(greatest(p_top, 1), 100)
$$;

-- Place du joueur : rang (0 si non classé), nombre de classés et points.
create or replace function public.hd_arena_standing(p_tournament text)
returns table (rank int, total int, points int)
language sql
stable
security definer
set search_path = ''
as $$
  with me as (
    select s.points, s.reached_at from public.hd_arena_scores s
     where s.tournament_id = p_tournament and s.user_id = auth.uid() and s.points > 0
  )
  select coalesce((select 1 + count(*)::int from public.hd_arena_scores o, me
                    where o.tournament_id = p_tournament and o.points > 0
                      and (o.points > me.points or (o.points = me.points and o.reached_at < me.reached_at))), 0),
         (select count(*)::int from public.hd_arena_scores o where o.tournament_id = p_tournament and o.points > 0),
         coalesce((select me.points from me), 0)
$$;

-- Adversaire hors ligne : une capture récente de score proche, jamais celle du joueur. Aucune ligne : le client génère une équipe de repli.
create or replace function public.hd_arena_find_opponent(p_tournament text, p_rating int)
returns table (match_id text, seed bigint, opponent_id uuid, snapshot jsonb)
language sql
volatile
security definer
set search_path = ''
as $$
  select gen_random_uuid()::text, floor(random() * 2147483647)::bigint, c.user_id, c.snapshot
    from (select d.user_id, d.snapshot
            from public.hd_arena_defenses d
           where d.user_id <> auth.uid()
             and d.updated_at > now() - interval '60 days'
           order by abs(d.rating - p_rating), d.user_id
           limit 5) c
   order by random()
   limit 1
$$;

-- Réclame un palier : 1 à 5 (points), 6 (top 10 %), 7 (top 3, après la fin). Refusé si déjà réclamé, hors délai (7 jours après p_ends_on) ou non mérité.
create or replace function public.hd_claim_tier(p_tournament text, p_tier int, p_ends_on date)
returns void
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_user uuid := auth.uid();
  v_points int;
  v_claimed int[];
  v_rank int;
  v_total int;
  c_tiers constant int[] := array[50, 150, 300, 500, 800];
begin
  if v_user is null then
    raise exception 'not_authenticated' using errcode = '28000';
  end if;
  if p_tier not between 1 and 7 or p_tournament !~ '^[a-z_]{1,40}:[0-9]{4}$' then
    raise exception 'invalid_arguments' using errcode = '22023';
  end if;
  if current_date > p_ends_on + 7 then
    raise exception 'claim_expired' using errcode = 'P0001';
  end if;

  select s.points, s.claimed_tiers into v_points, v_claimed
    from public.hd_arena_scores s where s.tournament_id = p_tournament and s.user_id = v_user for update;
  if not found then
    raise exception 'not_earned' using errcode = 'P0001';
  end if;
  if p_tier = any (v_claimed) then
    raise exception 'already_claimed' using errcode = 'P0001';
  end if;

  if p_tier <= 5 then
    if v_points < c_tiers[p_tier] then
      raise exception 'not_earned' using errcode = 'P0001';
    end if;
  else
    if current_date <= p_ends_on then
      raise exception 'not_finished' using errcode = 'P0001';
    end if;
    select st.rank, st.total into v_rank, v_total from public.hd_arena_standing(p_tournament) st;
    if v_rank = 0 or (p_tier = 7 and v_rank > 3) or (p_tier = 6 and v_rank > greatest(1, ceil(v_total * 0.10)::int)) then
      raise exception 'not_earned' using errcode = 'P0001';
    end if;
  end if;

  update public.hd_arena_scores s
     set claimed_tiers = array_append(s.claimed_tiers, p_tier)
   where s.tournament_id = p_tournament and s.user_id = v_user;
end
$$;

-- Droits : seules les fonctions de l'arène sont appelables, et seulement par un joueur connecté.
revoke all on function public.hd_submit_duel(text, text, text, text, uuid) from public, anon;
revoke all on function public.hd_arena_leaderboard(text, int) from public, anon;
revoke all on function public.hd_arena_standing(text) from public, anon;
revoke all on function public.hd_arena_find_opponent(text, int) from public, anon;
revoke all on function public.hd_claim_tier(text, int, date) from public, anon;
grant execute on function public.hd_submit_duel(text, text, text, text, uuid) to authenticated;
grant execute on function public.hd_arena_leaderboard(text, int) to authenticated;
grant execute on function public.hd_arena_standing(text) to authenticated;
grant execute on function public.hd_arena_find_opponent(text, int) to authenticated;
grant execute on function public.hd_claim_tier(text, int, date) to authenticated;
