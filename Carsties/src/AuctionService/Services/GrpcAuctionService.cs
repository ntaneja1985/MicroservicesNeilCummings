using System.Globalization;
using AuctionService.Data;
using Grpc.Core;

namespace AuctionService.Services;

public class GrpcAuctionService(AuctionDbContext dbContext) : GrpcAuction.GrpcAuctionBase
{
    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine("==> Received Grpc Request for Auction");
        var auction = await dbContext.Auctions.FindAsync(Guid.Parse(request.Id));
        if(auction == null) throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));
        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                Id = auction.Id.ToString(),
                AuctionEnd = auction.AuctionEnd.ToString(CultureInfo.InvariantCulture),
                ReservePrice = auction.ReservePrice,
                Seller = auction.Seller,
            },
        };
        return response;
    }
}