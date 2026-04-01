import { useEffect, useState } from "react";
import { getDso, getOutstanding, getRevenueSummary } from "../api/analyticsApi";
import Loader from "../components/common/Loader";
import ErrorMessage from "../components/common/ErrorMessage";
import RevenueSummaryCard from "../components/analytics/RevenueSummaryCard";
import DsoCard from "../components/analytics/DsoCard";
import OutstandingCard from "../components/analytics/OutstandingCard";

function RevenueDashboardPage() {
  const [revenue, setRevenue] = useState(null);
  const [dso, setDso] = useState(null);
  const [outstanding, setOutstanding] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function fetchDashboardData() {
      try {
        const [revenueData, dsoData, outstandingData] = await Promise.all([
          getRevenueSummary(),
          getDso(),
          getOutstanding(),
        ]);

        setRevenue(revenueData);
        setDso(dsoData);
        setOutstanding(outstandingData);
      } catch {
        setError("Failed to load revenue dashboard.");
      } finally {
        setLoading(false);
      }
    }

    fetchDashboardData();
  }, []);

  if (loading) return <Loader text="Loading revenue dashboard..." />;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Revenue Dashboard</h1>
          <p>Revenue, DSO, and outstanding summary</p>
        </div>
      </div>

      <ErrorMessage message={error} />

      <div className="card-grid">
        <RevenueSummaryCard revenue={revenue} />
        <DsoCard dso={dso} />
        <OutstandingCard outstanding={outstanding} />
      </div>
    </div>
  );
}

export default RevenueDashboardPage;