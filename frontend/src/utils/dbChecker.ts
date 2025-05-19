import api from '../api/api';

/**
 * Check if consultations table has data
 * @returns Promise with boolean result and count
 */
export const checkConsultationsTableEmpty = async (): Promise<{isEmpty: boolean, count: number}> => {
  try {
    // Make a request to get consultations or count
    const response = await api.get('/consultations');
    
    // Log the response for debugging
    console.log('Consultations response:', response.data);
    
    // Check if the response has data
    const isEmpty = !response.data || 
                   (Array.isArray(response.data) && response.data.length === 0);
    
    // Get count if available
    const count = Array.isArray(response.data) ? response.data.length : 0;
    
    return { isEmpty, count };
  } catch (error) {
    console.error('Error checking consultations table:', error);
    return { isEmpty: true, count: 0 };
  }
};
