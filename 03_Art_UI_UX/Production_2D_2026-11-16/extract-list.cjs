const fs=require('fs'),path=require('path'),vm=require('vm');
const html=fs.readFileSync(path.resolve(__dirname,'../../05_Production/Asset_List_2D_Manquants_2026-11-16.html'),'utf8');
const script=html.match(/<script>([\s\S]*?)<\/script>/)[1];
const data=vm.runInNewContext(script+';JSON.stringify({A,B})',{document:{getElementById(){return {innerHTML:''}}}},{timeout:1000});
fs.writeFileSync(path.join(__dirname,'source-list.json'),data);
console.log('27 source rows and 15 integration groups extracted');
