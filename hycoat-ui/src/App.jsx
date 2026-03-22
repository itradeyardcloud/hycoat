import { Routes, Route, Navigate } from 'react-router-dom';

// Placeholder pages — will be implemented in later features
const LoginPage = () => <div>Login Page (see 02-auth-system)</div>;
const DashboardPage = () => <div>Dashboard (see 09-dashboards)</div>;

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}

export default App;
