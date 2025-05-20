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
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await api.get(`/consultations/${id}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch consultation');
    }
  }
);

// Async thunk for fetching consultations by user ID
export const getConsultationsByUserId = createAsyncThunk(
  'consultations/getByUserId',
  async (userId: string, { rejectWithValue }) => {
    try {
      const response = await api.get(`/consultations/user/${userId}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user consultations');
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
        state.currentConsultation = action.payload;
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
      });
  }
});

export const { clearConsultationError, clearCurrentConsultation } = consultationsSlice.actions;
export default consultationsSlice.reducer;
