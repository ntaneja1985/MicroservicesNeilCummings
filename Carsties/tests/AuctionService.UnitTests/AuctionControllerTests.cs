using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    
    //This code is executed for each unit test we run. 
    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        
        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        //arrange
        //This method will create 10 Auction Dto objects and store them in auctions variable.
        //Auto fixture is really useful to create fake objects
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(repo=>repo.GetAuctionsAsync(It.IsAny<string>())).ReturnsAsync(auctions);
        
        //act
        var result = await _controller.GetAllAuctions("12/20/2024");
        
        //Assert
        Assert.Equal(10,result.Value.Count());
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);

    }
    
    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        //arrange
        //This method will create a fake auction Dto
        //Auto fixture is really useful to create fake objects
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(repo=>repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        
        //act
        var result = await _controller.GetAuctionById(Guid.NewGuid());
        
        //Assert
        Assert.Equal(auction.Make,result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);

    }
    
    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        //arrange
        //This method will create a fake auction Dto
        //Auto fixture is really useful to create fake objects
        _auctionRepo.Setup(repo=>repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value:null);
        
        //act
        var result = await _controller.GetAuctionById(Guid.NewGuid());
        
        //Assert
        Assert.IsType<NotFoundResult>(result.Result);

    }
    
    [Fact]
    public async Task CreateAuction_WithValidAuctionDto_ReturnsCreatedAtActionResult()
    {
        //arrange
        //This method will create a fake auction Dto
        //Auto fixture is really useful to create fake objects
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        //act
        var result = await _controller.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;
        //Assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById",createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }
    
    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        //arrange
        //This method will create a fake auction Dto
        //Auto fixture is really useful to create fake objects
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);
        //act
        var result = await _controller.CreateAuction(auction);
        //Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        //act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuctionDto);
        //Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test1";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        //act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuctionDto);
        //Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value:null);
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        //act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuctionDto);
        //Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _controller.DeleteAuction(Guid.NewGuid());
        //Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value:null);
        _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _controller.DeleteAuction(auction.Id);
        //Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        //arrange 
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x=>x.Auction).Create();
        auction.Seller = "test1";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _controller.DeleteAuction(Guid.NewGuid());
        //Assert
        Assert.IsType<ForbidResult>(result);
    }
    
    
}