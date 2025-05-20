export interface Consultation {
  id: number;
  title: string;
  description: string;
  status:string;
  expertId: string;
  expertName: string;
  expertAvatar?: string;
  userId: string;
  createdAt: string;
  completedAt?: string;
  isPublicable: boolean;
  tags?: string[];
}
