using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(d => d.PhotoUrl, 
                           opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(d => d.Age, opt => opt.MapFrom(
                    src => src.DateOfBirth.CalculateAge()));
            CreateMap<User, UserForDetailedDto>()
            .ForMember(d => d.PhotoUrl, 
                           opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url)
                           )
                           .ForMember(d => d.Age, opt => opt.MapFrom(
                    src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
        }
    }
}