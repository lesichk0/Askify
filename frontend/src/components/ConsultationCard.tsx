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
  onDelete?: (id: number) => void; // Optional delete callback
  showDelete?: boolean; // Whether to show delete button
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
  'Arts & Entertainment': 'bg-fuchsia-100 text-fuchsia-800 border-fuchsia-200',
  'Food & Cooking': 'bg-amber-100 text-amber-800 border-amber-200',
  'Sports & Fitness': 'bg-lime-100 text-lime-800 border-lime-200',
  'Pets & Animals': 'bg-emerald-100 text-emerald-800 border-emerald-200',
  'Science & Research': 'bg-violet-100 text-violet-800 border-violet-200',
  'Automotive': 'bg-slate-100 text-slate-800 border-slate-200',
  'Fashion & Beauty': 'bg-rose-100 text-rose-800 border-rose-200',
  'Parenting & Family': 'bg-sky-100 text-sky-800 border-sky-200',
  'Environment & Sustainability': 'bg-green-100 text-green-700 border-green-200',
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
  tags = [], // Default to empty array
  onDelete,
  showDelete = false
}) => {
  const navigate = useNavigate();
  
  const handleClick = () => {
    console.log(`Navigating to consultation #${id}`);
    navigate(`/consultations/${id}`);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent card click
    if (onDelete) {
      onDelete(id);
    }
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
      className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer h-full flex flex-col"
      onClick={handleClick}
    >
      <div className="p-6 flex flex-col flex-1">
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
        
        <p className="text-gray-600 mb-4 line-clamp-2 flex-1">{description}</p>
        
        <div className="flex items-center justify-between mt-auto pt-4 border-t border-gray-100">
          <div className="flex items-center">
            <div>
              <p className="text-sm font-medium text-stone-800">
                {expertName || 'Expert not assigned'}
              </p>
              <p className="text-xs text-stone-500">{date}</p>
            </div>
          </div>
          
          <div className="flex items-center gap-2">
            {/* Delete button */}
            {showDelete && onDelete && (
              <button
                onClick={handleDelete}
                className="bg-red-500 hover:bg-red-600 text-white p-2 rounded-md text-sm transition shadow-sm"
                title="Delete consultation"
              >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                </svg>
              </button>
            )}
            
            {/* View Details button */}
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
    </div>
  );
};

export default ConsultationCard;
