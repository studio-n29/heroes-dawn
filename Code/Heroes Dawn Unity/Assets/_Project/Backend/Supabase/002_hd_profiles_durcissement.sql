-- Lot L5 (0.15.0) : durcissement de hd_profiles, après la migration 001 (sécurité par ligne, limite de taille).
-- Les droits sont réduits au nécessaire (pas de suppression depuis le client), le profil doit être un objet JSON,
-- la version de schéma est positive, et la fonction de déclencheur n'est pas appelable par l'API.
-- À appliquer d'abord sur le projet de test, puis en production (N29 HQ) après validation.

revoke all on public.hd_profiles from anon, authenticated;
grant select, insert, update on public.hd_profiles to authenticated;

alter table public.hd_profiles
  add constraint hd_profiles_object check (jsonb_typeof(profile) = 'object'),
  add constraint hd_profiles_version check (schema_version >= 1);

revoke execute on function public.hd_profiles_touch() from public, anon, authenticated;

-- Même le propriétaire de la table passe par la sécurité par ligne (sauf le rôle de service, hors du client).
alter table public.hd_profiles force row level security;
