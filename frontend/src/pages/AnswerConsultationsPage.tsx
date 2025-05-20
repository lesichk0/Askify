import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import api from '../api/api';

interface Consultation {
  id: number;
  title: string;
  description: string;
  status: string;
  isFree: boolean;
  isPublicable: boolean;
  createdAt: string;
}

const AnswerConsultationsPage: React.FC = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAppSelector(state => state.auth);
  
  const [consultations, setConsultations] = useState<Consultation[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    // Redirect if not authenticated or not an expert
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    
    if (user?.role !== 'Expert') {
      navigate('/');
      return;
    }
    
    const fetchPublicableConsultations = async () => {
      setLoading(true);
      try {
        // Fetch only pending and publicable consultations
        const response = await api.get('/consultations/publicable-pending');
        setConsultations(response.data);
      } catch (err: any) {
        console.error('Error fetching publicable consultations:', err);
        setError('Failed to load consultations');
      } finally {
        setLoading(false);
      }
    };
    
    fetchPublicableConsultations();
  }, [isAuthenticated, navigate, user]);
  
  const handleViewConsultation = (id: number) => {
    navigate(`/consultations/${id}`);
  };
  
  // Format date helper function
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };
  
  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Answer Public Consultations</h1>
      <p className="text-gray-600 mb-8">
        These are pending consultations that users have made publicly available for expert responses.
      </p>
      
      {loading ? (
        <div className="flex justify-center py-20">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
        </div>
      ) : error ? (
        <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-8">
          <p className="text-red-700">{error}</p>
        </div>
      ) : consultations.length === 0 ? (
        <div className="bg-gray-50 p-8 rounded-lg text-center">
          <svg xmlns="http://www.w3.org/2000/svg" className="h-16 w-16 mx-auto text-gray-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h2 className="text-xl font-semibold text-gray-700 mb-2">No Consultations to Answer</h2>
          <p className="text-gray-500">There are currently no public consultations to answer.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {consultations.map((consultation) => (
            <div 
              key={consultation.id}
              className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition"
            >
              <div className="p-6">
                <div className="flex justify-between items-start mb-4">
                  <h2 className="text-xl font-bold text-gray-800 line-clamp-2">{consultation.title}</h2>
                  <span className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs font-medium rounded-full">
                    {consultation.status}
                  </span>
                </div>
                
                <p className="text-gray-600 mb-4 line-clamp-3">{consultation.description}</p>
                
                <div className="flex justify-between items-center mb-4">
                  <span className="text-sm text-gray-500">Created: {formatDate(consultation.createdAt)}</span>
                  
                  {consultation.isFree ? (
                    <span className="bg-green-100 text-green-800 text-xs px-2 py-1 rounded-full">Free</span>
                  ) : (
                    <span className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full">Paid</span>
                  )}
                </div>
                
                <button
                  onClick={() => handleViewConsultation(consultation.id)}
                  className="w-full bg-amber-600 hover:bg-amber-700 text-white py-2 rounded-md transition"
                >
                  View & Answer
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default AnswerConsultationsPage;
