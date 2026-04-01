import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getInvoiceById } from "../api/invoiceApi";
import Loader from "../components/common/Loader";
import ErrorMessage from "../components/common/ErrorMessage";
import InvoiceStatusBadge from "../components/invoices/InvoiceStatusBadge";
import { formatCurrency } from "../utils/formatters";

function InvoiceDetailPage() {
  const { id } = useParams();
  const [invoice, setInvoice] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchInvoice() {
      try {
        const data = await getInvoiceById(id);
        setInvoice(data);
      } catch {
        setError("Failed to load invoice details.");
      } finally {
        setLoading(false);
      }
    }

    fetchInvoice();
  }, [id]);

  if (loading) return <Loader text="Loading invoice details..." />;
  if (!invoice) return <ErrorMessage message={error || "Invoice not found."} />;

  const isPaid = invoice.status === "Paid";

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Invoice Detail</h1>
          <p>Invoice Number: {invoice.invoiceNumber}</p>
        </div>

        <Link
          className={`btn ${isPaid ? "btn-disabled" : ""}`}
          to={isPaid ? "#" : `/invoices/${id}/payment`}
          onClick={(e) => {
            if (isPaid) e.preventDefault();
          }}
        >
          {isPaid ? "Paid Invoice" : "Add Payment"}
        </Link>
      </div>

      <ErrorMessage message={error} />

      <div className="card-grid">
        <div className="card">
          <h3>Status</h3>
          <InvoiceStatusBadge status={invoice.status} />
        </div>
        <div className="card">
          <h3>Grand Total</h3>
          <p>{formatCurrency(invoice.grandTotal)}</p>
        </div>
        <div className="card">
          <h3>Outstanding Balance</h3>
          <p>{formatCurrency(invoice.outstandingBalance)}</p>
        </div>
      </div>

      <div className="card">
        <h3>Line Items</h3>
        <ul className="simple-list">
          {invoice.lineItems?.length ? (
            invoice.lineItems.map((item) => (
              <li key={item.lineItemId}>
                {item.description} | Qty: {item.quantity} | Total: {formatCurrency(item.lineTotal)}
              </li>
            ))
          ) : (
            <li>No line items found.</li>
          )}
        </ul>
      </div>

      <div className="card">
        <h3>Payments</h3>
        <ul className="simple-list">
          {invoice.payments?.length ? (
            invoice.payments.map((payment) => (
              <li key={payment.paymentId}>
                Amount: {formatCurrency(payment.paymentAmount)} | Method: {payment.paymentMethod}
              </li>
            ))
          ) : (
            <li>No payments found.</li>
          )}
        </ul>
      </div>
    </div>
  );
}

export default InvoiceDetailPage;