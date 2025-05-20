// src/features/auth/authSlice.ts
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import api from '../../api/api';

// Define response types for better type safety
interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

// Define LoginResponse to match the actual API response format
interface LoginResponse {
  isSuccess: boolean;
  message: string | null;
  token: string;
  userId: string;
  fullName: string;
  email: string;
  roles: string[];
}

// Async thunks for authentication
export const login = createAsyncThunk(
  'auth/login',
  async (credentials: { email: string; password: string }, { rejectWithValue }) => {
    try {
      console.log('Login credentials:', credentials);
      
      // Try with the proper casing (ASP.NET Core APIs often use PascalCase)
      const payload = {
        Email: credentials.email,
        Password: credentials.password,
      };
      
      console.log('Sending login request with payload:', payload);
      
      const response = await api.post('/auth/login', payload);
      console.log('Login response:', response.data);
      
      // Save token to localStorage
      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
      } else if (typeof response.data === 'string') {
        localStorage.setItem('token', response.data);
      }
      
      return response.data as LoginResponse;
    } catch (error: any) {
      console.error('Login error details:', error);
      
      // Extract the error message in a more structured way
      let errorMessage = 'Login failed';
      
      if (error.response?.data) {
        const responseData = error.response.data;
        
        // Handle validation errors in standard problem details format
        if (responseData.errors) {
          // Convert validation errors object to string
          errorMessage = Object.entries(responseData.errors)
            .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
            .join('; ');
        } else if (responseData.title) {
          errorMessage = responseData.title;
        } else if (typeof responseData === 'string') {
          errorMessage = responseData;
        }
      }
      
      return rejectWithValue(errorMessage);
    }
  }
);

export const logout = createAsyncThunk<null>(
  'auth/logout',
  async () => {
    localStorage.removeItem('token');
    return null;
  }
);

// Helper function to decode JWT token
const decodeToken = (token: string) => {
  try {
    // JWT tokens are in format: header.payload.signature
    // We only need the payload part
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => `%${(`00${c.charCodeAt(0).toString(16)}`).slice(-2)}`)
        .join('')
    );
    
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
};

export const checkAuthStatus = createAsyncThunk(
  'auth/checkStatus',
  async (_, { rejectWithValue }) => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    
    try {
      // Instead of using /auth/me, decode the token to get the user ID
      const decodedToken = decodeToken(token);
      
      // If token is invalid or expired, reject
      if (!decodedToken) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        return rejectWithValue('Invalid token');
      }
      
      // Check if token is expired
      const currentTime = Date.now() / 1000;
      if (decodedToken.exp && decodedToken.exp < currentTime) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        return rejectWithValue('Token expired');
      }
      
      // Try to get user from localStorage first
      const storedUser = getUserFromLocalStorage();
      if (storedUser) {
        return storedUser;
      }
      
      // If user data isn't in localStorage, fetch it using the ID from token
      const userId = decodedToken.sub || decodedToken.nameid; // Different JWT libraries use different claim names
      if (userId) {
        const response = await api.get(`/users/${userId}`);
        const userData = response.data;
        
        // Save user data to localStorage for future use
        saveUserToLocalStorage(userData);
        return userData;
      }
      
      // If we can't get the user ID from token, reject
      return rejectWithValue('Could not identify user from token');
    } catch (error: any) {
      console.error('Authentication check failed:', error);
      
      // Handle different types of errors
      if (error.response?.status === 404) {
        console.warn('Endpoint not found. Using token data instead.');
        // Try to use token information as fallback
        const decodedToken = decodeToken(token);
        if (decodedToken && decodedToken.sub) {
          // Create minimal user object from token claims
          const minimalUser = {
            id: decodedToken.sub || decodedToken.nameid,
            email: decodedToken.email || '',
            fullName: decodedToken.name || '',
            role: decodedToken.role || 'User'
          };
          saveUserToLocalStorage(minimalUser);
          return minimalUser;
        }
      }
      
      // If token is invalid or other error occurs, logout
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      return rejectWithValue(error.response?.data?.message || 'Authentication failed');
    }
  }
);

