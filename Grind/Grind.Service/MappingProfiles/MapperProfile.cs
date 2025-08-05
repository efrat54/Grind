// Grind.Service\MappingProfiles\MapperProfile.cs

using AutoMapper;
using Grind.Core.DTOs;
using Grind.Core.Entities;
using Grind.Core.Enums;
using System;
using System.Linq;

namespace Grind.Service.MappingProfiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // --- מיפויים עבור CLIENT ---
            CreateMap<UserRegistrationDTO, Client>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ClientPreferredTimes, opt => opt.MapFrom(src => src.PreferredTimes))
                .ForMember(dest => dest.ClientPreferredClasses, opt => opt.MapFrom(src => src.PreferredClasses));

            // מיפויים עבור Address
            CreateMap<AddressDTO, Address>();
            CreateMap<Address, AddressDTO>(); // <--- הוספה חשובה זו


            CreateMap<PreferredTimeDTO, ClientPreferredTime>();

            CreateMap<PreferredClassDTO, ClientPreferredClass>()
                .ForMember(dest => dest.ClassCategory, opt => opt.MapFrom(src =>
                    (Grind.Core.Enums.ClassCategory)Enum.Parse(typeof(Grind.Core.Enums.ClassCategory), src.ClassCategoryName)));

            CreateMap<Client, ClientDTO>()
    .ForMember(dest => dest.PreferredDays, opt => opt.MapFrom(src =>
        src.ClientPreferredTimes.Select(cpt => cpt.PreferredDay).ToList()))
    .ForMember(dest => dest.PreferredCategories, opt => opt.MapFrom(src =>
        src.ClientPreferredClasses.Select(cpc => cpc.ClassCategory.ToString()).ToList()));


            // --- מיפויים עבור TRAINER ---
            CreateMap<TrainerRegistrationDTO, Trainer>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Specializations, opt => opt.MapFrom(src => src.Specializations));

            // מיפוי מ-Trainer Entity ל-TrainerDTO
            // נניח ש-TrainerDTO מכיל AddressDTO ו-SpecializationDTO, אז הם ימופו אוטומטית אם המיפוי שלהם קיים.
            CreateMap<Trainer, TrainerDTO>();


            // --- מיפויים עבור CLASS ---
            CreateMap<ClassCreateDTO, Class>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentParticipants, opt => opt.Ignore())
                .ForMember(dest => dest.TrainerId, opt => opt.Ignore())
                .ForMember(dest => dest.IsCancelled, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore());

            CreateMap<Class, ClassDTO>()
                .ForMember(dest => dest.TrainerUsername, opt => opt.MapFrom(src => src.Trainer.Username));
            //CreateMap<Client, ClientProfileUpdateDTO>(); // או ClientDTO
            //CreateMap<ClientProfileUpdateDTO, Client>();
        }
    }
}