import { useDispatch, useSelector } from 'react-redux'
import type { TypedUseSelectorHook } from 'react-redux'
import type { RootState, AppDispatch } from './app/store'

// Використовуй useAppDispatch замість useDispatch
export const useAppDispatch: () => AppDispatch = useDispatch

// Використовуй useAppSelector замість useSelector
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector
