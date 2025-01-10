﻿using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles:Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<AuctionDto, Auction>();
        CreateMap<CreateAuctionDto, Auction>().ForMember(x => x.Item,
            opt => opt.MapFrom(s=>s));
        CreateMap<CreateAuctionDto, Item>();
    }
}