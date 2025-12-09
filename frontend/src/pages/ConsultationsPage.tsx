import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations, getConsultationsByUserId, fetchOpenConsultationRequests, deleteConsultation } from '../features/consultations/consultationsSlice';
import ConsultationCard from '../components/ConsultationCard';
import { webSocketService } from '../services/WebSocketService';

interface ConsultationsPageProps {
  expertView?: boolean;
  showMine?: boolean;
}

const ConsultationsPage: React.FC<ConsultationsPageProps> = ({ 
  expertView = false, 
  showMine = false 
}) => {
  const dispatch = useAppDispatch();
  const { consultations, loading, error } = useAppSelector(state => state.consultations);
  const { user, isAuthenticated } = useAppSelector(state => state.auth);
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [deleteConfirmId, setDeleteConfirmId] = useState<number | null>(null);

  const handleDeleteClick = (id: number) => {
    setDeleteConfirmId(id);
  };

  const handleDeleteConfirm = async () => {
    if (deleteConfirmId) {
      await dispatch(deleteConsultation(deleteConfirmId));
      setDeleteConfirmId(null);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmId(null);
  };
  
  useEffect(() => {
    const fetchAppropriateConsultations = async () => {
      if (!dispatch) return;
      
      try {
        if (showMine && user?.id) {
          dispatch(getConsultationsByUserId(user.id));
        } else if (expertView) {
          dispatch(fetchOpenConsultationRequests());
        } else {
          dispatch(fetchConsultations());
        }
      } catch (err) {
        console.error('Error fetching consultations:', err);
      }
    };

    fetchAppropriateConsultations();
    
    if (isAuthenticated && (showMine || expertView)) {
      const setupWebSocket = async () => {
        try {
          if (!webSocketService.isConsultationConnected()) {
            await webSocketService.initializeConsultationConnection();
          }
          
          webSocketService.onNewConsultationRequestCallback(() => {
            fetchAppropriateConsultations();
          });
          
          webSocketService.onConsultationUpdatedCallback(() => {
            fetchAppropriateConsultations();
          });
          
          webSocketService.onConsultationCompletedCallback(() => {
            fetchAppropriateConsultations();
          });
        } catch (error) {
          console.error('WebSocket setup failed:', error);
        }
      };
      
      setupWebSocket();
    }
  }, [dispatch, showMine, expertView, user, isAuthenticated]);
  
  // Filter consultations based on the view
  // This provides a client-side backup in case server filtering doesn't work
  const filteredConsultations = React.useMemo(() => {
    // First handle null consultations
    if (!consultations || !Array.isArray(consultations)) {
      return [];
    }
    
    let result: typeof consultations = [];
    
    if (showMine && user) {
      if (user.role === 'User') {
        // Users see consultations they created
        result = consultations.filter(c => c.userId === user.id);
      } else if (user.role === 'Expert') {
        // Experts see consultations assigned to them
        result = consultations.filter(c => c.expertId === user.id);
      }
    } else if (expertView) {
      // Available open requests
      result = consultations.filter(c => c.isOpenRequest && c.status?.toLowerCase() === 'pending');
    } else {
      // Public view - only publicable consultations
      result = consultations.filter(c => c.isPublicable === true);
    }
    
    // Sort by createdAt descending (newest first)
    return result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }, [consultations, showMine, expertView, user]);

  // Apply category filter
  const categoryFilteredConsultations = React.useMemo(() => {
    if (selectedCategory === 'All') {
      return filteredConsultations;
    }
    return filteredConsultations.filter(c => c.category === selectedCategory);
  }, [filteredConsultations, selectedCategory]);

  // Get unique categories from current consultations for the filter
  const availableCategories = React.useMemo(() => {
    const categories = new Set<string>();
    filteredConsultations.forEach(c => {
      if (c.category) {
        categories.add(c.category);
      }
    });
    return ['All', ...Array.from(categories).sort()];
  }, [filteredConsultations]);
  
  // Format date helper function
  const formatDate = (dateString: string) => {
    if (!dateString) return 'Date unavailable';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };
  
  if (loading) return <div className="text-center py-10">Loading consultations...</div>;
  if (error) return <div className="text-red-500 text-center py-10">Error: {error}</div>;
  
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">
        {showMine 
          ? (user?.role === 'User' ? 'My Consultation Requests' : 'My Assigned Consultations')
          : expertView 
            ? 'Available Consultation Requests' 
            : 'Public Consultations'
        }
      </h1>

      {/* Category Filter */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">Filter by Category:</label>
        <div className="flex flex-wrap gap-2">
          {availableCategories.map((category) => (
            <button
              key={category}
              onClick={() => setSelectedCategory(category)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                selectedCategory === category
                  ? 'bg-amber-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {category}
              {category !== 'All' && (
                <span className="ml-1 text-xs opacity-75">
                  ({filteredConsultations.filter(c => c.category === category).length})
                </span>
              )}
            </button>
          ))}
        </div>
      </div>
      
      {/* Add Create button for users */}
      {user?.role === 'User' && showMine && (
        <div className="mb-6">
          <Link 
            to="/consultations/new"
            className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded inline-flex items-center"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2" viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M10 5a1 1 0 011 1v3h3a1 1 0 110 2h-3v3a1 1 0 11-2 0v-3H6a1 1 0 110-2h3V6a1 1 0 011-1z" clipRule="evenodd" />
            </svg>
            Request New Consultation
          </Link>
        </div>
      )}
      
      {categoryFilteredConsultations.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 rounded-lg">
          <p className="text-lg text-gray-600">
            {selectedCategory !== 'All' 
              ? `No consultations found in "${selectedCategory}" category.`
              : showMine 
                ? 'You have no consultations yet.' 
                : expertView 
                  ? 'No open consultation requests available.' 
                  : 'No public consultations available at the moment.'
            }
          </p>
          
          {selectedCategory !== 'All' && (
            <button 
              onClick={() => setSelectedCategory('All')}
              className="mt-4 text-amber-600 hover:text-amber-800 font-medium"
            >
              Clear filter
            </button>
          )}
          
          {user?.role === 'User' && showMine && selectedCategory === 'All' && (
            <Link 
              to="/consultations/new"
              className="mt-4 bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded inline-block"
            >
              Request Your First Consultation
            </Link>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {categoryFilteredConsultations.map((consultation) => (
                <ConsultationCard 
                    key={consultation.id}
                    id={consultation.id}
                    title={consultation.title || 'Untitled Consultation'}
                    expertName={consultation.expertName || 'Unknown Expert'}
                    description={consultation.description || 'No description available.'}
                    status={consultation.status as any}
                    category={consultation.category}
                    date={formatDate(consultation.createdAt)}
                    showDelete={showMine && user?.role === 'User'}
                    onDelete={handleDeleteClick}
                />
            ))}
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteConfirmId && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md mx-4">
            <h3 className="text-lg font-bold text-gray-800 mb-4">Delete Consultation?</h3>
            <p className="text-gray-600 mb-6">
              Are you sure you want to delete this consultation? This action cannot be undone.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={handleDeleteCancel}
                className="px-4 py-2 text-gray-600 hover:text-gray-800 transition"
              >
                Cancel
              </button>
              <button
                onClick={handleDeleteConfirm}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded transition"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConsultationsPage;
