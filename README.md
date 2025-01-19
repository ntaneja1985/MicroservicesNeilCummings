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

## Using RabbitMQ
- ![alt text](image-14.png)
- We prefer the asynchronous approach
- ![alt text](image-15.png)
- In Asynchronous messaging there is no request/response 
- It is Fire and Forget 
- It uses an event model(publish/subscribe)
- It is used for service to service messaging
- Transports(RabbitMq, Azure Service Bus, Amazon SQS)
- Services only need to know about the bus 
- It is more complex than synchronous messaging.
- Service bus must be clustered, fault tolerant and it must have persistent storage to store messages.
- RabbitMq is a message broker. It accepts and forwards messages. 
- It uses the Producer/Consumer Model(Pub/Sub)
-  Messages are stored on queues(It uses a message buffer)
-  RabbitMq also has persistence associated with it, so that if RabbitMq service fails and we need to create a new one in its place, then we can use persistent storage to restore messages. 
### Rabbit Mq Exchanges 
-  Rabbit Mq uses Exchanges and Exchanges can be used for "routing" functionality. 
-  When we publish a message, we send it to an exchange, and exchanges have queues that are bound to it. 
-  RabbitMq uses AMQP(Advanced Message Queuing protocol)
-  ![alt text](image-16.png)
-  Direct Exchange: Delivers messages to queues based on a routing key. (Can only route to a single queue)
-  Fanout: Exchange has multiple queues and it waits for a consumer to pick it up 
-  ![alt text](image-17.png)
-  Topic Exchange: Routes messages to one or more queues based on its routing key. It is similar to Direct exchange but the same routing key can be used to go to more than one queue 
-  ![alt text](image-18.png)
-  Header Exchange: Allows us to specify a header with the message that the exchange can use to publish it to various multiple queues, one or more queues. It is not used much
-  ![alt text](image-19.png)
## MassTransit 
- Provides an abstraction over various transports
- ![alt text](image-20.png)
- Setup rabbitMq using docker compose like this:
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
  mongodb:
    image: mongo
    environment:
      - MONGO_INITDB_ROOT_USERNAME=root
      - MONGO_INITDB_ROOT_PASSWORD=mongopw
    ports:
      - 27017:27017
    volumes:
      - mongodata:/data/db
  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - 5672:5672
      - 15672:15672
volumes:
  pgdata:
  mongodata:

```

## Configuring Mass Transit to use RabbitMq inside Auction Service and Search Service 
- We need to install MassTransit.RabbitMq nuget package and set it up inside the Program.cs file of AuctionService and SearchService
- Inside Auction Service as follows:
```c#
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

```
- Inside Search Service as follows:
```c#
builder.Services.AddMassTransit(x =>
{
    //Add Consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    //Set up the formatters to display queue names
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

```
- In our architecture, the auction service will publish messages and search service will consume messages.
- We will create a separate class library called Contracts and add its references to both Auction Service and Search Service
- This class library will contain the objects that will be published and consumed
- These objects will be AuctionCreated, AuctionUpdated and AuctionDeleted
- To add a reference to the class library inside both Auction Service and Search Service we will use these commands:
```shell
//Go to the Auction Service directory
dotnet add reference ../../src/Contracts

```
- We will need to setup consumers inside the SearchService as follows:
```c#
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer(IMapper mapper): IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("-->Consuming Auction Created:"+context.Message.Id);
        var item = mapper.Map<Item>(context.Message);
        await item.SaveAsync();
    }
}

```
- Once we have added consumers in the Search Service, we need to inform MassTransit about these consumers.
- This can be done in Program.cs file using the following code:
```c#
builder.Services.AddMassTransit(x =>
{
    //Add Consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
}

```

## Publishing the Messages from the Auction Service 
- We will use the IPublishEndpoint provided by MassTransit to publish messages to rabbitMq within the Auction Service 
- For e.g here we are publishing AuctionCreated Message on successful auction creation.
  
```c#

public class AuctionsController(AuctionDbContext _context,IMapper _mapper, IPublishEndpoint publishEndpoint): ControllerBase
{
[HttpPost]


    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;

        var newAuction = _mapper.Map<AuctionDto>(auction);
        
        await publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        
        if(!result) return BadRequest("could not create auction");
        return CreatedAtAction(nameof(GetAuctionById),
            new { id = auction.Id }, newAuction);
    }

}

```

## Possible issues with Asynchronous Communication 
- In a monolith ACID is maintained within transactions:
- ![alt text](image-21.png)
- If we are updating 2 tables at the same time, either all will be updated successfully or none will be updated to maintain ACID principles.
- But in microservices, things are different. 
- ![alt text](image-22.png)
- What if any of the services are down?
- ![alt text](image-23.png)
- ![alt text](image-24.png)
- Data Inconsistency is one of the main challenges with microservices.
- One of the popular solutions to solve this is using the Outbox pattern.
- If the RabbitMq is down, our messages are saved to an outbox and retried at regular intervals. 

## Outbox Pattern 
- If our services are running but RabbitMq is down, it may result in the data across our services to be inconsistent. 
- To solve this we need to use Outbox pattern where when we publish our messages we will first save them to an OutboxMessage table 
- When RabbitMq comes back up we will push these messages from the outbox table to the rabbitMq to be consumed by the Search Service. 
- MassTransit library helps us to setup this outbox pattern quite easily. 
- We will first add a nuget package: MassTransit.EntityFrameworkCore 
- Then we will configure MassTransit inside Program.cs file of AuctionService as follows: 
  ```c#
  builder.Services.AddMassTransit(x =>
  {
    x.AddEntityFrameworkOutbox<AuctionDbContext>(opt =>
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);
        opt.UsePostgres();
        opt.UseBusOutbox();
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
  });

  ```
- Now we will override the OnModelCreating method inside the AuctionDbContext to add the Inbox,Outbox message and state entity .
```c#
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
    {
    public AuctionDbContext(DbContextOptions options): base(options)
    {
        
    }
    
    public DbSet<Auction> Auctions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        //Responsible for implementing outbox functionality
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

```
- Now we will run the migrations and it will create migrations to create the InboxState, OutboxState and OutboxMessage tables. 
- Now if the rabbitMq is down, then OutboxMessage table is populated and when RabbitMq comes backup the table will push the data to the RabbitMq and clear itself. 


## Using message retries 
- Consider the situation when database of the consumer is down.
- In our case, let us assume MongoDb is down for the SearchService.
- The search service will consume the messages from the rabbitMq but will not able to save them to the database due to it being down. 
- We should have a policy to retry to fetch and save the messages at regular intervals. 
- We can do this using the following configuration inside our Program.cs file like this: 
```c#
 builder.Services.AddMassTransit(x =>
{
    //Add Consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    //Set up the formatters to display queue names
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5,5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});

```
- Now in this case, it will keep trying to consume from this queue(or endpoint) for total 5 times every 5 seconds. 

## Consuming Fault Queues 
- If we go to RabbitMq, after our database was down, we see some additional queues being added 
- ![alt text](image-25.png)
- If we go to one of these queues like search-auction-created_error we see messages like this: 
- ![alt text](image-26.png)
- These are error messages generated by the consumer when the MongoDb database was down. 
- These specialty queues are known as Fault Queues. 
- We should be able to consume these fault queue messages and do something about them
- First let us generate an exception in our consumer like this 
```c#
 public class AuctionCreatedConsumer(IMapper mapper): IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("-->Consuming Auction Created:"+context.Message.Id);
        var item = mapper.Map<Item>(context.Message);
        
        if (item.Model == "Foo") throw new ArgumentException("Cannot see cars with name of Foo");
        
        await item.SaveAsync();
    }
}

