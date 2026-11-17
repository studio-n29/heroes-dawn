from pathlib import Path
from PIL import Image, ImageOps, ImageDraw, ImageFont
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, KeepTogether, Image as PdfImage
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from pypdf import PdfReader
from html import unescape, escape
import json,re,csv,math
R=Path(__file__).resolve().parent; O=R/'Exports'; P=R.parent.parent/'Code/Heroes Dawn Unity/Assets/_Project'; E=P/'Art/UI/AubeTactiqueExtension'
data=json.loads((R/'source-list.json').read_text(encoding='utf-8')); manifest=json.loads((R/'manifest.json').read_text(encoding='utf-8'))
pdf_source=R.parent.parent/'05_Production/Asset_List_2D_Manquants_2026-11-16.pdf'
page_text=[p.extract_text() for p in PdfReader(pdf_source).pages]
def clean(s):return unescape(re.sub('<[^>]+>','',s)).replace('\u2726','+').replace('\u2011','-').replace('\u2013','-').replace('\u2014','-')
def pageof(id):return next((str(i+1) for i,t in enumerate(page_text) if re.search(r'(?<![A-Z0-9])'+re.escape(id)+r'(?!\d)',t)),'voir source')
notes={
'M-01':'Ajouter mos_guard à HudPortraits.Foes et la règle hd-foe--mos_guard dans hd-combat.uss. Employer le 128 pour le HUD, le 512 pour les Archives, le 1024 pour les grands visuels.',
'M-02':'Ajouter mos_smuggler à HudPortraits.Foes et sa règle USS. Même routage de tailles que M-01.',
'M-03':'Ajouter mos_enforcer à HudPortraits.Foes et sa règle USS. Frames/enemy-elite.png est un cadre séparé à superposer au portrait ; ne pas l’étirer en 9-slice.',
'M-04':'Phase 1 : Marchandage au départ. Phase 2 : Les clients arrivent sous 66 % PV. Phase 3 : Tout doit partir sous 33 % PV. Les expressions restent séparées. Employer les 128 dans la frise et les 512 aux Archives. Trois tailles par phase sont livrées (9 fichiers, au lieu de l’estimation 6).',
'M-05':'Pour le chargement existant AgentArt.For, copier en plus Dialogue/enforcer.png vers Resources/Illustrations/Agents/enforcer.png et Dialogue/passeur.png vers Resources/Illustrations/Agents/passeur.png. Ces noms correspondent aux orateurs du dialogue.',
'M-06':'Fond, masques et calques utilisent tous le même canevas 3840 x 2160, sans recadrage indépendant. Les polygones sont ceux de CityMapLayout.MoscowBoroughs, y compris l’anneau ouvert à l’est. Superposer normal puis selected sur le même RectTransform. Seul Sergueiv reste jouable ; les autres quartiers gardent la logique Bientôt. La géographie illustrée reste stylisée : contrôler l’alignement visuel en jeu. Le fond est agrandi depuis un master 1672 x 941, pas généré nativement en 4K.',
'S-01':'8 recrues et 4 emblèmes. Le jeu applique le masque rond au moment de l’affichage. Conserver les identifiants recruit-01 à recruit-08 et family-bio / elemental / matter / perception.',
'S-02':'Associer la silhouette à la famille de l’agent absent. Le cadenas du kit reste une couche séparée ; aucune serrure n’est incrustée.',
'N-01':'Les noms de fichiers reprennent les identifiants des fragments : redhook_docks, brownsville_hostages, coney_trap, dumbo_bridge, mos_maglev_welcome, mos_monastery_register, mos_convoy_crates, mos_depot_passeur. Placer le visuel au-dessus du texte dans la fenêtre LIRE, avec conservation du ratio 16:9.',
'G-01':'Correspondance : burn=Brûlure ; freeze=Gel ; shock=Choc ; corrosion=Corrosion ; blind=Aveuglé ; vulnerable=Vulnérable ; snare=Entrave ; mark=Marque ; taunt=Provocation ; shield=Bouclier ; regeneration=Régénération ; haste=Hâte ; dodge=Esquive ; anchor=Ancrage. Durée et cumul restent ajoutés par le code. Affichage 32 à 48 px.',
'G-02':'frontline=Première ligne ; attacker=Attaquant ; control=Contrôle ; support=Soutien ; ranged=Précision à distance. Sprites blancs avec alpha, à teinter par la famille.',
'G-03':'amif-1 à amif-5 correspondent aux rangs croissants, pas à la rareté historique. Production incluse selon la demande de tout générer ; direction visuelle à valider car la liste source la signalait à confirmer.',
'G-04':'11 familles, chacune avec projectile et impact : impact, fire, ice, lightning, acid, chains, mark, heal, shield, haste, time. Chaque fichier est une image fixe destinée au mouvement/scale/fade et aux particules du code, pas une planche animée. Projectile orienté vers la droite, pivot central. Brancher dans SkillFxHud.Projectile/Burst. Matériau additif recommandé pour les lueurs ; tester la teinte sur fond clair et sombre.',
'U-01':'Associer normal / hover / pressed / focus aux états UI. La croix est déjà incluse. Sprite simple, pas de 9-slice. La zone interactive doit rester au moins 48 px même si le visuel est plus petit.',
'U-02':'Bulle + 4 pointes + portrait rond = 6 fichiers. La liste indiquait environ 5 mais énumérait bien ces six éléments. Border de la bulle : 20 px sur chaque côté. Les pointes tournent/alternent selon l’ancre ; elles restent des sprites séparés. Le buste rond est un export du portrait existant de Brie, sans nouvelle identité.',
'U-03':'Le nœud laisse le carré intérieur au portrait et la zone basse au nom/famille rendus par le code. Liens 3 / 8 / 15 / 25 du plus fin au plus fort. Ne pas étirer un lien à coude : ajuster sa taille ou utiliser les segments dans un VisualElement. La pastille +15 % contient déjà le texte français.',
'U-04':'reward-locked / claimable / claimed pour les états ; ribbon-free et ribbon-premium pour les rangées ; active-tier pour le losange ; claim-label pour RÉCLAMER. Les cases n’incluent aucun objet de récompense : superposer le visuel concerné. Border de 16 px pour les surfaces extensibles. Les rubans texte conviennent au français ; utiliser les SVG sans texte et des labels pour une autre langue.',
'U-05':'Ordre : booster-closed, booster-torn, booster-reveal ; booster-rays reste derrière le paquet. Les trois états ont un canevas 1024 carré. Direction proposée à valider, puisque le PDF la signalait à confirmer. Aucun système d’animation n’a été intégré.',
'C-01':'Associer outfit_agent_d / agent_m / victor_sorel / odile_ferrand aux IDs outfit.* correspondants de cosmetics.json. Les fichiers outfit_pass_vanguard / pass_nightshift / pass_dawn sont proposés sur Giulia : le catalogue ne définit pas d’agent propriétaire. Valider cette attribution avant branchement. Les portraits alternatifs et leurs vignettes sont séparés ; conserver la pose et contrôler l’identité face au détouré d’origine.',
'C-02':'hq_nocturne -> hq.nocturne ; hq_aube -> hq.aube ; hq_pass_dawn -> hq.pass_dawn. Afficher les JPG 1920 x 1080 comme fonds et les preview 512 x 288 dans la boutique/Pass. Même scène de référence, avec variations générées : vérifier la lisibilité sous les tuiles du QG.',
'C-03':'chrome-1 à 6 -> dice.chrome ; neon-1 à 6 -> dice.neon ; pass_amber-1 à 6 -> dice.pass_amber. Le chiffre du fichier est exactement le nombre de points. Les preview représentent la face 6, ce sont des vignettes du skin.',
'C-04':'brass-idle / active -> pawn.brass ; pass_scout-idle / active -> pawn.pass_scout. L’état actif comporte les accents lumineux. Pivot au centre de la case du plateau.',
'C-05':'new_year, valentine, spring, summer, holidays, back_to_school, halloween, christmas : chaque événement dispose d’un frame et d’un title. medal-1 à 5 : bronze, argent, or, platine, diamant ; top-10 et top-3 : insignes séparés. Seuls Noël et Nouvel An existent actuellement dans cosmetics.json ; déclarer les autres IDs avant branchement. Le cadre a un centre transparent. Ces propositions couvrent la ligne encore à confirmer du PDF.',
'R-01':'Fichier store uniquement, composé depuis HD_Icon_Foreground + HD_Icon_Background existants. PNG RGBA 512 x 512, taille vérifiée sous 1 Mo. Ne pas remplacer les deux couches adaptatives Android par cette image aplatie.',
'R-02':'Image de présentation 1024 x 500, logo côté gauche, héroïne et skyline à droite. Importer dans la fiche Google Play ; ce visuel est une illustration promotionnelle, pas une capture d’écran de jeu.',
'R-03':'AUCUN fichier synthétique livré. À capturer après intégration dans le jeu final : titre, QG, équipe, plateau, combat, boss, boutique, tournoi, en 1920 x 1080. Les quatre vues tablette restent optionnelles. Les captures internes et les maquettes de référence ne sont pas des captures de store finales.',
'R-04':'heroes-dawn.ico contient 16, 32, 48 et 256 px. L’affecter à l’icône Windows du Player Settings. launch-n29.png est l’écran de lancement ; son affichage nécessite une scène/image de démarrage ou une configuration compatible avec la licence Unity utilisée. Le logo a été adapté par génération depuis la référence officielle : comparer la marque avant publication.'}
notes['U-04'] += ' Conserver le ratio natif des rubans avec texte : ne pas étirer les lettres en 9-slice. Pour une largeur variable ou une autre langue, retirer le texte du SVG éditable et superposer un label.'
notes['U-03'] += ' Conserver le ratio natif de la pastille texte +15 %. Le border inférieur du nœud (68 px) protège la zone du nom.'
def dest(rec):
    if rec['id'].startswith('R-'):return 'Diffusion / packaging (hors Resources)'
    return 'Assets/_Project/Resources/UI/AubeTactique/Missing20261116/'+rec['file']
