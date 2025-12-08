import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import ConsultationCard from '../components/ConsultationCard';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations } from '../features/consultations/consultationsSlice';

const HomePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { consultations, loading, error } = useAppSelector(state => state.consultations);
  
  useEffect(() => {
    // Fetch public consultations when component mounts
    dispatch(fetchConsultations());
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
                expertName={consultation.expertName || 'Unknown Expert'}
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
    </div>
  );
};

export default HomePage;