```
- Now we will create another consumer inside the Auction Service to handle these exceptions, correct the data and publish again 
- Go to AuctionService and create an AuctionCreatedFaultConsumer class 
```c#
  using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer:IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("-->Consuming Faulty Creation");
        var exception = context.Message.Exceptions.First();
        if (exception.ExceptionType == "System.ArgumentException")
        {
            context.Message.Message.Model = "FooBar";
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("Not an argument exception update error dashboard somewhere");
        }
    }
}


```
- In the above code, it will consume the fault queue for Auction Created and if it detects an argument exception, it will modify the data and publish it again. 
- We will configure this new consumer inside Program.cs file of Auction Service as follows: 
```c#
 x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction",false));

```

## Adding the Update and Delete Consumers 

```c#

//Publishing Auction Updated and Auction Deleted Messages from the Auction Service Controller

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
        
        await publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
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
        await publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("could not delete auction");
        return Ok();
    }


//Consuming the Auction Updated and Auction Deleted Messages inside the Search Service

public class AuctionUpdatedConsumer(IMapper mapper): IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("-->Consuming Auction Updated: "+context.Message.Id);
        var item = mapper.Map<Item>(context.Message);
        
        var result = await DB.Update<Item>()
            .Match(a=>a.ID == context.Message.Id)
            .ModifyOnly(x =>new
            {
                x.Color,
                x.Make,
                x.Model,
                x.Year,
                x.Mileage
            },item)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
        }
    }
}


public class AuctionDeletedConsumer(IMapper mapper): IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("-->Consuming Auction Deleted:"+context.Message.Id);
        
        var result = await DB.DeleteAsync<Item>(context.Message.Id);
        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionDeleted),"Problem deleting auction");
        }
    }
}


```

## Identity Server 
- Create Identity Server Project 
- This is used to authenticate to our API endpoints
- Client App will use the Identity Server as the trusted token issuer to authenticate to our API endpoints 
- Open ID Connect and OAuth 2.0 is used.
- ![alt text](image-27.png)
- Alternative is Azure Active Directory B2C.
- In microservices, an identity provider is used to provide authentication related services. 

### What is Identity Server 
- It is an authentication server that implements OIDC and OAuth 2.0
- It is very customizable solution compared to Facebook, LinkedIn or Azure Active Directory B2C 
- Comes under Duende Identity Server, no longer open source. 
- Identity Server is a Single Sign On Solution. Not used for single-app authentication use Asp.net Core Identity for that.
- ![alt text](image-28.png)
- It is simple to configure but hides lot of complexity. 

## OAuth 2.0 and Open ID Connect(OIDC)
- OAuth 2.0 is a security standard where we give one app permission to access our data in another application. 
- Instead of giving them a username/password we give them a key to access our data and do things on our behalf in another application.
- Steps taken to grant permission is referred to as authorization, we authorize one app to access our data or use features in another app on our behalf.
- We can take back this key whenever we wish. 
- Facebook Login, Github Logic uses OAuth 2.0 
- ![alt text](image-29.png)
- ![alt text](image-30.png)

### Terminology 
- Resource Owner: That us, we are the owner of our identity, our data and any actions that can be performed with our account. 
- Client: It is the application that wants to access data or perform actions on behalf of us, the resource owners. 
- Authorization Server: This is the application that knows the resource owner where the resource owner already has an account. In our case, this is the Identity Server, but it can also be Facebook or Github. 
- Resource Server: This is the API or service the client wants to use on behalf of the resource owner. 
- ![alt text](image-31.png)
- Authorization Server is a third party server that the resource server trusts. 
- We also have a redirect URI and this is the URL, the auth server or the identity server will redirect the resource owner back to after granting permission to the client callback URL. 
- Response Type: This is the type of information the client is expecting to receive. 
- ![alt text](image-32.png)
- The most common response type is code where the client expects to receive an authorization code from the authorization server. 
- Scope: There are the granular permissions the client wants, such as access to data so that it can perform actions. 
- ![alt text](image-33.png)
- Consent Form: 
- ![alt text](image-34.png)
- Auth Server takes the scopes the client is requesting and verifies with the resource owner if they want to give the client permission. 
- We also have a client ID and this ID is used to identify the client with the authorization server. 
- ![alt text](image-35.png)
- In our Identity Server, we will configure 2 clients: NextJS app and Postman 
- Client Secret: This allows our clients(Next.js app) to securely share information privately behind the scenes 
- Authorization Code: This is a short lived temporary code that the authorization server(identity server) sends back to the client. 
- The client then sends the auth code back to the auth server along with the client secret in exchange for an access token. 
- Client then uses access token to communicate with the resource server(our API) 
- Think of Access Token as a keycard. 

### OAuth 2.0 
- We want client to access some information about us that is contained on the authorization server(Identity Server)
- We will use our identity server to store our user accounts. 
- Client redirects our browser to the authorization server that includes with the request, the client ID, the redirect URL, the response type and one or more scopes that we need. 
- Authorization Server then verifies who we are are if necessary prompts us for a login(or maybe we already have a session on the identity server)
- We will see a consent form based on the scopes requested by the client and the resource owner can grant or deny permission based on what the app is asking for. 
- Temporary authorization code is passed to the user's browser which then calls using the redirect URI to the client app server. 
- The browser then uses the callback url  provided by the authorization server to redirect to the client app server. 
- Client then contacts the authorization server directly. It doesnot use the resource owner's browser. 
- It securely sends its client ID and its client secret along with the authorization code to the authorization server. 
- The authorization server responds with an access token which is not returned to the user's browser. 
- It is retained on the application server. 
- Client App then sends the access token to the resource server when requesting data. 
- Behind the scenes, the resource server(or Auction Service API) will verify if this is a valid token and if the token is valid, then it returns the data.
- Authorization Server establishes a relationship with the client and the authorization server generates a client ID and client Secret. 
- ![alt text](image-36.png)
- Authorization Server verifies clients based on this Client ID and Client Secret. 

### Open ID Connect 
- It sits on top of OAuth 2.0 
- ![alt text](image-37.png)
- OAuth 2.0 is only designed for authorization and granting access to data. 
- OAuth 2.0 doesnot tell anything about you. 
- Open ID Connect that sits on top of OAuth 2.0 adds that additional functionality around login and profile information about the person who is logged in. 
- It doesnt just give the client permissions but it also includes some basic information about who you are. 
- OIDC helps to enable a client to establish a login session as well as to gain info about the person logged in, what is referred to as identity. 
- When an authorization server supports OIDC, it is often referred to as an identity provider since it provides information about the resource owner that us, the user back to the client, which is the server running our client application. 
-  ![alt text](image-38.png)


## Creating the Identity Server Project 
- ![alt text](image-39.png)
- Install Templates from Duende 
- Adds support for ASP.NET Identity and Entity Framework 
```shell 
 dotnet new --install Duende.IdentityServer.Templates

```
- ![alt text](image-40.png)
```shell 
 dotnet new isaspid -o src/IdentityService 
 dotnet sln add src/IdentityService 
