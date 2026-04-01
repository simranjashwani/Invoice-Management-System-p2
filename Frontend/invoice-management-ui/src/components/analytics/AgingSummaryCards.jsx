function AgingSummaryCards({ items = [] }) {
  return (
    <div className="card-grid">
      {items.map((item, index) => (
        <div className="card" key={index}>
          <h3>{item.bucket}</h3>
          <p>Total Amount: {item.totalAmount}</p>
          <p>Invoice Count: {item.invoiceCount}</p>
        </div>
      ))}
    </div>
  );
}

export default AgingSummaryCards;