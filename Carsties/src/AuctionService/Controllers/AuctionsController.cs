using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(IAuctionRepository repo,IMapper _mapper, IPublishEndpoint publishEndpoint): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await repo.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await repo.GetAuctionByIdAsync(id);

        if (auction == null)
        {
            return NotFound();
        }
        
        return auction;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        
        auction.Seller = User.Identity.Name;
        
        repo.AddAuction(auction);
        
        var newAuction = _mapper.Map<AuctionDto>(auction);
        
        await publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        
        var result = await repo.SaveChangesAsync();


        
        if(!result) return BadRequest("could not create auction");
        return CreatedAtAction(nameof(GetAuctionById),
            new { id = auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updatedAuctionDto)
    {
        var auction = await repo.GetAuctionEntityById(id);
        if (auction == null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }
        
        auction.Item.Make = updatedAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updatedAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updatedAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updatedAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updatedAuctionDto.Year ?? auction.Item.Year;
        
        await publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
        var result = await repo.SaveChangesAsync();
        if(!result) return BadRequest("could not update auction");
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await repo.GetAuctionEntityById(id);
        if (auction == null)
        {
            return NotFound();
        }
        
        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }
        repo.RemoveAuction(auction);
        await publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});
        var result = await repo.SaveChangesAsync();
        if(!result) return BadRequest("could not delete auction");
        return Ok();
    }
}