```
- ![alt text](image-41.png)

- Difference between Duende Identity Server and ASP.NET Core Identity 
- Identity Server is an identity provider whereas ASP.NET Core Identity is a user store and a user management feature. 
- Identity Provider is really responsible for issuance of tokens using OAuth2.0 and Open ID Connect protocols. 
-  Once we will add Identity Server ASP.NET Core Identity Template, then we will see that the following files are created:
```c#
//ApplicationDbContext.cs 
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}


//ApplicationUser.cs 
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
}

//ConfigureServices static method inside HostingExtensions.cs file 
public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        //Razor Pages for the Views
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        //Configure ASP.NET Core Identity and is responsible for creating tables in the database
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();

        builder.Services.AddAuthentication();

        return builder.Build();
    }

```

## Seeding Data and adding a migration 
- First we will delete the Migrations folder as it contains migrations for sql lite database
- We will have to add our own migrations for postgresSql database. 
- We can do so using this command:
```shell
dotnet ef migrations add "InitialCreate" -o Data/Migrations
```
- Also in our Program.cs file for Seeding Data we will have to call the SeedData class 
```c#
 public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        if(userMgr.Users.Any()) return;

        //Add users to seed 
    }

```
- Once the migrations are added we can just run our applications and all our ASP.NET Core Identity tables will be created. 

## Adding a Register Page to the Identity Server 
- Since we are working with Razor Pages, we will need to create a new folder called Register 
- Inside the folder we will create a Razor Page Index.cshtml and it will automatically add a codebehind file: Index.cshtml.cs 
- Code for that would be similar to this 
```c#
//Index.cshtml 
 @page
@model IdentityService.Pages.Register.Index;

<div>
    <partial name="_ValidationSummary" />
    <div class="row">
        <div class="col-6 offset-3">
            <div class="card">
                <div class="card-header">
                    <h2>Register</h2>
                </div>
                <div class="card-body">
                    <form asp-page="/Account/Register/Index">
                        <input type="hidden" asp-for="Input.ReturnUrl" />

                        <div class="form-group">
                            <label asp-for="Input.Username"></label>
                            <input class="form-control" placeholder="Username" asp-for="Input.Username" autofocus>
                        </div>
                        <div class="form-group">
                            <label asp-for="Input.Email"></label>
                            <input class="form-control" placeholder="Email" asp-for="Input.Email">
                        </div>
                        <div class="form-group">
                            <label asp-for="Input.Fullname"></label>
                            <input class="form-control" placeholder="Full Name" asp-for="Input.Fullname">
                        </div>
                        <div class="form-group">
                            <label asp-for="Input.Password"></label>
                            <input type="password" class="form-control" placeholder="Password" asp-for="Input.Password" autocomplete="off">
                        </div>

                      
                        <a class="pb-3 d-block" asp-page="../Login/Index" asp-route-returnUrl="@Model.Input.ReturnUrl">
                            Already Registered ? Login Here
                        </a>
                        <button class="btn btn-primary" name="Input.Button" value="register">Register</button>
                        <button class="btn btn-secondary" name="Input.Button" value="cancel">Cancel</button>
                    </form>
                </div>
                @if (Model.RegisterSuccess)
                {
                    <div class="alert alert-success mt-2">
                        <strong>
                            Successfully Registered
                        </strong> - You can now login
                    </div>
                }
                
            </div>
        </div>
    </div>
    </div>

//Index.cshtml.cs 
using System.Security.Claims;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Register
{

    [SecurityHeaders]
    [AllowAnonymous]
    public class Index(UserManager<ApplicationUser> userManager) : PageModel
    {
        
        [BindProperty]
        public RegisterViewModel Input { get; set; }
        
        [BindProperty] 
        public bool RegisterSuccess { get; set; }
        
        
        public IActionResult OnGet(string returnUrl)
        {
            Input = new RegisterViewModel()
            {
                ReturnUrl = returnUrl
            };
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if(Input.Button != "register") return Redirect("~/");

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    await userManager.AddClaimsAsync(user, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, Input.Fullname),
                    });
                    
                    RegisterSuccess = true;
                }
            }
            
            return Page();
        }
    }
}

```

## Adding Client Credentials to allow Clients to request a Token 
- ![alt text](image-42.png)
- We will add client details for postman inside Config.cs file of Identity Server so that we can use it to generate a token
```c#
 using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            //Provides Open ID Connect Functionality
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp","Auction app full access")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = {"openid", "profile","auctionApp"},
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = new [] {new Secret("NotASecret".Sha256())},
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword }
            }
            
            // m2m client credentials flow client
            // new Client
            // {
            //     ClientId = "m2m.client",
            //     ClientName = "Client Credentials Client",
            //
            //     AllowedGrantTypes = GrantTypes.ClientCredentials,
            //     ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
            //
            //     AllowedScopes = { "scope1" }
            // },

            // interactive client using code flow + pkce
            // new Client
            // {
            //     ClientId = "interactive",
            //     ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
            //
            //     AllowedGrantTypes = GrantTypes.Code,
            //
            //     RedirectUris = { "https://localhost:44300/signin-oidc" },
            //     FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
            //     PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },
            //
            //     AllowOfflineAccess = true,
            //     AllowedScopes = { "openid", "profile", "scope2" }
            // },
        };
}



```
- ![alt text](image-43.png)

## Customizing the JWT Token to return new Claim Information 
- We will create a new class inside Identity Server called CustomProfileService.cs which will implement IProfileService 
- ![alt text](image-44.png)
- We will register this in the HostingExtensions.cs file like this: 
```c#
 builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

```
- When we test it using postman we can see our custom claims inside the JWT Token 
- ![alt text](image-45.png)


## Same Site Attribute in Cookies 
- The SameSite attribute is a feature in cookies designed to help bolster web security and mitigate Cross-Site Request Forgery (CSRF) attacks. 
- It essentially gives website developers control over when browsers should send cookies along with cross-site requests.
- Here are the three main values it can take:
- SameSite=Lax: This is the default value in most modern browsers. Cookies are sent with top-level navigation GET requests, but not with less safe, cross-origin requests like POST requests or requests initiated by third-party elements.
- SameSite=Strict: With this setting, cookies are sent only with requests that originate from the same site that set the cookie. This means no cross-site request will include the cookie, even top-level navigation.
- SameSite=None: Cookies are sent with both same-site and cross-site requests. Note that for cookies with this attribute, the Secure attribute must also be set, meaning the cookie is sent over HTTPS only.

- Using the SameSite attribute, developers can enhance the security posture of their applications by limiting the conditions under which browsers attach cookies to outgoing requests. 
- This reduces the risk of certain types of CSRF attacks.


## Configuring Authentication on the Resource Server
- As of now, we have done the process of getting a token from the Identity Server. 
- Now we want to use that token for doing validation on our resource server(Auction API)
- So in Auction Service, we will have to install a nuget package: Microsoft.AspNetCore.Authentication.JwtBearer
- To Configure this, we will go to Program.cs file and add the following code: 
```c#
 builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        //Make sure we validate the incoming token from the Identity Server whose Url is specified in appsettings.json
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidateAudience = false;
        //This is the custom claim type we had set inside our JWT token
        opt.TokenValidationParameters.NameClaimType = "username";
    });

//Configure pipeline to do authentication and authorization
app.UseAuthentication();
app.UseAuthorization()

```

- Now we will add Authorize Attribute on our POST/PUT/DELETE endpoints 
```c#
 [Authorize]
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
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("could not update auction");
        return Ok();
    }


