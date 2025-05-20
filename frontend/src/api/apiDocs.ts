/**
 * API Documentation
 * This file documents all available endpoints in the Askify API.
 */

export const ApiEndpoints = {
  Auth: {
    login: '/auth/login', // POST
    register: '/auth/register', // POST
    me: '/auth/me', // GET
  },
  Users: {
    getAll: '/users', // GET
    getById: (id: string) => `/users/${id}`, // GET
    experts: '/users/experts', // GET
    search: (query: string) => `/users/search?query=${query}`, // GET
    update: (id: string) => `/users/${id}`, // PUT
    block: (id: string) => `/users/${id}/block`, // POST
    unblock: (id: string) => `/users/${id}/unblock`, // POST
    verifyExpert: (id: string) => `/users/${id}/verify`, // POST
    profile: '/users/profile', // GET - Current user's profile
  },
  Posts: {
    getAll: '/posts', // GET
    getById: (id: number) => `/posts/${id}`, // GET
    getByUserId: (userId: string) => `/posts/user/${userId}`, // GET
    create: '/posts', // POST
    update: (id: number) => `/posts/${id}`, // PUT
    delete: (id: number) => `/posts/${id}`, // DELETE
    like: (id: number) => `/posts/${id}/like`, // POST
    unlike: (id: number) => `/posts/${id}/like`, // DELETE
    save: (id: number) => `/posts/${id}/save`, // POST
    unsave: (id: number) => `/posts/${id}/save`, // DELETE
    getSaved: '/posts/saved', // GET
  },
  Comments: {
    getById: (id: number) => `/comments/${id}`, // GET
    getByPostId: (postId: number) => `/comments/post/${postId}`, // GET
    create: '/comments', // POST
    update: (id: number) => `/comments/${id}`, // PUT
    delete: (id: number) => `/comments/${id}`, // DELETE
    like: (id: number) => `/comments/${id}/like`, // POST
    unlike: (id: number) => `/comments/${id}/like`, // DELETE
  },
  Consultations: {
    getAll: '/consultations', // GET
    getById: (id: number) => `/consultations/${id}`, // GET
    getMy: '/consultations/my', // GET - User's consultations (as owner or expert)
    getMyOwned: '/consultations/my/owner', // GET - Only consultations user owns
    create: '/consultations', // POST
    update: (id: number) => `/consultations/${id}`, // PUT
    delete: (id: number) => `/consultations/${id}`, // DELETE
    complete: (id: number) => `/consultations/${id}/complete`, // POST
    accept: (id: number) => `/consultations/${id}/accept`, // POST
    cancel: (id: number) => `/consultations/${id}/cancel`, // POST
    getPublic: '/consultations/public', // GET
  },
  Messages: {
    getUserMessages: '/messages', // GET
    getById: (id: number) => `/messages/${id}`, // GET
    getForConsultation: (consultationId: number) => `/messages/consultation/${consultationId}`, // GET
    send: '/messages', // POST
    markAsRead: (id: number) => `/messages/${id}/read`, // PUT
  },
  Notifications: {
    getForCurrentUser: '/notifications', // GET
    getById: (id: number) => `/notifications/${id}`, // GET
    markAsRead: (id: number) => `/notifications/${id}/read`, // PUT
    markAllAsRead: '/notifications/read-all', // PUT
  },
  Feedbacks: {
    getAll: '/feedbacks', // GET
    getById: (id: number) => `/feedbacks/${id}`, // GET
    getForExpert: (expertId: string) => `/feedbacks/expert/${expertId}`, // GET
    create: '/feedbacks', // POST
  },
  Reports: {
    getAll: '/reports', // GET
    getById: (id: number) => `/reports/${id}`, // GET
    getByTargetId: (targetId: string) => `/reports/target/${targetId}`, // GET
    create: '/reports', // POST
    updateStatus: (id: number) => `/reports/${id}`, // PUT
  },
  Subscriptions: {
    getUserSubscriptions: (userId: string) => `/subscriptions/user/${userId}`, // GET
    getSubscribers: (expertId: string) => `/subscriptions/subscribers/${expertId}`, // GET
    subscribe: (targetUserId: string) => `/subscriptions/${targetUserId}`, // POST
    unsubscribe: (targetUserId: string) => `/subscriptions/${targetUserId}`, // DELETE
    checkIsSubscribed: (targetUserId: string) => `/subscriptions/check/${targetUserId}`, // GET
  },
};

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  bio?: string;
  joinDate: string;
  role: string;
  postsCount: number;
  consultationsCount: number;
}

export interface Post {
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

export interface Comment {
  id: number;
  content: string;
  authorId: string;
  authorName: string;
  postId: number;
  createdAt: string;
  likesCount: number;
}
