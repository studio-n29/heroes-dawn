from pathlib import Path
import zipfile,json
R=Path(__file__).resolve().parent
archive=R.parent/'HeroesDawn_Assets_2D_2026-11-16.zip'
if archive.exists():
    version=2
    while (R.parent/f'HeroesDawn_Assets_2D_2026-11-16-v{version}.zip').exists():version+=1
    archive=R.parent/f'HeroesDawn_Assets_2D_2026-11-16-v{version}.zip'
report=json.loads((R/'verification.json').read_text(encoding='utf-8'))
report['visual_review']='Reviewed all asset contact sheets and all PDF page layouts; latest affected pages re-rendered and inspected after corrections.'
(R/'verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
folders=['Exports','Catalogue','Sources']
names=['Guide_Integration_Assets_2D.pdf','Guide_Integration_Assets_2D.html','Integration_Nouveaux.csv','Integration_Existants_Liste_B.csv','manifest.csv','manifest.json','README.md','verification.json','generated-jobs.json','generated-fx-jobs.json','generated-corrections.json','core-generation-prompts.json','map-polygons.json']
files=[f for group in folders for f in (R/group).rglob('*') if f.is_file()]+[R/name for name in names if (R/name).exists()]
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=4) as z:
    for f in files:z.write(f,f.relative_to(R).as_posix())
with zipfile.ZipFile(archive) as z:
    assert z.testzip() is None
    exports=[n for n in z.namelist() if n.startswith('Exports/')]
    assert len(exports)==203
print(json.dumps({'archive':str(archive),'bytes':archive.stat().st_size,'entries':len(files),'exports':len(exports)}))