def runtime(rec):return '' if rec['id'].startswith('R-') else 'UI/AubeTactique/Missing20261116/'+str(Path(rec['file']).with_suffix('')).replace('\\','/')
for r in manifest:r['runtime_destination']=dest(r);r['resource_key']=runtime(r)
(R/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
with (R/'Integration_Nouveaux.csv').open('w',newline='',encoding='utf-8-sig') as f:
    w=csv.DictWriter(f,fieldnames=['id','file','width','height','runtime_destination','resource_key','border'],extrasaction='ignore');w.writeheader();w.writerows(manifest)
groups={'I-01':['Chapters','ChaptersLocked'],'I-04':['Poses'],'I-05':['MiniGameIntros'],'I-06':['MiniGames'],'I-07':['Rivals'],'I-08':['Dossiers'],'I-09':['MapMasks','MapOverlays'],'I-10':['BrandIcons'],'I-11':['ResourceIcons'],'I-12':['Decorations','IconsGold'],'I-14':['Components'],'I-15':['Icons']}
named={'I-02':['title-new-york','loading-team','construction-dawn','support-kremlin','profile-default'],'I-03':['event-quais-paralyses','event-bridge-evacuation','event-brownsville-hostages','event-coney-blockade','zone-secured','minigame-success','minigame-failure']}
bmap=[]
for row in data['B']:
    id=row['id'];files=[]
    for group in groups.get(id,[]):files+=sorted((E/group).glob('*.png'))
    for name in named.get(id,[]):files.append(E/'Scenes'/f'{name}.png')
    if id=='I-09':files.append(E/'Scenes/map-new-york.png')
    if id=='I-10':files=[f for f in files if f.stem in ['apple','google']]
    if id=='I-12':files=[f for f in files if f.stem in ['laurel-left','laurel-right','medal-1','medal-2','medal-3']]
    if id=='I-13':files=[E/'Enemies/enemy-helmet-thug.png']
    if id=='I-11':
        generated=P/'Art/UI/AubeTactiqueKit/Illustrations/Generated'
        for pattern in ['shop-pack-*','shop-credits-*','shop-gems-*','shop-gift-daily']:files+=sorted(generated.glob(pattern+'.png'))
    for f in files:
        if not f.exists():continue
        rel=f.relative_to(P).as_posix();group=f.parent.name
        target='Resources/UI/AubeTactique/Ext/'+group+'/'+f.name
        action='Copier ou vérifier la copie existante, puis brancher à l’écran indiqué. Aucun dessin à refaire.'
        if id=='I-01' and group in ['Chapters','ChaptersLocked']:
            chapter={'ny-01':'ny_brooklyn','ny-02':'ny_queens','ny-03':'ny_harlem','ny-04':'ny_manhattan','mos-01':'mos_sergueiv','mos-02':'mos_kapotnya','mos-03':'mos_sadovoye','mos-04':'mos_place_rouge'}[f.stem]
            target='Resources/Illustrations/'+group+'/'+chapter+'.png';action='ID vérifié dans Packages/com.n29.heroesdawn.domain/Definitions/Chapters.cs. AgentArt.Chapter charge Chapters ; brancher explicitement ChaptersLocked pour les vignettes verrouillées.'
        if id=='I-11' and group=='Generated':target='Resources/UI/AubeTactique/'+f.name
        bmap.append(dict(id=id,source='Assets/_Project/'+rel,destination='Assets/_Project/'+target,action=action))
with (R/'Integration_Existants_Liste_B.csv').open('w',newline='',encoding='utf-8-sig') as f:
    w=csv.DictWriter(f,fieldnames=['id','source','destination','action']);w.writeheader();w.writerows(bmap)
# Compact browsable catalogue and one dark-background QA thumbnail per new export.
preview=R/'Catalogue';preview.mkdir(exist_ok=True)
for r in manifest:
    f=O/r['file'];im=Image.open(f).convert('RGBA');im.thumbnail((400,210),Image.Resampling.LANCZOS)
    canvas=Image.new('RGBA',(420,230),'#162535');canvas.alpha_composite(im,((420-im.width)//2,(230-im.height)//2));canvas.convert('RGB').save(preview/(r['file'].replace('/','_')+'.jpg'),quality=88)
rows=[r for g in data['A'] for r in g['rows']]
html=['<!doctype html><meta charset="utf-8"><title>Heroes Dawn - Guide intégration 2D</title><style>body{font:16px Segoe UI,sans-serif;background:#101c29;color:#ece7dc;max-width:1200px;margin:auto;padding:32px}a{color:#e8c87c}h1,h2{color:#d6af5b}section{border-top:1px solid #64727c;margin-top:35px;padding-top:20px}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(230px,1fr));gap:12px}.card{background:#1c2d3b;padding:10px;overflow-wrap:anywhere}.card img{width:100%}code{overflow-wrap:anywhere}p{line-height:1.6}</style><h1>HEROES DAWN - Guide d’intégration 2D</h1>']
html.append(f'<p>Livraison du 16 novembre 2026 : <b>{len(manifest)} exports</b>. Références M / S / N / G / U / C / R identiques au PDF source. Les captures R-03 restent à prendre dans le jeu final. Aucun fichier Unity ni code de jeu existant n’a été modifié.</p><p><a href="Integration_Nouveaux.csv">Correspondance complète des nouveaux fichiers</a> · <a href="Integration_Existants_Liste_B.csv">Liste B : sources et destinations</a> · <a href="Guide_Integration_Assets_2D.pdf">Guide PDF</a></p>')
for row in rows:
    id=row['id'];assets=[m for m in manifest if m['id']==id]
    html.append(f'<section id="{id}"><h2>{id} - {escape(clean(row["t"]))}</h2><p>PDF source p. {pageof(id)} · {escape(clean(row["u"]))}</p><p>{escape(notes[id])}</p><div class="grid">')
    for a in assets:
        html.append(f'<div class="card"><a href="Exports/{a["file"]}"><img loading="lazy" src="Catalogue/{a["file"].replace("/","_")}.jpg"></a><b>{a["file"]}</b><p>{a["width"]} x {a["height"]}<br><code>{escape(a["resource_key"] or "Packaging / store")}</code></p></div>')
    html.append('</div></section>')
html.append('<section><h2>Liste B - déjà produits</h2><p>Utiliser le CSV dédié. Les chemins ont été retrouvés sur disque ; leur branchement et le rendu Unity restent à vérifier.</p>')
for row in data['B']:html.append(f'<h3>{row["id"]} - {escape(clean(row["t"]))}</h3><p>{escape(clean(row["d"]))}</p><p>Écrans : {escape(clean(row["u"]))}</p>')
html.append('</section>');(R/'Guide_Integration_Assets_2D.html').write_text('\n'.join(html),encoding='utf-8')
# Print guide: each source row has its own reference, example and exact file inventory.
pdfmetrics.registerFont(TTFont('Guide','C:/Windows/Fonts/arial.ttf'));pdfmetrics.registerFont(TTFont('GuideBold','C:/Windows/Fonts/arialbd.ttf'))
styles=getSampleStyleSheet();styles.add(ParagraphStyle(name='Guide',fontName='Guide',fontSize=9,leading=13,spaceAfter=6));styles.add(ParagraphStyle(name='SmallGuide',fontName='Guide',fontSize=7.5,leading=10,spaceAfter=4));styles.add(ParagraphStyle(name='HeadGuide',fontName='GuideBold',fontSize=17,leading=21,textColor=colors.HexColor('#101C29'),spaceAfter=12));styles.add(ParagraphStyle(name='SubGuide',fontName='GuideBold',fontSize=11,leading=14,textColor=colors.HexColor('#9A762F'),spaceAfter=7))
def p(s,small=False):return Paragraph(escape(clean(s)),styles['SmallGuide' if small else 'Guide'])
def table(rows,widths):
    t=Table([[p(c,True) for c in row] for row in rows],colWidths=widths,repeatRows=1,hAlign='LEFT');t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),colors.HexColor('#EEE9DD')),('VALIGN',(0,0),(-1,-1),'TOP'),('LINEBELOW',(0,0),(-1,0),.6,colors.HexColor('#D6AF5B')),('LINEBELOW',(0,1),(-1,-1),.2,colors.HexColor('#DDDDDD')),('LEFTPADDING',(0,0),(-1,-1),5),('RIGHTPADDING',(0,0),(-1,-1),5),('TOPPADDING',(0,0),(-1,-1),3),('BOTTOMPADDING',(0,0),(-1,-1),2)]));return t
