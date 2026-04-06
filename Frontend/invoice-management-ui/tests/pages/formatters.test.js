import assert from "node:assert/strict";

import { formatCurrency } from "../../src/utils/formatters.js";

export function runFormatterTests() {
  assert.equal(
    formatCurrency(2899.88),
    "₹2,899.88",
    "formatCurrency should format INR values"
  );

  assert.equal(
    formatCurrency(""),
    "₹0.00",
    "formatCurrency should treat empty values as zero"
  );

  assert.equal(
    formatCurrency("249.99"),
    "₹249.99",
    "formatCurrency should accept numeric strings"
  );
}
