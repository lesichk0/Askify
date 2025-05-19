// src/features/auth/authSlice.ts
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import api from '../../api/api';

// Define response types for better type safety
interface User {
  id: string;
  fullName: string;
  email: string;
  role: string; // Add role property
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
      
      // Use inline type instead of referencing LoginResponse
      return response.data as { user: User; token: string };
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

export const checkAuthStatus = createAsyncThunk(
  'auth/checkStatus',
  async (_, { rejectWithValue }) => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    
    try {
      const response = await api.get('/auth/me');
      return response.data;
    } catch (error: any) {
      // If token is invalid, logout
      localStorage.removeItem('token');
      return rejectWithValue(error.response?.data?.message || 'Authentication failed');
    }
  }
);

interface RegisterData {
  email: string;
  password: string;
  fullName: string;
}

export const register = createAsyncThunk(
  'auth/register',
  async (registerData: RegisterData, { rejectWithValue }) => {
    try {
      console.log('Registration data:', registerData);
      
      // Try the exact format your backend expects
      const payload = {
        email: registerData.email,
        password: registerData.password,
        confirmPassword: registerData.password, // Backend might require this
        fullName: registerData.fullName
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

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  username: string | null;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  isAuthenticated: false,
  user: null,
  username: null,
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
      .addCase(login.fulfilled, (state, action: PayloadAction<any>) => {
        state.isAuthenticated = true;
        
        // Handle different response structures
        if (action.payload.user) {
          // Response has a user object as expected
          state.user = action.payload.user;
          state.username = action.payload.user.fullName || action.payload.user.email;
        } else {
          // Response might be the user object directly or have a different structure
          // Try to extract user information from the response
          const userData = action.payload;
          state.user = {
            id: userData.id || '',
            email: userData.email || '',
            fullName: userData.fullName || userData.name || userData.email || '',
            role: userData.role || 'User'
          };
          state.username = userData.fullName || userData.name || userData.email || '';
        }
        
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
