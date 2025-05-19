export interface Consultation {
  id: number;
  title: string;
  description: string;
  status: 'pending' | 'accepted' | 'completed' | 'cancelled';
  expertId: string;
  expertName: string;
  expertAvatar?: string;
  userId: string;
  createdAt: string;
  completedAt?: string;
  publicable: boolean;
  tags?: string[];
}
