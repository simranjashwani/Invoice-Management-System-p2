export function calculateLineTotal(item) {
  const quantity = Number(item.quantity || 0);
  const unitPrice = Number(item.unitPrice || 0);
  const discount = Number(item.discount || 0);
  const tax = Number(item.tax || 0);

  return quantity * unitPrice - discount + tax;
}

export function calculateSubTotal(lineItems = []) {
  return lineItems.reduce((sum, item) => sum + calculateLineTotal(item), 0);
}

export function calculateGrandTotal(lineItems = [], discountAmount = 0, taxAmount = 0) {
  const subTotal = calculateSubTotal(lineItems);
  return subTotal - Number(discountAmount || 0) + Number(taxAmount || 0);
}