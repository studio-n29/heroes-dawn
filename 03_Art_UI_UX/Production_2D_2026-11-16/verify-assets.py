from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json,math,collections
R=Path(__file__).resolve().parent;O=R/'Exports';Q=R/'QA';Q.mkdir(exist_ok=True)
m=json.loads((R/'manifest.json').read_text(encoding='utf-8'))
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',12)
for label,items in [('vectors',[r for r in m if r['method'] in ['vector-kit-extension','preview-export']]),('illustrations',[r for r in m if r['method'] not in ['vector-kit-extension','preview-export','code-exact-mask','code-exact-overlay']]),('map',[r for r in m if r['id']=='M-06'])]:
    for start in range(0,len(items),25):
        chunk=items[start:start+25];sheet=Image.new('RGB',(1250,250*math.ceil(len(chunk)/5)),'#101C29');d=ImageDraw.Draw(sheet)
        for j,r in enumerate(chunk):
            im=Image.open(O/r['file']).convert('RGBA');im.thumbnail((230,206),Image.Resampling.LANCZOS)
            x=(j%5)*250;y=(j//5)*250
            tile=Image.new('RGBA',(240,210),'#233749');tile.alpha_composite(im,((240-im.width)//2,(210-im.height)//2));sheet.paste(tile.convert('RGB'),(x+5,y+4));d.text((x+5,y+216),r['id']+' '+Path(r['file']).name[:31],font=font,fill='#ECE7DC')
        sheet.save(Q/f'{label}-{start//25+1}.jpg',quality=92)
expected={'M-01':3,'M-02':3,'M-03':4,'M-04':9,'M-05':2,'M-06':13,'S-01':12,'S-02':4,'N-01':8,'G-01':14,'G-02':5,'G-03':5,'G-04':22,'U-01':4,'U-02':6,'U-03':6,'U-04':7,'U-05':4,'C-01':14,'C-02':6,'C-03':21,'C-04':4,'C-05':23,'R-01':1,'R-02':1,'R-03':0,'R-04':2}
counts=collections.Counter(r['id'] for r in m)
assert all(counts[k]==v for k,v in expected.items()),dict(counts)
alpha_ids={'M-01','M-02','M-03','M-04','M-05','S-02','G-02','G-04','U-01','U-02','U-03','U-04','U-05','C-01','C-03','C-04','C-05'}
for r in m:
    im=Image.open(O/r['file']);assert im.size==(r['width'],r['height'])
    if r['id'] in alpha_ids:assert im.mode=='RGBA' and im.getchannel('A').getextrema()[0]==0,r['file']
ico=Image.open(O/'Store/heroes-dawn.ico');assert ico.ico.sizes()=={(16,16),(32,32),(48,48),(256,256)}
assert (O/'Store/google-play-icon.png').stat().st_size<=1048576
report={'exports':len(m),'by_source_id':dict(sorted(counts.items())),'alpha':'verified','ico_sizes':'16,32,48,256','store_screenshots':'R-03 intentionally pending real final-game captures','game_integration':'not performed; user integrates using the guide','visual_review':'QA sheets prepared for review'}
(R/'verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