story=[Paragraph('HEROES DAWN<br/>Guide d’intégration des assets 2D',styles['HeadGuide']),p('16 novembre 2026 - Studio N29'),Spacer(1,12),p(f'{len(manifest)} fichiers exportés et vérifiés. Toutes les créations de la liste A sont couvertes, sauf R-03 : les captures de store doivent provenir du jeu final. La liste B est documentée pour réutilisation des fichiers existants.'),p('Racine de livraison : principal/03_Art_UI_UX/Production_2D_2026-11-16/'),p('Exports/ contient les fichiers à intégrer. Sources/ conserve les masters générés et les SVG éditables. Le catalogue HTML montre chaque fichier ; les CSV donnent la correspondance exhaustive, fichier par fichier.'),p('Source de référence : '+pdf_source.name),Spacer(1,10),Paragraph('Convention de copie',styles['SubGuide']),p('Pour un rangement isolé : copier Exports/ (hors Store/) dans Assets/_Project/Art/UI/AubeTactiqueMissing20261116/. Pour le chargement proposé : recopier la même arborescence dans Assets/_Project/Resources/UI/AubeTactique/Missing20261116/.'),p('Exemple : Status/burn.png devient la clé Resources UI/AubeTactique/Missing20261116/Status/burn. Avec le helper existant : TacticalArt.Load("Missing20261116/Status/burn"). Le dossier Resources ne suffit pas à modifier l’affichage : les écrans doivent encore appeler ces clés.'),p('Exception M-05 : les deux copies vers Resources/Illustrations/Agents/enforcer.png et passeur.png permettent au chargeur AgentArt.For existant de trouver les PNJ. Store/ ne doit pas être chargé comme une collection de sprites UI.'),Paragraph('Réglages Unity',styles['SubGuide']),p('PNG UI : Texture Type Sprite (2D and UI), Sprite Mode Single, sRGB activé, Alpha Source Input, Alpha Is Transparency activé, Wrap Clamp, Filter Bilinear, mipmaps désactivées. Full Rect pour les sprites UI et effets. Garder les proportions ; ne pas étirer les portraits.'),p('Surfaces 9-slice : régler Border selon le manifest, dans l’ordre Left / Bottom / Right / Top. Les liens d’affinité et cadres inclinés ne sont pas des surfaces 9-slice. Fonds JPG : conserver le ratio 16:9. Masques : éviter la compression avec pertes et les mipmaps ; garder le canevas commun.'),p('Les visuels ont été générés avec l’outil imagegen intégré. Les composants nets prolongent le kit vectoriel existant. Les masters et prompts sont archivés. Les propositions signalées à confirmer dans le PDF source sont livrées pour validation visuelle, sans décision de périmètre ni intégration automatique.'),PageBreak()]
summary=[['Référence','Fichiers livrés','État']]+[[r['id'],str(sum(m['id']==r['id'] for m in manifest)),'À capturer en jeu' if r['id']=='R-03' else 'Livré'] for r in rows]
story += [Paragraph('Correspondance avec la liste source',styles['HeadGuide']),table(summary,[95,120,300]),Spacer(1,12),p('Les quantités du PDF étaient estimatives : M-03 ajoute le cadre élite ; M-04 fournit les trois tailles pour chaque phase ; U-02 comprend bien 6 fichiers (bulle, quatre pointes, portrait). R-03 n’est pas compté dans les exports.'),PageBreak()]
for row in rows:
    id=row['id'];assets=[m for m in manifest if m['id']==id]
    story += [Paragraph(id+' - '+escape(clean(row['t'])),styles['HeadGuide']),p(f'PDF source : page {pageof(id)} | priorité {row["prio"]} | {len(assets)} fichiers livrés'),p('Emplacement prévu : '+clean(row['u']))]
    ref=R.parent.parent/'05_Production/Asset_List_2D_refs/crops'/f'{id}.jpg'
    pics=[]
    if ref.exists():pics.append(PdfImage(str(ref),width=245,height=153))
    if assets:
        a=next((x for x in assets if 'preview' not in x['file']),assets[0]);pics.append(PdfImage(str(preview/(a['file'].replace('/','_')+'.jpg')),width=245,height=134))
    if pics:story += [Table([pics],colWidths=[257]*len(pics),hAlign='LEFT'),Spacer(1,8)]
    story += [p('Repère de gauche : élément surligné dans la référence source.'+(' Aperçu de droite : un fichier de la livraison, sur fond marine.' if assets else ''),True),p(notes[id])]
    if assets:
        story += [p('Destination de base : Resources/UI/AubeTactique/Missing20261116/ (sauf Store/). La colonne Fichier conserve le sous-dossier à ajouter. CSV : Integration_Nouveaux.csv.',True),table([['Fichier dans Exports/','Dimensions','Border L/B/R/T']]+[[a['file'],f'{a["width"]} x {a["height"]}',str(a.get('border','-'))] for a in assets],[325,90,100])]
    story.append(PageBreak())
