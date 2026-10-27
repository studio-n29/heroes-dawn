-- Lot I (0.14.0) : sauvegarde cloud du profil joueur.
-- Une ligne par joueur ; chaque joueur ne lit et n'écrit que la sienne (sécurité par ligne).
-- Les tables du jeu portent le préfixe hd_ : les autres tables du projet appartiennent à un autre outil et ne sont jamais touchées.

create table if not exists public.hd_profiles (
  user_id uuid primary key references auth.users (id) on delete cascade,
  profile jsonb not null,
  schema_version int not null,
  updated_at timestamptz not null default now(),
  constraint hd_profiles_size check (pg_column_size(profile) <= 524288)
);

alter table public.hd_profiles enable row level security;

create policy hd_profiles_select on public.hd_profiles
  for select to authenticated
  using (auth.uid() = user_id);

create policy hd_profiles_insert on public.hd_profiles
  for insert to authenticated
  with check (auth.uid() = user_id);

create policy hd_profiles_update on public.hd_profiles
  for update to authenticated
  using (auth.uid() = user_id)
  with check (auth.uid() = user_id);

-- La date de dernière écriture est posée par le serveur : le client ne peut pas la truquer (elle sert de jeton de conflit).
create or replace function public.hd_profiles_touch()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  new.updated_at := now();
  return new;
end
$$;

create trigger hd_profiles_touch
  before insert or update on public.hd_profiles
  for each row execute function public.hd_profiles_touch();
