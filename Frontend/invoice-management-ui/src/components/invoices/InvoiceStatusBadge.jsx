function InvoiceStatusBadge({ status }) {
  const safeStatus = status || "Unknown";
  return <span className={`status-badge status-${safeStatus.toLowerCase()}`}>{safeStatus}</span>;
}

export default InvoiceStatusBadge;