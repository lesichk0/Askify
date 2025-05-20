import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations } from '../features/consultations/consultationsSlice';
import ConsultationCard from '../components/ConsultationCard';
import type { Consultation as ConsultationType } from '../types/consultation';
import type { Consultation as ReduxConsultation } from '../features/consultations/consultationsSlice';

const ConsultationsPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { consultations, loading, error } = useAppSelector(state => state.consultations);
  
  useEffect(() => {
    // This will call your backend API when the component mounts
    dispatch(fetchConsultations());
  }, [dispatch]);
  
  // Filter for only publicable consultations (removing completed status filter)
  const publicConsultations = consultations?.filter(
    (consultation) => consultation.isPublicable === true
  ).map((consultation: ReduxConsultation): ConsultationType => ({
    ...consultation,
    expertName: consultation.expertName || "Unknown Expert", // Provide a default value for expertName
    isPublicable: true // Ensure the publicable property is set
  })) || [];
  
  // After we filter consultations, log to see what we have
  console.log('Public consultations after filtering (only by isPublicable):', publicConsultations.length, publicConsultations);
  
  // Format date helper function
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };
  
  if (loading) return <div className="text-center py-10">Loading consultations...</div>;
  if (error) return <div className="text-red-500 text-center py-10">Error: {error}</div>;
  
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">Public Consultations</h1>
      
      {/* Add a debug count */}
      <div className="mb-4 p-2 bg-blue-50 text-blue-800 rounded">
        Found {publicConsultations.length} public consultations
      </div>
      
      {publicConsultations.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 rounded-lg">
          <p className="text-lg text-gray-600">No public consultations available at the moment.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Use index in map to confirm iteration is happening */}
            {publicConsultations.map((consultation, index) => {
              console.log(`Rendering consultation ${index + 1}/${publicConsultations.length}:`, consultation.id);
              return (
                <ConsultationCard 
                    key={consultation.id}
                    id={consultation.id}
                    title={consultation.title || 'Untitled Consultation'}
                    expertName={consultation.expertName || 'Unknown Expert'}
                    description={consultation.description || 'No description available.'}
                    status={consultation.status as any}
                    date={formatDate(consultation.createdAt)}
                />
              );
            })}
        </div>
      )}
    </div>
  );
};

export default ConsultationsPage;
