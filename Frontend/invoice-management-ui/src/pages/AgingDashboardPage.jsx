import { useEffect, useState } from "react";
import { getAgingReport } from "../api/analyticsApi";
import Loader from "../components/common/Loader";
import ErrorMessage from "../components/common/ErrorMessage";
import AgingSummaryCards from "../components/analytics/AgingSummaryCards";

function AgingDashboardPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchAgingReport() {
      try {
        const data = await getAgingReport();
        setItems(Array.isArray(data) ? data : data.items || []);
      } catch {
        setError("Failed to load aging report.");
      } finally {
        setLoading(false);
      }
    }

    fetchAgingReport();
  }, []);

  if (loading) return <Loader text="Loading aging dashboard..." />;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Aging Dashboard</h1>
          <p>View overdue aging buckets</p>
        </div>
      </div>

      <ErrorMessage message={error} />
      <AgingSummaryCards items={items} />
    </div>
  );
}

export default AgingDashboardPage;