story += [Paragraph('Liste B - assets existants à réutiliser',styles['HeadGuide']),p('Le fichier Integration_Existants_Liste_B.csv donne les sources retrouvées sur disque et des destinations proposées. Ces assets ne sont pas régénérés ni copiés automatiquement. Les chemins de données et les appels Unity doivent être contrôlés au moment de l’intégration. Pour I-01, utiliser impérativement les vrais ChapterDef.Id.'),p(f'{len(bmap)} correspondances de fichiers recensées. I-09 inclut le fond map-new-york en complément des 15 masques/calques ; les quantités peuvent donc différer du tableau initial.')]
for row in data['B']:
    story.append(KeepTogether([Paragraph(row['id']+' - '+escape(clean(row['t'])),styles['SubGuide']),p(clean(row['d'])),p('Écrans : '+clean(row['u'])),p('Correspondances détaillées : '+str(sum(x['id']==row['id'] for x in bmap))+' lignes dans le CSV dédié.',True)]))
story += [PageBreak(),Paragraph('Contrôle avant intégration',styles['HeadGuide']),p('Vérifiés automatiquement : présence et ouverture des exports, dimensions du manifest, alpha des PNG attendus, taille de l’icône Google Play, variantes de l’ICO et empreintes SHA-256. Le catalogue a servi au contrôle visuel.'),p('À vérifier dans Unity : cadrage des portraits dans les cadres inclinés ; bascule des phases du Passeur ; superposition des masques de carte ; lisibilité des états à 32 px ; zones de toucher ; rendu des 9-slice ; ordre des couches VFX et teinte ; attribution des tenues Pass ; aperçu des cosmétiques.'),p('À réaliser après intégration : les 8 captures R-03 et, si souhaité, 4 captures tablette. Les fichiers de store restent locaux : aucune publication n’a été faite.'),p('Remarque de résolution : les outils de génération peuvent retourner un master plus petit que le format demandé. Les exports ont les dimensions contractuelles, avec redimensionnement lorsque nécessaire. Les masters originaux sont conservés pour permettre un jugement de netteté ; la carte 4K est explicitement un agrandissement.'),p('Remarque d’animation : G-04 utilise la variante autorisée image fixe + particules. Les sprites ne constituent pas une animation précalculée. U-05 contient trois états distincts, à animer côté jeu.'),p('Ne pas confondre la liste source avec des instructions d’exécution : elle sert de spécification visuelle. Les décisions de publication, intégration et périmètre restent à appliquer dans le projet.')]
def footer(c,doc):
    c.setStrokeColor(colors.HexColor('#D6AF5B'));c.line(40,32,A4[0]-40,32);c.setFont('Guide',8);c.setFillColor(colors.HexColor('#586571'));c.drawString(40,20,'Heroes Dawn - Guide intégration 2D - 16/11/2026');c.drawRightString(A4[0]-40,20,str(doc.page))
doc=SimpleDocTemplate(str(R/'Guide_Integration_Assets_2D.pdf'),pagesize=A4,rightMargin=40,leftMargin=40,topMargin=35,bottomMargin=43,title='Heroes Dawn - Guide intégration assets 2D',author='Studio N29');doc.build(story,onFirstPage=footer,onLaterPages=footer)
print(f'PDF + HTML guides, {len(manifest)} new-file mappings, {len(bmap)} existing-file mappings written')
