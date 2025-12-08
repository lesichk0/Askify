import React, { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks';
import { getConsultationById } from '../features/consultations/consultationsSlice';
import api from '../api/api';
import { webSocketService } from '../services/WebSocketService';
import Toast from '../components/Toast';

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
  isPaid: boolean;
  price?: number;
  isOpenRequest: boolean;
  expertOffer?: ExpertOffer;
  messages?: Message[];
}

interface ToastState {
  message: string;
  type: 'success' | 'error' | 'info';
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
  const [localMessages, setLocalMessages] = useState<Message[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const [followUpQuestion, setFollowUpQuestion] = useState('');
  const [followUpSubmitting, setFollowUpSubmitting] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [toast, setToast] = useState<ToastState | null>(null);
  const [priceInput, setPriceInput] = useState('');
  const [priceSubmitting, setPriceSubmitting] = useState(false);

  const showToast = (message: string, type: 'success' | 'error' | 'info') => {
    setToast({ message, type });
  };
  
  // Auto-scroll to bottom when new messages arrive
  const scrollToBottom = () => {
    // Scroll within the messages container, not the whole page
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
  };

  useEffect(() => {
    scrollToBottom();
  }, [localMessages]);
  
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
      // Initialize local messages from consultation data
      const typed = currentConsultation as ExtendedConsultation;
      if (typed.messages) {
        setLocalMessages(typed.messages);
      }
    }
  }, [currentConsultation]);

  // WebSocket setup for real-time messages
  useEffect(() => {
    if (!id || !isAuthenticated) return;

    const setupWebSocket = async () => {
      try {
        // Initialize WebSocket connection
        if (!webSocketService.isConsultationConnected()) {
          await webSocketService.initializeConsultationConnection();
        }

        // Join the consultation room
        await webSocketService.joinConsultation(parseInt(id));

        // Set up message received callback
        webSocketService.onMessageReceivedCallback((message) => {
          console.log('New message received:', message);
          setLocalMessages((prev) => [...prev, message]);
        });

        // Set up consultation completed callback
        webSocketService.onConsultationCompletedCallback((data) => {
          console.log('Consultation completed:', data);
          if (data.consultationId === parseInt(id)) {
            dispatch(getConsultationById(parseInt(id)));
            showToast('Consultation has been completed!', 'success');
          }
        });

        // Set up consultation updated callback (for price changes, payments, etc.)
        webSocketService.onConsultationUpdatedCallback((data) => {
          console.log('Consultation updated:', data);
          if (data.consultationId === parseInt(id)) {
            dispatch(getConsultationById(parseInt(id)));
          }
        });
      } catch (error) {
        console.error('WebSocket setup failed:', error);
      }
    };

    setupWebSocket();

    return () => {
      // Cleanup on unmount
      webSocketService.leaveConsultation(parseInt(id));
    };
  }, [id, isAuthenticated, dispatch]);
  
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
      showToast('You have successfully accepted this consultation request.', 'success');
    } catch (error) {
      console.error('Error accepting consultation:', error);
      showToast('Failed to accept consultation. Please try again.', 'error');
    }
  };
  
  const handleDeclineConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/cancel`);
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
      showToast('You have declined this consultation request.', 'success');
    } catch (error) {
      console.error('Error declining consultation:', error);
      showToast('Failed to decline consultation. Please try again.', 'error');
    }
  };

  const handleCancelConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/cancel`);
      showToast('Consultation cancelled successfully.', 'success');
      navigate('/my-consultations');
    } catch (error) {
      console.error('Error cancelling consultation:', error);
      showToast('Failed to cancel consultation. Please try again.', 'error');
    }
  };

  const handleFollowUpQuestion = async () => {
    if (!id || !followUpQuestion.trim()) return;
    
    setFollowUpSubmitting(true);
    try {
      // Create a new consultation as a follow-up
      await api.post('/consultations', {
        title: `Follow-up: ${currentConsultation?.title}`,
        description: followUpQuestion,
        isOpenRequest: false,
        isFree: true,
        isPublicable: false,
        expertId: currentConsultation?.expertId
      });
      
      setFollowUpQuestion('');
      showToast('Your follow-up question has been submitted!', 'success');
    } catch (error) {
      console.error('Error submitting follow-up question:', error);
      showToast('Failed to submit follow-up question. Please try again.', 'error');
    } finally {
      setFollowUpSubmitting(false);
    }
  };

  const handleEditConsultation = async () => {
    if (!id || !editTitle.trim() || !editDescription.trim()) return;
    
    try {
      await api.put(`/consultations/${id}`, {
        title: editTitle,
        description: editDescription
      });
      
      setShowEditModal(false);
      dispatch(getConsultationById(parseInt(id)));
      showToast('Consultation updated successfully!', 'success');
    } catch (error) {
      console.error('Error updating consultation:', error);
      showToast('Failed to update consultation. Please try again.', 'error');
    }
  };

  const openEditModal = () => {
    if (currentConsultation) {
      setEditTitle(currentConsultation.title || '');
      setEditDescription(currentConsultation.description || '');
      setShowEditModal(true);
    }
  };
  
  const handleCompleteConsultation = async () => {
    if (!id) return;
    
    try {
      await api.post(`/consultations/${id}/complete`);
      // Refresh consultation data
      dispatch(getConsultationById(parseInt(id)));
      showToast('Consultation marked as completed!', 'success');
    } catch (error) {
      console.error('Error completing consultation:', error);
      showToast('Failed to complete consultation.', 'error');
    }
  };
  
  const handleRespondToConsultation = async () => {
    if (!id || !responseText.trim() || !currentConsultation) return;
    
    setSubmitting(true);
    
    try {
      // Determine who the receiver is (the other party in the conversation)
      const receiverId = user?.id === currentConsultation.userId 
        ? currentConsultation.expertId 
        : currentConsultation.userId;
      
      if (!receiverId) {
        showToast('Cannot determine recipient. Please try again later.', 'error');
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
      // Message will appear via WebSocket real-time update
    } catch (err: any) {
      console.error('Error responding to consultation:', err);
      showToast('Failed to send response. Please try again.', 'error');
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
      showToast('Expert accepted successfully!', 'success');
    } catch (err: any) {
      console.error('Error accepting expert:', err);
      showToast('Failed to accept expert offer', 'error');
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
      showToast('Expert declined', 'info');
    } catch (err: any) {
      console.error('Error declining expert:', err);
      showToast('Failed to decline expert offer', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleSetPrice = async () => {
    if (!id || !priceInput) return;
    
    const price = parseFloat(priceInput);
    if (isNaN(price) || price <= 0) {
      showToast('Please enter a valid price greater than 0', 'error');
      return;
    }
    
    setPriceSubmitting(true);
    try {
      await api.post(`/consultations/${id}/set-price`, { price });
      dispatch(getConsultationById(parseInt(id)));
      setPriceInput('');
      showToast(`Price of ₴${price} set successfully!`, 'success');
    } catch (err: any) {
      console.error('Error setting price:', err);
      showToast('Failed to set price. Please try again.', 'error');
    } finally {
      setPriceSubmitting(false);
    }
  };

  const handleAcceptPrice = async () => {
    if (!id) return;
    
    setPriceSubmitting(true);
    try {
      await api.post(`/consultations/${id}/accept-price`);
      dispatch(getConsultationById(parseInt(id)));
      showToast('Payment accepted! Consultation is now in progress.', 'success');
    } catch (err: any) {
      console.error('Error accepting price:', err);
      showToast('Failed to process payment. Please try again.', 'error');
    } finally {
      setPriceSubmitting(false);
    }
  };

  const handleRejectPrice = async () => {
    if (!id) return;
    
    setPriceSubmitting(true);
    try {
      await api.post(`/consultations/${id}/reject-price`);
      dispatch(getConsultationById(parseInt(id)));
      showToast('Price rejected. The consultation is now open for other experts.', 'info');
    } catch (err: any) {
      console.error('Error rejecting price:', err);
      showToast('Failed to reject price. Please try again.', 'error');
    } finally {
      setPriceSubmitting(false);
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
      {toast && (
        <Toast 
          type={toast.type} 
          message={toast.message} 
          onClose={() => setToast(null)} 
        />
      )}
      
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
              status?.toLowerCase() === 'pending' ? 'bg-yellow-100 text-yellow-800' :
              status?.toLowerCase() === 'accepted' ? 'bg-blue-100 text-blue-800' :
              status?.toLowerCase() === 'awaitingpayment' ? 'bg-orange-100 text-orange-800' :
              status?.toLowerCase() === 'inprogress' ? 'bg-purple-100 text-purple-800' :
              'bg-gray-100 text-gray-800'
            }`}>
              {/* Fix the charAt error by adding null check */}
              {status && typeof status === 'string' 
                ? (status === 'awaitingpayment' || status === 'AwaitingPayment' 
                    ? 'Awaiting Payment' 
                    : status === 'inprogress' || status === 'InProgress'
                      ? 'In Progress'
                      : status.charAt(0).toUpperCase() + status.slice(1))
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
            {/* Show consultation type and price */}
            <div className="flex items-center gap-4 mt-2">
              {typedConsultation?.isFree ? (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  Free Consultation
                </span>
              ) : (
                <>
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-800">
                    Paid Consultation
                  </span>
                  {typedConsultation?.price && (
                    <span className="text-sm font-medium text-gray-700">
                      Price: ₴{typedConsultation.price.toFixed(2)}
                      {typedConsultation?.isPaid && (
                        <span className="ml-2 text-green-600">✓ Paid</span>
                      )}
                    </span>
                  )}
                </>
              )}
            </div>
          </div>
          
          {isAuthenticated && status?.toLowerCase() === 'completed' && (
            <div className="mt-8">
              <h3 className="text-xl font-semibold mb-4">Ask a follow-up question</h3>
              <textarea 
                className="w-full border border-gray-300 rounded p-3 h-32 focus:outline-none focus:ring-2 focus:ring-amber-500"
                placeholder="Type your question..."
                value={followUpQuestion}
                onChange={(e) => setFollowUpQuestion(e.target.value)}
              ></textarea>
              <button 
                onClick={handleFollowUpQuestion}
                disabled={!followUpQuestion.trim() || followUpSubmitting}
                className="mt-2 bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {followUpSubmitting ? 'Submitting...' : 'Submit Question'}
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
        {/* Expert Action Buttons - Only show for pending consultations */}
      {isAuthenticated && user?.role === 'Expert' && 
        currentConsultation?.status?.toLowerCase() === 'pending' && (
        <div className="mt-6 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Expert Actions</h3>
          
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
        </div>
      )}

      {/* Expert Complete & Price Setting - Only for accepted consultations */}
      {isAuthenticated && user?.role === 'Expert' && 
        (currentConsultation?.status?.toLowerCase() === 'accepted' || 
         currentConsultation?.status?.toLowerCase() === 'inprogress') && 
        currentConsultation?.expertId === user?.id && (
        <div className="mt-6 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Consultation Management</h3>
          
          <div className="space-y-4">
            <button 
              onClick={handleCompleteConsultation}
              disabled={submitting}
              className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white px-4 py-2 rounded"
            >
              {submitting ? 'Completing...' : 'Mark as Completed'}
            </button>
            
            {/* Price Setting for Non-Free Consultations - Only for accepted status */}
            {!currentConsultation?.isFree && 
              !(currentConsultation as ExtendedConsultation)?.price && 
              currentConsultation?.status?.toLowerCase() === 'accepted' && (
              <div className="mt-4 p-4 bg-amber-50 rounded-lg border border-amber-200">
                <h4 className="font-medium text-amber-800 mb-2">Set Consultation Price</h4>
                <div className="flex items-center space-x-2">
                  <span className="text-gray-600">₴</span>
                  <input
                    type="number"
                    min="1"
                    value={priceInput}
                    onChange={(e) => setPriceInput(e.target.value)}
                    placeholder="Enter price"
                    className="border border-gray-300 rounded px-3 py-2 w-32"
                      />
                  <button
                    onClick={handleSetPrice}
                    disabled={priceSubmitting || !priceInput}
                    className="bg-amber-600 hover:bg-amber-700 disabled:bg-gray-400 text-white px-4 py-2 rounded"
                  >
                    {priceSubmitting ? 'Setting...' : 'Set Price'}
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
      
      {/* User Action Buttons */}
      {isAuthenticated && userRole === 'User' && (
        <div className="mt-6 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Your Actions</h3>
            {currentConsultation.userId === user?.id && currentConsultation.status?.toLowerCase() === 'pending' && (
            <div className="flex space-x-4">
              <button 
                onClick={openEditModal}
                className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              >
                Edit Consultation
              </button>
              
              <button 
                onClick={handleCancelConsultation}
                className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded"
              >
                Cancel Consultation
              </button>
            </div>
          )}
          
          {/* Price Payment Section for AwaitingPayment status */}
          {currentConsultation.userId === user?.id && 
            currentConsultation.status?.toLowerCase() === 'awaitingpayment' && (
            <div className="p-4 bg-amber-50 rounded-lg border border-amber-200">
              <h4 className="font-medium text-amber-800 mb-2">Payment Required</h4>
              <p className="text-gray-700 mb-3">
                The expert has set a price for this consultation:
                <span className="font-bold text-lg ml-2">₴{(currentConsultation as ExtendedConsultation)?.price?.toFixed(2)}</span>
              </p>
              <p className="text-sm text-gray-500 mb-3">
                If this price doesn't work for you, you can reject it and wait for another expert.
              </p>
              <div className="flex space-x-4">
                <button
                  onClick={handleAcceptPrice}
                  disabled={priceSubmitting}
                  className="bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white px-6 py-2 rounded font-medium"
                >
                  {priceSubmitting ? 'Processing...' : 'Pay Now'}
                </button>
                <button
                  onClick={handleRejectPrice}
                  disabled={priceSubmitting}
                  className="bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white px-6 py-2 rounded font-medium"
                >
                  {priceSubmitting ? 'Processing...' : 'Reject Price'}
                </button>
              </div>
            </div>
          )}
          
          {/* Show price if set but already paid */}
          {currentConsultation.userId === user?.id && 
            !currentConsultation.isFree && 
            (currentConsultation as ExtendedConsultation)?.isPaid && 
            (currentConsultation as ExtendedConsultation)?.price && (
            <div className="p-4 bg-green-50 rounded-lg border border-green-200">
              <p className="text-green-800">
                <span className="font-medium">✓ Paid:</span> ₴{(currentConsultation as ExtendedConsultation)?.price?.toFixed(2)}
              </p>
            </div>
          )}
          
          {/* Client can mark consultation as completed */}
          {currentConsultation.userId === user?.id && 
            (currentConsultation.status?.toLowerCase() === 'accepted' || 
             currentConsultation.status?.toLowerCase() === 'inprogress') && (
            <div className="mt-4">
              <button
                onClick={handleCompleteConsultation}
                disabled={submitting}
                className="bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white px-6 py-2 rounded font-medium"
              >
                {submitting ? 'Completing...' : 'Mark as Completed'}
              </button>
            </div>
          )}
        </div>
      )}      {/* Conversation Section - For accepted and in-progress consultations */}
      {(typedConsultation?.status?.toLowerCase() === 'accepted' || 
        typedConsultation?.status?.toLowerCase() === 'inprogress') && (
        <div className="mt-8 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Conversation</h3>
          
          <div className="bg-gray-50 rounded-lg p-4 mb-4 max-h-96 overflow-y-auto">
            {localMessages && localMessages.length > 0 ? (
              <div className="space-y-4">
                {localMessages.map((message: Message, index: number) => (
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
                <div ref={messagesEndRef} />
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

      {/* Read-only Conversation Section - For completed public consultations */}
      {typedConsultation?.status?.toLowerCase() === 'completed' && 
        localMessages && localMessages.length > 0 && (
        <div className="mt-8 border-t border-gray-200 pt-6">
          <h3 className="text-xl font-semibold mb-4">Consultation History</h3>
          
          <div className="bg-gray-50 rounded-lg p-4 max-h-96 overflow-y-auto">
            <div className="space-y-4">
              {localMessages.map((message: Message, index: number) => (
                <div 
                  key={index}
                  className={`flex ${message.senderId === user?.id ? 'justify-end' : 'justify-start'}`}
                >
                  <div 
                    className={`max-w-3/4 rounded-lg p-3 ${
                      message.senderId === typedConsultation?.expertId 
                        ? 'bg-amber-100 text-amber-900' 
                        : 'bg-gray-200 text-gray-800'
                    }`}
                  >
                    <p className="text-xs font-semibold mb-1">
                      {message.senderName || 'Unknown'}
                      {message.senderId === typedConsultation?.expertId && 
                        <span className="ml-2 text-amber-600">(Expert)</span>
                      }
                    </p>
                    <p className="text-sm mb-1">{message.text}</p>
                    <p className="text-xs text-gray-500">{new Date(message.sentAt).toLocaleString()}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
          
          <p className="text-sm text-gray-500 mt-2 italic">This consultation has been completed.</p>
        </div>
      )}

      {/* Edit Consultation Modal */}
      {showEditModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-lg mx-4">
            <h2 className="text-xl font-bold mb-4">Edit Consultation</h2>
            
            <div className="mb-4">
              <label className="block text-gray-700 text-sm font-bold mb-2">Title</label>
              <input
                type="text"
                value={editTitle}
                onChange={(e) => setEditTitle(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
              />
            </div>
            
            <div className="mb-4">
              <label className="block text-gray-700 text-sm font-bold mb-2">Description</label>
              <textarea
                value={editDescription}
                onChange={(e) => setEditDescription(e.target.value)}
                rows={5}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
              />
            </div>
            
            <div className="flex justify-end space-x-4">
              <button
                onClick={() => setShowEditModal(false)}
                className="px-4 py-2 text-gray-600 hover:text-gray-800"
              >
                Cancel
              </button>
              <button
                onClick={handleEditConsultation}
                disabled={!editTitle.trim() || !editDescription.trim()}
                className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded disabled:opacity-50"
              >
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConsultationDetailPage;
