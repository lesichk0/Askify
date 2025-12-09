import React, { useState, useRef, useEffect } from 'react';
import { useAppSelector, useAppDispatch } from '../hooks';
import { logout } from '../features/auth/authSlice';
import { fetchNotifications, markAllNotificationsAsRead, markNotificationAsRead } from '../features/notifications/notificationsSlice';
import { useNavigate, Link } from 'react-router-dom';

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
    navigate('/');
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
          <Link to="/" className="text-2xl font-bold">Askify</Link>
          <nav className="ml-8 hidden md:block">
            <ul className="flex space-x-6">
              <li><Link to="/" className="hover:text-yellow-200 transition">Home</Link></li>
              <li><Link to="/consultations" className="hover:text-yellow-200 transition">Consultations</Link></li>
              <li><Link to="/blog" className="hover:text-yellow-200 transition">Blog</Link></li>
              
              {/* Expert-specific navigation items */}
              {isAuthenticated && user?.role === 'Expert' && (
                <li>
                  <Link to="/my-consultations" className="hover:text-yellow-200 transition">
                    My Consultations
                  </Link>
                </li>
              )}
              
              {/* User-specific navigation items */}
              {isAuthenticated && user?.role === 'User' && (
                <>
                  <li>
                    <Link to="/consultations/new" className="hover:text-yellow-200 transition">
                      Request Consultation
                    </Link>
                  </li>
                  <li>
                    <Link to="/my-consultations" className="hover:text-yellow-200 transition">
                      My Consultations
                    </Link>
                  </li>
                </>
              )}
              
              <li><Link to="/experts" className="hover:text-yellow-200 transition">Find Experts</Link></li>
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
                        <Link to="/notifications" className="text-xs text-amber-600 hover:text-amber-800">
                          View All
                        </Link>
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
                  <Link 
                    to="/profile" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Profile
                  </Link>
                  <Link 
                    to="/my-consultations" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    {user?.role === 'Expert' ? 'My Assigned Consultations' : 'My Consultations'}
                  </Link>
                  
                  {user?.role === 'User' && (
                    <Link 
                      to="/consultations/new" 
                      className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                    >
                      Request Consultation
                    </Link>
                  )}
                  
                  <Link 
                    to="/notifications" 
                    className="block px-4 py-2 text-gray-800 hover:bg-amber-100"
                  >
                    Notifications
                  </Link>
                  
                  <button 
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 text-gray-800 hover:bg-amber-100 cursor-pointer"
                  >
                    Logout
                  </button>
                </div>
              </div>
            </>
          ) : (
            <> 
              <Link to="/login" className="hover:text-yellow-200 transition">Login</Link>
              <Link 
                to="/register" 
                className="bg-yellow-600 hover:bg-yellow-700 px-4 py-2 rounded-md transition"
              >
                Register
              </Link>
            </>
          )}
        </div>
      </div>
      
      {/* Mobile Navigation */}
      <div className="md:hidden border-t border-amber-800 py-2">
        <ul className="flex justify-around">
          <li><Link to="/" className="hover:text-yellow-200 transition">Home</Link></li>
          
          <li><Link to="/consultations" className="hover:text-yellow-200 transition">Consult</Link></li>
          
          <li><Link to="/experts" className="hover:text-yellow-200 transition">Experts</Link></li>
          <li><Link to="/notifications" className="hover:text-yellow-200 transition">Alerts</Link></li>
        </ul>
      </div>
    </header>
  );
};

export default Header;
