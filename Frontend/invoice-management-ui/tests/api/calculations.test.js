import assert from "node:assert/strict";

import {
  calculateGrandTotal,
  calculateLineTotal,
  calculateSubTotal,
} from "../../src/utils/calculations.js";

export function runCalculationTests() {
  assert.equal(
    calculateLineTotal({
      quantity: 12,
      unitPrice: 249.99,
      discount: 100,
      tax: 18,
    }),
    2917.88,
    "calculateLineTotal should apply quantity, discount, and tax"
  );

  assert.equal(
    calculateLineTotal({
      quantity: "",
      unitPrice: "",
      discount: "",
      tax: "",
    }),
    0,
    "calculateLineTotal should fall back to zero"
  );

  assert.equal(
    calculateSubTotal([
      { quantity: 2, unitPrice: 100, discount: 0, tax: 0 },
      { quantity: 1, unitPrice: 50, discount: 5, tax: 10 },
    ]),
    255,
    "calculateSubTotal should sum all line totals"
  );

  assert.equal(
    calculateGrandTotal(
      [
        { quantity: 2, unitPrice: 100, discount: 0, tax: 0 },
        { quantity: 1, unitPrice: 50, discount: 5, tax: 10 },
      ],
      25,
      18
    ),
    248,
    "calculateGrandTotal should apply invoice-level discount and tax"
  );
}
