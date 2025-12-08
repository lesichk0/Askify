import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import api from '../api/api';

const CreateConsultationPage: React.FC = () => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isFree, setIsFree] = useState(false);
  const [isPublicable, setIsPublicable] = useState(true);
  const [isOpenRequest, setIsOpenRequest] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedExpert, setSelectedExpert] = useState<{id: string; fullName: string; email: string} | null>(null);
  
  const { user, isAuthenticated } = useAppSelector(state => state.auth);
  const navigate = useNavigate();
  const location = useLocation();
  
  // Get expertId from query string if present
  const searchParams = new URLSearchParams(location.search);
  const expertId = searchParams.get('expertId');
  
  // Fetch expert info if expertId is provided
  useEffect(() => {
    const fetchExpertInfo = async () => {
      if (expertId) {
        try {
          const response = await api.get(`/users/${expertId}`);
          setSelectedExpert(response.data);
          setIsOpenRequest(false); // If expert is pre-selected, not an open request
        } catch (err) {
          console.error('Error fetching expert info:', err);
        }
      }
    };
    fetchExpertInfo();
  }, [expertId]);
  
  // Redirect if not authenticated or if user is not a User
  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    
    if (user?.role !== 'User') {
      setError('Only regular users can create consultation requests.');
    }
  }, [isAuthenticated, navigate, user]);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    
    try {
      const consultationData: any = {
        title,
        description,
        isFree,
        isPublicable,
        isOpenRequest
      };
      
      if (expertId) {
        consultationData.expertId = expertId;
        // If expert is chosen, force isOpenRequest to false
        consultationData.isOpenRequest = false;
      }
      
      const response = await api.post('/consultations', consultationData);
      console.log('Consultation created:', response.data);
      
      // Redirect to consultation detail page or my consultations
      navigate('/my-consultations');
    } catch (err: any) {
      console.error('Error creating consultation:', err);
      setError(err.response?.data?.message || 'Failed to create consultation. Please try again.');
    } finally {
      setLoading(false);
    }
  };
  
  if (user?.role !== 'User') {
    return (
      <div className="max-w-2xl mx-auto mt-10 p-6 bg-red-50 border border-red-200 rounded-lg text-center">
        <h2 className="text-xl font-bold text-red-700 mb-4">Access Denied</h2>
        <p className="text-red-600 mb-6">You don't have permission to create consultations as an expert. Only regular users can request consultations.</p>
        <button
          onClick={() => navigate('/')}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
        >
          Return to Home
        </button>
      </div>
    );
  }
  
  return (
    <div className="max-w-2xl mx-auto py-8 px-4">
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Request a Consultation</h1>
      
      {error && (
        <div className="mb-6 p-4 bg-red-50 border-l-4 border-red-500 text-red-700">
          <p>{error}</p>
        </div>
      )}

      {/* Show selected expert if any */}
      {selectedExpert && (
        <div className="mb-6 p-4 bg-amber-50 border border-amber-200 rounded-lg">
          <h3 className="text-lg font-semibold text-amber-800 mb-2">Requesting consultation with:</h3>
          <div className="flex items-center">
            <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mr-3">
              <span className="text-amber-700 font-bold text-lg">
                {selectedExpert.fullName?.charAt(0) || 'E'}
              </span>
            </div>
            <div>
              <p className="font-medium text-gray-800">{selectedExpert.fullName}</p>
              <p className="text-sm text-gray-500">{selectedExpert.email}</p>
            </div>
          </div>
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="bg-white shadow-md rounded-lg p-6">
        <div className="mb-4">
          <label htmlFor="title" className="block text-gray-700 font-bold mb-2">
            Consultation Title
          </label>
          <input
            type="text"
            id="title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
            placeholder="Enter a descriptive title for your consultation"
            required
          />
        </div>
        
        <div className="mb-4">
          <label htmlFor="description" className="block text-gray-700 font-bold mb-2">
            Description
          </label>
          <textarea
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={5}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
            placeholder="Describe what you need help with in detail"
            required
          />
        </div>
        
        <div className="mb-4">
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={isFree}
              onChange={() => setIsFree(!isFree)}
              className="form-checkbox text-amber-600 h-5 w-5"
            />
            <span className="ml-2 text-gray-700">This is a free consultation request</span>
          </label>
        </div>
        
        <div className="mb-4">
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={isPublicable}
              onChange={() => setIsPublicable(!isPublicable)}
              className="form-checkbox text-amber-600 h-5 w-5"
            />
            <span className="ml-2 text-gray-700">Make this consultation public after completion</span>
          </label>
        </div>
        
        <div className="mb-6">
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={isOpenRequest}
              onChange={() => setIsOpenRequest(!isOpenRequest)}
              className="form-checkbox text-amber-600 h-5 w-5"
            />
            <span className="ml-2 text-gray-700">Open request (any expert can accept)</span>
          </label>
        </div>
        
        <div className="flex justify-end">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="px-4 py-2 text-gray-700 mr-2"
          >
            Cancel
          </button>
          <button
            type="submit"
            className={`bg-amber-600 hover:bg-amber-700 text-white px-6 py-2 rounded-md ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
            disabled={loading}
          >
            {loading ? 'Submitting...' : 'Submit Request'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreateConsultationPage;
