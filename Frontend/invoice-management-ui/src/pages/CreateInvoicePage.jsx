import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createInvoice } from "../api/invoiceApi";
import LineItemsForm from "../components/invoices/LineItemsForm";
import ErrorMessage from "../components/common/ErrorMessage";
import { calculateGrandTotal, calculateSubTotal } from "../utils/calculations";

function CreateInvoicePage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    customerId: "",
    quoteId: "",
    invoiceDate: "",
    dueDate: "",
    discountAmount: 0,
    taxAmount: 0,
    lineItems: [
      {
        description: "",
        quantity: 1,
        unitPrice: 0,
        discount: 0,
        tax: 0,
      },
    ],
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  function handleInvoiceChange(e) {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  }

  function handleLineItemChange(index, e) {
    const updatedItems = [...formData.lineItems];
    updatedItems[index] = {
      ...updatedItems[index],
      [e.target.name]: e.target.value,
    };

    setFormData((prev) => ({
      ...prev,
      lineItems: updatedItems,
    }));
  }

  function addLineItem() {
    setFormData((prev) => ({
      ...prev,
      lineItems: [
        ...prev.lineItems,
        {
          description: "",
          quantity: 1,
          unitPrice: 0,
          discount: 0,
          tax: 0,
        },
      ],
    }));
  }

  function removeLineItem(index) {
    const updatedItems = formData.lineItems.filter((_, i) => i !== index);
    setFormData((prev) => ({
      ...prev,
      lineItems: updatedItems,
    }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    try {
      setSubmitting(true);

      const payload = {
        ...formData,
        customerId: Number(formData.customerId),
        quoteId: formData.quoteId ? Number(formData.quoteId) : null,
        discountAmount: Number(formData.discountAmount || 0),
        taxAmount: Number(formData.taxAmount || 0),
        lineItems: formData.lineItems.map((item) => ({
          description: item.description,
          quantity: Number(item.quantity),
          unitPrice: Number(item.unitPrice),
          discount: Number(item.discount || 0),
          tax: Number(item.tax || 0),
        })),
      };

      await createInvoice(payload);
      navigate("/invoices");
    } catch {
      setError("Failed to create invoice.");
    } finally {
      setSubmitting(false);
    }
  }

  const subTotal = calculateSubTotal(formData.lineItems);
  const grandTotal = calculateGrandTotal(
    formData.lineItems,
    formData.discountAmount,
    formData.taxAmount
  );

  return (
    <form onSubmit={handleSubmit}>
      <div className="page-header">
        <div>
          <h1>Create Invoice</h1>
          <p>Create invoice with dynamic line items</p>
        </div>
      </div>

      <div className="card form-grid">
        <input
          type="number"
          name="customerId"
          placeholder="Customer Id"
          value={formData.customerId}
          onChange={handleInvoiceChange}
          required
        />

        <input
          type="number"
          name="quoteId"
          placeholder="Quote Id (optional)"
          value={formData.quoteId}
          onChange={handleInvoiceChange}
        />

        <input
          type="date"
          name="invoiceDate"
          value={formData.invoiceDate}
          onChange={handleInvoiceChange}
          required
        />

        <input
          type="date"
          name="dueDate"
          value={formData.dueDate}
          onChange={handleInvoiceChange}
          required
        />

        <input
          type="number"
          step="0.01"
          name="discountAmount"
          placeholder="Invoice Discount"
          value={formData.discountAmount}
          onChange={handleInvoiceChange}
        />

        <input
          type="number"
          step="0.01"
          name="taxAmount"
          placeholder="Invoice Tax"
          value={formData.taxAmount}
          onChange={handleInvoiceChange}
        />
      </div>

      <LineItemsForm
        lineItems={formData.lineItems}
        onChange={handleLineItemChange}
        onAdd={addLineItem}
        onRemove={removeLineItem}
      />

      <div className="card totals-box">
        <h3>SubTotal: {subTotal.toFixed(2)}</h3>
        <h3>Grand Total: {grandTotal.toFixed(2)}</h3>
      </div>

      <ErrorMessage message={error} />

      <button className="btn" type="submit" disabled={submitting}>
        {submitting ? "Saving..." : "Save Invoice"}
      </button>
    </form>
  );
}

export default CreateInvoicePage;