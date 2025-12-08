import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAppSelector } from '../hooks';
import api from '../api/api';

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
  isLikedByCurrentUser?: boolean;
  isSavedByCurrentUser?: boolean;
}

interface Comment {
  id: number;
  content: string;
  authorId: string;
  authorName: string;
  createdAt: string;
}

const BlogPostPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAppSelector(state => state.auth);
  
  const [post, setPost] = useState<Post | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [newComment, setNewComment] = useState('');
  const [submittingComment, setSubmittingComment] = useState(false);
  const [liked, setLiked] = useState(false);
  const [saved, setSaved] = useState(false);
  const [likesCount, setLikesCount] = useState(0);

  useEffect(() => {
    const fetchPost = async () => {
      if (!id) return;
      
      setLoading(true);
      try {
        const response = await api.get(`/posts/${id}`);
        setPost(response.data);
        setLiked(response.data.isLikedByCurrentUser || false);
        setSaved(response.data.isSavedByCurrentUser || false);
        setLikesCount(response.data.likesCount || 0);
        
        // Fetch comments
        try {
          const commentsResponse = await api.get(`/comments/post/${id}`);
          if (Array.isArray(commentsResponse.data)) {
            setComments(commentsResponse.data);
          }
        } catch (err) {
          console.log('No comments endpoint or no comments');
        }
      } catch (err) {
        console.error('Error fetching post:', err);
        setError('Failed to load post');
      } finally {
        setLoading(false);
      }
    };
    
    fetchPost();
  }, [id]);

  const handleLike = async () => {
    if (!isAuthenticated || !id) return;
    
    try {
      if (liked) {
        await api.delete(`/posts/${id}/like`);
        setLikesCount(prev => prev - 1);
      } else {
        await api.post(`/posts/${id}/like`);
        setLikesCount(prev => prev + 1);
      }
      setLiked(!liked);
    } catch (err) {
      console.error('Error toggling like:', err);
    }
  };

  const handleSave = async () => {
    if (!isAuthenticated || !id) return;
    
    try {
      if (saved) {
        await api.delete(`/posts/${id}/save`);
      } else {
        await api.post(`/posts/${id}/save`);
      }
      setSaved(!saved);
    } catch (err) {
      console.error('Error toggling save:', err);
    }
  };

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !id || !isAuthenticated) return;
    
    setSubmittingComment(true);
    try {
      await api.post(`/comments`, {
        postId: parseInt(id),
        content: newComment
      });
      
      // Refetch comments to get the full comment data
      const commentsResponse = await api.get(`/comments/post/${id}`);
      if (Array.isArray(commentsResponse.data)) {
        setComments(commentsResponse.data);
      }
      setNewComment('');
    } catch (err) {
      console.error('Error posting comment:', err);
    } finally {
      setSubmittingComment(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-amber-500"></div>
      </div>
    );
  }

  if (error || !post) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-4">
          <p className="text-red-700">{error || 'Post not found'}</p>
        </div>
        <button
          onClick={() => navigate(-1)}
          className="text-amber-600 hover:text-amber-800"
        >
          ← Go back
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Back button */}
      <button
        onClick={() => navigate(-1)}
        className="text-amber-600 hover:text-amber-800 mb-6 flex items-center"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 mr-1" viewBox="0 0 20 20" fill="currentColor">
          <path fillRule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clipRule="evenodd" />
        </svg>
        Back
      </button>

      {/* Post content */}
      <article className="bg-white rounded-lg shadow-md overflow-hidden">
        <div className="p-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-4">{post.title}</h1>
          
          <div className="flex items-center text-gray-500 mb-6">
            <Link to={`/user/${post.authorId}`} className="w-10 h-10 rounded-full bg-amber-100 flex items-center justify-center mr-3 hover:bg-amber-200 transition">
              <span className="text-amber-700 font-semibold">
                {post.authorName?.charAt(0) || 'U'}
              </span>
            </Link>
            <div>
              <Link to={`/user/${post.authorId}`} className="font-medium text-gray-700 hover:text-amber-600 transition">
                {post.authorName}
              </Link>
              <p className="text-sm">{new Date(post.createdAt).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
              })}</p>
            </div>
          </div>

          {post.tags && post.tags.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-6">
              {post.tags.map((tag, index) => (
                <span 
                  key={index}
                  className="bg-amber-100 text-amber-800 text-sm px-3 py-1 rounded-full"
                >
                  {tag}
                </span>
              ))}
            </div>
          )}

          <div className="prose max-w-none text-gray-700 whitespace-pre-line mb-8">
            {post.content}
          </div>

          {/* Actions */}
          <div className="flex items-center space-x-6 pt-6 border-t border-gray-200">
            <button
              onClick={handleLike}
              disabled={!isAuthenticated}
              className={`flex items-center space-x-2 ${
                liked ? 'text-red-500' : 'text-gray-500 hover:text-red-500'
              } ${!isAuthenticated ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill={liked ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
              </svg>
              <span>{likesCount} {likesCount === 1 ? 'Like' : 'Likes'}</span>
            </button>

            <button
              onClick={handleSave}
              disabled={!isAuthenticated}
              className={`flex items-center space-x-2 ${
                saved ? 'text-amber-500' : 'text-gray-500 hover:text-amber-500'
              } ${!isAuthenticated ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill={saved ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z" />
              </svg>
              <span>{saved ? 'Saved' : 'Save'}</span>
            </button>

            <span className="flex items-center space-x-2 text-gray-500">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
              </svg>
              <span>{comments.length} {comments.length === 1 ? 'Comment' : 'Comments'}</span>
            </span>
          </div>
        </div>
      </article>

      {/* Comments section */}
      <section className="mt-8">
        <h2 className="text-2xl font-bold text-gray-800 mb-6">Comments</h2>
        
        {/* Comment form */}
        {isAuthenticated ? (
          <form onSubmit={handleSubmitComment} className="mb-8">
            <textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Write a comment..."
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-500"
              rows={3}
            />
            <button
              type="submit"
              disabled={!newComment.trim() || submittingComment}
              className="mt-2 bg-amber-600 hover:bg-amber-700 text-white px-6 py-2 rounded disabled:opacity-50"
            >
              {submittingComment ? 'Posting...' : 'Post Comment'}
            </button>
          </form>
        ) : (
          <div className="bg-gray-50 p-4 rounded-lg mb-8">
            <p className="text-gray-600">
              <Link to="/login" className="text-amber-600 hover:text-amber-800">Log in</Link> to leave a comment.
            </p>
          </div>
        )}

        {/* Comments list */}
        {comments.length === 0 ? (
          <p className="text-gray-500 italic">No comments yet. Be the first to comment!</p>
        ) : (
          <div className="space-y-4">
            {comments.map(comment => (
              <div key={comment.id} className="bg-white p-4 rounded-lg shadow">
                <div className="flex items-center mb-2">
                  <Link to={`/user/${comment.authorId}`} className="w-8 h-8 rounded-full bg-amber-100 flex items-center justify-center mr-2 hover:bg-amber-200 transition">
                    <span className="text-amber-700 text-sm font-semibold">
                      {comment.authorName?.charAt(0) || 'U'}
                    </span>
                  </Link>
                  <div>
                    <Link to={`/user/${comment.authorId}`} className="font-medium text-gray-700 hover:text-amber-600 transition">
                      {comment.authorName}
                    </Link>
                    <p className="text-xs text-gray-500">
                      {new Date(comment.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
                <p className="text-gray-700">{comment.content}</p>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
};

export default BlogPostPage;
