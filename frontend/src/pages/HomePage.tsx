import React, { useEffect } from 'react';
import ConsultationCard from '../components/ConsultationCard';
import { useAppDispatch, useAppSelector } from '../hooks';
import { fetchPublicConsultations } from '../features/consultations/consultationsSlice';

// This would typically come from your API or Redux store
const sampleConsultations = [
  {
    id: 1,
    title: "JavaScript Modern Frameworks Consultation",
    expertName: "Alex Johnson",
    description: "Get expert advice on React, Angular, and Vue.js for your web application development. Learn about best practices and architecture patterns.",
    status: "pending" as const,
    date: "May 25, 2023"
  },
  {
    id: 2,
    title: "Database Design and Optimization",
    expertName: "Maria Garcia",
    description: "Consultation on designing efficient database schemas, indexing strategies, and query optimization for high-performance applications.",
    status: "accepted" as const,
    date: "May 27, 2023"
  },
  {
    id: 3,
    title: "Cloud Architecture and Deployment",
    expertName: "James Wilson",
    description: "Expert guidance on cloud infrastructure design, serverless architecture, and continuous deployment pipelines for scalable applications.",
    status: "completed" as const,
    date: "May 21, 2023"
  },
  {
    id: 4,
    title: "Mobile App Development Strategy",
    expertName: "Sarah Chen",
    description: "Strategic consultation on cross-platform vs. native app development, user experience design, and effective monetization strategies.",
    status: "pending" as const,
    date: "May 30, 2023"
  },
];

const HomePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { loading } = useAppSelector(state => state.consultations);
  
  useEffect(() => {
    // Fetch public consultations when component mounts
    dispatch(fetchPublicConsultations());
  }, [dispatch]);

  // Filter to only show completed consultations
  const completedConsultations = sampleConsultations.filter(
    consultation => consultation.status === 'completed'
  );
  
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
            <a 
              href="/consultations/new" 
              className="bg-white text-amber-700 hover:bg-gray-100 px-8 py-3 rounded-md font-semibold text-lg transition"
            >
              Book a Consultation
            </a>
            <a 
              href="/experts" 
              className="bg-amber-800 hover:bg-amber-900 px-8 py-3 rounded-md font-semibold text-lg transition"
            >
              Find an Expert
            </a>
          </div>
        </div>
      </section>

      <section className="mb-12">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-3xl font-bold text-gray-800">Featured Consultations</h2>
          <a href="/consultations" className="text-amber-600 hover:text-amber-700 font-medium">
            View All →
          </a>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {loading ? (
            <div className="col-span-full flex justify-center py-10">
              <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
            </div>
          ) : completedConsultations.length > 0 ? (
            completedConsultations.map((consultation) => (
              <ConsultationCard
                key={consultation.id}
                id={consultation.id}
                title={consultation.title}
                expertName={consultation.expertName}
                description={consultation.description}
                status={consultation.status}
                date={consultation.date}
              />
            ))
          ) : (
            <div className="col-span-full text-center py-10 bg-gray-50 rounded-lg">
              <p className="text-lg text-gray-600">No public consultations available at the moment.</p>
            </div>
          )}
        </div>
      </section>

      <section className="bg-gray-200 rounded-xl p-8 mb-12">
        <div className="text-center mb-8">
          <h2 className="text-3xl font-bold text-gray-800 mb-4">Why Choose Askify?</h2>
          <p className="text-lg text-gray-600 max-w-3xl mx-auto">
            Our platform connects you with verified experts ready to solve your most challenging problems.
          </p>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div className="bg-white p-6 rounded-lg shadow-md text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-amber-600" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Verified Experts</h3>
            <p className="text-gray-600">
              Every expert on our platform is thoroughly vetted for their expertise and credentials.
            </p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow-md text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-amber-600" viewBox="0 0 20 20" fill="currentColor">
                <path d="M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v3h8v-3zM6 8a2 2 0 11-4 0 2 2 0 014 0zM16 18v-3a5.972 5.972 0 00-.75-2.906A3.005 3.005 0 0119 15v3h-3zM4.75 12.094A5.973 5.973 0 004 15v3H1v-3a3 3 0 013.75-2.906z" />
              </svg>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Community Driven</h3>
            <p className="text-gray-600">
              Join a supportive community of learners and experts sharing knowledge.
            </p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow-md text-center">
            <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-amber-600" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
              </svg>
            </div>
            <h3 className="text-xl font-semibold text-gray-800 mb-2">Personalized Experience</h3>
            <p className="text-gray-600">
              Get tailored advice specific to your unique challenges and goals.
            </p>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;
