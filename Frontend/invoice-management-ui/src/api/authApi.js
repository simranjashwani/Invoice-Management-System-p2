export async function loginUser(payload) {
  // MOCK LOGIN (temporary)
  return {
    token: "fake-jwt-token",
    username: payload.username,
    role: "Admin",
  };
}