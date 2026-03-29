import * as signalR from '@microsoft/signalr';
import { acquireAccessToken, isBypassAuthEnabled } from '@/config/msalConfig';

const configuredApiUrl = (import.meta.env.VITE_API_URL || '').trim();
const useDevProxy =
  import.meta.env.DEV &&
  (!configuredApiUrl || /localhost|127\.0\.0\.1/i.test(configuredApiUrl));
const API_BASE_URL =
  useDevProxy
    ? ''
    : configuredApiUrl ||
      (import.meta.env.DEV ? 'https://localhost:5001' : 'https://hycoat-dev-api.azurewebsites.net');
let connection;

export async function startNotificationConnection(onReceiveNotification) {
  if (isBypassAuthEnabled) {
    return null;
  }

  const token = await acquireAccessToken();
  if (!token) {
    return null;
  }

  if (!connection) {
    const hubUrl = useDevProxy ? '/hubs/notifications' : `${API_BASE_URL}/hubs/notifications`;

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => (await acquireAccessToken()) || '',
        withCredentials: false,
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
