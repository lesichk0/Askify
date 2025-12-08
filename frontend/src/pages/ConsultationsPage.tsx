import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations, getConsultationsByUserId, fetchOpenConsultationRequests } from '../features/consultations/consultationsSlice';
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
      
      {filteredConsultations.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 rounded-lg">
          <p className="text-lg text-gray-600">
            {showMine 
              ? 'You have no consultations yet.' 
              : expertView 
                ? 'No open consultation requests available.' 
                : 'No public consultations available at the moment.'
            }
          </p>
          
          {user?.role === 'User' && showMine && (
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
            {filteredConsultations.map((consultation) => (
                <ConsultationCard 
                    key={consultation.id}
                    id={consultation.id}
                    title={consultation.title || 'Untitled Consultation'}
                    expertName={consultation.expertName || 'Unknown Expert'}
                    description={consultation.description || 'No description available.'}
                    status={consultation.status as any}
                    category={consultation.category}
                    date={formatDate(consultation.createdAt)}
                />
            ))}
        </div>
      )}
    </div>
  );
};

export default ConsultationsPage;
