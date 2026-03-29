import { create } from 'zustand';

const useNotificationStore = create((set) => ({
  notifications: [],
  unreadCount: 0,

  setLatestNotifications: (items) =>
    set(() => ({ notifications: Array.isArray(items) ? items : [] })),

  setUnreadCount: (count) =>
    set(() => ({ unreadCount: Number.isFinite(count) ? Math.max(0, count) : 0 })),

  addNotification: (notification) =>
    set((state) => {
      const existing = state.notifications.filter((n) => n.id !== notification.id);
      return {
        notifications: [notification, ...existing].slice(0, 10),
        unreadCount: notification.isRead ? state.unreadCount : state.unreadCount + 1,
      };
    }),

  markRead: (id) =>
    set((state) => {
      const wasUnread = state.notifications.some((n) => n.id === id && !n.isRead);
      return {
        notifications: state.notifications.map((n) =>
          n.id === id ? { ...n, isRead: true } : n,
        ),
        unreadCount: wasUnread ? Math.max(0, state.unreadCount - 1) : state.unreadCount,
      };
    }),

  markAllRead: () =>
    set((state) => ({
      notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
      unreadCount: 0,
    })),

  removeNotification: (id) =>
    set((state) => {
      const target = state.notifications.find((n) => n.id === id);
      return {
        notifications: state.notifications.filter((n) => n.id !== id),
        unreadCount: !target || target.isRead
          ? state.unreadCount
          : Math.max(0, state.unreadCount - 1),
      };
    }),
}));

export default useNotificationStore;
