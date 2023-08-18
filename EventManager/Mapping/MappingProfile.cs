using AutoMapper;

namespace EventManager.Mapping
{
    public class EntityMappingProfile : Profile
    {
        public EntityMappingProfile()
        {
            CreateMap<Model.FileEvent, DTO.FileEvent>();
            CreateMap<Model.FileEvent, DTO.FolderEvent>();
        }
        
    }
}
