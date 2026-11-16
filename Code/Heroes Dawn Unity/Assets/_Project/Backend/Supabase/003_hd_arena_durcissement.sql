-- Durcissement de l'arène, après 002_hd_arena.sql. Appliqué sur N29 HQ le 2026-11-16.
-- La fonction de déclencheur n'est pas appelable par l'API ; les droits de table sont réduits au nécessaire
-- (aucun accès anonyme, aucune suppression, scores et journal en lecture seule : seules les fonctions hd_* les écrivent).

revoke execute on function public.hd_arena_defenses_touch() from public, anon, authenticated;
revoke all on public.hd_arena_defenses, public.hd_arena_scores, public.hd_arena_matches from anon;
revoke all on public.hd_arena_defenses, public.hd_arena_scores, public.hd_arena_matches from authenticated;
grant select, insert, update on public.hd_arena_defenses to authenticated;
grant select on public.hd_arena_scores, public.hd_arena_matches to authenticated;
