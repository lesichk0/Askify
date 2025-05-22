import axios from 'axios';

// Use the environment variable from .env
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7264/api';

// Create an axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Function to check token expiration
const isTokenExpired = () => {
  const token = localStorage.getItem('token');
  if (!token) return true;
  
  try {
    // Get the expiration from the token
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64).split('').map(c => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join('')
    );
    
    const { exp } = JSON.parse(jsonPayload);
    // Add 1 hour buffer to account for clock differences
    return (exp * 1000) < (Date.now() - 3600000); 
  } catch (e) {
    console.error('Error checking token expiration:', e);
    return true;
  }
};

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      // Only add the token if it's not expired
      if (!isTokenExpired()) {
        config.headers.Authorization = `Bearer ${token}`;
      } else {
        console.warn('Token is expired, not adding to headers');
        // Don't immediately clear token - let the response interceptor handle it
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);


api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.status, error.response?.data);
    
    // Handle 401 Unauthorized - but only clear auth data if it's a real auth issue
    // This allows components to handle the error gracefully
    if (error.response?.status === 401) {
      // Check if this is an API endpoint that actually requires authentication
      // Don't clear tokens for requests that might fail for other reasons
      const isAuthEndpoint = error.config.url?.includes('/auth/') || false;
      const isUserAction = error.config.url?.includes('/posts') || 
                         error.config.url?.includes('/consultations') ||
                         error.config.url?.includes('/users');
                         
      if (!isAuthEndpoint || isUserAction) {
        console.warn('Authentication token is invalid or expired. Setting auth error flag.');
        
        // Add custom property to the error object to make it easier for components to detect auth errors
        error.isAuthError = true;
        
        // Store token expiration time to prevent multiple redirects
        const lastAuthErrorTime = localStorage.getItem('lastAuthErrorTime');
        const currentTime = Date.now();
        
        // Only clear auth data and set flag if we haven't recently handled an auth error
        if (!lastAuthErrorTime || currentTime - parseInt(lastAuthErrorTime) > 10000) {
          localStorage.setItem('lastAuthErrorTime', currentTime.toString());
          localStorage.setItem('authError', 'true');
          
          // Important: Don't remove the token here, let the individual components
          // decide whether to redirect based on the error response
        }
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;
