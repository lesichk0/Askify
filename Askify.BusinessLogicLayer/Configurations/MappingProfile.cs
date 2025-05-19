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
            CreateMap<UpdateUserDto, User>();

            // Post
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PostTags.Select(pt => pt.Tag.Name)))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));
            CreateMap<CreatePostDto, Post>();
            CreateMap<UpdatePostDto, Post>();

            // Comment
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));
            CreateMap<CreateCommentDto, Comment>();
            CreateMap<UpdateCommentDto, Comment>();

            // Consultation
            CreateMap<Askify.DataAccessLayer.Entities.Consultation, ConsultationDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<CreateConsultationDto, Consultation>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<UpdateConsultationDto, Consultation>();

            // Message
            CreateMap<Message, MessageDto>();
            CreateMap<CreateMessageDto, Message>();

            // Feedback
            CreateMap<Feedback, FeedbackDto>();
            CreateMap<CreateFeedbackDto, Feedback>();

            // Report
            CreateMap<Report, ReportDto>();
            CreateMap<CreateReportDto, Report>();
            CreateMap<UpdateReportDto, Report>();

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
        }
    }
}
