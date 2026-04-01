function DsoCard({ dso }) {
  return (
    <div className="card">
      <h3>DSO</h3>
      <p>{dso?.daysSalesOutstanding ?? dso?.dso ?? 0}</p>
    </div>
  );
}

export default DsoCard;