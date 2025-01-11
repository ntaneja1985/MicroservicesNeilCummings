# Microservices by Neil Cummings
Microservices based Application using Next.js
- ![alt text](image.png)
- Microservices are loosely coupled and each service handles a specific function
- Each microservice does one thing, and it does it really well
- Each microservice have their own database
- ![alt text](image-1.png)
- Communication between microservice can be through HTTP or gRPC
- Microservices must be autonomous
- If one service goes down, it should not impact other microservices
- We should be able to deploy each microservice independently
- We don't have to deploy our entire application every time a change is introduced
- Microservices can also scale independently of other services
- Monoliths don't provide this flexibility.

## Difference between Microservice and Monolith
- In monolith, we have only one database, it has to ensure referential integrity within the database itself using Foreign Keys
- However, each microservice has its own database, and it is job of the Application layer to keep data in sync across various services
- Maintaining data consistency and integrity between services is tough
- ![alt text](image-2.png)
- We also need a gateway and identity provider for microservices
- Microservices are not the cheapest way to build an application.
- Not suitable for small teams
- Meant for large teams

## Creating the First Microservice
- ![alt text](image-3.png)
- ![alt text](image-4.png)
- use dotnet --info command to see what dotnet packages and runtimes are installed
- use dotnet new list to see the list of project templates that we can install
- Use the following command to create a solution file and then a web api project inside it
```shell
//Create a solution file
dotnet new sln

//Create a new webpi inside src folder
dotnet new webapi -o src/AuctionService

//Add webpi to the solution file
dotnet sln add src/AuctionService

//Build the solution,start application add a watcher
//Go to Carsties/src/AuctionService and run this command
//This also adds hot reload functionality
dotnet watch
```
- If we disabled the ImplicitUsing setting inside the csproj file, then even for simple LINQ statement we will have to import their namespaces.
- Entity Framework is convention based
- In Entity Framework, when defining our entities we can define navigation properties also to specify relationship between entities in the database
```csharp
public class Item
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
    
    //Nav Properties
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
    
}
```
## Creating Entity Classes

```c#
public class Auction
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; } = 0;
    public string Seller { get; set; }
    public string Winner { get; set; }

    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime AuctionEnd { get; set; }
    public Status Status { get; set; }
    public Item Item { get; set; }
}

public enum Status
{
    Live,
    Finished,
    ReserveNotMet
}

```

## Adding DbContext
- DbContext itself implements unit of work and repository patterns
- We will create AuctionDbContext class
  ```c#
    using AuctionService.Entities;
    using Microsoft.EntityFrameworkCore;

    namespace AuctionService.Data;

    public class AuctionDbContext : DbContext
    {
    public AuctionDbContext(DbContextOptions options): base(options)
    {
        
    }
    
    public DbSet<Auction> Auctions { get; set; }
    }


  ```
  - We will then need to update Program.cs file to include the AuctionDbContext like this :
  ```c#
   builder.Services.AddDbContext<AuctionDbContext>(opt =>
    {
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

  ```
  ## Adding Migrations 
  - We will add migrations using the following commands:
  ```shell
    //Add a migration
    dotnet ef migrations add "InitialCreate" -o Data/Migrations 
    //Update database
    dotnet ef database update
    //Drop database 
    dotnet ef database drop 

  ```
