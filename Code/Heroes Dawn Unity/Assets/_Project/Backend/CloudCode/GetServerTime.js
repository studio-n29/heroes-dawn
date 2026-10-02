/*
 * GetServerTime — heure serveur (ms UTC).
 * Sert à valider la chaîne de déploiement depuis Unity (Window ▸ Deployment)
 * et, au M2, à corriger le décalage d'horloge du client (timers du QG).
 */
module.exports = async ({ params, context, logger }) => {
  return { utcMs: Date.now() };
};

module.exports.params = {};
