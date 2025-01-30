using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures;

// Create a test instance of our WebApplication and we can add test services inside here
public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    //provided by the TestContainer for PostgreSql
    private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            //Replace DbContext from Program.cs of PostgresSql with the one from TestContainer
           services.RemoveDbContext<AuctionDbContext>();

            services.AddDbContext<AuctionDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
            
            // Replace Mass Transit Configuration from Program.cs file and replace it with Test Harness
            services.AddMassTransitTestHarness();
            
            //Build the database for AuctionDbContext inside our postgresSql Test Container
            services.EnsureCreated<AuctionDbContext>();
            
            //Add Fake Authentication Service to replace the AuthenticationService in Program.cs
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });

        });
    }

     Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
    
}