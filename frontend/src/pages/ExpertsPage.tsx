import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/api';

interface Expert {
  id: string;
  fullName: string;
  email: string;
  bio?: string;
  specialization?: string;
  expertise?: string[];
  averageRating?: number;
  reviewsCount?: number;
  role: string;
}

const ExpertsPage: React.FC = () => {
  const navigate = useNavigate();
  const [experts, setExperts] = useState<Expert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedExpert, setSelectedExpert] = useState<Expert | null>(null);
  const [showExpertModal, setShowExpertModal] = useState(false);

  const openExpertModal = (expert: Expert) => {
    setSelectedExpert(expert);
    setShowExpertModal(true);
  };

  useEffect(() => {
    const fetchExperts = async () => {
      setLoading(true);
      try {
        console.log('Fetching experts...');
        
        // Try experts endpoint first
        let expertUsers = [];
        try {
          const expertsResponse = await api.get('/users/experts');
          console.log('Experts API Response:', expertsResponse.data);
          
          if (Array.isArray(expertsResponse.data) && expertsResponse.data.length > 0) {
            expertUsers = expertsResponse.data;
          } 
        } catch (error) {
          console.error('Error fetching from experts endpoint:', error);
        }
        
        // If no experts found, try the general users endpoint and filter
        if (expertUsers.length === 0) {
          console.log('No experts found from specialized endpoint, fetching all users...');
          const usersResponse = await api.get('/users');
          console.log('All Users API Response:', usersResponse.data);
          
          if (Array.isArray(usersResponse.data)) {
            expertUsers = usersResponse.data.filter(user => {
              // Case-insensitive check for Expert role
              const isExpertByRole = user.role?.toLowerCase() === 'expert' || 
                                     user.Role?.toLowerCase() === 'expert';
              
              // Check verified flag
              const isVerifiedExpert = user.isVerifiedExpert === true || 
                                      user.IsVerifiedExpert === true;
              
              console.log(`User ${user.fullName || user.FullName}: Role=${user.role || user.Role}, IsExpert=${isExpertByRole}, IsVerified=${isVerifiedExpert}`);
              
              // Include user if either condition is true
              return isExpertByRole || isVerifiedExpert;
            });
          }
        }
        
        // If we have experts, set them in state
        if (expertUsers.length > 0) {
          console.log(`Found ${expertUsers.length} experts`);
          setExperts(expertUsers);
        } else {
          console.warn('No experts found through any method');
          setExperts([]);
        }
      } catch (err) {
        console.error('Error fetching experts:', err);
        setError('Failed to load experts. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchExperts();
  }, []);
  
  // Filter experts based on search term
  const filteredExperts = experts.filter(expert => 
    searchTerm === '' || 
    expert.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (expert.bio && expert.bio.toLowerCase().includes(searchTerm.toLowerCase())) ||
    (expert.specialization && expert.specialization.toLowerCase().includes(searchTerm.toLowerCase()))
  );
  
  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }
  
  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Find an Expert</h1>
      
      {/* Search Bar */}
      <div className="bg-white p-6 rounded-lg shadow-md mb-8">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1">
            <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">Search</label>
            <input
              type="text"
              id="search"
              placeholder="Search by name or expertise..."
              className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
      </div>
      
      {error && (
        <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-8 text-red-700">
          {error}
        </div>
      )}
      
      {/* Experts List */}
      {filteredExperts.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 rounded-lg">
          <p className="text-lg text-gray-600 mb-4">No experts found matching your criteria.</p>
          {searchTerm && (
            <button 
              onClick={() => setSearchTerm('')}
              className="text-amber-600 hover:text-amber-800 font-semibold"
            >
              Clear search
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredExperts.map((expert) => (
            <div 
              key={expert.id} 
              className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
              onClick={() => openExpertModal(expert)}
            >
              <div className="p-6">
                <div className="flex items-center mb-4">
                  <div className="w-16 h-16 rounded-full bg-amber-100 flex items-center justify-center text-xl font-bold text-amber-700 mr-4">
                    {expert.fullName.charAt(0)}
                  </div>
                  <div>
                    <h3 className="text-xl font-bold text-gray-800">{expert.fullName}</h3>
                    {expert.specialization && (
                      <p className="text-amber-600">{expert.specialization}</p>
                    )}
                  </div>
                </div>
                
                {expert.bio && (
                  <p className="text-gray-600 mb-4 line-clamp-3">{expert.bio}</p>
                )}
                
                {expert.expertise && expert.expertise.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-4">
                    {expert.expertise.map((skill, index) => (
                      <span 
                        key={index}
                        className="bg-amber-100 text-amber-800 text-xs px-2 py-1 rounded"
                      >
                        {skill}
                      </span>
                    ))}
                  </div>
                )}
                
                <div className="flex justify-between items-center mt-4 pt-4 border-t border-gray-100">
                  {expert.averageRating !== undefined && expert.averageRating !== null ? (
                    <div className="flex items-center">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-yellow-500 mr-1" viewBox="0 0 20 20" fill="currentColor">
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                      <span className="text-gray-700">{expert.averageRating.toFixed(1)}</span>
                      {expert.reviewsCount !== undefined && (
                        <span className="text-gray-500 text-sm ml-1">({expert.reviewsCount} reviews)</span>
                      )}
                    </div>
                  ) : (
                    <span className="text-gray-500 text-sm">No reviews yet</span>
                  )}
                  
                  <button 
                    className="bg-amber-600 hover:bg-amber-700 text-white px-3 py-1 rounded text-sm"
                    onClick={(e) => {
                      e.stopPropagation();
                      navigate(`/consultations/new?expertId=${expert.id}`);
                    }}
                  >
                    Request Consultation
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Expert Detail Modal */}
      {showExpertModal && selectedExpert && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg w-full max-w-2xl mx-4 max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-start mb-6">
                <div className="flex items-center">
                  <div className="w-20 h-20 rounded-full bg-amber-100 flex items-center justify-center text-2xl font-bold text-amber-700 mr-4">
                    {selectedExpert.fullName.charAt(0)}
                  </div>
                  <div>
                    <h2 className="text-2xl font-bold text-gray-800">{selectedExpert.fullName}</h2>
                    {selectedExpert.specialization && (
                      <p className="text-amber-600 text-lg">{selectedExpert.specialization}</p>
                    )}
                    <p className="text-gray-500">{selectedExpert.email}</p>
                  </div>
                </div>
                <button
                  onClick={() => setShowExpertModal(false)}
                  className="text-gray-500 hover:text-gray-700"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              {selectedExpert.averageRating !== undefined && selectedExpert.averageRating !== null && (
                <div className="flex items-center mb-4">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-yellow-500 mr-1" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                  <span className="text-lg font-semibold text-gray-700">{selectedExpert.averageRating.toFixed(1)}</span>
                  {selectedExpert.reviewsCount !== undefined && (
                    <span className="text-gray-500 ml-2">({selectedExpert.reviewsCount} reviews)</span>
                  )}
                </div>
              )}

              {selectedExpert.bio && (
                <div className="mb-6">
                  <h3 className="text-lg font-semibold text-gray-800 mb-2">About</h3>
                  <p className="text-gray-600">{selectedExpert.bio}</p>
                </div>
              )}

              {selectedExpert.expertise && selectedExpert.expertise.length > 0 && (
                <div className="mb-6">
                  <h3 className="text-lg font-semibold text-gray-800 mb-2">Expertise</h3>
                  <div className="flex flex-wrap gap-2">
                    {selectedExpert.expertise.map((skill, index) => (
                      <span 
                        key={index}
                        className="bg-amber-100 text-amber-800 px-3 py-1 rounded-full"
                      >
                        {skill}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              <div className="flex justify-end space-x-4 pt-4 border-t border-gray-200">
                <button
                  onClick={() => setShowExpertModal(false)}
                  className="px-4 py-2 text-gray-600 hover:text-gray-800"
                >
                  Close
                </button>
                <button
                  onClick={() => {
                    setShowExpertModal(false);
                    navigate(`/consultations/new?expertId=${selectedExpert.id}`);
                  }}
                  className="bg-amber-600 hover:bg-amber-700 text-white px-6 py-2 rounded"
                >
                  Request Consultation
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ExpertsPage;
