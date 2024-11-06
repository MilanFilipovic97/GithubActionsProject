using AutoMapper;
using IdentityApi.DTOs;
using IdentityApi.Models;

namespace IdentityApi.MappingProfiles
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            CreateMap<CompanyPostModel, Company>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
                    srcMember != null && (srcMember is not string || !string.IsNullOrWhiteSpace((string)srcMember))));
        }
    }
}
