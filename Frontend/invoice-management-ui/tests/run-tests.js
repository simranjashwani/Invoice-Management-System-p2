import { runCalculationTests } from "./api/calculations.test.js";
import { runMockApiTests } from "./api/mock-api.test.js";
import { runFormatterTests } from "./pages/formatters.test.js";
import { runInvoiceFormSourceTests } from "./pages/invoice-form-source.test.js";
import { runPaymentFormSourceTests } from "./pages/payment-form-source.test.js";

const suites = [
  ["calculations", runCalculationTests],
  ["invoice-form-source", runInvoiceFormSourceTests],
  ["payment-form-source", runPaymentFormSourceTests],
  ["mock-api", runMockApiTests],
  ["formatters", runFormatterTests],
];

let failures = 0;

for (const [name, runSuite] of suites) {
  try {
    await runSuite();
    console.log(`PASS ${name}`);
  } catch (error) {
    failures += 1;
    console.error(`FAIL ${name}`);
    console.error(error);
  }
}

if (failures > 0) {
  process.exitCode = 1;
} else {
  console.log(`PASS ${suites.length} frontend test suites`);
}
