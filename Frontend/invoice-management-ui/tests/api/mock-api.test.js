import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";

export function runMockApiTests() {
  const authApiSource = fs.readFileSync(path.resolve("src/api/authApi.js"), "utf8");
  const invoiceApiSource = fs.readFileSync(path.resolve("src/api/invoiceApi.js"), "utf8");

  assert.match(authApiSource, /token:\s*"fake-jwt-token"/, "authApi should provide a mock JWT token");
  assert.match(authApiSource, /role:\s*"Admin"/, "authApi should provide a mock role");

  assert.match(invoiceApiSource, /invoiceNumber:\s*"INV-001"/, "invoiceApi should mock invoice list data");
  assert.match(invoiceApiSource, /invoiceId:\s*3/, "invoiceApi should mock create invoice response");
  assert.match(invoiceApiSource, /Mock create invoice payload/, "invoiceApi should log mock create payload");
}
