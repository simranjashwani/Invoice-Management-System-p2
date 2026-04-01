function PaymentForm({ formData, onChange, onSubmit, error, submitting }) {
  return (
    <form onSubmit={onSubmit} className="card form-grid">
      <h2>Add Payment</h2>

      <input
        type="number"
        name="paymentAmount"
        placeholder="Payment Amount"
        value={formData.paymentAmount}
        onChange={onChange}
        required
      />

      <input
        type="date"
        name="paymentDate"
        value={formData.paymentDate}
        onChange={onChange}
        required
      />

      <select
        name="paymentMethod"
        value={formData.paymentMethod}
        onChange={onChange}
      >
        <option value="Cash">Cash</option>
        <option value="CreditCard">CreditCard</option>
        <option value="BankTransfer">BankTransfer</option>
      </select>

      <input
        type="text"
        name="referenceNumber"
        placeholder="Reference Number"
        value={formData.referenceNumber}
        onChange={onChange}
      />

      <input
        type="date"
        name="receivedDate"
        value={formData.receivedDate}
        onChange={onChange}
      />

      {error && <div className="error-box">{error}</div>}

      <button className="btn" type="submit" disabled={submitting}>
        {submitting ? "Submitting..." : "Submit Payment"}
      </button>
    </form>
  );
}

export default PaymentForm;