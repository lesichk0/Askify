import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks';

interface ConsultationCardProps {
  id: number;
  title: string;
  description: string;
  status: string;
  expertName?: string;
  date: string;
  category?: string; // ML-classified category
  tags?: string[]; // Add tags prop
}

// Category color mapping
const categoryColors: Record<string, string> = {
  'Technology': 'bg-blue-100 text-blue-800 border-blue-200',
  'Health & Medicine': 'bg-red-100 text-red-800 border-red-200',
  'Legal': 'bg-purple-100 text-purple-800 border-purple-200',
  'Finance & Business': 'bg-green-100 text-green-800 border-green-200',
  'Education': 'bg-indigo-100 text-indigo-800 border-indigo-200',
  'Career & Employment': 'bg-orange-100 text-orange-800 border-orange-200',
  'Relationships': 'bg-pink-100 text-pink-800 border-pink-200',
  'Home & Living': 'bg-teal-100 text-teal-800 border-teal-200',
  'Travel': 'bg-cyan-100 text-cyan-800 border-cyan-200',
  'Other': 'bg-gray-100 text-gray-800 border-gray-200',
};

const ConsultationCard: React.FC<ConsultationCardProps> = ({
  id,
  title,
  description,
  status,
  expertName,
  date,
  category,
  tags = [] // Default to empty array
}) => {
  const navigate = useNavigate();
  
  const handleClick = () => {
    console.log(`Navigating to consultation #${id}`);
    navigate(`/consultations/${id}`);
  };
  
  // Check if user is authenticated
  const { isAuthenticated } = useAppSelector(state => state.auth);
  const canViewDetails = isAuthenticated || status?.toLowerCase() === 'completed';
  
  // Define status styles
  const statusStyles = {
    completed: 'bg-green-100 text-green-800',
    pending: 'bg-yellow-100 text-yellow-800',
    accepted: 'bg-blue-100 text-blue-800',
    cancelled: 'bg-red-100 text-red-800',
    default: 'bg-gray-100 text-gray-800'
  };

  return (
    <div 
      className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
      onClick={handleClick}
    >
      <div className="p-6">
        <div className="flex justify-between items-start mb-2">
          <h3 className="text-xl font-bold text-gray-800 truncate">{title}</h3>
          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
            statusStyles[status as keyof typeof statusStyles] || statusStyles.default
          }`}>
            {status}
          </span>
        </div>
        
        {/* Display ML Category */}
        {category && (
          <div className="mb-3">
            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
              categoryColors[category] || categoryColors['Other']
            }`}>
              <svg className="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M17.707 9.293a1 1 0 010 1.414l-7 7a1 1 0 01-1.414 0l-7-7A.997.997 0 012 10V5a3 3 0 013-3h5c.256 0 .512.098.707.293l7 7zM5 6a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
              </svg>
              {category}
            </span>
          </div>
        )}
        
        {/* Display Tags */}
        {tags.length > 0 && (
          <div className="flex flex-wrap gap-1 mb-3">
            {tags.slice(0, 3).map((tag, index) => (
              <span 
                key={index}
                className="bg-amber-100 text-amber-800 text-xs px-2 py-0.5 rounded-full"
              >
                {tag}
              </span>
            ))}
            {tags.length > 3 && (
              <span className="text-xs text-gray-500">+{tags.length - 3} more</span>
            )}
          </div>
        )}
        
        <p className="text-gray-600 mb-4 line-clamp-2">{description}</p>
        
        <div className="flex items-center justify-between mt-4">
          <div className="flex items-center">
            {expertName && (
              <div className="mr-3">
                <p className="text-sm font-medium text-stone-800">{expertName}</p>
                <p className="text-xs text-stone-500">{date}</p>
              </div>
            )}
          </div>
          
          {/* Replace the View Details link */}
          {canViewDetails ? (
            <Link
              to={`/consultations/${id}`}
              className="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-md text-sm transition border border-amber-600 shadow-sm"
            >
              View Details
            </Link>
          ) : (
            <button
              className="bg-gray-400 text-white px-4 py-2 rounded-md text-sm cursor-not-allowed"
              disabled
            >
              Not Available
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConsultationCard;
