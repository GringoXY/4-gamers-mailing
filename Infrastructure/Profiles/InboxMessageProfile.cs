using Contracts.Dtos;
using AutoMapper;
using Shared.Entities;

namespace Infrastructure.Profiles;

public class InboxMessageProfile : Profile
{
    public InboxMessageProfile() => CreateMap<InboxMessage, InboxMessageDto>();
}
