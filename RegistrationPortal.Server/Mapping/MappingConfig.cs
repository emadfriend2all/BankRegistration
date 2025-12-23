using Mapster;
using RegistrationPortal.Server.DTOs;
using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Mapping
{
    public static class MappingConfig
    {
        public static void Configure()
        {
            // Configure CreateCustomerDto to CustMast mapping
            TypeAdapterConfig<CreateCustomerDto, CustMast>
                .NewConfig()
                .Ignore(dest => dest.Id) // Ignore ID - will be generated on backend
                .Ignore(dest => dest.AccountMasts) // Ignore navigation property
                .Ignore(dest => dest.CustomerFatca) // Ignore navigation property - will be handled separately
                .Map(dest => dest.CustIId, src => 0); // Will be set in service

            // Configure CreateCustomerDto to CustomerFatca mapping
            TypeAdapterConfig<CreateCustomerDto, CustomerFatca>
                .NewConfig()
                .Ignore(dest => dest.Id) // Ignore ID - will be generated on backend
                .Ignore(dest => dest.CustomerId) // Will be set in service
                .Ignore(dest => dest.CreatedAt) // Will be set in service
                .Ignore(dest => dest.UpdatedAt); // Will be set in service

            // Configure CreateAccountDto to AccountMast mapping
            TypeAdapterConfig<CreateAccountDto, AccountMast>
                .NewConfig()
                .Ignore(dest => dest.Id) // Ignore ID - will be generated on backend
                .Ignore(dest => dest.CustINo) // Will be set in service
                .Ignore(dest => dest.CustId) // Will be set in service
                .Ignore(dest => dest.CustMast); // Ignore navigation property
        }
    }
}
