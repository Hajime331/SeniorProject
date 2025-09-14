using AutoMapper;
using Meow.Api.Data;
using Meow.Shared.Dtos;
using Meow.Shared.Dtos.Accounts;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AvatarProfileMapping : Profile
{
    public AvatarProfileMapping()
    {
        CreateMap<AvatarCatalog, AvatarDto>();
        CreateMap<MemberProfile, MemberProfileDto>();
    }
}