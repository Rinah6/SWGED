using AutoMapper;
using API.Dto;
using API.Model;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, User>();

            CreateMap<Project, ProjectDto>();
            CreateMap<ProjectDto, Project>();

            CreateMap<User, UserDto>().ForMember(
                from => from.Nproject,
                to => to.MapFrom(a => a.Project!.Name)
            ).ForMember(
                from => from.ProjectId,
                to => to.MapFrom(a => a.ProjectId)
            );
            CreateMap<FlowHistoriqueDto, ValidationsHistory>();
            CreateMap<ValidationsHistory, FlowHistoriqueDto>();
            CreateMap<UserDto, User>().ForMember(
                from => from.ProjectId,
                to => to.MapFrom(a => a.ProjectId)
            ); ;
            CreateMap<User, string>();
            CreateMap<string, User>();

            CreateMap<Document, DocumentDto>();
            CreateMap<DocumentDto, Document>();
            CreateMap<ShowDocument, Document>();
            CreateMap<Document, ShowDocument>();

            CreateMap<DocumentByUserDto, UserDocument>();
            CreateMap<UserDocument, DocumentByUserDto>();

            CreateMap<UserDocument, DocumentRecipientsDto>();
            CreateMap<DocumentRecipientsDto, UserDocument>();

            CreateMap<Field, FieldDto>();
            CreateMap<FieldDto, Field>();
        }
    }
}
