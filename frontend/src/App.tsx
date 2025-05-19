import { useAppDispatch, useAppSelector } from './hooks.ts'
import { login, logout } from './features/auth/authSlice'

function App() {
  const dispatch = useAppDispatch()
  const { isAuthenticated, username } = useAppSelector(state => state.auth)

  return (
    <div className="text-center p-10">
      <h1 className="text-3xl font-bold mb-4">
        {isAuthenticated ? `Welcome, ${username}!` : 'Not logged in'}
      </h1>
      <button
        className="bg-blue-500 text-white px-4 py-2 rounded"
        onClick={() => dispatch(isAuthenticated ? logout() : login('Olesia'))}
      >
        {isAuthenticated ? 'Logout' : 'Login'}
      </button>
    </div>
  )
}
export default App