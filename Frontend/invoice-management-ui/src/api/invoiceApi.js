

import axiosClient from "./axiosClient";

export async function getAllInvoices() {
  return [
    {
      invoiceId: 1,
      invoiceNumber: "INV-001",
      customerId: 101,
      status: "Draft",
      grandTotal: 2000,
      outstandingBalance: 2000,
    },
    {
      invoiceId: 2,
      invoiceNumber: "INV-002",
      customerId: 102,
      status: "PartiallyPaid",
      grandTotal: 5000,
      outstandingBalance: 2000,
    },
  ];
}

export async function getInvoiceById(id) {
  return {
    invoiceId: Number(id),
    invoiceNumber: `INV-00${id}`,
    customerId: 101,
    status: "Draft",
    grandTotal: 2500,
    outstandingBalance: 1500,
    lineItems: [
      {
        lineItemId: 1,
        description: "Website Design",
        quantity: 1,
        lineTotal: 1500,
      },
      {
        lineItemId: 2,
        description: "Hosting",
        quantity: 1,
        lineTotal: 1000,
      },
    ],
    payments: [
      {
        paymentId: 1,
        paymentAmount: 1000,
        paymentMethod: "Cash",
      },
    ],
  };
}

export async function createInvoice(payload) {
  console.log("Mock create invoice payload:", payload);
  return {
    message: "Invoice created successfully",
    invoiceId: 3,
  };
}