```
- When we try to access these endpoints without the Bearer Token, then we get a 401 Unauthorized message. 
- Identity Service will now be used for authentication by our microservices. 
  

## Adding a Gateway Service 
- We want to provide a single access point into our API Services.
- ![alt text](image-46.png)
- Options of YARP, Ocelot
- ![alt text](image-47.png)
- ![alt text](image-48.png)
- Proxy sits in front of a client browser. 
- Reverse proxy sits in front of a bunch of backend servers.
- We usually have reverse proxy for microservices 
- It provides a single surface area for requests. Client only needs to know one URL to get to our backend (our gateway address)
- Client is unaware of any internal services
- This gateway can also be used for security. We can check authentication on gateway side of things. 
- We use it for SSL termination. 
- We can also use it for URL rewriting. 
- We can also use it for Load Balancing.
- It can also be used for caching. When response comes from backend service, we can cache the response also. 

## Setting up the Reverse Proxy(YARP)
- Add an empty ASP.NET Core Project to our solution 
```shell
 dotnet new web -o src/GatewayService
 dotnet sln add src/GatewayService

```
- Add couple of nuget packages to this project
```c#
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.12" />
<PackageReference Include="Yarp.ReverseProxy" Version="2.2.0" />

```
- We will setup a configuration file for all the routes of this reverse proxy. 
- However, we will specify that in the Program.cs file 
```c#
 var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();


```
- Reverse proxy needs a configuration file so that as it receives a request from the client it knows what to do with those requests.
- Whether, we need to transform that request in some way, whether that request needs authentication
- It can also take a look at the method coming in to the proxy server whether it is a GET/PUT/POST,DELETE etc 
- ![alt text](image-49.png)
- We can add a config file as follows: 
- Routes: Define how incoming requests should be matched and routed.
- Clusters: Specify destination servers (addresses) that your proxy should forward requests to.
```json 
 {
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ReverseProxy": {
    "Routes": {
      "auctions": {
        "ClusterId": "auctions",
        "Match": {
          "Path": "/auctions/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "api/auctions/{**catch-all}"
          }
        ]
      },
      "search": {
        "ClusterId": "search",
        "Match": {
          "Path": "/search/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/search/{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      "auctions": {
        "Destinations": {
          "auctionApi": {
            "Address": "http://localhost:7001"
          }
        }
      },
      "search": {
        "Destinations": {
          "searchApi": {
            "Address": "http://localhost:7002"
          }
        }
      }
    }
  }
}


```
- Load Balancing: Adjust your appsettings.json to include load balancing strategies:
```json 
 "Clusters": {
  "cluster1": {
    "Destinations": {
      "destination1": { "Address": "https://example.com/" },
      "destination2": { "Address": "https://anotherexample.com/" }
    },
    "LoadBalancingPolicy": "RoundRobin"
  }
}


```
- Session Affinity: Ensure sticky sessions to a particular server:
```json 
  "Clusters": {
  "cluster1": {
    "Destinations": { "destination1": { "Address": "https://example.com/" } },
    "SessionAffinity": {
      "Enabled": true,
      "Policy": "Cookie",
      "FailurePolicy": "Return503Error",
      "Settings": {
        "AffinityKeyName": "Yarp.Session"
      }
    }
  }
}



```
- Retries: Set up retries for request failures:
```json 
 "Clusters": {
  "cluster1": {
    "Destinations": {
      "destination1": { "Address": "https://example.com/" }
    },
    "HttpClient": {
      "MaxRetries": 3,
      "RequestTimeout": "00:00:30"
    }
  }
}


```
- 

## Adding authentication to our Gateway configuration
- If we know that the path of the request needs to be authenticated, then we can stop the user at the gateway 
- ![alt text](image-50.png)
- Please note Identity Service lives outside our Gateway and Resource Apis(auction, search etc)
- Incoming requests will be authenticated via JWT tokens, and authorized users will be able to access the secured routes
- This setup ensures that only authenticated users can reach the backend services
- We will have the following configuration inside our appsettings.Development.json file
- In the below configuration all GET requests to the auction Service are not authenticated. 
- However all POST requests are authenticated with authorization policy of default.  
```json 
  {
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "IdentityServiceUrl": "http://localhost:5000",
  "ReverseProxy": {
    "Routes": {
      "auctionsRead": {
        "ClusterId": "auctions",
        "Match": {
          "Path": "/auctions/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/auctions/{**catch-all}"
          }
        ]
      },
      "auctionsWrite": {
        "ClusterId": "auctions",
        "AuthorizationPolicy": "default",
        "Match": {
          "Path": "/auctions/{**catch-all}",
          "Methods": ["POST","PUT","DELETE"]
        },
        "Transforms": [
          {
            "PathPattern": "api/auctions/{**catch-all}"
          }
        ]
      },
      "search": {
        "ClusterId": "search",
        "Match": {
          "Path": "/search/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/search/{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      "auctions": {
        "Destinations": {
          "auctionApi": {
            "Address": "http://localhost:7001"
          }
        }
      },
      "search": {
        "Destinations": {
          "searchApi": {
            "Address": "http://localhost:7002"
          }
        }
      }
    }
  }
}



```
- We will have to add Authentication in the middleware pipeline for Gateway Service 
- Here is the code in Program.cs file 
```c#
 using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.NameClaimType = "username";
    });

var app = builder.Build();

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
app.Run();


```

## Create couple of more consumers 
- We will add 2 consumers(Auction Finished and Bid Placed) inside the Auction Service 

```c#
//Auction Finished Consumer 
 public class AuctionFinishedConsumer(AuctionDbContext auctionDbContext):IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("-->Consuming Auction Finished");
        var auction = await auctionDbContext.Auctions.FindAsync(context.Message.AuctionId);
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Status.Finished
            : Status.ReserveNotMet;
        
        await auctionDbContext.SaveChangesAsync();
    }
}


//Bid Placed Consumer
 public class BidPlacedConsumer(AuctionDbContext auctionDbContext):IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("-->Consuming Bid Placed");
        var auction = await auctionDbContext.Auctions.FindAsync(context.Message.AuctionId);
        if (auction.CurrentHighBid == null
            || context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auctionDbContext.SaveChangesAsync();
        }
    }
}


```
- Similar as above, we will add 2 more consumers for the Search Service 
```c#
//Auction Finished

public class AuctionFinishedConsumer:IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = (int)context.Message.Amount;
        }

        auction.Status = "Finished";
        await auction.SaveAsync();
    }
}

//Bid Placed 
public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("-->Consuming Bid Placed");
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid
           )
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}


```

## Adding a new client for Next.js App to the Identity Service 
- We will have to add the new Client for the Next.js app inside Config.cs file of the Identity Server like this 
```c#
 new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256()) },
                //ID Token and Access Token can be shared without any browser involvement
                //In case of Mobile App it would only be Code
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                //Pkce is required in case of mobile applications not for web applications
                RequirePkce = false,
                RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
                //Allows us to use Refresh Token Functionality
                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile","auctionApp"},
                AccessTokenLifetime = 3600*24*30
            }


```

## Dockerizing our Application 
- ![alt text](image-51.png)
- We will create a Dockerfile per service
- ![alt text](image-52.png)
- Running the docker build command 
```shell 
  docker build -f src/AuctionService/Dockerfile -t testing123 .
  docker run testing123
```

- We can add a Docker file for Auction Service as follows: 
  
```shell 
 FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
