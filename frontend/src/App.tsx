import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from './hooks.ts';
import { checkAuthStatus } from './features/auth/authSlice';
import MainLayout from './layouts/MainLayout';
import HomePage from './pages/HomePage';
import ConsultationsPage from './pages/ConsultationsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ConsultationDetailPage from './pages/ConsultationDetailPage';
import ProfilePage from './pages/ProfilePage';
import CreateConsultationPage from './pages/CreateConsultationPage';
import AnswerConsultationsPage from './pages/AnswerConsultationsPage';
import NotificationsPage from './pages/NotificationsPage';
import ConsultationRequestsPage from './pages/ConsultationRequestsPage';
import ExpertsPage from './pages/ExpertsPage';

function App() {
  const dispatch = useAppDispatch();
  const { isAuthenticated, user } = useAppSelector(state => state.auth);


  useEffect(() => {
    dispatch(checkAuthStatus());
    
    // Check for auth errors from API interceptor
    const authError = localStorage.getItem('authError');
    if (authError === 'true') {
      console.warn('Authentication error detected from previous request');
      // Clear the flag after handling it
      localStorage.removeItem('authError');
    }
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
        
        {/* Protected routes */}
        <Route path="/profile" element={
          isAuthenticated ? (
            <MainLayout>
              <ProfilePage />
            </MainLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        } />
        <Route path="/consultations/new" element={
          <MainLayout>
            {user?.role === 'User' ? (
              <CreateConsultationPage />
            ) : (
              <Navigate to="/" replace />
            )}
          </MainLayout>
        } />
        <Route path="/consultation-requests" element={
          <MainLayout>
            {user?.role === 'Expert' ? (
              <ConsultationRequestsPage />
            ) : (
              <Navigate to="/" replace />
            )}
          </MainLayout>
        } />
        <Route path="/my-consultations" element={
          <MainLayout>
            {isAuthenticated ? (
              <ConsultationsPage showMine={true} />
            ) : (
              <Navigate to="/login" replace />
            )}
          </MainLayout>
        } />
        <Route path="/notifications" element={
          isAuthenticated ? (
            <MainLayout>
              <NotificationsPage />
            </MainLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        } />
        <Route path="/answer-consultations" element={
          isAuthenticated && user?.role === 'Expert' ? (
            <MainLayout>
              <AnswerConsultationsPage />
            </MainLayout>
          ) : (
            <Navigate to="/" replace />
          )
        } />
        <Route path="/experts" element={
          <MainLayout>
            <ExpertsPage />
          </MainLayout>
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