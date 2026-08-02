/**
 * Test API connectivity
 */
import https from 'https';

const AGENT = new https.Agent({ rejectUnauthorized: false });

function httpsGet(url) {
  return new Promise((resolve, reject) => {
    https.get(url, { agent: AGENT }, res => {
      let body = '';
      res.on('data', c => body += c);
      res.on('end', () => resolve({ status: res.statusCode, body }));
    }).on('error', reject);
  });
}

try {
  const { status, body } = await httpsGet('https://localhost:5001/api/profile-diagrams?pageSize=2');
  console.log('Status:', status, '— body length:', body.length);
  if (body.startsWith('{')) {
    const data = JSON.parse(body);
    console.log('Success:', data.success, 'TotalCount:', data.data?.totalCount);
    if (data.data?.items?.length) console.log('Sample:', JSON.stringify(data.data.items[0]));
  } else {
    console.log('Raw:', body.slice(0, 300));
  }
} catch (e) {
  console.error('Error:', e.message);
}
