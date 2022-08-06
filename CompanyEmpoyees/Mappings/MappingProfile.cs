using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;
using System;

namespace CompanyEmployees.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDTO>()
                .ForMember(c => c.FullAddress,
                    opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<Employee, EmployeeDTO>();
            CreateMap<CompanyForCreationDTO, Company>();
            CreateMap<EmployeeForCreationDTO, Employee>();
            CreateMap<EmployeeForUpdateDTO, Employee>().ReverseMap();
            CreateMap<CompanyForUpdateDTO, Company>();
            CreateMap<UserForRegistrationDTO, User>();
        }
    }
}