# This is a directory inside docker
WORKDIR /app
# This is a port inside docker
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use the same
# COPY command for every dockerfile in the project to take advantage of docker
# caching
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore package dependencies
RUN dotnet restore Carsties.sln

# Copy the app folders over 
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts

WORKDIR /app/src/AuctionService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet","AuctionService.dll"]


```
- Similar to above, we can docker files for Search Service and Identity Service as follows 
```shell
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
# This is a directory inside docker
WORKDIR /app
# This is a port inside docker
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use the same
# COPY command for every dockerfile in the project to take advantage of docker
# caching
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore package dependencies
RUN dotnet restore Carsties.sln

# Copy the app folders over 
COPY src/SearchService src/SearchService
COPY src/Contracts src/Contracts

WORKDIR /app/src/SearchService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet","SearchService.dll"]


```

- Docker file for Identity Service is as follows:
```shell
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
# This is a directory inside docker
WORKDIR /app
# This is a port inside docker
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use the same
# COPY command for every dockerfile in the project to take advantage of docker
# caching
COPY Carsties.sln Carsties.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore package dependencies
RUN dotnet restore Carsties.sln

# Copy the app folders over 
COPY src/IdentityService src/IdentityService


WORKDIR /app/src/IdentityService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet","IdentityService.dll"]

```

- We can then set it up inside docker-compose as follows:

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
  mongodb:
    image: mongo
    environment:
      - MONGO_INITDB_ROOT_USERNAME=root
      - MONGO_INITDB_ROOT_PASSWORD=mongopw
    ports:
      - 27017:27017
    volumes:
      - mongodata:/data/db
  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - 5672:5672
      - 15672:15672
  auction-svc:
    image: nishant198509/auction-svc:latest
    build:
      context: .
      dockerfile: src/AuctionService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - RabbitMq__Host=rabbitmq
      - ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=auctions
      - IdentityServiceUrl=http://identity-svc
    ports:
      - 7001:80
    depends_on:
      - postgres
      - rabbitmq
  search-svc:
    image: nishant198509/search-svc:latest
    build:
      context: .
      dockerfile: src/SearchService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - RabbitMq__Host=rabbitmq
      - ConnectionStrings__MongoDbConnection=mongodb://root:mongopw@mongodb
      - AuctionServiceUrl=http://auction-svc
    ports:
      - 7002:80
    depends_on:
      - mongodb
      - rabbitmq
  identity-svc:
    image: nishant198509/identity-svc:latest
    build:
      context: .
      dockerfile: src/IdentityService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=identity
    ports:
      - 5000:80
    depends_on:
      - postgres
volumes:
  pgdata:
  mongodata:


```

## Debugging a .NET Application inside Docker 
- In Rider, we get the following screen where we can attach our code to a process like this: 
- ![alt text](image-53.png)
- It will install Jetbrains remote tools inside the container and then we can setup breakpoints in our code. 
- In VSCode we have option to create launch.json to achieve the same purpose. 

## Dockerizing our Gateway Service
- If we remember,we had configured our reverse proxy configuration inside our appsettings.Development.json file 
- That configuration consisted of routes and clusters. 
- Now we are going to split that configuration 
- appsettings.json file is run irrespective of which environment we are running in whether in Development or Docker 
- So we are going to copy over routes inside appsettings.json 
- We are going to create a new appsettings.Docker.json 
- Inside that we will copy everything from appsettings.Development.json and modify the urls accordingly to what is running inside docker compose 
- So cluster information will be in both appsettings.Development.json and appsettings.Docker.json 
- and routes will be inside appsettings.json 

```json 
// appsettings.json 
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "auctionsRead": {
        "ClusterId": "auctions",
        "Match": {
          "Path": "/auctions/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/auctions/{**catch-all}"
          }
        ]
      },
      "auctionsWrite": {
        "ClusterId": "auctions",
        "AuthorizationPolicy": "default",
        "Match": {
          "Path": "/auctions/{**catch-all}",
          "Methods": ["POST","PUT","DELETE"]
        },
        "Transforms": [
          {
            "PathPattern": "api/auctions/{**catch-all}"
          }
        ]
      },
      "search": {
        "ClusterId": "search",
        "Match": {
          "Path": "/search/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/search/{**catch-all}"
          }
        ]
      }
    }
  }
}


//appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "IdentityServiceUrl": "http://localhost:5000",
  "ReverseProxy": {
    "Clusters": {
      "auctions": {
        "Destinations": {
          "auctionApi": {
            "Address": "http://localhost:7001"
          }
        }
      },
      "search": {
        "Destinations": {
          "searchApi": {
            "Address": "http://localhost:7002"
          }
        }
      }
    }
  }
}



//appsettings.Docker.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "IdentityServiceUrl": "http://identity-svc",
  "ReverseProxy": {
    "Clusters": {
      "auctions": {
        "Destinations": {
          "auctionApi": {
            "Address": "http://auction-svc"
          }
        }
      },
      "search": {
        "Destinations": {
          "searchApi": {
            "Address": "http://search-svc"
          }
        }
      }
    }
  }
}




```
- Now the configuration for gateway service inside docker-compose will be as follows: 
```shell
  gateway-svc:
    image: nishant198509/gateway-svc:latest
    build:
      context: .
      dockerfile: src/GatewayService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:80
    ports:
      - 6001:80

```

## Building Client Application using Next.js
- ![alt text](image-54.png)
- Here BFF = BACKEND FOR FRONTEND 
- Next.js provides client side functionality (React), but Next.js is server side first, so it generates HTML on the server and sends it to the client user interface. 
- If we want some events like button clicks, then Next.js is going to render that part of the page as client side code and return that javascript to the browser. 
- Majority of the code we send back is pre-rendered HTML and that is great for SEO and it also gives us server side functionality.(BFF)

### Why Next.js ?
- Performance is excellent 
- Load times are reduced with lazy loading and pre-fetching. 
- Good SEO due to server side rendering. 
- Can acts as BFF for our client app. 
- We need Next.js to store the token that identity service sends to it and we will store the token on the server side and because Next.js is going to act as a server, we are going to have server side code.
- It can keep secrets like Client Secrets which we can give from identity server. 
- It also hides unnecessary or sensitive data before transferring it to the client browser. 
- Our browser doesnot know where the requests are going to.
- Client only talks to Next.js server 
- Next.js server talks to the backend .NET services. 
- User Interface code is pretty simple 
- It is React-based. 
- Next.js is however opinionated and we have to name files/folders as per certain names. 
- Next.js has excellent hot-reload
- ![alt text](image-55.png)

## Creating the Next.js Project
```shell
npx create-next-app@latest
```
## Getting data from API server using server side component 
```js 
import React from 'react'

async function getData() {
    //Caches the data coming from the API
    const res = await fetch('http://localhost:6001/search');
    if (!res.ok) {
        throw new Error('Failed to fetch data');
    }
    return res.json();
}
export default async function Listings() {
    const data = await getData();
    return (
        <div>
            {JSON.stringify(data, null, 2)}
        </div>
    )
}


```
- To get complete logging in Next.js 14 project, we need to modify next.config.mjs 
```js 
/** @type {import('next').NextConfig} */
const nextConfig = {
    logging:{
        fetches:{
            fullUrl:true
        }
    }
};

export default nextConfig;


```

