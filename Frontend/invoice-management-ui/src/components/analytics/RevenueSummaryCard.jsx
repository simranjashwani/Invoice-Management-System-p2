function RevenueSummaryCard({ revenue }) {
  return (
    <div className="card">
      <h3>Revenue Summary</h3>
      <p>Total Revenue: {revenue?.totalRevenue ?? 0}</p>
    </div>
  );
}

export default RevenueSummaryCard;