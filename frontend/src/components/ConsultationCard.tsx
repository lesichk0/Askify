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
  tags?: string[]; // Add tags prop
}

const ConsultationCard: React.FC<ConsultationCardProps> = ({
  id,
  title,
  description,
  status,
  expertName,
  date,
  tags = [] // Default to empty array
}) => {
  const navigate = useNavigate();
  
  const handleClick = () => {
    console.log(`Navigating to consultation #${id}`);
    navigate(`/consultations/${id}`);
  };
  
  // Check if user is authenticated
  const { isAuthenticated } = useAppSelector(state => state.auth);
  const canViewDetails = isAuthenticated || status === 'completed';
  
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
