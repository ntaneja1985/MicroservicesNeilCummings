using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(AuctionDbContext _context,IMapper _mapper): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x=>x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x=>x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime())>0);
        }
        
        // var auctions = await _context.Auctions
        //     .Include(x => x.Item)
        //     .OrderBy(x => x.Item.Make)
        //     .ToListAsync();
        // return _mapper.Map<List<AuctionDto>>(auctions);
        
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }
        
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("could not create auction");
        return CreatedAtAction(nameof(GetAuctionById),
            new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updatedAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        //TODO: check seller == username
        
        auction.Item.Make = updatedAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updatedAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updatedAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updatedAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updatedAuctionDto.Year ?? auction.Item.Year;
        
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("could not update auction");
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
        {
            return NotFound();
        }
        
        //TODO: check seller == username
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("could not delete auction");
        return Ok();
    }
}