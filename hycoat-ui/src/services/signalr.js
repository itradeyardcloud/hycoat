import * as signalR from '@microsoft/signalr';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001';
let connection;

export async function startNotificationConnection(onReceiveNotification) {
  const token = localStorage.getItem('accessToken');
  if (!token) {
    return null;
  }

  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notifications`, {
        accessTokenFactory: () => localStorage.getItem('accessToken') || '',
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }

  connection.off('ReceiveNotification');
  if (onReceiveNotification) {
    connection.on('ReceiveNotification', onReceiveNotification);
  }

  if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start();
  }

  return connection;
}

export async function stopNotificationConnection() {
  if (!connection) {
    return;
  }

  connection.off('ReceiveNotification');

  if (connection.state !== signalR.HubConnectionState.Disconnected) {
    await connection.stop();
  }
}
