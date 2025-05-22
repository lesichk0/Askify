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
  const [showCreatePost, setShowCreatePost] = useState(false);
  const [postTitle, setPostTitle] = useState('');
  const [postContent, setPostContent] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [postError, setPostError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated || !user) {
      navigate('/login');
      return;
    }
    
    const fetchProfileData = async () => {
      setLoading(true);
      try {
        // Fetch profile data
        const profileResponse = await api.get(`/users/${user.id}`);
        setProfile(profileResponse.data);
        
        // Fetch posts
        const postsResponse = await api.get(`/posts/user/${user.id}`);
        setPosts(postsResponse.data);
      } catch (err: any) {
        console.error('Error fetching profile data:', err);
        setError(err.message || 'Failed to load profile data');
      } finally {
        setLoading(false);
      }
    };

    fetchProfileData();
  }, [isAuthenticated, navigate, user]);

  const handleCreatePost = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!postTitle.trim() || !postContent.trim()) {
      setPostError('Please fill in all fields');
      return;
    }
    
    setSubmitting(true);
    setPostError(null);
    
    try {
      // Make the API call using the axios interceptor for auth
      await api.post('/posts', {
        title: postTitle,
        content: postContent
      });
      
      // After successful creation, refresh the posts list
      if (user?.id) {
        const postsResponse = await api.get(`/posts/user/${user.id}`);
        if (Array.isArray(postsResponse.data)) {
          setPosts(postsResponse.data);
        }
      }
      
      setPostTitle('');
      setPostContent('');
      setShowCreatePost(false);
      setSuccessMessage('Post created successfully!');
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        setSuccessMessage(null);
      }, 3000);
    } catch (err: any) {
      console.error('Error creating post:', err);
      
      setPostError(err.response?.data?.message || 'Failed to create post. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

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
        <p className="text-red-600 mb-4">{error}</p>
        <button
          onClick={() => navigate('/')}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded"
        >
          Return to Home
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto py-8 px-4">
      {successMessage && (
        <div className="mb-6 p-4 bg-green-50 border-l-4 border-green-500 text-green-700">
          <p>{successMessage}</p>
        </div>
      )}
      
      {/* Profile Header */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="flex items-start gap-6">
          <div className="flex-shrink-0">
            <div className="w-24 h-24 bg-gradient-to-r from-amber-500 to-amber-600 rounded-full flex items-center justify-center text-4xl font-bold text-white">
              {user?.fullName ? user.fullName.charAt(0).toUpperCase() : '?'}
            </div>
          </div>
          
          <div className="flex-grow">
            <h1 className="text-2xl font-bold text-gray-800 mb-1">{profile?.fullName}</h1>
            <p className="text-gray-500 mb-2">{profile?.email}</p>
            <p className="text-gray-500 mb-3">Joined: {profile?.joinDate ? new Date(profile.joinDate).toLocaleDateString() : 'Unknown'}</p>
            <p className="text-gray-700">{profile?.bio || 'No bio available'}</p>
            
            <div className="mt-4 flex gap-4">
              <div className="text-center">
                <span className="block text-xl font-bold text-amber-600">{profile?.postsCount || 0}</span>
                <span className="text-sm text-gray-500">Posts</span>
              </div>
              
              <div className="text-center">
                <span className="block text-xl font-bold text-amber-600">{profile?.consultationsCount || 0}</span>
                <span className="text-sm text-gray-500">Consultations</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Create Post Button and Section */}
      <div className="mb-6">
        <button
          onClick={() => setShowCreatePost(!showCreatePost)}
          className="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded flex items-center"
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-2" viewBox="0 0 20 20" fill="currentColor">
            <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" />
          </svg>
          Create Post
        </button>
      </div>
      
      {/* Create Post Form */}
      {showCreatePost && (
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-bold text-gray-800 mb-4">Create New Post</h2>
          
          {postError && (
            <div className="mb-4 p-3 bg-red-50 border-l-4 border-red-500 text-red-700">
              <p>{postError}</p>
            </div>
          )}
          
          <form onSubmit={handleCreatePost}>
            <div className="mb-4">
              <label htmlFor="postTitle" className="block text-gray-700 text-sm font-bold mb-2">
                Title
              </label>
              <input
                type="text"
                id="postTitle"
                value={postTitle}
                onChange={(e) => setPostTitle(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
                placeholder="Enter post title"
                required
              />
            </div>
            
            <div className="mb-4">
              <label htmlFor="postContent" className="block text-gray-700 text-sm font-bold mb-2">
                Content
              </label>
              <textarea
                id="postContent"
                value={postContent}
                onChange={(e) => setPostContent(e.target.value)}
                rows={6}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500"
                placeholder="Write your post content here..."
                required
              />
            </div>
            
            <div className="flex justify-end">
              <button
                type="button"
                onClick={() => setShowCreatePost(false)}
                className="px-4 py-2 text-gray-700 mr-2"
              >
                Cancel
              </button>
              <button
                type="submit"
                className={`bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded ${submitting ? 'opacity-50 cursor-not-allowed' : ''}`}
                disabled={submitting}
              >
                {submitting ? 'Creating...' : 'Create Post'}
              </button>
            </div>
          </form>
        </div>
      )}
      
      {/* Posts List */}
      <h2 className="text-xl font-bold text-gray-800 mb-4">Your Posts</h2>
      
      {posts.length === 0 ? (
        <div className="bg-gray-50 p-8 rounded-lg text-center">
          <p className="text-gray-600">You haven't created any posts yet.</p>
        </div>
      ) : (
        <div className="space-y-6">
          {posts.map(post => (
            <div key={post.id} className="bg-white rounded-lg shadow-md overflow-hidden">
              <div className="p-6">
                <h3 className="text-xl font-bold text-gray-800 mb-2">{post.title}</h3>
                <p className="text-gray-500 text-sm mb-4">
                  {new Date(post.createdAt).toLocaleDateString()}
                </p>
                <p className="text-gray-700 mb-4">{post.content.length > 150 ? `${post.content.substring(0, 150)}...` : post.content}</p>
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <span className="flex items-center text-gray-500">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                      </svg>
                      {post.likesCount}
                    </span>
                    <span className="flex items-center text-gray-500">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
                      </svg>
                      {post.commentsCount}
                    </span>
                  </div>
                  <button
                    onClick={() => navigate(`/posts/${post.id}`)}
                    className="text-amber-600 hover:text-amber-800"
                  >
                    Read more
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ProfilePage;