## Adding a Seed Class
- We can add the following seed class called DbInitializer.cs 
```c#
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        SeedData(scope.ServiceProvider.GetService<AuctionDbContext>());
    }

    private static void SeedData(AuctionDbContext context)
    {
        context.Database.Migrate();
        if (context.Auctions.Any())
        {
            Console.WriteLine("Already data exists!");
            return;
        }

        var auctions = new List<Auction>()
        {
            //auctions
            	    // 1 Ford GT
            new Auction
            {
                Id = Guid.Parse("afbee524-5972-4075-8800-7d1f9d7b0a0c"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(10),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "GT",
                    Color = "White",
                    Mileage = 50000,
                    Year = 2020,
                    ImageUrl = "https://cdn.pixabay.com/photo/2016/05/06/16/32/car-1376190_960_720.jpg"
                }
            },
            // 2 Bugatti Veyron
            new Auction
            {
                Id = Guid.Parse("c8c3ec17-01bf-49db-82aa-1ef80b833a9f"),
                Status = Status.Live,
                ReservePrice = 90000,
                Seller = "alice",
                AuctionEnd = DateTime.UtcNow.AddDays(60),
                Item = new Item
                {
                    Make = "Bugatti",
                    Model = "Veyron",
                    Color = "Black",
                    Mileage = 15035,
                    Year = 2018,
                    ImageUrl = "https://cdn.pixabay.com/photo/2012/05/29/00/43/car-49278_960_720.jpg"
                }
            },
            // 3 Ford mustang
            new Auction
            {
                Id = Guid.Parse("bbab4d5a-8565-48b1-9450-5ac2a5c4a654"),
                Status = Status.Live,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(4),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "Mustang",
                    Color = "Black",
                    Mileage = 65125,
                    Year = 2023,
                    ImageUrl = "https://cdn.pixabay.com/photo/2012/11/02/13/02/car-63930_960_720.jpg"
                }
            },
            // 4 Mercedes SLK
            new Auction
            {
                Id = Guid.Parse("155225c1-4448-4066-9886-6786536e05ea"),
                Status = Status.ReserveNotMet,
                ReservePrice = 50000,
                Seller = "tom",
                AuctionEnd = DateTime.UtcNow.AddDays(-10),
                Item = new Item
                {
                    Make = "Mercedes",
                    Model = "SLK",
                    Color = "Silver",
                    Mileage = 15001,
                    Year = 2020,
                    ImageUrl = "https://cdn.pixabay.com/photo/2016/04/17/22/10/mercedes-benz-1335674_960_720.png"
                }
            },
            // 5 BMW X1
            new Auction
            {
                Id = Guid.Parse("466e4744-4dc5-4987-aae0-b621acfc5e39"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "alice",
                AuctionEnd = DateTime.UtcNow.AddDays(30),
                Item = new Item
                {
                    Make = "BMW",
                    Model = "X1",
                    Color = "White",
                    Mileage = 90000,
                    Year = 2017,
                    ImageUrl = "https://cdn.pixabay.com/photo/2017/08/31/05/47/bmw-2699538_960_720.jpg"
                }
            },
            // 6 Ferrari spider
            new Auction
            {
                Id = Guid.Parse("dc1e4071-d19d-459b-b848-b5c3cd3d151f"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(45),
                Item = new Item
                {
                    Make = "Ferrari",
                    Model = "Spider",
                    Color = "Red",
                    Mileage = 50000,
                    Year = 2015,
                    ImageUrl = "https://cdn.pixabay.com/photo/2017/11/09/01/49/ferrari-458-spider-2932191_960_720.jpg"
                }
            },
            // 7 Ferrari F-430
            new Auction
            {
                Id = Guid.Parse("47111973-d176-4feb-848d-0ea22641c31a"),
                Status = Status.Live,
                ReservePrice = 150000,
                Seller = "alice",
                AuctionEnd = DateTime.UtcNow.AddDays(13),
                Item = new Item
                {
                    Make = "Ferrari",
                    Model = "F-430",
                    Color = "Red",
                    Mileage = 5000,
                    Year = 2022,
                    ImageUrl = "https://cdn.pixabay.com/photo/2017/11/08/14/39/ferrari-f430-2930661_960_720.jpg"
                }
            },
            // 8 Audi R8
            new Auction
            {
                Id = Guid.Parse("6a5011a1-fe1f-47df-9a32-b5346b289391"),
                Status = Status.Live,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(19),
                Item = new Item
                {
                    Make = "Audi",
                    Model = "R8",
                    Color = "White",
                    Mileage = 10050,
                    Year = 2021,
                    ImageUrl = "https://cdn.pixabay.com/photo/2019/12/26/20/50/audi-r8-4721217_960_720.jpg"
                }
            },
            // 9 Audi TT
            new Auction
            {
                Id = Guid.Parse("40490065-dac7-46b6-acc4-df507e0d6570"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "tom",
                AuctionEnd = DateTime.UtcNow.AddDays(20),
                Item = new Item
                {
                    Make = "Audi",
                    Model = "TT",
                    Color = "Black",
                    Mileage = 25400,
                    Year = 2020,
                    ImageUrl = "https://cdn.pixabay.com/photo/2016/09/01/15/06/audi-1636320_960_720.jpg"
                }
            },
            // 10 Ford Model T
            new Auction
            {
                Id = Guid.Parse("3659ac24-29dd-407a-81f5-ecfe6f924b9b"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(48),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "Model T",
                    Color = "Rust",
                    Mileage = 150150,
                    Year = 1938,
                    ImageUrl = "https://cdn.pixabay.com/photo/2017/08/02/19/47/vintage-2573090_960_720.jpg"
                }
            }
        };
        
        context.AddRange(auctions);
        context.SaveChanges();
    }
}

```
- To make this seed class we need to update Program.cs file like this 
```c#
 try
{
 DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

```
- We will add a postgresSql database using docker-compose.yml file like this 
```shell
 services:
  postgres:
    image: postgres
    environment:
      - POSTGRES_PASSWORD=postgrespw
    ports:
      - 5432:5432
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:

```

