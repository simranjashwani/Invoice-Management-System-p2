import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";

export function runInvoiceFormSourceTests() {
  const filePath = path.resolve("src/pages/CreateInvoicePage.jsx");
  const source = fs.readFileSync(filePath, "utf8");

  assert.match(source, /<form onSubmit=\{handleSubmit\}>/, "CreateInvoicePage should render an invoice form");
  assert.match(source, /placeholder="Customer Id"/, "CreateInvoicePage should include customer input");
  assert.match(source, /name="invoiceDate"/, "CreateInvoicePage should include invoice date input");
  assert.match(source, /name="dueDate"/, "CreateInvoicePage should include due date input");
}
