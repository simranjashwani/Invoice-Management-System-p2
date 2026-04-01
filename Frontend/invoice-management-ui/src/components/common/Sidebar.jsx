import { NavLink } from "react-router-dom";
import { useAuthContext } from "../../context/AuthContext";

function Sidebar() {
  const { user } = useAuthContext();

  const canViewAnalytics =
    user?.role === "FinanceManager" || user?.role === "Admin";

  return (
    <aside className="sidebar">
      <h3 className="sidebar-title">Menu</h3>

      <nav className="sidebar-nav">
        <NavLink to="/invoices" className="sidebar-link">
          Invoices
        </NavLink>

        <NavLink to="/invoices/create" className="sidebar-link">
          Create Invoice
        </NavLink>

        {canViewAnalytics && (
          <>
            <NavLink to="/analytics/aging" className="sidebar-link">
              Aging Dashboard
            </NavLink>

            <NavLink to="/analytics/revenue" className="sidebar-link">
              Revenue Dashboard
            </NavLink>
          </>
        )}
      </nav>
    </aside>
  );
}

export default Sidebar;