import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import api from '../../api/api';
import type { Consultation } from '../../types/consultation';

// Real API calls instead of mock data
export const fetchConsultations = createAsyncThunk<Consultation[]>(
  'consultations/fetchAll',
  async (_, { rejectWithValue }) => {
    try {
      const response = await api.get('/consultations');
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch consultations');
    }
  }
);

export const getConsultationById = createAsyncThunk<Consultation, number>(
  'consultations/getById',
  async (id, { rejectWithValue }) => {
    try {
      // First try the public endpoint
      try {
        const response = await api.get(`/consultations/public/${id}`);
        return response.data;
      } catch (publicError) {
        // If public endpoint fails, try the authenticated endpoint
        const response = await api.get(`/consultations/${id}`);
        return response.data;
      }
    } catch (error: unknown) {
      // Properly type the error object
      console.error('Error fetching consultation:', error);
      
      let errorMessage = 'Failed to fetch consultation details';
      
      // Type guard for axios error structure
      if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as { 
          response?: { 
            status?: number; 
            data?: { message?: string } 
          } 
        };
        
        // Now we can safely access properties
        if (axiosError.response?.status === 500) {
          errorMessage = 'Server error occurred. Please try again later.';
        } else if (axiosError.response?.status === 404) {
          errorMessage = 'Consultation not found';
        } else if (axiosError.response?.data?.message) {
          errorMessage = axiosError.response.data.message;
        }
      }
      
      return rejectWithValue(errorMessage);
    }
  }
);

// Add a specific endpoint for public consultations
export const fetchPublicConsultations = createAsyncThunk<Consultation[]>(
  'consultations/fetchPublic',
  async (_, { rejectWithValue }) => {
    try {
      // Get only public consultations that anyone can view
      const response = await api.get('/consultations/public');
      return response.data;
    } catch (error: any) {
      console.error('Error fetching public consultations:', error.response || error);
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch public consultations');
    }
  }
);

export const createConsultation = createAsyncThunk<number, Partial<Consultation>>(
  'consultations/create',
  async (consultationData, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.post('/consultations', consultationData);
      // Refresh the consultations list after creating a new one
      dispatch(fetchConsultations());
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create consultation');
    }
  }
);

export const updateConsultation = createAsyncThunk<boolean, { id: number, data: Partial<Consultation> }>(
  'consultations/update',
  async ({ id, data }, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.put(`/consultations/${id}`, data);
      // Refresh the consultations list after updating
      dispatch(fetchConsultations());
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update consultation');
    }
  }
);

export const deleteConsultation = createAsyncThunk<boolean, number>(
  'consultations/delete',
  async (id, { rejectWithValue, dispatch }) => {
    try {
      const response = await api.delete(`/consultations/${id}`);
      // Refresh the consultations list after deleting
      dispatch(fetchConsultations());
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete consultation');
    }
  }
);

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

const consultationsSlice = createSlice({
  name: 'consultations',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      // Fetch all consultations
      .addCase(fetchConsultations.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchConsultations.fulfilled, (state, action: PayloadAction<Consultation[]>) => {
        state.loading = false;
        state.consultations = action.payload;
      })
      .addCase(fetchConsultations.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch consultations';
      })
      
      // Get consultation by ID
      .addCase(getConsultationById.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getConsultationById.fulfilled, (state, action: PayloadAction<Consultation>) => {
        state.loading = false;
        state.currentConsultation = action.payload;
      })
      .addCase(getConsultationById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string || action.error.message || 'Failed to fetch consultation';
      });
      
      // Note: We don't need to handle the other actions in the reducer
      // since we're refreshing the list after each operation
  }
});

export const { clearError } = consultationsSlice.actions;
export default consultationsSlice.reducer;
