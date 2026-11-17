"""Export generated masters, deterministic masks and brand packaging. No generative API calls."""
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import json, math, shutil, csv, hashlib, sys
ROOT=Path(__file__).resolve().parent
OUT=ROOT/'Exports'; SRC=ROOT/'Sources'; PROJECT=ROOT.parent.parent/'Code/Heroes Dawn Unity/Assets/_Project'
GEN=Path('C:/Users/ledim/.codex/generated_images/01a1e43a-2332-7991-9195-cb53bdf30dae')
records=[]
def emit(im,rel,id,method='imagegen-export',**kw):
    dest=OUT/rel; dest.parent.mkdir(parents=True,exist_ok=True)
    if dest.suffix.lower() in ['.jpg','.jpeg']: im=im.convert('RGB'); im.save(dest,quality=95,subsampling=0)
    else: im.save(dest,optimize=True)
    records.append(dict(id=id,file=rel,width=im.width,height=im.height,method=method,**kw))
def fit(im,size,centering=(.5,.5)):return ImageOps.fit(im,size,Image.Resampling.LANCZOS,centering=centering)
def contain(im,size):return ImageOps.pad(im,size,Image.Resampling.LANCZOS,color=(0,0,0,0))
def master(file,name):
    f=Path(file)
    if not f.is_absolute():f=GEN/file
    dest=SRC/'Generated'/f'{name}.png';dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(f,dest)
    return Image.open(f).convert('RGBA')
def enemies():
    for id,name,file in [('M-01','mos_guard','exec-5841d578-7071-4b73-86a6-6e969772d498.png'),('M-02','mos_smuggler','exec-a2b354fb-1392-4f54-b586-d8fd6c220b6b.png'),('M-03','mos_enforcer','exec-3615b0d7-4e8b-4b6d-8664-ebaa7bfcfe5c.png')]:
        im=master(file,name)
        for n in [1024,512,128]:emit(contain(im,(n,n)),f'Enemies/{name}-{n}.png',id)
    phases=['exec-0ff19b5e-290c-4f8f-9f54-993ccdd8800f.png','exec-7afe9122-3809-4763-8910-dadedad5b25c.png','exec-7c417503-b008-4a18-ab3a-ee87e2610c26.png']
    for i,file in enumerate(phases,1):
        im=master(file,f'mos_passeur-phase-{i}')
        emit(contain(im,(1536,1024)),f'Enemies/mos_passeur-phase-{i}.png','M-04')
        # Crop central portrait without stretching the original wide canvas.
        for n in [512,128]:emit(fit(im,(n,n),(.52,.2)),f'Enemies/mos_passeur-phase-{i}-{n}.png','M-04')
    for name,file in [('enforcer','exec-306abe31-eb00-4a47-9715-45cff9a9a503.png'),('passeur','exec-21305066-be3d-4c22-b988-ec098ee0f4b9.png')]:emit(contain(master(file,'dialogue-'+name),(1024,1536)),f'Dialogue/{name}.png','M-05')
