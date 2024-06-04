using AutoMapper;
using TestVnPay.DTOs;
using TestVnPay.Models;

namespace TestVnPay.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() 
        {
            CreateMap<Payments, PaymentDtos>().ReverseMap();
        }
    }
}
