import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import api from '../api/api';

interface UserProfile {
  id: string;
  fullName: string;
  email?: string;
  bio?: string;
  avatarUrl?: string;
  isVerifiedExpert: boolean;
  role?: string;
  averageRating?: number;
  reviewsCount?: number;
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
  isLikedByCurrentUser?: boolean;
}

const UserProfilePage: React.FC = () => {
  const { userId } = useParams<{ userId: string }>();
  const { user: currentUser } = useAppSelector((state) => state.auth);
  
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!userId) return;
      
      setLoading(true);
      try {
        // Fetch user profile
        const userResponse = await api.get(`/users/${userId}`);
        setProfile(userResponse.data);
        
        // Fetch user's posts
        try {
          const postsResponse = await api.get(`/posts/user/${userId}`);
          setPosts(postsResponse.data || []);
        } catch {
          // User might not have posts
          setPosts([]);
        }
      } catch (err) {
        console.error('Error fetching user profile:', err);
        setError('Failed to load user profile');
      } finally {
        setLoading(false);
      }
    };

    fetchUserProfile();
  }, [userId]);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const truncateContent = (content: string, maxLength: number = 150) => {
    if (content.length <= maxLength) return content;
    return content.substring(0, maxLength) + '...';
  };

  const handleLike = async (postId: number, isLiked: boolean) => {
    if (!currentUser) return;
    
    try {
      if (isLiked) {
        await api.delete(`/posts/${postId}/like`);
      } else {
        await api.post(`/posts/${postId}/like`);
      }
      
      // Update local state
      setPosts(prevPosts => 
        prevPosts.map(post => 
          post.id === postId 
            ? { 
                ...post, 
                isLikedByCurrentUser: !isLiked,
                likesCount: isLiked ? (post.likesCount || 1) - 1 : (post.likesCount || 0) + 1
              }
            : post
        )
      );
    } catch (error) {
      console.error('Error toggling like:', error);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="bg-red-50 text-red-700 p-4 rounded-lg">
          {error || 'User not found'}
        </div>
      </div>
    );
  }

  const isOwnProfile = currentUser?.id === userId;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Profile Header */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <div className="flex items-start space-x-6">
          {/* Avatar */}
          <div className="flex-shrink-0">
            {profile.avatarUrl ? (
              <img 
                src={profile.avatarUrl} 
                alt={profile.fullName}
                className="w-24 h-24 rounded-full object-cover"
              />
            ) : (
              <div className="w-24 h-24 rounded-full bg-amber-500 flex items-center justify-center text-white text-3xl font-bold">
                {profile.fullName?.charAt(0).toUpperCase() || '?'}
              </div>
            )}
          </div>
          
          {/* Profile Info */}
          <div className="flex-1">
            <div className="flex items-center space-x-3">
              <h1 className="text-2xl font-bold text-gray-800">{profile.fullName}</h1>
              {profile.isVerifiedExpert && (
                <span className="bg-amber-100 text-amber-800 px-2 py-1 rounded-full text-sm font-medium">
                  ✓ Verified Expert
                </span>
              )}
            </div>
            
            {/* Expert Rating Display */}
            {profile.isVerifiedExpert && (
              <div className="flex items-center mt-2">
                {profile.averageRating !== undefined && profile.averageRating !== null ? (
                  <>
                    <div className="flex text-yellow-500">
                      {[1, 2, 3, 4, 5].map((star) => (
                        <span key={star} className={star <= Math.round(profile.averageRating!) ? 'text-yellow-500' : 'text-gray-300'}>
                          ★
                        </span>
                      ))}
                    </div>
                    <span className="ml-2 text-gray-700 font-medium">{profile.averageRating.toFixed(1)}</span>
                    <span className="ml-1 text-gray-500">({profile.reviewsCount || 0} reviews)</span>
                  </>
                ) : (
                  <span className="text-gray-500 italic">No reviews yet</span>
                )}
              </div>
            )}
            
            {profile.bio && (
              <p className="text-gray-600 mt-2">{profile.bio}</p>
            )}
            
            {isOwnProfile && (
              <Link 
                to="/profile" 
                className="inline-block mt-4 text-amber-600 hover:text-amber-800"
              >
                Edit Profile
              </Link>
            )}
          </div>
        </div>
      </div>

      {/* User's Posts */}
      <div>
        <h2 className="text-xl font-bold text-gray-800 mb-4">
          Posts by {profile.fullName}
        </h2>
        
        {posts.length === 0 ? (
          <div className="bg-gray-50 rounded-lg p-8 text-center">
            <p className="text-gray-500">No posts yet.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {posts.map((post) => (
              <div key={post.id} className="bg-white rounded-lg shadow-md p-6">
                <Link to={`/blog/${post.id}`}>
                  <h3 className="text-lg font-semibold text-gray-800 hover:text-amber-600 transition">
                    {post.title}
                  </h3>
                </Link>
                
                <p className="text-gray-600 mt-2">{truncateContent(post.content)}</p>
                
                <div className="flex items-center justify-between mt-4">
                  <div className="flex items-center space-x-4 text-sm text-gray-500">
                    <span>{formatDate(post.createdAt)}</span>
                    <button 
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        handleLike(post.id, post.isLikedByCurrentUser || false);
                      }}
                      className={`flex items-center hover:text-red-500 transition-colors ${
                        post.isLikedByCurrentUser ? 'text-red-500' : ''
                      }`}
                      disabled={!currentUser}
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" fill={post.isLikedByCurrentUser ? "currentColor" : "none"} viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                      </svg>
                      {post.likesCount || 0}
                    </button>
                    <span className="flex items-center">
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
                      </svg>
                      {post.commentsCount || 0}
                    </span>
                  </div>
                  
                  {post.tags && post.tags.length > 0 && (
                    <div className="flex flex-wrap gap-2">
                      {post.tags.slice(0, 3).map((tag, index) => (
                        <span 
                          key={index}
                          className="bg-gray-100 text-gray-600 px-2 py-1 rounded text-xs"
                        >
                          {tag}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
                
                <Link 
                  to={`/blog/${post.id}`}
                  className="inline-block mt-3 text-amber-600 hover:text-amber-800 text-sm font-medium"
                >
                  Read more →
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default UserProfilePage;