def mapping():
    im=fit(master('exec-3afbb990-e833-4ca5-bfc1-bb17f0758dc1.png','map-moscow'),(3840,2160))
    emit(im,'Map/map-moscow.jpg','M-06',source_resolution='1672x941',note='Export 4K redimensionné depuis le master généré ; pas un rendu natif 4K.')
    ring=[]
    for a in range(25,336,30):ring.append((.5+.17*math.cos(math.radians(a)),.54+.17*math.sin(math.radians(a))))
    for a in range(335,24,-30):ring.append((.5+.10*math.cos(math.radians(a)),.54+.10*math.sin(math.radians(a))))
    polys={'sergueiv':[(.05,.12),(.40,.06),(.46,.30),(.30,.46),(.08,.42)],'kapotnya':[(.62,.60),(.92,.56),(.95,.88),(.66,.92),(.56,.76)],'sadovoye':ring,'place_rouge':[(.46,.49),(.54,.49),(.55,.56),(.50,.60),(.45,.56)]}
    for name,pts in polys.items():
        xy=[(round(x*3840),round(y*2160)) for x,y in pts]
        mask=Image.new('L',im.size,0);ImageDraw.Draw(mask).polygon(xy,fill=255)
        m=Image.new('RGBA',im.size,'white');m.putalpha(mask);emit(m,f'MapMasks/{name}.png','M-06','code-exact-mask')
        for state,alpha in [('normal',150),('selected',225)]:
            layer=im.copy();layer.putalpha(mask.point(lambda p:p*alpha//255))
            if state=='selected':ImageDraw.Draw(layer).line(xy+[xy[0]],fill='#D6AF5B',width=8,joint='curve')
            emit(layer,f'MapOverlays/{name}-{state}.png','M-06','code-exact-overlay')
    (ROOT/'map-polygons.json').write_text(json.dumps(polys,indent=2),encoding='utf-8')
def generated_jobs(only=None):
    jobs=json.loads((ROOT/'generated-jobs.json').read_text(encoding='utf-8'))+json.loads((ROOT/'generated-fx-jobs.json').read_text(encoding='utf-8'))
    corrections=json.loads((ROOT/'generated-corrections.json').read_text(encoding='utf-8')) if (ROOT/'generated-corrections.json').exists() else []
    overrides={j['key']:j for j in corrections}
    for job in jobs:
        job=overrides.get(job['key'],job)
        if only and job['key']!=only:continue
        im=master(job['source'],job['key']);key=job['key']
        if job['kind']=='memory':emit(fit(im,(1200,675)),f'Memories/{key}.jpg','N-01')
        elif job['kind']=='avatar':emit(fit(im,(256,256)),f'Avatars/{key}.png','S-01')
        elif job['kind']=='outfit':
            emit(contain(im,(1024,1536)),f'Outfits/{key}.png','C-01')
            # The head occupies the top half of these authored half-body cutouts.
            emit(fit(im.crop((0,0,im.width,im.width)),(512,512)),f'Outfits/{key}-preview.png','C-01')
        elif job['kind']=='hq':
            emit(fit(im,(1920,1080)),f'HQ/{key}.jpg','C-02',note='Export redimensionné ; composition préservée au mieux par génération.')
            emit(fit(im,(512,288)),f'HQ/{key}-preview.jpg','C-02')
        elif job['kind']=='fx':emit(contain(im,(256,256)),f'FX/{key}.png','G-04',blend='Additive ou alpha selon le matériau ; projectile vers la droite, pivot central.')
        elif job['kind']=='booster':emit(contain(im,(2048,1024) if key=='booster-rays' else (1024,1024)),f'Boosters/{key}.png','U-05')
        elif job['kind']=='feature':emit(fit(im,(1024,500)),f'Store/feature-graphic.png','R-02')
        elif job['kind']=='launch':emit(fit(im,(1920,1080)),f'Store/launch-n29.png','R-04')
def existing_exports():
    # Brand packaging uses the existing official identity, not a regenerated logo.
    art=PROJECT/'Art/UI'
    bg=Image.open(art/'HD_Icon_Background.png').convert('RGBA').resize((512,512),Image.Resampling.LANCZOS)
    fg=Image.open(art/'HD_Icon_Foreground.png').convert('RGBA').resize((512,512),Image.Resampling.LANCZOS)
    bg.alpha_composite(fg);emit(bg,'Store/google-play-icon.png','R-01','brand-composition')
    dest=OUT/'Store/heroes-dawn.ico';bg.save(dest,format='ICO',sizes=[(16,16),(32,32),(48,48),(256,256)])
    records.append(dict(id='R-04',file='Store/heroes-dawn.ico',width=256,height=256,method='brand-export',sizes=[16,32,48,256]))
    brie=Image.open(art/'AubeTactiqueExtension/Agents/brie_rayner.png').convert('RGBA')
    brie=fit(brie.crop((140,50,820,730)),(256,256));mask=Image.new('L',(256,256),0);ImageDraw.Draw(mask).ellipse((5,5,251,251),fill=255);brie.putalpha(mask);ImageDraw.Draw(brie).ellipse((5,5,251,251),outline='#D6AF5B',width=4);emit(brie,'Tutorial/brie-rayner-round.png','U-02','existing-portrait-export')
def main():
    if len(sys.argv)>1:
        generated_jobs(sys.argv[1]);changed={r['file'] for r in records}
        base=[r for r in json.loads((ROOT/'manifest.json').read_text(encoding='utf-8')) if r['file'] not in changed]
    else:
        enemies();mapping();generated_jobs();existing_exports()
        base=json.loads((ROOT/'vector-manifest.json').read_text(encoding='utf-8'))
    merged=sorted(base+records,key=lambda r:(r['id'],r['file']))
    vector_settings={r['file']:r for r in json.loads((ROOT/'vector-manifest.json').read_text(encoding='utf-8'))}
    for rec in merged:
        if rec['file'] in vector_settings and 'border' in vector_settings[rec['file']]:rec['border']=vector_settings[rec['file']]['border']
    for rec in merged:
        f=OUT/rec['file'];im=Image.open(f);im.verify();im=Image.open(f)
        assert im.size==(rec['width'],rec['height']),rec
        rec['bytes']=f.stat().st_size;rec['sha256']=hashlib.sha256(f.read_bytes()).hexdigest()
        rec['has_alpha']=im.mode in ['RGBA','LA'];rec['alpha_min']=im.getchannel('A').getextrema()[0] if im.mode=='RGBA' else None
    assert (OUT/'Store/google-play-icon.png').stat().st_size<=1048576
    (ROOT/'manifest.json').write_text(json.dumps(merged,ensure_ascii=False,indent=2),encoding='utf-8')
    with (ROOT/'manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:
        w=csv.DictWriter(f,fieldnames=['id','file','width','height','method','border','bytes','sha256'],extrasaction='ignore');w.writeheader();w.writerows(merged)
    print(f'{len(merged)} exports verified, {len(list((SRC/"Generated").glob("*.png")))} generated masters saved')
if __name__=='__main__':main()
