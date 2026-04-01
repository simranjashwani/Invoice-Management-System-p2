function OutstandingCard({ outstanding }) {
  return (
    <div className="card">
      <h3>Outstanding</h3>
      <p>{outstanding?.totalOutstanding ?? 0}</p>
    </div>
  );
}

export default OutstandingCard;