- ![alt text](image-56.png)
- If the html rendered by server and javascript sent back in a client component doesnot match, we get a hydration warning like this 
- ![alt text](image-57.png)
- We can fix it like this: 
```js 
 <span suppressHydrationWarning={true} >
                    {zeroPad(days)}:{zeroPad(hours)}:{zeroPad(minutes)}:{zeroPad(seconds)}
                </span>

```

## Creating a List of Auctions 
- ![alt text](image-58.png)
```js 
//Define the types:
export type PagedResult<T> = {
    results: T[],
    pageCount: number,
    totalCount: number
}

export type Auction =  {
    reservePrice: number
    seller: string
    winner?: string
    soldAmount: number
    currentHighBid: number
    createdAt: string
    updatedAt: string
    auctionEnd: string
    status: string
    make: string
    model: string
    year: number
    color: string
    mileage: number
    imageUrl: string
    id: string
}

//Define the Listing Component inside page.tsx 
import React from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import {Auction, PagedResult} from "@/types";

async function getData(): Promise<PagedResult<Auction>> {
    //Caches the data coming from the API
    const res = await fetch('http://localhost:6001/search?pageSize=10');
    if (!res.ok) {
        throw new Error('Failed to fetch data');
    }
    return res.json();
}
export default async function Listings() {
    const data = await getData();
    return (
        <div className="grid grid-cols-4 gap-6">
            {data && data.results.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
            ))}
        </div>
    )
}

//Define the Auction Card Component 
import React from 'react'
import CountdownTimer from "@/app/auctions/CountdownTimer";
import CarImage from "@/app/auctions/CarImage";
import {Auction} from "@/types";

type Props = {
    auction: Auction
}

export default function AuctionCard({auction}: Props) {
    return (
        <a href='#' className='group'>
            <div className='relative w-full bg-gray-200 aspect-[16/10] rounded-lg overflow-hidden'>
                <CarImage imageUrl={auction.imageUrl} />
                <div className='absolute bottom-2 left-2'>
                    <CountdownTimer auctionEnd={auction.auctionEnd}/>
                </div>
            </div>
            <div className='flex justify-between items-center mt-4'>
                <h3 className='text-gray-700'>{auction.make} {auction.model}</h3>
                <p className='font-semibold text-sm'>{auction.year}</p>
            </div>

        </a>
    )
}


//Define the CarImage Component 
'use client'
import React, {useState} from 'react'
import Image from "next/image";

type Props = {
    imageUrl: string
}

export default function CarImage({imageUrl}: Props) {
    const [isLoading, setLoading] = useState(true);
    return (
        <Image
            src={imageUrl}
            alt='image of car'
            fill
            priority
            sizes='(max-width:768px) 100vw,(max-width:1200px) 50vw, 25vw'
            className={
            object-cover group-hover:opacity-75 duration-700 ease-in-out
            ${isLoading ? 'grayscale blur-2xl scale-110':
                'grayscale-0 blur-0 scale-100'}
            }
            onLoad={() => setLoading(false)}
        />
    )
}



```

## Adding Pagination to the Cars List Component 
- We will use flowbite-react library for pagination 
```js 
//App Pagination Component 
'use client'
import React from 'react'
import {Pagination} from "flowbite-react";

type Props = {
    currentPage: number;
    pageCount: number;
    pageChanged: (page: number) => void;
}
export default function AppPagination({currentPage, pageCount, pageChanged}: Props) {
    return (
        <Pagination
        currentPage={currentPage}
        onPageChange={e=>pageChanged(e)}
        totalPages={pageCount}
        layout='pagination'
        showIcons={true}
        className='text-blue-500 mb-5'
        ></Pagination>
    )
}


//Calling the AppPagination from Listings Component
'use client'
import React, {useEffect, useState} from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import {Auction} from "@/types";
import AppPagination from "@/app/components/AppPagination";
import {getData} from "@/app/actions/auctionActions";
import Filters from "@/app/auctions/Filters";


export default function Listings() {
    const [auctions,setAuctions] = useState<Auction[]>([]);
    const [pageCount,setPageCount] = useState(0);
    const [pageNumber,setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(4);

    useEffect(() => {
        getData(pageNumber, pageSize).then(data=>{
            setAuctions(data.results);
            setPageCount(data.pageCount);
        })
    },[pageNumber,pageSize])

    if(auctions.length === 0){
        return <h3>Loading...</h3>
    }
    return (
        <>
            <Filters pageSize={pageSize} setPageSize={setPageSize}/>
        <div className="grid grid-cols-4 gap-6">
            {auctions.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
            ))}
        </div>
            <div className="flex justify-center mt-4">
                <AppPagination currentPage={pageNumber} pageCount={pageCount} pageChanged={setPageNumber} />
            </div>
        </>
    )
}


```
- To support external links for images, we have to modify next.config.ts file 
```js 
/** @type {import('next').NextConfig} */
const nextConfig = {
    logging:{
        fetches:{
            fullUrl:true
        }
    },
    images:{
        remotePatterns:[
            {protocol:'https',hostname:'cdn.pixabay.com'},
        ]
    }
};

export default nextConfig;


```

## Using Zustand for State Management 
- We create stores as hooks 
- Zustand is a lightweight and fast state management solution for React applications
- It helps you avoid boilerplate code and keeps your state management clean and efficient.
```js 
import create from 'zustand';

const useStore = create((set) => ({
  count: 0,
  increase: () => set((state) => ({ count: state.count + 1 })),
  decrease: () => set((state) => ({ count: state.count - 1 })),
}));


```
- We can access the store like this 
```js 
 import React from 'react';
import { useStore } from './store'; // Adjust the path as needed

const Counter = () => {
  const { count, increase, decrease } = useStore();
  
  return (
    <div>
      <h1>{count}</h1>
      <button onClick={increase}>Increase</button>
      <button onClick={decrease}>Decrease</button>
    </div>
  );
};

export default Counter;


```
- We can update the state using actions defined inside the store like this 
```js 
 // Example: Increase the count
useStore.getState().increase();


```
- Zustand also provides additional features like get and getState for more advanced state management
```js 
 const useStore = create((set, get) => ({
  count: 0,
  increase: (amount) => {
    const currentState = get();
    set((state) => ({ count: state.count + amount }));
  },
}));


```
- We will first create a store to store all our query string parameters like pageNumber, pageSize etc 

```js 
import {create} from "zustand/react";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
}

type Actions = {
    setParams:(params: Partial<State>) => void;
    reset:() => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: ''
}

export const useParamsStore = create<State & Actions>((set) => ({
    ...initialState,
    setParams:(newParams:Partial<State>)=>{
        set((state)=> {
            if(newParams.pageNumber){
                return {...state,pageNumber: newParams.pageNumber};
            } else {
                return {...state,...newParams, pageNumber:1};
            }
        });
    },
    reset:()=>set(initialState),
}))



```
- We will now use the store inside the Listings.tsx component 
-  Notice that now we are getting the state of all the params from the store we created above using the useParamsStore hook. 
-  We are also using the query-string package to build the url.
-  We are passing this url to our server side action "getdata" and then using its response to set the data. 
-  Also we are passing the state and functions from the store to the AppPagination component(which actually changes the state)
  

