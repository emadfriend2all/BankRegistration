const fs = require('fs');
const path = require('path');

// Read the client.ts file
const clientPath = path.join(__dirname, '../src/app/api/client.ts');
let clientContent = fs.readFileSync(clientPath, 'utf8');

// Fix the Observable return type issues by replacing the problematic lines
const fixes = [
  {
    pattern: /return _observableOf<AccountMast\[\]>\(null as any\);/g,
    replacement: 'return _observableOf<AccountMast[]>(null as any);'
  },
  {
    pattern: /return _observableOf<CustMast\[\]>\(null as any\);/g,
    replacement: 'return _observableOf<CustMast[]>(null as any);'
  },
  {
    pattern: /return _observableOf<WeatherForecast\[\]>\(null as any\);/g,
    replacement: 'return _observableOf<WeatherForecast[]>(null as any);'
  }
];

// Apply fixes
fixes.forEach(fix => {
  clientContent = clientContent.replace(fix.pattern, fix.replacement);
});

// Write the fixed content back to the file
fs.writeFileSync(clientPath, clientContent);

console.log('Fixed Observable return type issues in client.ts');