## Adding Dtos and using Automapper 
- We will create various DTOs such as AuctionDto, CreateAuctionDto, UpdateAuctionDto etc.
- Next we will create Mapping Profiles using Automapper like this 
```c#
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles:Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<AuctionDto, Auction>();
        CreateMap<CreateAuctionDto, Auction>().ForMember(x => x.Item,
            opt => opt.MapFrom(s=>s));
        CreateMap<CreateAuctionDto, Item>();
    }
}

```
- Finally we will include them inside Program.cs file like this 
```c#
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

```

## Adding the API Controllers
- We can create AuctionController.cs file like this 
```c#
   using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(AuctionDbContext _context,IMapper _mapper): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
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


```

### Adding a gitignore file 
```shell
 dotnet new gitignore

```
## Creating a Search Microservice
- Creating a Search Service
- Adding Mongo Db 
- Adding Sync Communication between services
- ![alt text](image-5.png)
- ![alt text](image-7.png)
- Elastic Search is a good option but it is complex to configure compared to MongoDb
- However, if we want to replace MongoDb with Elastic Search in the future we can do it  
- ![alt text](image-8.png)
- ![alt text](image-9.png)
- ![alt text](image-10.png)
- We will install the MongoDb.Entities package
- Then we will create an Item.cs file like this 
```c#
 using MongoDB.Entities;

namespace SearchService.Models;

//Already has Id object from Entity
public class Item : Entity
{
    public int ReservePrice { get; set; } 
    public string Seller { get; set; }
    public string Winner { get; set; }
    public int SoldAmount { get; set; }
    public int CurrentHighBid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } 
    public DateTime AuctionEnd { get; set; }
    public string Status { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
}

```
- We will create a DBInitializer file here also
  ```c#
     using System.Text.Json;
    using MongoDB.Driver;
    using MongoDB.Entities;
    using SearchService.Models;

    namespace SearchService.Data;

    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            //Here DB comes from MongoDb.Entities
            await DB.InitAsync("SearchDb",MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));
            await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text) //We make indexes here so that we can carry out search over them
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

            var count = await DB.CountAsync<Item>();
            if (count == 0)
            {
            Console.WriteLine("No data will attempt to seed");
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
            await DB.SaveAsync(items);
            }
        }
    }


  ```
  - We will create a separate auctions.json file to seed data
  - We will call the DBInitializer in Program.cs file 
  ```c#
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

  ```

  ## Implementing the Search Controller
  - First we will create a file SearchParams.cs:
  ```c#
  namespace SearchService.RequestHelpers;

    public class SearchParams
    {
        public string SearchTerm { get; set; }
        public int PageSize { get; set; } = 4;
        public int PageNumber { get; set; } = 1;
        public string Seller { get; set; }
        public string Winner { get; set; }
        public string OrderBy { get; set; }
        public string FilterBy { get; set; }
    }

  ```
  - Next we will create our Search functionality inside the SearchController.cs file like this 
  ```c#
  using Microsoft.AspNetCore.Mvc;
  using MongoDB.Entities;
  using SearchService.Models;
  using SearchService.RequestHelpers;

  namespace SearchService.Controllers;

  [ApiController]
  [Route("api/search")]
   public class SearchController : ControllerBase
  {
       [HttpGet]
       public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
     {
        var query = DB.PagedSearch<Item,Item>();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        
        //Add Sorting
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)),
        };
        
        //Add Filtering
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow.AddHours(6)
                                             && x.AuctionEnd > DateTime.Now),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }
        
        // Page the results
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        var result = await query.ExecuteAsync();
        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
     }
  }

  ```

  ## Microservices Messaging
  - How do we communicate between AuctionService and SearchService ?
  - First we will start with Synchronous Communication 
  - Synchronous Messaging is only 2 types: HTTP and gRPC
  - ![alt text](image-11.png)
  - This creates dependency between Service A and Service B
  - If Service B goes down, Service A also goes down, this approach makes the services dependent not independent.
  - Here Service A needs to know address of Service B, what if there are 100 services? Who is going to manage the addresses. There are tools like service discovery like Consul, Eureka 
  - HTTP is a synchronous type of external communication.
  - If a browser makes a request to HTTP endpoint it is going to wait for the response.
  - Internally though it uses async/await.
  - In the messaging world, the client has to wait for response = synchronous.
  - ![alt text](image-12.png)
  - This is also called a distributed monolith.
  - What is Service B needed to get data from Service C
  - This can lead to dependency chains.
  - In asynchronous messaging we use a Service Bus 
  - ![alt text](image-13.png)