```js 
 'use client'
import React, {useEffect, useState} from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import {Auction, PagedResult} from "@/types";
import AppPagination from "@/app/components/AppPagination";
import {getData} from "@/app/actions/auctionActions";
import Filters from "@/app/auctions/Filters";
import {useParamsStore} from "@/hooks/useParamsStore";
import {useShallow} from "zustand/react/shallow";
import qs from 'query-string'


export default function Listings() {
    // const [auctions,setAuctions] = useState<Auction[]>([]);
    // const [pageCount,setPageCount] = useState(0);
    // const [pageNumber,setPageNumber] = useState(1);
    // const [pageSize, setPageSize] = useState(4);
    const [data,setData] = useState<PagedResult<Auction>>();
    const params = useParamsStore(useShallow (state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm
    })));

    const setParams = useParamsStore(state =>state.setParams);
    const url = qs.stringifyUrl({url:'',query:params});

    function setPageNumber(pageNumber: number) {
        setParams({pageNumber: pageNumber});
    }

    useEffect(() => {
        getData(url).then(data=>{
            // setAuctions(data.results);
            // setPageCount(data.pageCount);
            setData(data);
        })
    },[url])

    if(!data){
        return <h3>Loading...</h3>
    }
    return (
        <>
            <Filters />
        <div className="grid grid-cols-4 gap-6">
            {data.results.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
            ))}
        </div>
            <div className="flex justify-center mt-4">
                <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount} pageChanged={setPageNumber} />
            </div>
        </>
    )
}



```
### useShallow hook 
- The useShallow hook in Zustand is used to optimize re-renders by memoizing selector functions using shallow comparison. This means that the selector function will only cause a re-render if the output has changed according to shallow equality (i.e., if the references or primitive values have changed, but not deep equality)
```js
 import { create } from 'zustand';
import { useShallow } from 'zustand/react/shallow';

const useStore = create((set) => ({
  items: ['apple', 'banana', 'cherry'],
}));

export const BearNames = () => {
  const names = useStore(useShallow((state) => state.items));
  return <div>{names.join(', ')}</div>;
};


```
- In this example, BearNames component uses useShallow to memoize the selector function that retrieves the items from the store. 
- If the items array is updated but remains shallowly equal (i.e., the same references), the component will not re-render unnecessarily
  


## Adding the Search Functionality
- We will add a Search component for this 
- First we will have to update the useParams Store to set the search value 
```js 
 import {create} from "zustand/react";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    searchValue: string;
}

type Actions = {
    setParams:(params: Partial<State>) => void;
    reset:() => void;
    setSearchValue: (value: string) => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: '',
    searchValue:''
}

export const useParamsStore = create<State & Actions>((set) => ({
    ...initialState,
    setParams:(newParams:Partial<State>)=>{
        set((state)=> {
            if(newParams.pageNumber){
                return {...state,pageNumber: newParams.pageNumber};
            } else {
                return {...state,...newParams, pageNumber:1};
            }
        });
    },
    reset:()=>set(initialState),
    setSearchValue: (value: string) => {
        set({searchValue:value});
    }
}))

```
- Next we will have to create a Search component which will use the useParamsStore to update the search Term params, build a new url and trigger a refresh in useEffect of Listing component and update the data.
```js 
  'use client'
import {FaSearch} from "react-icons/fa";
import {useParamsStore} from "@/hooks/useParamsStore";

export default function Search() {
    const setParams = useParamsStore(state =>state.setParams);
    const setSearchValue = useParamsStore(state => state.setSearchValue);
    const searchValue = useParamsStore(state => state.searchValue);


    function search(){
        setParams({searchTerm:searchValue});
    }

    return (
        <div className="flex w-full items-center border-2 rounded-full py-2 shadow-sm">
            <input
                type ='text'
                placeholder='Search for cars'
                onKeyDown={(e)=>{
                    if(e.key === 'Enter'){
                        search()
                    }
                }}
                value={searchValue}
                onChange={(event) => setSearchValue(event.target.value)}
                className=flex-grow pl-5 bg-transparent  focus:outline-none
                border-transparent
                focus:border-transparent
                focus:ring-0
                text-sm
                text-gray-600
                
            />
            <button onClick={search}>
                <FaSearch size={34} className="bg-red-400 text-white rounded-full p-2 cursor-pointer mx-2" />
            </button>
        </div>
    )
}


```

## Adding the sorting functionality 
- First we will have to make changes to the useParamsStore to add an orderBy parameter which we will be used to generate the URL and pass it to the Listing Component which will send it to the server action to sort the data .
```js 
import {create} from "zustand/react";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    searchValue: string;
    orderBy: string;
}

type Actions = {
    setParams:(params: Partial<State>) => void;
    reset:() => void;
    setSearchValue: (value: string) => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: '',
    searchValue:'',
    orderBy:'make',
}

export const useParamsStore = create<State & Actions>((set) => ({
    ...initialState,
    setParams:(newParams:Partial<State>)=>{
        set((state)=> {
            if(newParams.pageNumber){
                return {...state,pageNumber: newParams.pageNumber};
            } else {
                return {...state,...newParams, pageNumber:1};
            }
        });
    },
    reset:()=>set(initialState),
    setSearchValue: (value: string) => {
        set({searchValue:value});
    }
}))

```
- Next we will have to update the Filters component to first have the buttons for sorting like this 
```js 
 const orderButtons = [
    {
        label:'Alphabetical',
        icon: AiOutlineSortAscending,
        value:'make'
    },
    {
        label:'End Date',
        icon: AiOutlineClockCircle,
        value:'endingSoon'
    },
    {
        label:'Recently Added',
        icon: BsFillStopCircleFill,
        value:'new'
    }

]

```
- Then we will have to use these buttons inside the Filter component like this 
```js 
 'use client'

import React from 'react'
import {Button} from "flowbite-react";
import {useParamsStore} from "@/hooks/useParamsStore";
import {AiOutlineClockCircle, AiOutlineSortAscending} from "react-icons/ai";
import {BsFillStopCircleFill} from "react-icons/bs";

const pageSizeButtons = [4,8,12];
const orderButtons = [
    {
        label:'Alphabetical',
        icon: AiOutlineSortAscending,
        value:'make'
    },
    {
        label:'End Date',
        icon: AiOutlineClockCircle,
        value:'endingSoon'
    },
    {
        label:'Recently Added',
        icon: BsFillStopCircleFill,
        value:'new'
    }

]

export default function Filters() {
    const pageSize = useParamsStore(state => state.pageSize);
    const setParams = useParamsStore(state => state.setParams);
    const orderBy = useParamsStore(state => state.orderBy);

    return (
        <div className="flex justify-between items-center mb-4">
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Order By</span>
                <Button.Group>
                    {orderButtons.map(({label,icon:Icon,value}) => (
                        <Button color={`${orderBy===value ? 'red':'gray'}`} key={value} onClick={()=>setParams({orderBy:value})} >
                            <Icon className='mr-3 h-4 w-4'/>
                            {label}
                        </Button>
                    ))}
                </Button.Group>
            </div>
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Page Size</span>
                <Button.Group>
                    {pageSizeButtons.map((size,i) => (
                        <Button key={i}
                                onClick={() => setParams({pageSize:size})}
                        color={`${pageSize === size ? 'red':'gray'}`}
                        className='focus:ring-0'
                        >
                            {size}
                        </Button>
                    )) }
                </Button.Group>
            </div>
        </div>
    )
}



```
- Adding the Filtering functionality 
- It follows the same pattern as orderBy 
- First we update the useParamsStore() like this 
```js 
 import {create} from "zustand/react";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    searchValue: string;
    orderBy: string;
    filterBy: string;
}

type Actions = {
    setParams:(params: Partial<State>) => void;
    reset:() => void;
    setSearchValue: (value: string) => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: '',
    searchValue:'',
    orderBy:'make',
    filterBy:'live'
}

export const useParamsStore = create<State & Actions>((set) => ({
    ...initialState,
    setParams:(newParams:Partial<State>)=>{
        set((state)=> {
            if(newParams.pageNumber){
                return {...state,pageNumber: newParams.pageNumber};
            } else {
                return {...state,...newParams, pageNumber:1};
            }
        });
    },
    reset:()=>set(initialState),
    setSearchValue: (value: string) => {
        set({searchValue:value});
    }
}))

```
- Then we have to update the Filter component and add the buttons 

