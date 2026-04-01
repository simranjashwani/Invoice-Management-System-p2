import { Navigate } from "react-router-dom";
import { useAuthContext } from "../../context/AuthContext";

function ProtectedRoute({ children }) {
  const { isAuthenticated, bootstrapped } = useAuthContext();

  if (!bootstrapped) {
    return <p style={{ padding: "24px" }}>Loading...</p>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
}

export default ProtectedRoute;