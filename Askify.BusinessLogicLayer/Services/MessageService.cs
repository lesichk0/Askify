using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MessageDto?> GetByIdAsync(int id)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(id);
            return message != null ? _mapper.Map<MessageDto>(message) : null;
        }

        public async Task<IEnumerable<MessageDto>> GetUserMessagesAsync(string userId)
        {
            var messages = await _unitOfWork.Messages.GetUserMessagesAsync(userId);
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesForConsultationAsync(int consultationId)
        {
            var messages = await _unitOfWork.Messages.GetMessagesForConsultationAsync(consultationId);
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<int> SendMessageAsync(string senderId, CreateMessageDto messageDto)
        {
            var message = _mapper.Map<Message>(messageDto);
            message.SenderId = senderId;
            message.Status = "Sent";
            message.SentAt = DateTime.UtcNow;

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CompleteAsync();

            return message.Id;
        }

        public async Task<bool> MarkAsReadAsync(int messageId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null) return false;

            message.Status = "Read";
            _unitOfWork.Messages.Update(message);
            return await _unitOfWork.CompleteAsync();
        }
    }
}
