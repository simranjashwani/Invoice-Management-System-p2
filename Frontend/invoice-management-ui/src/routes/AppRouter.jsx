import { Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "../components/common/ProtectedRoute";
import AppLayout from "../components/common/AppLayout";
import LoginPage from "../pages/LoginPage";
import InvoiceListPage from "../pages/InvoiceListPage";
import CreateInvoicePage from "../pages/CreateInvoicePage";
import InvoiceDetailPage from "../pages/InvoiceDetailPage";
import AddPaymentPage from "../pages/AddPaymentPage";
import AgingDashboardPage from "../pages/AgingDashboardPage";
import RevenueDashboardPage from "../pages/RevenueDashboardPage";

function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/invoices"
        element={
          <ProtectedRoute>
            <AppLayout>
              <InvoiceListPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/invoices/create"
        element={
          <ProtectedRoute>
            <AppLayout>
              <CreateInvoicePage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/invoices/:id"
        element={
          <ProtectedRoute>
            <AppLayout>
              <InvoiceDetailPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/invoices/:id/payment"
        element={
          <ProtectedRoute>
            <AppLayout>
              <AddPaymentPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/analytics/aging"
        element={
          <ProtectedRoute>
            <AppLayout>
              <AgingDashboardPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/analytics/revenue"
        element={
          <ProtectedRoute>
            <AppLayout>
              <RevenueDashboardPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}

export default AppRouter;