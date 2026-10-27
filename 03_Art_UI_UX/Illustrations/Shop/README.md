# Icônes de la boutique (SVG)

Huit icônes dessinées en SVG (512 × 512, fond sombre, formes à facettes) pour les cartes de la boutique :
`booster`, `gems_small`, `gems_medium`, `gems_large`, `exchange_credits`, `exchange_pr`, `exchange_rest`, `gift`.

Le jeu les charge par identifiant depuis `Assets/_Project/Resources/Illustrations/Shop/<id>.png` (voir `AgentArt.Shop`).
Les PNG sont rasterisés à partir des SVG ; pour les régénérer après une retouche d'un SVG, avec Node et `sharp` :

```
node -e "const sharp=require('sharp');const fs=require('fs');for(const f of fs.readdirSync('.').filter(f=>f.endsWith('.svg')))sharp(f).resize(512,512).png().toFile('../../../Code/Heroes Dawn Unity/Assets/_Project/Resources/Illustrations/Shop/'+f.replace('.svg','.png'))"
```
