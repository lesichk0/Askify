import React, { useState, useRef, useEffect } from 'react';
import { useAppSelector, useAppDispatch } from '../hooks';
import { logout } from '../features/auth/authSlice';
import { fetchNotifications, markAllNotificationsAsRead, markNotificationAsRead } from '../features/notifications/notificationsSlice';
import { useNavigate } from 'react-router-dom';

const Header: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, username, user } = useAppSelector(state => state.auth);
  const { notifications, unreadCount } = useAppSelector(state => state.notifications);
  
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [notificationsOpen, setNotificationsOpen] = useState(false);
  const timeoutRef = useRef<number | null>(null);
  const notificationTimeoutRef = useRef<number | null>(null);
  
  // Fetch notifications when user is authenticated
  useEffect(() => {
    if (isAuthenticated) {
      dispatch(fetchNotifications());
      
      // Set up polling for new notifications every 30 seconds
      const interval = setInterval(() => {
        if (isAuthenticated) {
          dispatch(fetchNotifications());
        }
      }, 30000);
      
      return () => clearInterval(interval);
    }
  }, [isAuthenticated, dispatch]);
  
  const handleLogout = () => {
    dispatch(logout());
  };

  const handleMouseEnter = () => {
    if (timeoutRef.current) {
      window.clearTimeout(timeoutRef.current);
      timeoutRef.current = null;
    }
    setDropdownOpen(true);
  };

  const handleMouseLeave = () => {
    // Set a timeout to close the dropdown after 1 second
    timeoutRef.current = window.setTimeout(() => {
      setDropdownOpen(false);
    }, 1000); // 1 second delay
  };
  
  const handleNotificationClick = () => {
    setNotificationsOpen(!notificationsOpen);
  };
  
  const handleNotificationMouseEnter = () => {
    if (notificationTimeoutRef.current) {
      window.clearTimeout(notificationTimeoutRef.current);
      notificationTimeoutRef.current = null;
    }
  };
  
  const handleNotificationMouseLeave = () => {
    notificationTimeoutRef.current = window.setTimeout(() => {
      setNotificationsOpen(false);
    }, 1000);
  };
  
  const handleNotificationItemClick = (notification: any) => {
    dispatch(markNotificationAsRead(notification.id));
    
    // Navigate to related content
    if (notification.relatedItemType && notification.relatedItemId) {
      navigate(`/${notification.relatedItemType.toLowerCase()}s/${notification.relatedItemId}`);
    }
    
    setNotificationsOpen(false);
  };

  // Clean up timeout when component unmounts
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        window.clearTimeout(timeoutRef.current);
      }
      if (notificationTimeoutRef.current) {
        window.clearTimeout(notificationTimeoutRef.current);
      }
    };
  }, []);

  return (
    <header className="bg-amber-700 text-white shadow-md">
      <div className="container mx-auto px-4 py-4 flex flex-col md:flex-row justify-between items-center">
        <div className="flex items-center mb-4 md:mb-0">
          <a href="/" className="text-2xl font-bold">Askify</a>
          <nav className="ml-8 hidden md:block">
            <ul className="flex space-x-6">
              <li><a href="/" className="hover:text-yellow-200 transition">Home</a></li>
              <li><a href="/consultations" className="hover:text-yellow-200 transition">Consultations</a></li>
              
              {/* Expert-specific navigation items */}
              {isAuthenticated && user?.role === 'Expert' && (
                <>
                  <li>
                    <a href="/consultation-requests" className="hover:text-yellow-200 transition">
                      Open Requests
                    </a>
                  </li>
                  <li>
                    <a href="/answer-consultations" className="hover:text-yellow-200 transition">
                      Answer Consultations
                    </a>
                  </li>
                  <li>
                    <a href="/my-consultations" className="hover:text-yellow-200 transition">
                      My Consultations
                    </a>
                  </li>
                </>
              )}
              
              {/* User-specific navigation items */}
              {isAuthenticated && user?.role === 'User' && (
                <li>
                  <a href="/consultations/new" className="hover:text-yellow-200 transition">
                    Request Consultation
                  </a>
                </li>
              )}
              
              <li><a href="/experts" className="hover:text-yellow-200 transition">Find Experts</a></li>
              <li><a href="/questions" className="hover:text-yellow-200 transition">Q&A</a></li>
            </ul>
          </nav>
        </div>
        
        <div className="flex items-center space-x-4">
          {isAuthenticated ? (
            <>
              {/* Notifications Icon */}
              <div 
                className="relative"
                onMouseEnter={handleNotificationMouseEnter}
                onMouseLeave={handleNotificationMouseLeave}
              >
                <button 
                  onClick={handleNotificationClick}
                  className="relative p-1 rounded-full hover:bg-amber-600 transition"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                  </svg>
                  
                  {/* Notification Badge */}
                  {unreadCount > 0 && (
                    <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs w-5 h-5 flex items-center justify-center rounded-full">
                      {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                  )}
                </button>
                
                {/* Notifications Dropdown */}
                {notificationsOpen && (
                  <div className="absolute right-0 mt-2 w-80 bg-white rounded-md shadow-lg overflow-hidden z-10">
                    <div className="px-4 py-2 border-b border-gray-100 flex justify-between items-center">
                      <h3 className="text-sm font-bold text-gray-800">Notifications</h3>
                      <div className="flex space-x-2">
                        <button 
                          onClick={() => dispatch(markAllNotificationsAsRead())}
                          className="text-xs text-amber-600 hover:text-amber-800"
                        >
                          Mark all as read
                        </button>
                        <a href="/notifications" className="text-xs text-amber-600 hover:text-amber-800">
                          View All
                        </a>
                      </div>
                    </div>
                    
                    <div className="max-h-80 overflow-y-auto">
                      {notifications.length === 0 ? (
                        <div className="px-4 py-8 text-center">
                          <p className="text-gray-500">No notifications yet</p>
                        </div>
                      ) : (
                        notifications.slice(0, 5).map((notification) => (
                          <div 
                            key={notification.id} 
                            onClick={() => handleNotificationItemClick(notification)}
                            className={`block px-4 py-3 border-b border-gray-100 hover:bg-gray-50 cursor-pointer 
                              ${!notification.isRead ? 'bg-amber-50' : ''}`}
                          >
                            <div className="flex">
                              <div className={`w-2 h-2 rounded-full mt-2 mr-2 flex-shrink-0 ${
                                !notification.isRead ? 'bg-amber-500' : 'bg-gray-300'
                              }`}></div>
                              <div>
                                <p className="text-sm font-medium text-gray-800">{notification.title}</p>
                                <p className="text-xs text-gray-500 mt-1">{notification.message}</p>
                                <p className="text-xs text-gray-400 mt-1">
                                  {new Date(notification.createdAt).toLocaleString()}
                                </p>
                              </div>
                            </div>
                          </div>
                        )))
                      }
                    </div>
                  </div>
                )}
              </div>
              
              <div 
                className="relative" 
                onMouseEnter={handleMouseEnter}
                onMouseLeave={handleMouseLeave}
              >
                <div className="flex items-center cursor-pointer">
                  <div className="w-8 h-8 rounded-full bg-amber-200 flex items-center justify-center mr-2">
                    <span className="text-amber-800 font-semibold">
                      {username?.charAt(0) || 'U'}
                    </span>
                  </div>
                  <span className="text-yellow-100">{username}</span>
                </div>
                
                <div 
                  className={`absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-10 transition-opacity duration-300 ${
                    dropdownOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'
                  }`}
                >
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
                    {user?.role === 'Expert' ? 'My Assigned Consultations' : 'My Consultations'}
                  </a>
                  
                  {user?.role === 'User' && (
                    <a 
                      href="/consultations/new" 
                      className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                    >
                      Request Consultation
                    </a>
                  )}
                  
                  {user?.role === 'Expert' && (
                    <>
                      <a 
                        href="/consultation-requests" 
                        className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                      >
                        Available Requests
                      </a>
                      <a 
                        href="/answer-consultations" 
                        className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                      >
                        Answer Consultations
                      </a>
                    </>
                  )}
                  
                  <a 
                    href="/notifications" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Notifications
                  </a>
                  
                  <a 
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Logout
                  </a>
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
          
          {isAuthenticated && user?.role === 'Expert' ? (
            <>
              <li><a href="/consultation-requests" className="hover:text-yellow-200 transition">Requests</a></li>
              <li><a href="/answer-consultations" className="hover:text-yellow-200 transition">Answer</a></li>
            </>
          ) : (
            <li><a href="/consultations" className="hover:text-yellow-200 transition">Consult</a></li>
          )}
          
          <li><a href="/experts" className="hover:text-yellow-200 transition">Experts</a></li>
          <li><a href="/notifications" className="hover:text-yellow-200 transition">Alerts</a></li>
        </ul>
      </div>
    </header>
  );
};

export default Header;
