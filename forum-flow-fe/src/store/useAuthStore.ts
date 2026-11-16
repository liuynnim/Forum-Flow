import { create } from "zustand";
import { persist } from "zustand/middleware";

export type AuthUser = {
  userId: string;
  username: string;
  displayName: string;
  email: string;
  role: string;
};

export type AuthState = {
  user: AuthUser | null;
  accessToken: string | null;
  setAuth: (token: string, user: AuthUser) => void;
  logout: () => void;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      setAuth: (token, user) =>
        set({
          accessToken: token,
          user,
        }),
      logout: () => set({ accessToken: null, user: null }),
    }),
    {
      name: "forumflow-auth-storage",
    },
  ),
);
