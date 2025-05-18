using Askify.BusinessLogicLayer.DTO;
using Askify.DataAccessLayer.Entities;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>();

            // Post
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag.Name)))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));

            CreateMap<CreatePostDto, Post>();

            // Comment
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));

            // Consultation
            CreateMap<Consultation, ConsultationDto>();
            CreateMap<CreateConsultationDto, Consultation>();

            // Message
            CreateMap<Message, MessageDto>();

            // Feedback
            CreateMap<Feedback, FeedbackDto>();

            // Notification
            CreateMap<Notification, NotificationDto>();

            // Tag
            CreateMap<Tag, TagDto>();

            // SavedPost
            CreateMap<SavedPost, SavedPostDto>()
                .ForMember(dest => dest.PostTitle, opt => opt.MapFrom(src => src.Post.Title));

            // PostLike
            CreateMap<PostLike, PostLikeDto>();

            // CommentLike
            CreateMap<CommentLike, CommentLikeDto>();

            // Subscription
            CreateMap<Subscription, SubscriptionDto>();

            // Payment
            CreateMap<Payment, PaymentDto>();

            // Report
            CreateMap<Report, ReportDto>();
        }
    }
}
