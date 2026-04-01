import { Link } from "react-router-dom";
import InvoiceStatusBadge from "./InvoiceStatusBadge";
import { formatCurrency } from "../../utils/formatters";

function InvoiceTable({ invoices = [] }) {
  return (
    <div className="card">
      <table className="data-table">
        <thead>
          <tr>
            <th>Invoice No</th>
            <th>Customer</th>
            <th>Status</th>
            <th>Grand Total</th>
            <th>Outstanding</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {invoices.length === 0 ? (
            <tr>
              <td colSpan="6">No invoices found.</td>
            </tr>
          ) : (
            invoices.map((invoice) => (
              <tr key={invoice.invoiceId}>
                <td>{invoice.invoiceNumber}</td>
                <td>{invoice.customerId}</td>
                <td>
                  <InvoiceStatusBadge status={invoice.status} />
                </td>
                <td>{formatCurrency(invoice.grandTotal)}</td>
                <td>{formatCurrency(invoice.outstandingBalance)}</td>
                <td>
                  <Link className="btn btn-sm" to={`/invoices/${invoice.invoiceId}`}>
                    View
                  </Link>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

export default InvoiceTable;