import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { useEffect } from 'react';
import { useAppDispatch } from './hooks.ts';
import { checkAuthStatus } from './features/auth/authSlice';
import MainLayout from './layouts/MainLayout';
import HomePage from './pages/HomePage';
import ProtectedRoute from './components/ProtectedRoute';
import ConsultationsPage from './pages/ConsultationsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ConsultationDetailPage from './pages/ConsultationDetailPage';

function App() {
  const dispatch = useAppDispatch();

  // Check authentication status when app loads
  useEffect(() => {
    dispatch(checkAuthStatus());
  }, [dispatch]);

  return (
    <Router>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={
          <MainLayout>
            <HomePage />
          </MainLayout>
        } />
        <Route path="/login" element={
          <MainLayout>
            <LoginPage />
          </MainLayout>
        } />
        <Route path="/register" element={
          <MainLayout>
            <RegisterPage />
          </MainLayout>
        } />
        <Route path="/consultations" element={
          <MainLayout>
            <ConsultationsPage />
          </MainLayout>
        } />
        <Route path="/consultations/:id" element={
          <MainLayout>
            <ConsultationDetailPage />
          </MainLayout>
        } />

        {/* Protected routes - add these as you build the pages */}
        <Route path="/my-consultations" element={
          <ProtectedRoute>
            <MainLayout>
              <ConsultationsPage />
            </MainLayout>
          </ProtectedRoute>
        } />

        {/* Fallback route */}
        <Route path="*" element={
          <MainLayout>
            <div className="text-center py-10">
              <h1 className="text-3xl font-bold text-gray-800 mb-4">Page Not Found</h1>
              <p className="text-gray-600">The page you're looking for doesn't exist.</p>
            </div>
          </MainLayout>
        } />
      </Routes>
    </Router>
  );
}

export default App;