using Askify.BusinessLogicLayer.DTO;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using AutoMapper;

namespace Askify.BusinessLogicLayer.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TagDto?> GetByIdAsync(int id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            return tag != null ? _mapper.Map<TagDto>(tag) : null;
        }

        public async Task<TagDto?> GetByNameAsync(string name)
        {
            var tag = await _unitOfWork.Tags.GetByNameAsync(name);
            return tag != null ? _mapper.Map<TagDto>(tag) : null;
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync()
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }

        public async Task<int> CreateTagAsync(string name)
        {
            var existingTag = await _unitOfWork.Tags.GetByNameAsync(name);
            if (existingTag != null)
                return existingTag.Id;

            var tag = new Tag { Name = name };
            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.CompleteAsync();

            return tag.Id;
        }
    }
}
