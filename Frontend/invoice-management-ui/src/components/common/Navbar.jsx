import { useAuthContext } from "../../context/AuthContext";

function Navbar() {
  const { user, logout } = useAuthContext();

  return (
    <header className="navbar">
      <div>
        <h2 className="navbar-title">Invoice Management System</h2>
        <p className="navbar-subtitle">Welcome, {user?.username || "User"}</p>
      </div>

      <div className="navbar-actions">
        <span className="role-badge">{user?.role || "FinanceUser"}</span>
        <button className="btn btn-outline" onClick={logout}>
          Logout
        </button>
      </div>
    </header>
  );
}

export default Navbar;