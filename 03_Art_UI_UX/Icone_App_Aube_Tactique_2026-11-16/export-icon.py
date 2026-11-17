from pathlib import Path
from PIL import Image
import shutil,json
root=Path(__file__).resolve().parent
source=Path('C:/Users/ledim/.codex/generated_images/01a1e43a-2332-7991-9195-cb53bdf30dae/exec-c794ccfe-8d75-4d94-8407-d5c710f1d550.png')
shutil.copyfile(source,root/'heroes-dawn-icon-master.png')
im=Image.open(source).convert('RGBA')
for size in [1024,512,256,128,64,48,32,16]:
    export=im.resize((size,size),Image.Resampling.LANCZOS)
    export.save(root/f'heroes-dawn-icon-{size}.png',optimize=True)
icon=im.resize((256,256),Image.Resampling.LANCZOS)
icon.save(root/'heroes-dawn-icon.ico',format='ICO',sizes=[(16,16),(32,32),(48,48),(256,256)])
assert (root/'heroes-dawn-icon-512.png').stat().st_size<1048576
for f in root.glob('*.png'):
    with Image.open(f) as check:check.verify()
assert Image.open(root/'heroes-dawn-icon.ico').ico.sizes()=={(16,16),(32,32),(48,48),(256,256)}
print(json.dumps({'source_dimensions':im.size,'exports':[1024,512,256,128,64,48,32,16],'ico_sizes':[16,32,48,256],'png_512_bytes':(root/'heroes-dawn-icon-512.png').stat().st_size}))