```js 
 'use client'

import React from 'react'
import {Button} from "flowbite-react";
import {useParamsStore} from "@/hooks/useParamsStore";
import {AiOutlineClockCircle, AiOutlineSortAscending} from "react-icons/ai";
import {BsFillStopCircleFill, BsStopwatchFill} from "react-icons/bs";
import {GiFinishLine, GiFlame} from "react-icons/gi";

const pageSizeButtons = [4,8,12];
const orderButtons = [
    {
        label:'Alphabetical',
        icon: AiOutlineSortAscending,
        value:'make'
    },
    {
        label:'End Date',
        icon: AiOutlineClockCircle,
        value:'endingSoon'
    },
    {
        label:'Recently Added',
        icon: BsFillStopCircleFill,
        value:'new'
    }

]

const filterButtons = [
    {
        label:'Live Auctions',
        icon: GiFlame,
        value:'live'
    },
    {
        label:'Ending < 6 hours',
        icon: GiFinishLine,
        value:'endingSoon'
    },
    {
        label:'Completed',
        icon: BsStopwatchFill,
        value:'finished'
    }

]

export default function Filters() {
    const pageSize = useParamsStore(state => state.pageSize);
    const setParams = useParamsStore(state => state.setParams);
    const orderBy = useParamsStore(state => state.orderBy);
    const filterBy = useParamsStore(state => state.filterBy);

    return (
        <div className="flex justify-between items-center mb-4">
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Filter By</span>
                <Button.Group>
                    {filterButtons.map(({label, icon: Icon, value}) => (
                        <Button color={`${filterBy === value ? 'red' : 'gray'}`} key={value}
                                onClick={() => setParams({filterBy: value})}>
                            <Icon className='mr-3 h-4 w-4'/>
                            {label}
                        </Button>
                    ))}
                </Button.Group>
            </div>
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Order By</span>
                <Button.Group>
                    {orderButtons.map(({label, icon: Icon, value}) => (
                        <Button color={`${orderBy === value ? 'red' : 'gray'}`} key={value}
                                onClick={() => setParams({orderBy: value})}>
                            <Icon className='mr-3 h-4 w-4'/>
                            {label}
                        </Button>
                    ))}
                </Button.Group>
            </div>
            <div>
                <span className='uppercase text-sm text-gray-500 mr-2'>Page Size</span>
                <Button.Group>
                    {pageSizeButtons.map((size, i) => (
                        <Button key={i}
                                onClick={() => setParams({pageSize: size})}
                                color={`${pageSize === size ? 'red' : 'gray'}`}
                                className='focus:ring-0'
                        >
                            {size}
                        </Button>
                    ))}
                </Button.Group>
            </div>
        </div>
    )
}


```
- Finally we have to update Listing component to use FilterBy filter 
```js 
  'use client'
import React, {useEffect, useState} from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import {Auction, PagedResult} from "@/types";
import AppPagination from "@/app/components/AppPagination";
import {getData} from "@/app/actions/auctionActions";
import Filters from "@/app/auctions/Filters";
import {useParamsStore} from "@/hooks/useParamsStore";
import {useShallow} from "zustand/react/shallow";
import qs from 'query-string'


export default function Listings() {
    const [data,setData] = useState<PagedResult<Auction>>();
    const params = useParamsStore(useShallow (state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm,
        orderBy: state.orderBy,
        filterBy: state.filterBy
    })));

    const setParams = useParamsStore(state =>state.setParams);
    const url = qs.stringifyUrl({url:'',query:params});

    function setPageNumber(pageNumber: number) {
        setParams({pageNumber: pageNumber});
    }

    useEffect(() => {
        getData(url).then(data=>{
            // setAuctions(data.results);
            // setPageCount(data.pageCount);
            setData(data);
        })
    },[url])

    if(!data){
        return <h3>Loading...</h3>
    }
    return (
        <>
            <Filters />
        <div className="grid grid-cols-4 gap-6">
            {data.results.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
            ))}
        </div>
            <div className="flex justify-center mt-4">
                <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount} pageChanged={setPageNumber} />
            </div>
        </>
    )
}


```

## Adding a component to be displayed when there are zero results. 
- Add an Empty Filter component like this which will call reset function of our state 
```js 
import React from 'react'
import {useParamsStore} from "@/hooks/useParamsStore";
import Heading from "@/app/components/Heading";
import {Button} from "flowbite-react";
type Props = {
    title?:string
    subtitle?:string
    showReset?: boolean
}

export default function EmptyFilter({
    title='No matches for this filter',
    subtitle='Try changing the filter',
    showReset
                                    }:Props) {

    const reset = useParamsStore(state =>state.reset);
    return (
        <div className="h-[40vh] flex flex-col gap-2 justify-center items-center shadow-lg">
            <Heading title={title} subTitle={subtitle} center />
            <div className="mt-4">
                {showReset && (
                    <Button outline onClick={reset}>
                        Remove Filters
                    </Button>
                )}
            </div>
        </div>
    )
}


```

- We can use it inside Listing component like this 
```js 
 'use client'
import React, {useEffect, useState} from 'react'
import AuctionCard from "@/app/auctions/AuctionCard";
import {Auction, PagedResult} from "@/types";
import AppPagination from "@/app/components/AppPagination";
import {getData} from "@/app/actions/auctionActions";
import Filters from "@/app/auctions/Filters";
import {useParamsStore} from "@/hooks/useParamsStore";
import {useShallow} from "zustand/react/shallow";
import qs from 'query-string'
import EmptyFilter from "@/app/components/EmptyFilter";


export default function Listings() {
    const [data,setData] = useState<PagedResult<Auction>>();
    const params = useParamsStore(useShallow (state => ({
        pageNumber: state.pageNumber,
        pageSize: state.pageSize,
        searchTerm: state.searchTerm,
        orderBy: state.orderBy,
        filterBy: state.filterBy
    })));

    const setParams = useParamsStore(state =>state.setParams);
    const url = qs.stringifyUrl({url:'',query:params});

    function setPageNumber(pageNumber: number) {
        setParams({pageNumber: pageNumber});
    }

    useEffect(() => {
        getData(url).then(data=>{
            setData(data);
        })
    },[url])

    if(!data){
        return <h3>Loading...</h3>
    }

    return (
        <>
            <Filters />
            {data.totalCount  === 0 ? (
                <EmptyFilter showReset/>
            ):(
                <>
                    <div className="grid grid-cols-4 gap-6">
                        {data.results.map((auction) => (
                            <AuctionCard key={auction.id} auction={auction}/>
                        ))}
                    </div>
                    <div className="flex justify-center mt-4">
                        <AppPagination currentPage={params.pageNumber} pageCount={data.pageCount}
                                       pageChanged={setPageNumber}/>
                    </div>
                </>
            )
            }
        </>
    )
}


```


