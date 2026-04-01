import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getAllInvoices } from "../api/invoiceApi";
import InvoiceTable from "../components/invoices/InvoiceTable";
import Loader from "../components/common/Loader";
import ErrorMessage from "../components/common/ErrorMessage";

function InvoiceListPage() {
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchInvoices() {
      try {
        const data = await getAllInvoices();
        setInvoices(Array.isArray(data) ? data : data.items || []);
      } catch {
        setError("Failed to load invoices.");
      } finally {
        setLoading(false);
      }
    }

    fetchInvoices();
  }, []);

  if (loading) return <Loader text="Loading invoices..." />;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Invoices</h1>
          <p>View and manage all invoices</p>
        </div>

        <Link className="btn" to="/invoices/create">
          Create Invoice
        </Link>
      </div>

      <ErrorMessage message={error} />
      <InvoiceTable invoices={invoices} />
    </div>
  );
}

export default InvoiceListPage;