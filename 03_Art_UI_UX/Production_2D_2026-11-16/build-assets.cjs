// Deterministic extensions of the existing Aube tactique vector kit. Run with bundled Node.
const fs=require('fs'), path=require('path');
const sharp=require('C:/Program Files/WindowsApps/OpenAI.CodexPrimaryRuntime.v26-1112-737-0_26.1112.737.0_x64__3k8sg7r9htsxt/dependencies/node/node_modules/sharp');
const root=__dirname, out=path.join(root,'Exports'), src=path.join(root,'Sources');
const project=path.resolve(root,'../../Code/Heroes Dawn Unity/Assets/_Project');
const ink='#101C29', ivory='#ECE7DC', gold='#D6AF5B', cyan='#65DDE8', green='#75D5A7', red='#EF6873';
const records=[];
function xml(s){return String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;')}
function poly(points,fill,stroke='none',sw=1){return `<polygon points="${points}" fill="${fill}" stroke="${stroke}" stroke-width="${sw}" stroke-linejoin="round"/>`}
function line(points,c=ivory,w=2.3,extra=''){return `<polyline points="${points}" fill="none" stroke="${c}" stroke-width="${w}" stroke-linecap="round" stroke-linejoin="round" ${extra}/>`}
function circ(x,y,r,fill='none',stroke=ivory,w=2.3){return `<circle cx="${x}" cy="${y}" r="${r}" fill="${fill}" stroke="${stroke}" stroke-width="${w}"/>`}
function rect(x,y,w,h,fill,stroke='none',sw=1){return `<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${fill}" stroke="${stroke}" stroke-width="${sw}"/>`}
function text(s,x,y,size=18,c=ivory){return `<text x="${x}" y="${y}" fill="${c}" font-size="${size}" font-weight="600" font-family="Segoe UI,Arial,sans-serif" text-anchor="middle">${xml(s)}</text>`}
function star(cx,cy,r,c=gold){return poly(`${cx},${cy-r} ${cx+r*.2},${cy-r*.2} ${cx+r},${cy} ${cx+r*.2},${cy+r*.2} ${cx},${cy+r} ${cx-r*.2},${cy+r*.2} ${cx-r},${cy} ${cx-r*.2},${cy-r*.2}`,c)}
function panel(w,h,fill=ink,stroke=gold,sw=2){return poly(`2,14 14,2 ${w-2},2 ${w-2},${h-14} ${w-14},${h-2} 2,${h-2}`,fill,stroke,sw)}
function svg(w,h,b){return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${b}</svg>`}
async function save(id,group,name,w,h,body,border=[0,0,0,0]){let rel=group+'/'+name;fs.mkdirSync(path.join(src,group),{recursive:true});fs.mkdirSync(path.join(out,group),{recursive:true});const source=svg(w,h,body);fs.writeFileSync(path.join(src,rel+'.svg'),source);await sharp(Buffer.from(source)).png().toFile(path.join(out,rel+'.png'));records.push({id,file:rel+'.png',width:w,height:h,method:'vector-kit-extension',border});}
const shapes={
 burn:'<path d="M48 12 C56 31 77 36 72 59 C70 78 28 84 24 59 C20 44 35 37 35 25 L44 43 C52 35 52 24 48 12Z"/>',
 freeze:line('48,16 48,80')+line('20,32 76,64')+line('20,64 76,32')+line('39,22 48,31 57,22')+line('39,74 48,65 57,74')+line('24,41 34,41 34,31')+line('62,65 62,55 72,55'),
 shock:poly('52,12 27,51 46,51 40,84 72,40 53,40','none',ivory,3),
 corrosion:poly('48,14 27,48 26,59 34,69 48,73 62,69 70,59 69,48','none',ivory,3)+line('19,81 77,81')+line('35,76 31,84')+line('59,76 63,84'),
 blind:'<path d="M14 48Q48 12 82 48Q48 84 14 48Z" fill="none"/>'+circ(48,48,13)+line('17,17 79,79',ivory,5),
 vulnerable:poly('48,14 75,26 72,56 48,81 24,56 21,26','none',ivory,3)+line('53,25 42,45 55,49 43,71'),
 snare:line('21,20 75,76')+line('21,76 75,20')+circ(48,48,23)+line('17,48 79,48'),
 mark:circ(48,48,25)+circ(48,48,10)+line('48,10 48,29')+line('48,67 48,86')+line('10,48 29,48')+line('67,48 86,48'),
 taunt:poly('16,22 80,22 80,61 50,61 30,79 30,61 16,61','none',ivory,3)+line('48,32 48,46',ivory,4)+circ(48,53,2,ivory,'none'),
 shield:poly('48,14 76,25 72,58 48,82 24,58 20,25','none',ivory,3)+line('48,23 48,71'),
 regeneration:line('48,24 48,72',ivory,8)+line('24,48 72,48',ivory,8)+'<path d="M18 37A32 32 0 0 1 73 24M78 59A32 32 0 0 1 23 72" fill="none"/>'+poly('70,15 77,29 62,28',ivory)+poly('26,81 19,67 34,68',ivory),
 haste:line('17,25 43,48 17,71',ivory,5)+line('49,25 75,48 49,71',ivory,5),
 dodge:'<path d="M17 70Q20 27 67 27" fill="none"/>'+poly('62,15 80,27 62,39',ivory)+line('24,79 76,79')+line('44,52 64,66'),
 anchor:circ(48,23,9)+line('48,32 48,79')+line('29,44 67,44')+'<path d="M18 57Q18 80 48 80Q78 80 78 57" fill="none"/>'+poly('12,62 18,50 25,62',ivory)+poly('71,62 78,50 84,62',ivory),
 frontline:line('18,24 78,24',ivory,5)+poly('48,38 71,47 68,65 48,80 28,65 25,47','none',ivory,3),
 attacker:poly('21,73 63,18 78,16 78,31 35,80','none',ivory,3)+line('18,54 46,78')+line('15,81 24,69'),
 control:circ(48,48,12)+line('48,15 48,36')+line('48,60 48,81')+line('15,48 36,48')+line('60,48 81,48')+circ(48,15,5)+circ(48,81,5)+circ(15,48,5)+circ(81,48,5),
 support:poly('48,79 17,49 17,31 29,20 41,20 48,29 55,20 67,20 79,31 79,49','none',ivory,3)+line('48,37 48,62',ivory,5)+line('35,49 61,49',ivory,5),
 ranged:circ(48,48,24)+line('48,9 48,34')+line('48,62 48,87')+line('9,48 34,48')+line('62,48 87,48')+circ(48,48,3,ivory,'none')
};
async function main(){
 await save('M-03','Frames','enemy-elite',512,512,poly('74,10 502,10 438,502 10,502','none','#EC76B0',8)+poly('84,22 488,22 428,490 24,490','none',gold,2));
 for(const [i,name] of ['burn','freeze','shock','corrosion','blind','vulnerable','snare','mark','taunt','shield','regeneration','haste','dodge','anchor'].entries()){
 const c=i<9?red:(i===9?cyan:green);await save('G-01','Status',name,128,128,`<g transform="translate(4 4) scale(1.25)">${circ(48,48,45,ink,c,2.5)}<g fill="none" stroke="${ivory}" stroke-width="3" stroke-linejoin="round" stroke-linecap="round">${shapes[name]}</g></g>`);}
 for(const name of ['frontline','attacker','control','support','ranged'])await save('G-02','Positions',name,128,128,`<g transform="scale(1.333333)" fill="none" stroke="white" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round">${shapes[name]}</g>`);
 for(let n=1;n<=5;n++){let c=['#A97C55','#C1CED4','#D6AF5B','#E0C881','#B3F3FF'][n-1],b=panel(128,128,ink,c,3);for(let j=0;j<n;j++)b+=line(`30,${94-j*12} 64,${76-j*12} 98,${94-j*12}`,c,4);b+=star(64,25,12,c);await save('G-03','Ranks','amif-'+n,128,128,b);}
 for(const [state,bg,edge] of [['normal',ink,'#78858B'],['hover','#243848',gold],['pressed','#08101A',gold],['focus',ink,cyan]])await save('U-01','Components','close-'+state,96,96,panel(96,96,bg,edge,state==='focus'?4:2)+line('34,34 62,62',ivory,3)+line('62,34 34,62',ivory,3));
 await save('U-02','Tutorial','bubble',512,256,panel(512,256,ivory,gold,3),[20,20,20,20]);
 for(const [name,pts] of [['up','2,30 24,2 46,30'],['down','2,2 24,30 46,2'],['left','46,2 2,24 46,46'],['right','2,2 46,24 2,46']])await save('U-02','Tutorial','pointer-'+name,48,48,poly(pts,ivory,gold,2));
 await save('U-03','Affinity','node',256,320,panel(256,320,ink,gold,2)+rect(14,14,228,228,'none','#78858B',1)+line('16,258 240,258',gold,1)+star(25,285,9),[16,68,16,16]);
 for(const [i,n]of [3,8,15,25].entries())await save('U-03','Affinity','link-'+n,256,128,line('8,114 128,114 128,14 248,14',gold,i+1,i===0?'stroke-dasharray="6 7"':'')+(i===3?line('8,114 128,114 128,14 248,14','#FFF0BB',1):''));
 await save('U-03','Affinity','combo-bonus',192,64,panel(192,64,ink,gold,2)+text('+15 %',96,42,30,gold),[16,16,16,16]);
 for(const [state,c]of [['locked','#78858B'],['claimable',gold],['claimed',green]])await save('U-04','Pass','reward-'+state,192,192,panel(192,192,ink,c,state==='claimable'?4:2)+(state==='claimable'?'<defs><filter id="halo"><feGaussianBlur stdDeviation="4"/></filter></defs><g opacity=".65" filter="url(#halo)">'+poly('10,19 19,10 182,10 182,173 173,182 10,182','none',gold,8)+'</g>':'')+(state==='claimed'?line('130,150 146,167 174,130',green,6):state==='locked'?rect(142,143,25,23,'none','#78858B',3)+'<path d="M147 143v-9a8 8 0 0 1 16 0v9" fill="none" stroke="#78858B" stroke-width="3"/>':''),[16,16,16,16]);
 await save('U-04','Pass','ribbon-free',256,64,panel(256,64,ivory,gold)+text('GRATUIT',128,42,25,ink),[16,16,16,16]);
 await save('U-04','Pass','ribbon-premium',256,64,panel(256,64,ink,gold)+text('PREMIUM',140,42,25,gold)+rect(20,29,20,17,gold)+'<path d="M23 29v-8a7 7 0 0 1 14 0v8" fill="none" stroke="#D6AF5B" stroke-width="3"/>',[16,16,16,16]);
 await save('U-04','Pass','active-tier',96,96,poly('48,4 92,48 48,92 4,48',gold,ivory,2)+star(48,48,24,ink));
 await save('U-04','Pass','claim-label',256,64,panel(256,64,ivory,gold)+text('RÉCLAMER',128,42,25,ink),[16,16,16,16]);
 const fam=[['matter','#F4B748','family-matter'],['elemental',cyan,'family-elemental'],['bio','#EC76B0','family-bio'],['perception','#B88BE7','family-perception']];
 for(const [name,c,icon]of fam){let iconFile=path.join(project,'Art/UI/AubeTactiqueKit/Sources/Icons',icon+'.svg');let inner=fs.readFileSync(iconFile,'utf8').replace(/<svg[^>]*>/,'').replace('</svg>','').replaceAll('#FFFFFF',c);await save('S-01','Avatars','family-'+name,256,256,rect(0,0,256,256,ink)+circ(128,128,108,'none',c,3)+`<g transform="translate(32 32) scale(2)">${inner}</g>`);
 await save('S-02','Silhouettes','unrecruited-'+name,256,256,'<path d="M55 242L59 198L80 177L103 168L103 147L87 126L84 91L96 64L126 50L156 61L172 89L168 124L151 148L151 169L177 179L198 202L203 242Z" fill="#0B1623" stroke="'+c+'" stroke-width="2"/>');}
 const positions={1:[[128,128]],2:[[78,78],[178,178]],3:[[78,78],[128,128],[178,178]],4:[[78,78],[178,78],[78,178],[178,178]],5:[[78,78],[178,78],[128,128],[78,178],[178,178]],6:[[78,70],[178,70],[78,128],[178,128],[78,186],[178,186]]};
 for(const [skin,c,bg] of [['chrome','#E5F2F5','#52616C'],['neon',cyan,'#10283C'],['pass_amber','#FFD17A','#754A20']]){for(let n=1;n<=6;n++){const defs=`<defs><linearGradient id="metal" x2=".7" y2="1"><stop stop-color="${c}"/><stop offset=".45" stop-color="${bg}"/><stop offset=".52" stop-color="${c}"/><stop offset="1" stop-color="${bg}"/></linearGradient></defs>`;let b=defs+poly('25,12 231,12 244,25 244,231 231,244 25,244 12,231 12,25','url(#metal)',c,3)+poly('31,25 225,25 231,31 231,225 225,231 31,231 25,225 25,31',skin==='neon'?ink:'none',c,2);for(const [x,y]of positions[n])b+=circ(x,y,13,skin==='chrome'?ink:c,'none');await save('C-03','Dice',skin+'-'+n,256,256,b);}await fs.promises.copyFile(path.join(out,'Dice',skin+'-6.png'),path.join(out,'Dice',skin+'-preview.png'));records.push({id:'C-03',file:'Dice/'+skin+'-preview.png',width:256,height:256,method:'preview-export'});}
 for(const [skin,c]of [['brass',gold],['pass_scout',cyan]])for(const state of ['idle','active']){let b=circ(128,128,state==='active'?120:105,'none',c,state==='active'?4:2)+circ(128,128,97,ink,c,4)+circ(128,128,86,'none',c,1)+star(128,128,62,c);if(state==='active')b+=star(128,15,10,ivory)+star(241,128,10,ivory)+star(128,241,10,ivory)+star(15,128,10,ivory);if(skin==='pass_scout')b+=line('96,191 128,170 160,191',c,5);await save('C-04','Pawns',skin+'-'+state,256,256,b);}
 const seasons=[['new_year','NOUVEL AN','#F5D47A','star'],['valentine','SAINT-VALENTIN','#F098AF','heart'],['spring','PRINTEMPS','#9DD9AF','leaf'],['summer','ÉTÉ','#F4CA70','sun'],['holidays','VACANCES','#63D5DF','wave'],['back_to_school','RENTRÉE','#A5B6E6','book'],['halloween','HALLOWEEN','#F0A05F','bat'],['christmas','NOËL','#89CAAD','tree']];
 function motif(kind,c){let s=kind==='star'?star(48,48,28,c):kind==='heart'?'<path d="M48 76L19 47C5 22 34 10 48 32C62 10 91 22 77 47Z" fill="'+c+'"/>':kind==='leaf'?'<path d="M20 76Q5 18 77 17Q82 74 20 76Z" fill="none" stroke="'+c+'" stroke-width="4"/>'+line('20,76 66,30',c,3):kind==='sun'?circ(48,48,19,c,'none')+Array.from({length:8},(_,i)=>`<g transform="rotate(${i*45} 48 48)">${line('48,13 48,23',c,4)}</g>`).join(''):kind==='wave'?'<path d="M10 55Q25 32 40 55T70 55T90 55M10 72Q25 49 40 72T70 72T90 72" fill="none" stroke="'+c+'" stroke-width="4"/>':kind==='book'?poly('15,21 47,29 81,21 81,71 48,80 15,71','none',c,3)+line('48,29 48,80',c,3):kind==='bat'?poly('8,31 31,42 40,28 48,36 56,28 65,42 88,31 76,66 57,58 48,78 39,58 20,66',c):poly('48,10 70,38 61,38 80,64 56,64 56,82 40,82 40,64 16,64 35,38 26,38',c);return s;}
 for(const [id,label,c,kind]of seasons){await save('C-05','Tournament',id+'-frame',256,256,circ(128,128,117,'none',c,5)+circ(128,128,105,'none',gold,1)+`<g transform="translate(104 205) scale(.5)">${motif(kind,c)}</g>`+star(128,12,10,c));await save('C-05','Tournament',id+'-title',512,128,panel(512,128,ink,c,3)+`<g transform="translate(10 16)">${motif(kind,c)}</g>`+text(label,300,76,26,c),[24,24,24,24]);}
 for(const [i,c]of ['#AD7954','#C8D4DA',gold,'#DCEAF1','#A4F1F7'].entries())await save('C-05','Tournament','medal-'+(i+1),128,128,poly('36,6 61,6 72,48 45,56',ink,c,2)+poly('67,6 92,6 83,56 56,48',ink,c,2)+circ(64,77,41,ink,c,4)+circ(64,77,33,'none',c,1)+star(64,77,22,c));
 for(const n of [10,3])await save('C-05','Tournament','top-'+n,128,128,panel(128,128,ink,gold,2)+text('TOP',64,43,22,gold)+text(n,64,94,46,gold));
 fs.writeFileSync(path.join(root,'vector-manifest.json'),JSON.stringify(records,null,2));console.log(`${records.length} vector exports written`);
}
main().catch(e=>{console.error(e);process.exit(1)});
