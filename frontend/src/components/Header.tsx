import React from 'react';
import { useAppSelector, useAppDispatch } from '../hooks';
import { logout } from '../features/auth/authSlice';

const Header: React.FC = () => {
  const dispatch = useAppDispatch();
  const { isAuthenticated, username } = useAppSelector(state => state.auth);
  
  const handleLogout = () => {
    dispatch(logout());
  };

  return (
    <header className="bg-amber-700 text-white shadow-md">
      <div className="container mx-auto px-4 py-4 flex flex-col md:flex-row justify-between items-center">
        <div className="flex items-center mb-4 md:mb-0">
          <a href="/" className="text-2xl font-bold">Askify</a>
          <nav className="ml-8 hidden md:block">
            <ul className="flex space-x-6">
              <li><a href="/" className="hover:text-yellow-200 transition">Home</a></li>
              <li><a href="/consultations" className="hover:text-yellow-200 transition">Consultations</a></li>
              <li><a href="/experts" className="hover:text-yellow-200 transition">Find Experts</a></li>
              <li><a href="/questions" className="hover:text-yellow-200 transition">Q&A</a></li>
            </ul>
          </nav>
        </div>
        
        <div className="flex items-center space-x-4">
          {isAuthenticated ? (
            <>
              <div className="relative group">
                <div className="flex items-center cursor-pointer">
                  <div className="w-8 h-8 rounded-full bg-amber-200 flex items-center justify-center mr-2">
                    <span className="text-amber-800 font-semibold">
                      {username?.charAt(0) || 'U'}
                    </span>
                  </div>
                  <span className="text-yellow-100">{username}</span>
                </div>
                
                <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-10 hidden group-hover:block">
                  <a 
                    href="/profile" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Profile
                  </a>
                  <a 
                    href="/my-consultations" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    My Consultations
                  </a>
                  <a 
                    href="/settings" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Settings
                  </a>
                  <button 
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Logout
                  </button>
                </div>
              </div>
            </>
          ) : (
            <>
              <a href="/login" className="hover:text-yellow-200 transition">Login</a>
              <a 
                href="/register" 
                className="bg-yellow-600 hover:bg-yellow-700 px-4 py-2 rounded-md transition"
              >
                Register
              </a>
            </>
          )}
        </div>
      </div>
      
      {/* Mobile Navigation */}
      <div className="md:hidden border-t border-amber-800 py-2">
        <ul className="flex justify-around">
          <li><a href="/" className="hover:text-yellow-200 transition">Home</a></li>
          <li><a href="/consultations" className="hover:text-yellow-200 transition">Consult</a></li>
          <li><a href="/experts" className="hover:text-yellow-200 transition">Experts</a></li>
          <li><a href="/questions" className="hover:text-yellow-200 transition">Q&A</a></li>
        </ul>
      </div>
    </header>
  );
};

export default Header;
