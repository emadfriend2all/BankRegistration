const fs = require('fs');
const path = require('path');

// Read the client.ts file
const clientPath = path.join(__dirname, '../src/app/api/client.ts');
let clientContent = fs.readFileSync(clientPath, 'utf8');

console.log('Fixing Observable return type issues in client.ts...');

// Fix the specific problematic lines by ensuring they match the method signatures
const lineFixes = [
  {
    // Line 83: processAccountMastAll should return AccountMast[]
    search: 'return _observableOf<AccountMast[]>(null as any);',
    replace: 'return _observableOf<AccountMast[]>(null as any);'
  },
  {
    // Line 398: processCustomer should return AccountMast[]
    search: 'return _observableOf<AccountMast[]>(null as any);',
    replace: 'return _observableOf<AccountMast[]>(null as any);'
  },
  {
    // Line 459: processBranch should return AccountMast[]
    search: 'return _observableOf<AccountMast[]>(null as any);',
    replace: 'return _observableOf<AccountMast[]>(null as any);'
  },
  {
    // Line 517: processCustMastAll should return CustMast[]
    search: 'return _observableOf<CustMast[]>(null as any);',
    replace: 'return _observableOf<CustMast[]>(null as any);'
  },
  {
    // Line 811: processBranch2 should return CustMast[]
    search: 'return _observableOf<CustMast[]>(null as any);',
    replace: 'return _observableOf<CustMast[]>(null as any);'
  },
  {
    // Line 875: processAccounts should return AccountMast[]
    search: 'return _observableOf<AccountMast[]>(null as any);',
    replace: 'return _observableOf<AccountMast[]>(null as any);'
  },
  {
    // Line 933: processGetWeatherForecast should return WeatherForecast[]
    search: 'return _observableOf<WeatherForecast[]>(null as any);',
    replace: 'return _observableOf<WeatherForecast[]>(null as any);'
  }
];

// The issue is that the method signatures expect arrays but the return statements are correct
// The real problem is that the method signatures themselves are wrong
// Let's fix the method signatures instead

// Fix method signatures that should return single objects but are declared to return arrays
const methodSignatureFixes = [
  {
    // These methods should return single objects, not arrays
    search: 'protected processAccountMastPOST(response: HttpResponseBase): Observable<AccountMast[]>',
    replace: 'protected processAccountMastPOST(response: HttpResponseBase): Observable<AccountMast>'
  },
  {
    search: 'protected processAccountMastGET(response: HttpResponseBase): Observable<AccountMast[]>',
    replace: 'protected processAccountMastGET(response: HttpResponseBase): Observable<AccountMast>'
  },
  {
    search: 'protected processAccountMastPUT(response: HttpResponseBase): Observable<AccountMast[]>',
    replace: 'protected processAccountMastPUT(response: HttpResponseBase): Observable<AccountMast>'
  },
  {
    search: 'protected processCustMastPOST(response: HttpResponseBase): Observable<CustMast[]>',
    replace: 'protected processCustMastPOST(response: HttpResponseBase): Observable<CustMast>'
  },
  {
    search: 'protected processCustMastGET(response: HttpResponseBase): Observable<CustMast[]>',
    replace: 'protected processCustMastGET(response: HttpResponseBase): Observable<CustMast>'
  },
  {
    search: 'protected processCustMastPUT(response: HttpResponseBase): Observable<CustMast[]>',
    replace: 'protected processCustMastPUT(response: HttpResponseBase): Observable<CustMast>'
  }
];

// Apply method signature fixes
methodSignatureFixes.forEach(fix => {
  if (clientContent.includes(fix.search)) {
    clientContent = clientContent.replace(fix.search, fix.replace);
    console.log(`Fixed: ${fix.search}`);
  }
});

// Write the fixed content back to the file
fs.writeFileSync(clientPath, clientContent);

console.log('Fixed method signature issues in client.ts');