## Adding HTTP Communication to get the Data
- To call the Auction Service from Search Service, we have to make a few changes
- Changes to the Auction Service are as follows 
```c#
 [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x=>x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x=>x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime())>0);
        }
        
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

```

- Changes to Search Service as as follows:
- We will create an Auction Service Http Client file as follows:
```c#
 public class AuctionServiceHttpClient(HttpClient httpClient,IConfiguration config)
{
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();
        
        return await httpClient
            .GetFromJsonAsync<List<Item>>
            (config["AuctionServiceUrl"] +"/api/auctions?date="+lastUpdated);
    }
}

```
- We will register it inside Program.cs as follows:
  ```c#
  builder.Services
    .AddHttpClient<AuctionServiceHttpClient>()

  ```
- We will call it from the DBInitializer as follows:
```c#
 public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb",MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));
        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();
     
        
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        Console.WriteLine($"Found {items.Count} items");
        if (items.Any())
        {
            await DB.SaveAsync(items);
        }
    }
}

```

## Making our Http Client Resilient
- To make our AuctionServiceHttpClient resilient we will need to use Microsoft.Extensions.Polly nuget package 
- Lets assume that Auction Service goes down. We want the Search Service which is querying the auction service using AuctionServiceHttpClient to not go down as well 
- We want to make sure that the Search Service keeps trying to connect to Auction Service at regular intervals
- Using this package we will need to make a Retry Policy as follows :
```c#
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

```
- Then inside our Program.cs file we need to attach this retry policy to our HttpClient as follows: 
```c#
  builder.Services
    .AddHttpClient<AuctionServiceHttpClient>()
    .AddPolicyHandler(GetRetryPolicy());

```
- We also want this data seeding operation to run once the SearchService application has started. For this we can write the following code in Program.cs 
```c#
 app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
});

```