interface RegisterData {
  email: string;
  password: string;
  fullName: string;
  role: string;  // Add role field
}

export const register = createAsyncThunk(
  'auth/register',
  async (registerData: RegisterData, { rejectWithValue }) => {
    try {
      console.log('Registration data:', registerData);
      
      // Update the payload to include role
      const payload = {
        email: registerData.email,
        password: registerData.password,
        confirmPassword: registerData.password,
        fullName: registerData.fullName,
        role: registerData.role
      };
      
      console.log('Sending registration request with payload:', payload);
      
      const response = await api.post('/auth/register', payload);
      console.log('Registration response:', response.data);
      
      return response.data;
    } catch (error: any) {
      console.error('Registration error details:', error);
      
      // Extract the error message in a more structured way
      let errorMessage = 'Registration failed';
      
      if (error.response?.data) {
        const responseData = error.response.data;
        
        // Handle validation errors in standard problem details format
        if (responseData.errors) {
          // Convert validation errors object to string
          errorMessage = Object.entries(responseData.errors)
            .map(([field, errors]) => `${field}: ${Array.isArray(errors) ? errors.join(', ') : errors}`)
            .join('; ');
        } else if (responseData.title) {
          errorMessage = responseData.title;
        } else if (typeof responseData === 'string') {
          errorMessage = responseData;
        }
      }
      
      return rejectWithValue(errorMessage);
    }
  }
);

// Helper functions for localStorage
const saveUserToLocalStorage = (user: any) => {
  try {
    localStorage.setItem('user', JSON.stringify(user));
  } catch (error) {
    console.error('Error saving user to localStorage:', error);
  }
};

const getUserFromLocalStorage = () => {
  try {
    const userString = localStorage.getItem('user');
    return userString ? JSON.parse(userString) : null;
  } catch (error) {
    console.error('Error getting user from localStorage:', error);
    return null;
  }
};

const removeUserFromLocalStorage = () => {
  try {
    localStorage.removeItem('user');
  } catch (error) {
    console.error('Error removing user from localStorage:', error);
  }
};

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  username: string | null;
  loading: boolean;
  error: string | null;
}

// Update initial state to check localStorage first
const initialState: AuthState = {
  isAuthenticated: !!localStorage.getItem('token'),
  user: getUserFromLocalStorage(),
  username: getUserFromLocalStorage()?.fullName || null,
  loading: false,
  error: null
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      // Login cases
      .addCase(login.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<LoginResponse>) => {
        state.isAuthenticated = true;
        
        const userData = action.payload;
        
        // Map the API response to our User interface
        const user: User = {
          id: userData.userId, // Use userId instead of id
          email: userData.email,
          fullName: userData.fullName,
          role: userData.roles && userData.roles.length > 0 ? userData.roles[0] : 'User'
        };
        
        state.user = user;
        state.username = user.fullName;
        
        // Save user to localStorage
        saveUserToLocalStorage(user);
        
        state.loading = false;
      })
      .addCase(login.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Login failed';
      })
      
      // Logout case
      .addCase(logout.fulfilled, (state) => {
        state.isAuthenticated = false;
        state.user = null;
        state.username = null;
        // Remove user from localStorage
        removeUserFromLocalStorage();
      })
      
      // Check auth status cases
      .addCase(checkAuthStatus.pending, (state) => {
        state.loading = true;
      })
      .addCase(checkAuthStatus.fulfilled, (state, action: PayloadAction<User | null>) => {
        state.loading = false;
        if (action.payload) {
          state.isAuthenticated = true;
          state.user = action.payload;
          state.username = action.payload.fullName || action.payload.email;
        }
      })
      .addCase(checkAuthStatus.rejected, (state) => {
        state.loading = false;
        state.isAuthenticated = false;
        state.user = null;
        state.username = null;
      })
      
      // Register cases
      .addCase(register.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(register.rejected, (state, action) => {
        state.loading = false;
        state.error = (action.payload as string) || action.error.message || null;
      });
  }
});

export const { clearError } = authSlice.actions;
export default authSlice.reducer;
