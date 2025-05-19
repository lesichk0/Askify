import React, { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks';
import { getConsultationById } from '../features/consultations/consultationsSlice';

const ConsultationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { currentConsultation, loading, error } = useAppSelector(state => state.consultations);
  const { isAuthenticated } = useAppSelector(state => state.auth);
  
  useEffect(() => {
    if (id) {
      dispatch(getConsultationById(parseInt(id)));
    }
  }, [dispatch, id]);
  
  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }
  
  // Update the error display section
  if (error) {
    const is403 = error.toString().includes("permission") || error.toString().includes("Access Denied");
    
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="mb-6">
          <button 
            onClick={() => navigate(-1)}
            className="flex items-center text-amber-600 hover:text-amber-800"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clipRule="evenodd" />
            </svg>
            Back
          </button>
        </div>
        
        <div className="bg-red-50 border-l-4 border-red-500 p-8 rounded-lg shadow-md">
          <h2 className="text-2xl font-bold text-red-700 mb-4">{is403 ? "Access Denied" : "Error"}</h2>
          <p className="text-gray-700 mb-6">{error}</p>
          
          {is403 && (
            <div className="mt-4 space-y-4">
              <p className="text-gray-700">This could be due to one of the following reasons:</p>
              <ul className="list-disc pl-5 text-gray-700 space-y-2">
                <li>You are not logged in</li>
                <li>This consultation is private</li>
                <li>Only completed consultations with public visibility can be viewed</li>
                <li>You need to be the owner or expert for this consultation</li>
              </ul>
              
              <div className="flex flex-wrap gap-4 mt-6">
                {!isAuthenticated && (
                  <button 
                    onClick={() => navigate('/login')}
                    className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded transition"
                  >
                    Log In
                  </button>
                )}
                <button 
                  onClick={() => navigate('/consultations')}
                  className="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded transition"
                >
                  Browse Public Consultations
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    );
  }
  
  if (!currentConsultation) {
    return (
      <div className="text-center py-10">
        <h2 className="text-2xl font-bold text-gray-800 mb-4">Consultation Not Found</h2>
        <p className="text-gray-600 mb-6">The consultation you're looking for doesn't exist or has been removed.</p>
        <button 
          onClick={() => navigate('/consultations')}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded transition"
        >
          Back to Consultations
        </button>
      </div>
    );
  }
  
  const { 
    title, 
    description, 
    status, 
    expertName, 
    expertAvatar,
    createdAt, 
    completedAt 
  } = currentConsultation;
  
  // Format dates
  const formattedCreatedAt = new Date(createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
  
  const formattedCompletedAt = completedAt ? new Date(completedAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }) : null;
  
  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="mb-6">
        <button 
          onClick={() => navigate(-1)}
          className="flex items-center text-amber-600 hover:text-amber-800"
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
            <path fillRule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clipRule="evenodd" />
          </svg>
          Back
        </button>
      </div>
      
      <div className="bg-white shadow-md rounded-lg overflow-hidden">
        <div className="p-6">
          <div className="flex justify-between items-start mb-6">
            <h1 className="text-3xl font-bold text-gray-800">{title}</h1>
            <span className={`px-4 py-1 rounded-full text-sm font-medium ${
              status === 'completed' ? 'bg-green-100 text-green-800' :
              status === 'pending' ? 'bg-yellow-100 text-yellow-800' :
              status === 'accepted' ? 'bg-blue-100 text-blue-800' :
              'bg-gray-100 text-gray-800'
            }`}>
              {status.charAt(0).toUpperCase() + status.slice(1)}
            </span>
          </div>
          
          <div className="mb-8">
            <p className="text-gray-700 whitespace-pre-line">{description}</p>
          </div>
          
          <div className="flex items-center mb-6">
            {expertAvatar ? (
              <img 
                src={expertAvatar} 
                alt={expertName} 
                className="w-12 h-12 rounded-full mr-4"
              />
            ) : (
              <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mr-4 border border-amber-300">
                <span className="text-amber-700 font-semibold">
                  {expertName ? expertName.charAt(0) : 'E'}
                </span>
              </div>
            )}
            <div>
              <h3 className="text-lg font-medium text-stone-800">Expert: {expertName}</h3>
              {status === 'completed' && (
                <p className="text-sm text-stone-500">
                  Completed on {formattedCompletedAt}
                </p>
              )}
            </div>
          </div>
          
          <div className="border-t border-gray-200 pt-4">
            <p className="text-sm text-gray-500">Created: {formattedCreatedAt}</p>
          </div>
          
          {isAuthenticated && status === 'completed' && (
            <div className="mt-8">
              <h3 className="text-xl font-semibold mb-4">Ask a follow-up question</h3>
              <textarea 
                className="w-full border border-gray-300 rounded p-3 h-32 focus:outline-none focus:ring-2 focus:ring-amber-500"
                placeholder="Type your question..."
              ></textarea>
              <button 
                className="mt-2 bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              >
                Submit Question
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConsultationDetailPage;
