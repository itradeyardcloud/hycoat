import { create } from 'zustand';

const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,

  setUser: (user) => set({ user, isAuthenticated: !!user }),

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    set({ user: null, isAuthenticated: false });
  },

  // Initialize from localStorage on app load
  initialize: () => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const isExpired = payload.exp * 1000 < Date.now();
        if (!isExpired) {
          set({
            user: {
              id: payload.sub,
              email: payload.email,
              fullName: payload.name,
              role: payload.role,
              department: payload.department,
            },
            isAuthenticated: true,
          });
        }
      } catch {
        // Invalid token format — clear
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      }
    }
  },
}));

export default useAuthStore;
