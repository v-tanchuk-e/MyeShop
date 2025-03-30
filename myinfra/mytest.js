import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  vus: 100,
  iterations:100000000,
  duration: '3600s'
 };

export default function() {
  let res = http.get('https://api-vivethere-yg7dalmmz52tg.azurewebsites.net/api/catalog-items/1');
  check(res, { "status is 200": (res) => res.status === 200 });
  sleep(1);
}
