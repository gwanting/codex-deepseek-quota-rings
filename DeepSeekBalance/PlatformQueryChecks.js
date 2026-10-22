const fs = require('fs');
const vm = require('vm');
const source = fs.readFileSync(require('path').join(__dirname,'PlatformUsageQuery.js'),'utf8').replaceAll('__START__','1792598400').replaceAll('__END__','1792684800');
async function run(origin, stored, reply) {
  const calls=[]; let result;
  await vm.runInNewContext(source, {
    location:{origin}, localStorage:{getItem:()=>stored}, AbortSignal,
    window:{chrome:{webview:{postMessage:message=>{result=JSON.parse(message);}}}},
    fetch:async (url,options)=>{calls.push({url,options});return reply;}
  });
  return {calls,result};
}
function check(name,value){if(!value)throw new Error(name);process.stdout.write('PASS: '+name+'\n');}
(async()=>{
  const stored=JSON.stringify({value:'synthetic-session-only'});
  const success={status:200,ok:true,json:async()=>({code:0,data:{biz_code:0,biz_data:{}}})};
  const valid=await run('https://platform.deepseek.com',stored,success);
  check('Both daily endpoints use Beijing range without key filter',valid.calls.length===2&&valid.calls.every(x=>/^\/api\/v0\/usage\/by_api_key\/(amount|cost)\?start=1792598400&end=1792684800&tz=28800$/.test(x.url)));
  check('Only same-origin GET requests without redirect',valid.calls.every(x=>x.options.method==='GET'&&x.options.credentials==='same-origin'&&x.options.redirect==='error'));
  check('Session value does not appear in returned message',valid.result.ok&&!JSON.stringify(valid.result).includes('synthetic-session-only'));
  const absent=await run('https://platform.deepseek.com',null,success);check('Missing login makes no request and stays unknown',absent.calls.length===0&&absent.result.login);
  const other=await run('https://example.com',stored,success);check('Other origin cannot use session',other.calls.length===0&&!other.result.ok);
  const invalid=await run('https://platform.deepseek.com',stored,{status:401,ok:false});check('Authentication failure requests login instead of zero',invalid.result.login&&!invalid.result.ok);
})().catch(error=>{process.stderr.write(error.message+'\n');process.exitCode=1;});
