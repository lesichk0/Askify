import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import api from '../../api/api';

// Define the consultation interface to match the API response
export interface Consultation {
  id: number;
  userId: string;
  expertId: string;
  expertName?: string;
  title: string;
  description: string;
  category?: string; // ML-classified category
  isFree: boolean;
  isOpenRequest: boolean;
  isPublicable: boolean;
  status: string;
  createdAt: string;
  completedAt?: string;
}

interface ConsultationsState {
  consultations: Consultation[];
  currentConsultation: Consultation | null;
  loading: boolean;
  error: string | null;
}

const initialState: ConsultationsState = {
  consultations: [],
  currentConsultation: null,
  loading: false,
  error: null
};

// Async thunk for fetching public consultations
export const fetchConsultations = createAsyncThunk(
  'consultations/fetchPublic',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get('/Consultations');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch public consultations');
    }
  }
);

// Keep the old name for backward compatibility


// Async thunk for fetching consultation by ID
export const getConsultationById = createAsyncThunk(
  'consultations/getById',
  async (consultationId: number, { rejectWithValue }) => {
    try {
      // Add more detailed logging
      console.log(`Making API request to get consultation #${consultationId}`);
      
      // Include a timeout to ensure we get a response
      const response = await api.get(`/consultations/${consultationId}`, {
        timeout: 30000  // 30 seconds timeout
      });
      
      console.log('API response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('Error fetching consultation:', error);
      
      // More detailed error logging
      if (error.response) {
        console.error(`Status: ${error.response.status}`);
        console.error('Headers:', error.response.headers);
        console.error('Data:', error.response.data);
        
        // Handle specific error codes
        if (error.response.status === 403) {
          return rejectWithValue('You do not have permission to view this consultation. If you are an expert, please ensure your role is set correctly.');
        }
      }
      
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch consultation details');
    }
  }
);

// Async thunk for fetching consultations by user ID
export const getConsultationsByUserId = createAsyncThunk(
  'consultations/getByUserId',
  async (userId: string, { rejectWithValue }) => {
    try {
      const response = await api.get(`/Consultations/user/${userId}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user consultations');
    }
  }
);

// Async thunk for fetching open consultation requests
export const fetchOpenConsultationRequests = createAsyncThunk(
  'consultations/fetchOpenRequests',
  async (_, { rejectWithValue }) => {
    try {
      // Use the expert-specific endpoint for available consultations
      const response = await api.get('/Consultations/expert/available');
      return response.data;
    } catch (error: any) {
      console.error('Error fetching open requests:', error);
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch open requests');
    }
  }
);

// Async thunk for deleting a consultation
export const deleteConsultation = createAsyncThunk(
  'consultations/delete',
  async (consultationId: number, { rejectWithValue }) => {
    try {
      await api.delete(`/Consultations/${consultationId}`);
      return consultationId;
    } catch (error: any) {
      console.error('Error deleting consultation:', error);
      return rejectWithValue(error.response?.data?.message || 'Failed to delete consultation');
    }
  }
);

const consultationsSlice = createSlice({
  name: 'consultations',
  initialState,
  reducers: {
    clearConsultationError: (state) => {
      state.error = null;
    },
    clearCurrentConsultation: (state) => {
      state.currentConsultation = null;
    }
  },
  extraReducers: (builder) => {
    builder
      // fetchPublicConsultations
      .addCase(fetchConsultations.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchConsultations.fulfilled, (state, action) => {
        state.loading = false;
        state.consultations = action.payload;
      })
      .addCase(fetchConsultations.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      
      // getConsultationById
      .addCase(getConsultationById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getConsultationById.fulfilled, (state, action) => {
        state.loading = false;
        state.error = null;
        // Ensure we're setting the full consultation data
        state.currentConsultation = action.payload;
        console.log('Updated current consultation in store:', action.payload);
      })
      .addCase(getConsultationById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      
      // getConsultationsByUserId
      .addCase(getConsultationsByUserId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getConsultationsByUserId.fulfilled, (state, action) => {
        state.loading = false;
        state.consultations = action.payload;
      })
      .addCase(getConsultationsByUserId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      
      // fetchOpenConsultationRequests
      .addCase(fetchOpenConsultationRequests.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchOpenConsultationRequests.fulfilled, (state, action) => {
        state.loading = false;
        state.consultations = action.payload;
      })
      .addCase(fetchOpenConsultationRequests.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      
      // deleteConsultation
      .addCase(deleteConsultation.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteConsultation.fulfilled, (state, action) => {
        state.loading = false;
        state.consultations = state.consultations.filter(c => c.id !== action.payload);
      })
      .addCase(deleteConsultation.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  }
});

export const { clearConsultationError, clearCurrentConsultation } = consultationsSlice.actions;
export default consultationsSlice.reducer;
