import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { addPayment } from "../api/paymentApi";
import PaymentForm from "../components/payments/PaymentForm";

function AddPaymentPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    paymentAmount: "",
    paymentDate: "",
    paymentMethod: "Cash",
    referenceNumber: "",
    receivedDate: "",
  });

  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  function handleChange(e) {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    try {
      setSubmitting(true);

      const payload = {
        ...formData,
        paymentAmount: Number(formData.paymentAmount),
      };

      await addPayment(id, payload);
      navigate(`/invoices/${id}`);
    } catch {
      setError("Failed to add payment.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <PaymentForm
      formData={formData}
      onChange={handleChange}
      onSubmit={handleSubmit}
      error={error}
      submitting={submitting}
    />
  );
}

export default AddPaymentPage;