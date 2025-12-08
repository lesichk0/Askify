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
  senderId: string;
  senderName?: string;
  text?: string;
  status: string;
  sentAt: string;
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
  
  // Move ALL useEffect hooks to the top, before any conditional returns
  useEffect(() => {
    if (id) {
      console.log('Fetching consultation with ID:', id);
      dispatch(getConsultationById(parseInt(id)));
    }
  }, [dispatch, id]);

  // Add the debugging useEffect BEFORE any conditional returns
  useEffect(() => {
    if (currentConsultation) {
      console.log('Loaded consultation data:', currentConsultation);
    }
  }, [currentConsultation]);
  
  // Helper functions for formatting - move up before conditional returns
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Unknown date';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };
  
  // All handler functions must be declared before any conditional returns
  const handleAcceptConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/accept`);
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
      // Show success message
      alert('You have successfully accepted this consultation request.');
    } catch (error) {
      console.error('Error accepting consultation:', error);
      alert('Failed to accept consultation. Please try again.');
    }
  };
  
  const handleDeclineConsultation = async () => {
    if (!id) return;
    
    if (window.confirm('Are you sure you want to decline this consultation request?')) {
      try {
        await api.post(`/consultations/${id}/cancel`);
        // Refresh consultation data
        dispatch(getConsultationById(parseInt(id)));
        // Show success message
        alert('You have declined this consultation request.');
      } catch (error) {
        console.error('Error declining consultation:', error);
        alert('Failed to decline consultation. Please try again.');
      }
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
    if (!id || !responseText.trim() || !currentConsultation) return;
    
    setSubmitting(true);
    setResponseError(null);
    
    try {
      // Determine who the receiver is (the other party in the conversation)
      const receiverId = user?.id === currentConsultation.userId 
        ? currentConsultation.expertId 
        : currentConsultation.userId;
      
      if (!receiverId) {
        setResponseError('Cannot determine recipient. Please try again later.');
        return;
      }
      
      // Send the response as a message with correct DTO structure
      await api.post('/messages', {
        receiverId: receiverId,
        consultationId: parseInt(id),
        text: responseText
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
  
  // NOW we can use conditional returns
  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }
  
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
  
  // Extract data from consultation - AFTER all hooks and conditions
  const { 
    title = 'Untitled Consultation', 
    description = 'No description available', 
    status = 'pending', 
    expertName = '',
    createdAt,
    completedAt
  } = currentConsultation;
  
  // Format dates using the helper function
  const formattedCreatedAt = formatDate(createdAt);
  const formattedCompletedAt = completedAt ? formatDate(completedAt) : null;
  
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
            {/* Use fallbacks inline rather than in destructuring */}
            <h1 className="text-3xl font-bold text-gray-800">{title || 'Untitled Consultation'}</h1>
            <span className={`px-4 py-1 rounded-full text-sm font-medium ${
              status?.toLowerCase() === 'completed' ? 'bg-green-100 text-green-800' :
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
            <p className="text-gray-700 whitespace-pre-line">{description || 'No description available'}</p>
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
              {status?.toLowerCase() === 'completed' && formattedCompletedAt && (
                <p className="text-sm text-stone-500">
                  Completed on {formattedCompletedAt}
                </p>
              )}
            </div>
          </div>
          
          <div className="border-t border-gray-200 pt-4">
            <p className="text-sm text-gray-500">Created: {formattedCreatedAt}</p>
          </div>
          
          {isAuthenticated && status?.toLowerCase() === 'completed' && (
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
          {(currentConsultation?.status?.toLowerCase() === 'pending' || 
            currentConsultation?.status?.toLowerCase() === 'Pending') && (
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
          )}            {/* Expert Action buttons */}
          {(currentConsultation?.status?.toLowerCase() === 'pending' || 
            currentConsultation?.status?.toLowerCase() === 'Pending') && (
            <div className="flex space-x-4">
              <button 
                onClick={handleAcceptConsultation}
                className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded"
              >
                Accept Consultation
              </button>
              <button 
                onClick={handleDeclineConsultation}
                className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded"
              >
                Decline Consultation
              </button>
            </div>
          )}            {(currentConsultation?.status?.toLowerCase() === 'accepted' || 
              currentConsultation?.status?.toLowerCase() === 'Accepted') && 
              currentConsultation?.expertId === user?.id && (
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
            {currentConsultation.userId === user?.id && currentConsultation.status?.toLowerCase() === 'pending' && (
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
      )}      {/* Conversation Section - For accepted consultations */}
      {(typedConsultation?.status?.toLowerCase() === 'accepted' || 
        typedConsultation?.status?.toLowerCase() === 'Accepted') && (
        <div className="mt-8 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Conversation</h3>
          
          <div className="bg-gray-50 rounded-lg p-4 mb-4 max-h-96 overflow-y-auto">
            {typedConsultation.messages && typedConsultation.messages.length > 0 ? (
              <div className="space-y-4">
                {typedConsultation.messages.map((message: Message, index: number) => (
                  <div 
                    key={index}
                    className={`flex ${message.senderId === user?.id ? 'justify-end' : 'justify-start'}`}
                  >
                    <div 
                      className={`max-w-3/4 rounded-lg p-3 ${
                        message.senderId === user?.id 
                          ? 'bg-amber-100 text-amber-900' 
                          : 'bg-gray-200 text-gray-800'
                      }`}
                    >
                      <p className="text-xs font-semibold mb-1">{message.senderName || 'Unknown'}</p>
                      <p className="text-sm mb-1">{message.text}</p>
                      <p className="text-xs text-gray-500">{new Date(message.sentAt).toLocaleString()}</p>
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
