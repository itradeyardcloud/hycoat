import { create } from 'zustand';

const useUiStore = create((set) => ({
  sidebarOpen: true,
  mobileDrawerOpen: false,

  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  toggleMobileDrawer: () => set((state) => ({ mobileDrawerOpen: !state.mobileDrawerOpen })),
  closeMobileDrawer: () => set({ mobileDrawerOpen: false }),
}));

export default useUiStore;
