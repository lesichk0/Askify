// src/app/store.ts
import { configureStore } from '@reduxjs/toolkit'
import authReducer from '../features/auth/authSlice'
import consultationsReducer from '../features/consultations/consultationsSlice'

export const store = configureStore({
  reducer: {
    auth: authReducer,
    consultations: consultationsReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
