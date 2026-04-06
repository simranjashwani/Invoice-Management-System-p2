import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";

export function runPaymentFormSourceTests() {
  const filePath = path.resolve("src/components/payments/PaymentForm.jsx");
  const source = fs.readFileSync(filePath, "utf8");

  assert.match(source, /<form onSubmit=\{onSubmit\}/, "PaymentForm should render a form");
  assert.match(source, /name="paymentAmount"/, "PaymentForm should include payment amount");
  assert.match(source, /name="paymentMethod"/, "PaymentForm should include payment method select");
  assert.match(source, /Submit Payment/, "PaymentForm should include submit button text");
}
