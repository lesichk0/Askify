import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks';
import { getConsultationById } from '../features/consultations/consultationsSlice';
import api from '../api/api';

// Define additional interfaces to fix type errors
interface ExpertOffer {
  expertId: string;
  expertName: string;
  message: string;
  createdAt: string;
}

interface Message {
  id: number;
  consultationId: number;
  userId: string;
  content: string;
  createdAt: string;
}

// Extend consultation interface to include missing properties
interface ExtendedConsultation {
  id: number;
  title: string;
  description: string;
  status: string;
  userId: string;
  expertId?: string;
  expertName?: string;
  createdAt: string;
  completedAt?: string;
  isPublicable: boolean;
  isFree: boolean;
  isOpenRequest: boolean;
  expertOffer?: ExpertOffer;
  messages?: Message[];
}

const ConsultationDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { currentConsultation, loading, error } = useAppSelector(state => state.consultations);
  const { isAuthenticated, user } = useAppSelector(state => state.auth);
  const userRole = user?.role || null;
  const [responseText, setResponseText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [responseError, setResponseError] = useState<string | null>(null);
  
  // Cast currentConsultation to our extended type
  const typedConsultation = currentConsultation as ExtendedConsultation;
  
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
  
  // Before rendering the main content, add proper null checks and default values
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
  
  // Safely destructure properties with default values to avoid undefined errors
  const { 
    title = 'Untitled Consultation', 
    description = 'No description available', 
    status = 'pending', 
    expertName = '',
    createdAt = new Date().toISOString(), 
    completedAt = undefined,
    // Add other properties you need with defaults
  } = currentConsultation || {};
  
  // Format dates safely
  const formattedCreatedAt = createdAt ? new Date(createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }) : 'Unknown date';
  
  const formattedCompletedAt = completedAt ? new Date(completedAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }) : null;
  
  const handleAcceptConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/accept`);
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
    } catch (error) {
      console.error('Error accepting consultation:', error);
    }
  };
  
  const handleCompleteConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/complete`);
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
    } catch (error) {
      console.error('Error completing consultation:', error);
    }
  };
  
  const handleRespondToConsultation = async () => {
    if (!id || !responseText.trim()) return;
    
    setSubmitting(true);
    setResponseError(null);
    
    try {
      // Send the response as a message
      await api.post('/messages', {
        consultationId: parseInt(id),
        content: responseText
      });
      
      // Clear the response text
      setResponseText('');
      // Show success message
      alert('Response sent successfully!');
      
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
    } catch (err: any) {
      console.error('Error responding to consultation:', err);
      setResponseError('Failed to send response. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleExpertResponse = async () => {
    if (!id || !responseText.trim()) return;
    
    setSubmitting(true);
    setResponseError(null);
    
    try {
      // Send expert response
      await api.post(`/consultations/${id}/respond`, {
        message: responseText
      });
      
      // Clear form and refresh data
      setResponseText('');
      dispatch(getConsultationById(parseInt(id)));
      
      // Show success message
      alert('Your response has been sent to the user');
    } catch (err: any) {
      console.error('Error sending response:', err);
      setResponseError(err.response?.data?.message || 'Failed to send response');
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleAcceptExpert = async () => {
    if (!id) return;
    
    setSubmitting(true);
    try {
      await api.post(`/consultations/${id}/accept-expert`);
      dispatch(getConsultationById(parseInt(id)));
    } catch (err: any) {
      console.error('Error accepting expert:', err);
      alert('Failed to accept expert offer');
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleDeclineExpert = async () => {
    if (!id) return;
    
    setSubmitting(true);
    try {
      await api.post(`/consultations/${id}/decline-expert`);
      dispatch(getConsultationById(parseInt(id)));
    } catch (err: any) {
      console.error('Error declining expert:', err);
      alert('Failed to decline expert offer');
    } finally {
      setSubmitting(false);
    }
  };
  
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
              {/* Fix the charAt error by adding null check */}
              {status && typeof status === 'string' 
                ? status.charAt(0).toUpperCase() + status.slice(1) 
                : 'Unknown'}
            </span>
          </div>
          
          <div className="mb-8">
            <p className="text-gray-700 whitespace-pre-line">{description}</p>
          </div>
          
          <div className="flex items-center mb-6">
            <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mr-4 border border-amber-300">
              <span className="text-amber-700 font-semibold">
                {/* Add safe check before accessing charAt */}
                {expertName && expertName.length > 0 ? expertName.charAt(0) : '?'}
              </span>
            </div>
            <div>
              <h3 className="text-lg font-medium text-stone-800">Expert: {expertName || 'Not Assigned'}</h3>
              {status === 'completed' && formattedCompletedAt && (
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
      
      {/* Expert Offer Section - Visible to users with pending expert offers */}
      {isAuthenticated && 
       user?.role === 'User' && 
       typedConsultation?.userId === user.id && 
       typedConsultation?.status === 'pending' && 
       typedConsultation?.expertOffer && (
        <div className="mt-6 bg-amber-50 border border-amber-200 rounded-lg p-6">
          <h3 className="text-xl font-semibold text-amber-800 mb-3">Expert Offer</h3>
          <p className="mb-4">
            <strong>{typedConsultation.expertOffer.expertName}</strong> has offered to help with your consultation.
          </p>
          <div className="bg-white p-4 rounded-md border border-amber-100 mb-4">
            <p className="italic text-gray-700">{typedConsultation.expertOffer.message}</p>
          </div>
          <div className="flex space-x-4">
            <button
              onClick={handleAcceptExpert}
              disabled={submitting}
              className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded disabled:opacity-50"
            >
              Accept Offer
            </button>
            <button
              onClick={handleDeclineExpert}
              disabled={submitting}
              className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded disabled:opacity-50"
            >
              Decline
            </button>
          </div>
        </div>
      )}
      
      {/* Expert Action Buttons */}
      {isAuthenticated && user?.role === 'Expert' && (
        <div className="mt-6 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Expert Actions</h3>
          
          {/* Expert Response Form - For pending consultations */}
          {currentConsultation.status === 'pending' && (
            <div className="mb-6 bg-gray-50 p-4 rounded-lg">
              <h4 className="font-medium text-gray-800 mb-2">Send a Response to This Request</h4>
              <textarea
                value={responseText}
                onChange={(e) => setResponseText(e.target.value)}
                placeholder="Introduce yourself and explain how you can help with this consultation..."
                className="w-full border border-gray-300 rounded p-3 mb-3"
                rows={4}
              ></textarea>
              
              {responseError && (
                <p className="text-red-600 text-sm mb-2">{responseError}</p>
              )}
              
              <button 
                onClick={handleExpertResponse}
                disabled={!responseText.trim() || submitting}
                className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded disabled:opacity-50"
              >
                {submitting ? 'Sending...' : 'Send Response'}
              </button>
            </div>
          )}
          
          {/* Existing buttons */}
          {currentConsultation.status === 'Pending' && (
            <button 
              onClick={handleAcceptConsultation}
              className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded mr-3"
            >
              Accept Consultation
            </button>
          )}
          
          {currentConsultation.status === 'accepted' && currentConsultation.expertId === user?.id && (
            <button 
              onClick={handleCompleteConsultation}
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded"
            >
              Mark as Completed
            </button>
          )}
        </div>
      )}
      
      {/* User Action Buttons */}
      {isAuthenticated && userRole === 'User' && (
        <div className="mt-6 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Your Actions</h3>
          
          {currentConsultation.userId === user?.id && currentConsultation.status === 'Pending' && (
            <div className="flex space-x-4">
              <button 
                onClick={() => navigate(`/consultations/${id}/edit`)}
                className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              >
                Edit Consultation
              </button>
              
              <button 
                className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded"
              >
                Cancel Consultation
              </button>
            </div>
          )}
        </div>
      )}
      
      {/* Conversation Section - For accepted consultations */}
      {typedConsultation?.status === 'accepted' && (
        <div className="mt-8 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Conversation</h3>
          
          <div className="bg-gray-50 rounded-lg p-4 mb-4 max-h-96 overflow-y-auto">
            {typedConsultation.messages && typedConsultation.messages.length > 0 ? (
              <div className="space-y-4">
                {typedConsultation.messages.map((message: Message, index: number) => (
                  <div 
                    key={index}
                    className={`flex ${message.userId === user?.id ? 'justify-end' : 'justify-start'}`}
                  >
                    <div 
                      className={`max-w-3/4 rounded-lg p-3 ${
                        message.userId === user?.id 
                          ? 'bg-amber-100 text-amber-900' 
                          : 'bg-gray-200 text-gray-800'
                      }`}
                    >
                      <p className="text-sm mb-1">{message.content}</p>
                      <p className="text-xs text-gray-500">{new Date(message.createdAt).toLocaleString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-center text-gray-500 italic">No messages yet. Start the conversation!</p>
            )}
          </div>
          
          <div className="flex">
            <textarea
              value={responseText}
              onChange={(e) => setResponseText(e.target.value)}
              placeholder="Type your message..."
              className="flex-1 border border-gray-300 rounded-l p-3"
              rows={2}
            ></textarea>
            <button 
              onClick={handleRespondToConsultation}
              disabled={!responseText.trim() || submitting}
              className="bg-amber-600 hover:bg-amber-700 text-white px-4 rounded-r disabled:opacity-50"
            >
              Send
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConsultationDetailPage;
