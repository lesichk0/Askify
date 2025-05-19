import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchConsultations } from '../features/consultations/consultationsSlice';
import ConsultationCard from '../components/ConsultationCard';
import type { Consultation } from '../types/consultation';

const ConsultationsPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { consultations, loading, error } = useAppSelector(state => state.consultations);
  
  useEffect(() => {
    // This will call your backend API when the component mounts
    dispatch(fetchConsultations());
  }, [dispatch]);
  
  // Filter for only completed and publicable consultations
  const publicConsultations = consultations?.filter(
    (consultation: Consultation) => consultation.status === 'completed' && consultation.publicable
  ) || [];
  
  if (loading) return <div className="text-center py-10">Loading consultations...</div>;
  if (error) return <div className="text-red-500 text-center py-10">Error: {error}</div>;
  
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">Public Consultations</h1>
      
      {publicConsultations.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 rounded-lg">
          <p className="text-lg text-gray-600">No public consultations available at the moment.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {publicConsultations.map((consultation: Consultation) => (
            <ConsultationCard 
              key={consultation.id}
              consultation={consultation}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default ConsultationsPage;
