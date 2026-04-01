import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { loginUser } from "../api/authApi";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [bootstrapped, setBootstrapped] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token");
    const username = localStorage.getItem("username");
    const role = localStorage.getItem("role");

    if (token) {
      setUser({ token, username, role });
    }
    setBootstrapped(true);
  }, []);

  async function login(credentials) {
    const data = await loginUser(credentials);

    const normalizedUser = {
      token: data.token,
      username: data.username ?? credentials.username,
      role: data.role ?? "FinanceUser",
    };

    localStorage.setItem("token", normalizedUser.token);
    localStorage.setItem("username", normalizedUser.username);
    localStorage.setItem("role", normalizedUser.role);

    setUser(normalizedUser);
  }

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("username");
    localStorage.removeItem("role");
    setUser(null);
  }

  const value = useMemo(
    () => ({
      user,
      login,
      logout,
      bootstrapped,
      isAuthenticated: !!user?.token,
    }),
    [user, bootstrapped]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuthContext() {
  return useContext(AuthContext);
}