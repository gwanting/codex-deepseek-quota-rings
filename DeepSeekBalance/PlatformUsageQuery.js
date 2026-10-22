(async () => {
  if (location.origin !== 'https://platform.deepseek.com') return JSON.stringify({ok:false});
  let token;
  try { const stored = JSON.parse(localStorage.getItem('userToken') || 'null'); token = typeof stored === 'string' ? stored : stored && stored.value; } catch {}
  if (!token) return JSON.stringify({ok:false,login:true});
  const query = '?start=__START__&end=__END__&tz=28800';
  async function read(kind) {
    const response = await fetch('/api/v0/usage/by_api_key/' + kind + query, {
      method:'GET', credentials:'same-origin', redirect:'error',
      headers:{Authorization:'Bearer ' + token, Accept:'application/json','x-client-platform':'web'},
      signal:AbortSignal.timeout(15000)
    });
    if (response.status === 401 || response.status === 403) throw new Error('login');
    if (!response.ok) throw new Error('http');
    const data = await response.json();
    if (data.code !== 0 || !data.data || data.data.biz_code !== 0) throw new Error('login');
    return JSON.stringify(data);
  }
  try { const [amount,cost] = await Promise.all([read('amount'),read('cost')]); return JSON.stringify({ok:true,amount,cost}); }
  catch(error) { return JSON.stringify({ok:false,login:error.message === 'login'}); }
})().then(result => window.chrome.webview.postMessage(result));
