import React from 'react';
import { Link } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import type { Consultation } from '../types/consultation';

interface ConsultationCardProps {
  // Support both individual props and unified consultation object
  consultation?: Consultation;
  id?: number;
  title?: string;
  expertName?: string;
  expertAvatar?: string;
  description?: string;
  status?: 'pending' | 'accepted' | 'completed' | 'cancelled';
  date?: string;
  isPublicable?: boolean; // Add publicable property to the interface
}

const ConsultationCard: React.FC<ConsultationCardProps> = (props) => {
  // If passed as individual props, use those
  // Otherwise use the consultation object
  const {
    consultation,
    id = consultation?.id,
    title = consultation?.title,
    expertName = consultation?.expertName,
    expertAvatar = consultation?.expertAvatar,
    description = consultation?.description,
    status = consultation?.status,
    date = consultation?.completedAt || consultation?.createdAt
  } = props;

  // Check if user is authenticated
  const { isAuthenticated } = useAppSelector(state => state.auth);
  const canViewDetails = isAuthenticated || status === 'completed';
  
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden border border-gray-200 hover:shadow-lg transition">
      <div className="p-6">
        <div className="flex justify-between items-start mb-4">
          <h3 className="text-xl font-semibold text-amber-800 mb-2">{title}</h3>
          <span className={`px-3 py-1 rounded-full text-xs font-medium ${
            status === 'completed' ? 'bg-green-100 text-green-800' :
            status === 'pending' ? 'bg-yellow-100 text-yellow-800' :
            status === 'accepted' ? 'bg-blue-100 text-blue-800' :
            'bg-gray-100 text-gray-800'
          }`}>
            {status ? status.charAt(0).toUpperCase() + status.slice(1) : 'Unknown'}
          </span>
        </div>
        
        <p className="text-gray-600 mb-4 line-clamp-3">{description}</p>
        
        <div className="flex items-center justify-between mt-4">
          <div className="flex items-center">
            {expertAvatar ? (
              <img 
                src={expertAvatar} 
                alt={expertName || 'Expert'} 
                className="w-10 h-10 rounded-full mr-3"
              />
            ) : (
              <div className="w-10 h-10 rounded-full bg-amber-100 flex items-center justify-center mr-3 border border-amber-300">
                <span className="text-amber-700 font-semibold">
                  {expertName ? expertName.charAt(0) : 'E'}
                </span>
              </div>
            )}
            <div>
              <p className="text-sm font-medium text-stone-800">{expertName}</p>
              <p className="text-xs text-stone-500">{date}</p>
            </div>
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
