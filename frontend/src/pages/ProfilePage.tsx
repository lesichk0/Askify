import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import api from '../api/api';

interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  bio?: string;
  joinDate: string;
  role: string;
  postsCount: number;
  consultationsCount: number;
}

interface Post {
  id: number;
  title: string;
  content: string;
  authorId: string;
  authorName: string;
  createdAt: string;
  tags: string[];
  likesCount: number;
  commentsCount: number;
  coverImageUrl?: string;
}

const ProfilePage: React.FC = () => {
  const { user, isAuthenticated } = useAppSelector((state) => state.auth);
  const navigate = useNavigate();
  
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Redirect to login if not authenticated
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    // Validate that user object exists and has valid ID before proceeding
    if (!user || !user.id) {
        console.log(user);
      setError("User information is missing. Please login again.");
      setLoading(false);
      return;
    }

    const fetchProfileData = async () => {
      setLoading(true);
      try {
        // Make sure we have a valid user id
        if (!user || !user.id) {
          throw new Error("User ID is missing");
        }

        const userId = user.id;
        console.log(`Using user ID for profile data: ${userId}`);

        // Get user profile data - use profile endpoint first
        try {
          const profileResponse = await api.get('/users/profile');
          setProfile(profileResponse.data);
        } catch (profileErr) {
          console.warn('Error fetching from profile endpoint, trying user endpoint:', profileErr);
          // Fallback to getting user by ID
          const userResponse = await api.get(`/users/${userId}`);
          setProfile(userResponse.data);
        }

        // Fetch user posts
        try {
          console.log(`Fetching posts for user ID: ${userId}`);
          const postsResponse = await api.get(`/posts/user/${userId}`);
          
          // Handle the case where posts is empty (which is valid, not an error)
          if (Array.isArray(postsResponse.data)) {
            setPosts(postsResponse.data);
          } else {
            // If response is not an array, set as empty array
            console.warn('Posts response is not an array:', postsResponse.data);
            setPosts([]);
          }
        } catch (postsErr) {
          console.warn('Error fetching user posts:', postsErr);
          // Don't treat this as a fatal error, just set empty posts
          setPosts([]);
        }
      } catch (err: any) {
        console.error('Error fetching profile data:', err);
        setError(err.message || 'Failed to load profile data');
      } finally {
        setLoading(false);
      }
    };

    fetchProfileData();
  }, [isAuthenticated, navigate, user]);

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto mt-10 p-6 bg-red-50 border border-red-200 rounded-lg text-center">
        <h2 className="text-xl font-bold text-red-700 mb-2">Error</h2>
        <p className="text-red-600">{error}</p>
        <button
          onClick={() => navigate('/')}
          className="mt-4 bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
        >
          Go Home
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      {/* Profile Header */}
      <div className="bg-white rounded-xl shadow-md overflow-hidden mb-8">
        <div className="p-8">
          <div className="flex flex-col md:flex-row items-center md:items-start gap-6">
            {/* Profile Image or Initial */}
            <div className="w-32 h-32 rounded-full bg-amber-100 flex items-center justify-center text-4xl font-bold text-amber-700 border-2 border-amber-300">
              {profile?.fullName.charAt(0) || 'U'}
            </div>
            
            <div className="flex-1 text-center md:text-left">
              <h1 className="text-3xl font-bold text-gray-800">{profile?.fullName}</h1>
              <p className="text-amber-600 font-medium">{profile?.role}</p>
              <p className="text-gray-600 mt-2">{profile?.bio || 'No bio provided'}</p>
              
              <div className="flex flex-wrap gap-4 mt-4 justify-center md:justify-start">
                <div className="text-center">
                  <span className="block text-2xl font-bold text-gray-800">{profile?.postsCount || 0}</span>
                  <span className="text-gray-600">Posts</span>
                </div>
                <div className="text-center">
                  <span className="block text-2xl font-bold text-gray-800">{profile?.consultationsCount || 0}</span>
                  <span className="text-gray-600">Consultations</span>
                </div>
              </div>
            </div>
            
            {/* Edit Profile Button */}
            <div>
              <button 
                className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded-md"
                onClick={() => navigate('/profile/edit')}
              >
                Edit Profile
              </button>
            </div>
          </div>
        </div>
      </div>
      
      {/* User Posts */}
      <div className="mb-8">
        <h2 className="text-2xl font-bold text-gray-800 mb-6">My Posts</h2>
        
        {posts.length === 0 ? (
          <div className="bg-gray-50 rounded-lg p-10 text-center">
            <p className="text-gray-600 mb-4">You haven't created any posts yet.</p>
            <button 
              className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              onClick={() => navigate('/posts/new')}
            >
              Create Your First Post
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {posts.map((post) => (
              <div 
                key={post.id} 
                className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow cursor-pointer"
                onClick={() => navigate(`/posts/${post.id}`)}
              >
                {post.coverImageUrl && (
                  <img 
                    src={post.coverImageUrl} 
                    alt={post.title} 
                    className="w-full h-48 object-cover"
                  />
                )}
                <div className="p-6">
                  <h3 className="text-xl font-bold text-gray-800 mb-2 truncate">{post.title}</h3>
                  <p className="text-gray-600 mb-4 line-clamp-3">{post.content}</p>
                  
                  <div className="flex flex-wrap gap-2 mb-4">
                    {post.tags.map((tag, index) => (
                      <span 
                        key={index} 
                        className="bg-amber-100 text-amber-800 text-xs px-2 py-1 rounded"
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                  
                  <div className="flex justify-between text-sm text-gray-500">
                    <span>{new Date(post.createdAt).toLocaleDateString()}</span>
                    <div className="flex gap-3">
                      <span className="flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                          <path d="M2 10.5a1.5 1.5 0 113 0v6a1.5 1.5 0 01-3 0v-6zM6 10.333v5.43a2 2 0 001.106 1.79l.05.025A4 4 0 008.943 18h5.416a2 2 0 001.962-1.608l1.2-6A2 2 0 0015.56 8H12V4a2 2 0 00-2-2 1 1 0 00-1 1v.667a4 4 0 01-.8 2.4L6.8 7.933a4 4 0 00-.8 2.4z" />
                        </svg>
                        {post.likesCount}
                      </span>
                      <span className="flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 mr-1" viewBox="0 0 20 20" fill="currentColor">
                          <path fillRule="evenodd" d="M18 5v8a2 2 0 01-2 2h-5l-5 4v-4H4a2 2 0 01-2-2V5a2 2 0 012-2h12a2 2 0 012 2zM7 8H5v2h2V8zm2 0h2v2H9V8zm6 0h-2v2h2V8z" clipRule="evenodd" />
                        </svg>
                        {post.commentsCount}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
      
      {/* Additional Profile Sections - can be expanded later */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Consultations Summary */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-xl font-bold text-gray-800 mb-4">My Consultations</h3>
          <div className="flex justify-center">
            <button 
              className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              onClick={() => navigate('/consultations/my')}
            >
              View All Consultations
            </button>
          </div>
        </div>
        
        {/* Saved Posts */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-xl font-bold text-gray-800 mb-4">Saved Posts</h3>
          <div className="flex justify-center">
            <button 
              className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
              onClick={() => navigate('/posts/saved')}
            >
              View Saved Posts
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
