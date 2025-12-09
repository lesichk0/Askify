import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import ConsultationCard from '../components/ConsultationCard';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations } from '../features/consultations/consultationsSlice';
import api from '../api/api';

interface Expert {
  id: string;
  fullName: string;
  bio?: string;
  avatarUrl?: string;
  averageRating?: number;
  reviewsCount?: number;
}

const HomePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { consultations, loading, error } = useAppSelector(state => state.consultations);
  const [topExperts, setTopExperts] = useState<Expert[]>([]);
  const [expertsLoading, setExpertsLoading] = useState(true);
  
  useEffect(() => {
    // Fetch public consultations when component mounts
    dispatch(fetchConsultations());
    
    // Fetch top experts
    const fetchTopExperts = async () => {
      try {
        const response = await api.get('/users/experts');
        if (Array.isArray(response.data)) {
          // Sort by rating descending and take top 3
          const sorted = response.data
            .filter((e: Expert) => e.averageRating !== undefined && e.averageRating !== null)
            .sort((a: Expert, b: Expert) => (b.averageRating || 0) - (a.averageRating || 0))
            .slice(0, 3);
          setTopExperts(sorted);
        }
      } catch (err) {
        console.error('Error fetching top experts:', err);
      } finally {
        setExpertsLoading(false);
      }
    };
    fetchTopExperts();
  }, [dispatch]);

  // Get latest consultations with categories (sorted by newest first)
  const latestWithCategories = [...consultations]
    .filter(c => c.category && c.category !== 'Other' && c.isPublicable)
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, 8);
  
  // Format date helper function
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };
  
  return (
    <div>
      <section className="bg-gradient-to-r from-amber-700 to-yellow-600 text-white py-16 px-4 rounded-xl mb-12">
        <div className="container mx-auto text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-6">
            Expert Consultations Made Simple
          </h1>
          <p className="text-xl md:text-2xl mb-8 max-w-3xl mx-auto">
            Connect with industry experts and get the answers you need to succeed.
          </p>
          <div className="flex flex-col sm:flex-row justify-center gap-4">
            <Link 
              to="/consultations/new" 
              className="bg-white text-amber-700 hover:bg-gray-100 px-8 py-3 rounded-md font-semibold text-lg transition"
            >
              Book a Consultation
            </Link>
            <Link 
              to="/experts" 
              className="bg-amber-800 hover:bg-amber-900 px-8 py-3 rounded-md font-semibold text-lg transition"
            >
              Find an Expert
            </Link>
          </div>
        </div>
      </section>

      {/* Latest Categorized Consultations Section */}
      <section className="mb-12">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-3xl font-bold text-gray-800">Browse by Category</h2>
            <p className="text-gray-600 mt-1">Latest consultations organized by topic</p>
          </div>
          <Link to="/consultations" className="text-amber-600 hover:text-amber-700 font-medium">
            View All →
          </Link>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {loading ? (
            <div className="col-span-full flex justify-center py-10">
              <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
            </div>
          ) : error ? (
            <div className="col-span-full text-center py-10 bg-red-50 rounded-lg">
              <p className="text-lg text-red-600">Error loading consultations: {error}</p>
            </div>
          ) : latestWithCategories.length > 0 ? (
            latestWithCategories.map((consultation) => (
              <ConsultationCard
                key={consultation.id}
                id={consultation.id}
                title={consultation.title || 'Untitled Consultation'}
                expertName={consultation.expertName}
                description={consultation.description || 'No description available.'}
                status={consultation.status as any}
                date={formatDate(consultation.createdAt)}
                category={consultation.category}
              />
            ))
          ) : (
            <div className="col-span-full text-center py-10 bg-gray-50 rounded-lg">
              <p className="text-lg text-gray-600">No categorized consultations available at the moment.</p>
            </div>
          )}
        </div>
      </section>

      {/* How It Works Section */}
      <section className="mb-12 bg-gray-50 rounded-xl p-8">
        <h2 className="text-3xl font-bold text-gray-800 text-center mb-8">How It Works</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div className="text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl font-bold text-amber-600">1</span>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Ask Your Question</h3>
            <p className="text-gray-600">
              Describe your problem or question in detail. Our AI will automatically categorize it.
            </p>
          </div>
          <div className="text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl font-bold text-amber-600">2</span>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Get Matched</h3>
            <p className="text-gray-600">
              Experts in your topic area will review your request and offer to help.
            </p>
          </div>
          <div className="text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl font-bold text-amber-600">3</span>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Get Answers</h3>
            <p className="text-gray-600">
              Chat with your expert, get personalized advice, and solve your problem.
            </p>
          </div>
        </div>
      </section>

      {/* Categories Overview */}
      <section className="mb-12">
        <h2 className="text-3xl font-bold text-gray-800 text-center mb-8">Popular Categories</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
          {[
            { name: 'Technology', color: 'bg-blue-100 text-blue-700 hover:bg-blue-200' },
            { name: 'Health', color: 'bg-rose-100 text-rose-700 hover:bg-rose-200' },
            { name: 'Legal', color: 'bg-purple-100 text-purple-700 hover:bg-purple-200' },
            { name: 'Finance', color: 'bg-emerald-100 text-emerald-700 hover:bg-emerald-200' },
            { name: 'Education', color: 'bg-indigo-100 text-indigo-700 hover:bg-indigo-200' },
            { name: 'Career', color: 'bg-amber-100 text-amber-700 hover:bg-amber-200' },
          ].map((cat) => (
            <Link
              key={cat.name}
              to="/consultations"
              className={`${cat.color} rounded-lg py-5 px-4 text-center transition font-medium shadow-sm`}
            >
              {cat.name}
            </Link>
          ))}
        </div>
      </section>

      {/* Top Rated Experts */}
      <section className="mb-12">
        <div className="text-center mb-6">
          <h2 className="text-3xl font-bold text-gray-800">Top Rated Experts</h2>
          <p className="text-gray-600 mt-1">Our highest rated professionals</p>
        </div>
        <div className="text-right mb-4">
          <Link to="/experts" className="text-amber-600 hover:text-amber-700 font-medium">
            View All Experts →
          </Link>
        </div>
        
        {expertsLoading ? (
          <div className="flex justify-center py-10">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
          </div>
        ) : topExperts.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {topExperts.map((expert) => (
              <div 
                key={expert.id} 
                className="bg-white rounded-xl shadow-md p-6 hover:shadow-lg transition cursor-pointer"
                onClick={() => navigate(`/user/${expert.id}`)}
              >
                <div className="flex items-center mb-4">
                  <div className="w-16 h-16 rounded-full bg-amber-100 flex items-center justify-center text-amber-600 text-2xl font-bold mr-4">
                    {expert.avatarUrl ? (
                      <img src={expert.avatarUrl} alt={expert.fullName} className="w-full h-full rounded-full object-cover" />
                    ) : (
                      expert.fullName?.charAt(0)?.toUpperCase() || 'E'
                    )}
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-800">{expert.fullName}</h3>
                    <div className="flex items-center text-amber-500">
                      <svg className="w-5 h-5 fill-current" viewBox="0 0 20 20">
                        <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z"/>
                      </svg>
                      <span className="ml-1 font-medium">{expert.averageRating?.toFixed(1) || 'N/A'}</span>
                      <span className="ml-2 text-gray-500 text-sm">({expert.reviewsCount || 0} reviews)</span>
                    </div>
                  </div>
                </div>
                {expert.bio && (
                  <p className="text-gray-600 text-sm line-clamp-2">{expert.bio}</p>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-10 bg-gray-50 rounded-lg">
            <p className="text-lg text-gray-600">No experts available at the moment.</p>
          </div>
        )}
      </section>
    </div>
  );
};

export default HomePage;
