import fs from "fs";
let t = fs.readFileSync("tsconfig.json", "utf8");
const paths = `    "@hotel/shared/models": ["projects/shared-models/src/public-api.ts"],
    "@hotel/shared/core": ["projects/shared-core/src/public-api.ts"],
    "@hotel/shared/data-access": ["projects/shared-data-access/src/public-api.ts"],
    "@hotel/shared/auth": ["projects/shared-auth/src/public-api.ts"],
    "@hotel/shared/i18n": ["projects/shared-i18n/src/public-api.ts"],
    "@hotel/shared/monitoring": ["projects/shared-monitoring/src/public-api.ts"],
    "@hotel/shared/ui": ["projects/shared-ui/src/public-api.ts"]`;
t = t.replace(/"paths": \{[\s\S]*?\},/m, `"paths": {\n${paths}\n    },`);
fs.writeFileSync("tsconfig.json", t);
console.log("tsconfig patched");
