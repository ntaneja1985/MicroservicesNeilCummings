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

## Authentication in Next.js using NextAuth
- NextAuth (soon to become Auth.js)
- We will look at NextAuth v4. 
- We will first login to Identity Server by passing our Client ID 
- We will login to the Identity Server using our credentials and then get an authorization code. 
- We will then be redirect to the client App to the callback url. 
- The client app will send authorization code, client id, client secret to the identity server and it will respond with an access token. 
- ![alt text](image-59.png)
- NextAuth at this time will store an encrypted cookie into our browser, which it will use to maintain a session with itself. 
- Client App can at this point of time use the access token to request resources from our resource server.
-  ![alt text](image-60.png)
- Auth.js is a runtime agnostic library based on standard Web APIs. 
- It integrates deeply with multiple modern JavaScript frameworks to provide an authentication experience that's simple to get started with, easy to extend, and always private and secure
- Auth.js supports various authentication methods, including OAuth authentication (e.g., Google, GitHub), Magic Links (e.g., email providers), Credentials (e.g., username and password), and WebAuthn (e.g., passkeys). 
- It also supports multiple databases through adapters, such as Prisma, Firebase, and Supabase.
```shell 
npm install next-auth@beta

```
- The only environment variable that is mandatory is the AUTH_SECRET. This is a random value used by the library to encrypt tokens and email verification hashes.
- This is used to decrypt our JWT Token.
```shell
 npx auth secret
```
- This will also add it to your .env file, respecting the framework conventions (eg.: Next.js's .env.local).
- Next, create the Auth.js config file and object. This is where you can control the behaviour of the library and specify custom authentication logic, adapters, etc. 
- We recommend all frameworks to create an auth.ts file in the project.
- In this file we'll pass in all the options to the framework specific initalization function and then export the route handler(s), signin and signout methods, and more.
```js 
//auth.ts file
import NextAuth from "next-auth"

export const { handlers, signIn, signOut, auth } = NextAuth({
    providers: [],
})

```
### Route Handler: It is Next.js way of being a backend server just like .NET 
- Next.js can also serve as API server and it can also receive GET/POST requests. 
- In Next.js if anything wants to be an API route, file name is called route.ts 
- We will create a file here: ./app/api/auth/[...nextauth]/route.ts and paste this code: 
```js 
import { handlers } from "@/auth" // Referring to the auth.ts we just created
export const { GET, POST } = handlers

```
- Add optional Middleware to keep the session alive, this will update the session expiry every time its called.
```js 
export { auth as middleware } from "@/auth"
```
- Since we are using Duende Identity Server, we will have to set it up in the providers array in the auth.ts file 
```js 
import NextAuth, {Profile} from "next-auth"
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"
import {OIDCConfig} from "@auth/core/providers";

export const { handlers, signIn, signOut, auth } = NextAuth({
    session:{
        strategy: 'jwt'
    },
    providers: [
        //Here client Id, client Secret are the ones we configured in Config.cs file of Identity Server
        DuendeIDS6Provider({
            id:'id-server',
            clientId: "nextApp",
            clientSecret: "secret",
            issuer: "http://localhost:5000",
            authorization:{params:{scope:'openid profile auctionApp'}},
            idToken:true
        } as OIDCConfig<Profile>),
    ],
})

```
- Now to use we will create a separate client component: LoginButton.tsx like this which we use in the Navbar component 
```js 
'use client'
import {Button} from "flowbite-react";
import {signIn} from "next-auth/react";

export default function LoginButton() {
    return (
        <Button outline onClick={()=> signIn('id-server',{redirectTo:'/'},{prompt: 'login'}) }>Login</Button>
    )
}



```
- Now if we click on the login button, we will be redirected to the Duende Identity Server login page and once successfully logged in, we will get an auth.js.session.token 
- ![alt text](image-61.png)
- When we click on Login button, our identity provider information is sent to the identity server along with the scope information, clientId, secret 
- We get a code back from the identity server. 
- We then pass this code to the identity server and get the access token which we store inside our cookie 
- ![alt text](image-62.png)

## Getting Session Details from the SessionToken in the Next.js App 
- Once a user is logged in, you often want to get the session object in order to use the data in some way. A common use-case is to show their profile picture or display some other user information.
```js 
 import { auth } from "../auth"
 
export default async function UserAvatar() {
  const session = await auth()
 
  if (!session?.user) return null
 
  return (
    <div>
      <img src={session.user.image} alt="User Avatar" />
    </div>
  )
}

```
- To get the current user info we will create a separate server actions file authActions.ts 
```js 
 'use server'

import {auth} from "@/auth";

export async function getCurrentUser() {
    try {
        const session = await auth();
        if(!session) {return null}

        return session.user;
    }
    catch (error) {
        console.log(error)

        return null;
    }
}

```
- We will also setup callback functions inside our auth.ts file 
```js 
 import NextAuth, {Profile} from "next-auth"
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6"
import {OIDCConfig} from "@auth/core/providers";

export const { handlers, signIn, signOut, auth } = NextAuth({
    session:{
        strategy: 'jwt'
    },
    //Identity Server configuration information provided to Next.js server
    providers: [
        DuendeIDS6Provider({
            id:'id-server',
            clientId: "nextApp",
            clientSecret: "secret",
            issuer: "http://localhost:5000",
            authorization:{params:{scope:'openid profile auctionApp'}},
            idToken:true
        } as OIDCConfig<Omit<Profile,'username'>>),
    ],
    callbacks:{
        async jwt({token,profile}){
            if(profile) {
                token.username = profile.username;
            }
            return token;
        },
        async session({session,token}){
            console.log({session,token})
            if(token) {
                session.user.username = token.username;
            }
            return session;
        }
    }
}) 


```
- In NextAuth.js, callback functions provide a way to control what happens at various stages of the authentication process. They are useful for modifying the default behavior and adding custom logic. 
- Here are the main types of callbacks and how you can use them:
- **signIn**: This callback is triggered whenever a user signs in. It can be used to control whether the sign-in is allowed. 
- For example, you can use it to deny sign-in based on certain conditions:
```js 
 callbacks: {
  async signIn({ user, account, profile, email, credentials }) {
    // Only allow sign-in if the user's email domain is 'example.com'
    if (email.endsWith('@example.com')) {
      return true
    }
    return false
  }
}



```
- **redirect**: This callback is called whenever a user is redirected after a successful sign-in or sign-out. 
- It can be used to control the URL to which the user is redirected:
```js 
 callbacks: {
  async redirect({ url, baseUrl }) {
    // Redirect to the home page after sign-in
    return baseUrl
  }
}


```
- **session**: This callback is triggered whenever a session is checked, such as on API requests or client-side requests. 
- It can be used to modify the session object before it is returned to the client:
```js 
 callbacks: {
  async session({ session, user, token }) {
    // Add user ID to the session object
    session.user.id = token.id
    return session
  }
}


```
- **jwt**: This callback is called whenever a JSON Web Token (JWT) is created or updated. 
- It can be used to add custom properties to the token:
```js 
 callbacks: {
  async jwt({ token, user, account, profile, isNewUser }) {
    // Add user ID to the token
    if (user) {
      token.id = user.id
    }
    return token
  }
}


```
- **signOut**: This callback is triggered whenever a user signs out. 
- It can be used to perform any actions required upon sign-out:
```js 
 callbacks: {
  async signOut({ token, user }) {
    // Perform any sign-out actions
    return true
  }
}


```
- To use callbacks, use them like this 
```js 
 import NextAuth from 'next-auth'
import Providers from 'next-auth/providers'

export default NextAuth({
  providers: [
    Providers.Google({
      clientId: process.env.GOOGLE_CLIENT_ID,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET,
    }),
  ],
  callbacks: {
    // Your custom callbacks go here
  }
})


```

## Populating the User Actions Dropdown
- This can be done as follows: 
```js 
 'use client'
import React from 'react'
import { Dropdown, DropdownDivider, DropdownItem} from "flowbite-react";
import Link from "next/link";
import {User} from 'next-auth'
//import {useRouter} from "next/router";
import {HiCog, HiUser} from "react-icons/hi";
import {AiFillCar, AiFillTrophy, AiOutlineLogout} from "react-icons/ai";
import {signOut} from "next-auth/react";


type Props = {
    user: User
}
export default function UserActions({user}:Props) {
    //const router = useRouter();

    return (
       <Dropdown inline label = {`Welcome ${user.name}`} >
            <DropdownItem icon = {HiUser}>
                <Link href="/">
                    My Auctions
                </Link>
            </DropdownItem>
           <DropdownItem icon = {AiFillTrophy}>
               <Link href="/">
                   Auctions Won
               </Link>
           </DropdownItem>
           <DropdownItem icon = {AiFillCar}>
               <Link href="/">
                   Sell My Car
               </Link>
           </DropdownItem>
           <DropdownItem icon = {HiCog}>
               <Link href="/session">
                   Session (dev)
               </Link>
           </DropdownItem>
           <DropdownDivider/>
           <DropdownItem icon = {AiOutlineLogout} onClick={()=> signOut({redirectTo:'/'})}>
              Sign Out
           </DropdownItem>
       </Dropdown>
    )
}



```
- useRouter: This hook allows you to access the Next.jsrouter object, providing methods and properties that help you navigate and manipulate the router. It can be very useful for programmatically changing routes, getting the current route parameters, and more. 
- Here's a basic example:
```js 
 import { useRouter } from 'next/router'

const MyComponent = () => {
  const router = useRouter()

  const handleClick = () => {
    router.push('/about')
  }

  return (
    <button onClick={handleClick}>Go to About</button>
  )
}


```
- usePathname: This hook returns the current pathname, which is the path part of the URL. It is useful when you need to perform actions based on the current route without triggering a rerender. 
- An example usage would be:
```js 
 import { usePathname } from 'next/navigation'

const MyComponent = () => {
  const pathname = usePathname()

  return (
    <div>Current Pathname: {pathname}</div>
  )
}


```

## Protecting Routes 
- Securing routes in Next.js is an essential aspect of building applications, especially when you want to restrict access to certain pages based on user authentication or roles. 
- Protecting routes can be done generally by checking for the session and taking an action if an active session is not found, like redirecting the user to the login page or simply returning a 401: Unauthenticated response.
- Here are some common methods to protect routes:
- 1. Client-Side Protection:
- You can use React hooks like useEffect and useRouter to perform client-side checks for authentication. 
- This approach can redirect users if they are not authenticated.
```js 
 import { useEffect } from 'react'
import { useRouter } from 'next/router'
import { useAuth } from '../context/AuthContext' // Custom hook for authentication

const ProtectedPage = () => {
  const router = useRouter()
  const { user } = useAuth() 

  useEffect(() => {
    if (!user) {
      router.push('/login')
    }
  }, 

```
- One more example can be using session object like this 
```js 
 import { auth } from "@/auth"
 
export default async function Page() {
  const session = await auth()
  if (!session) return <div>Not authenticated</div>
 
  return (
    <div>
      <pre>{JSON.stringify(session, null, 2)}</pre>
    </div>
  )
}


```
- With Next.js 12+, the easiest way to protect a set of pages is using the middleware file. You can create a middleware.ts file in your root pages directory with the following contents.
```js 
 export { auth as middleware } from "@/auth"

```
- Then define authorized callback in your auth.ts file.
```js 
 import NextAuth from "next-auth"
 
export const { auth, handlers } = NextAuth({
  callbacks: {
    authorized: async ({ auth }) => {
      // Logged in users are authenticated, otherwise redirect to login page
      return !!auth
    },
  },
})

```
- You can also use the auth method as a wrapper if you'd like to implement more logic inside the middleware.
```js 
 import { auth } from "@/auth"
 
export default auth((req) => {
  if (!req.auth && req.nextUrl.pathname !== "/login") {
    const newUrl = new URL("/login", req.nextUrl.origin)
    return Response.redirect(newUrl)
  }
})


```
- In our application we implement a middleware.ts file like this
- Middleware will protect pages as defined by the matcher config export. 
- Here we can also use a regex to match multiple routes or you can negate certain routes in order to protect all remaining routes. 

 
```js 
 export {auth as middleware} from "@/auth"

export const config = {
    matcher: [
        '/session'
    ],
    pages:{
        signIn: '/api/auth/signIn'
    }
}

```
- Then we define a file called signIn inside api/auth folder 
```js 
 import React from 'react'
import EmptyFilter from "@/app/components/EmptyFilter";

export default function SignIn({searchParams}:{searchParams:{callbackUrl:string}}) {
    return (
        <EmptyFilter
        title="You need to be logged in to do that"
        subtitle='Please click below to login'
        showLogin
        callbackUrl={searchParams.callbackUrl}

        />
    )
}



```
- Then we specify the Empty Filter component like this: 
```js 
 'use client'
import {useParamsStore} from "@/hooks/useParamsStore";
import Heading from "@/app/components/Heading";
import {Button} from "flowbite-react";
import {signIn} from "next-auth/react";
type Props = {
    title?:string
    subtitle?:string
    showReset?: boolean
    showLogin?:boolean
    callbackUrl?:string
}

export default function EmptyFilter({
    title='No matches for this filter',
    subtitle='Try changing the filter',
    showReset,
    showLogin,
    callbackUrl
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
                {showLogin && (
                    <Button outline onClick={()=>signIn('id-server',{redirectTo:callbackUrl})}>
                        Login
                    </Button>
                )}
            </div>
        </div>
    )
}


```

## Testing API Authentication
- We know we get a session object using var session = await auth(). 
- But we donot have the access token populated inside it. We can fix it by modifying the callback functions inside auth.ts like this: 
- Keep in mind we will have to update next-ath.d.ts 
```js 
 import NextAuth, { type DefaultSession } from "next-auth"
import {JWT} from 'next-auth/jwt'

declare module "next-auth" {
    /**
     * Returned by `auth`, `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
     */
    interface Session {
        user: {
            /** The user's postal address. */
            username: string
            /**
             * By default, TypeScript merges new interface properties and overwrites existing ones.
             * In this case, the default session user properties will be overwritten,
             * with the new ones defined above. To keep the default session user properties,
             * you need to add them back into the newly declared interface.
             */
        } & DefaultSession["user"]
        accessToken: string
    }

    interface Profile {
        username: string
    }
}

declare module "next-auth/jwt" {
    interface JWT {
        username:string
        accessToken:string
    }
}


```
- We will modify the callbacks to include the access token.
```js 
 callbacks:{
        async jwt({token,profile, account}){
            if(account && account.access_token){
                token.accessToken = account.access_token;
            }
            if(profile) {
                token.username = profile.username;
            }
            console.log(token);
            return token;
        },
        async session({session,token}){
            if(token) {
                session.user.username = token.username;
                session.accessToken = token.accessToken;
            }
            return session;
        },

```
- Now we will make a server action to update the Auction like this 
```js 
  export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 10000) + 1,
    }

    const session = await auth();
    const res = await fetch('http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${session?.accessToken}`
        },
        body: JSON.stringify(data)
    });

    if(!res.ok) {
        return {status:res.status, data:res.statusText}
    }

    return res.statusText;
}

```

## Why are we storing session token inside our client browser 
- That token is encrypted using NEXTAUTH_SECRET 
- Token is an HTTP only cookie so it cannot be accessed by malicious JS 


## CRUD Operations in the client app 
- Forms are client side functionality. They are not server side functionality. 
- We need to get data from the form and submit it to the server. 
- We will use react-hook-form package for this 
```shell 
 npm install react-hook-form react-datepicker


```
- react-hook-form is a library that helps you manage form state and validation in React applications with ease.
-  It's a lightweight library, which makes it a popular choice for developers who want to improve performance and scalability.
```js 
 import React from 'react'
import { useForm } from 'react-hook-form'

const MyForm = () => {
  const { register, handleSubmit, formState: { errors } } = useForm()
  
  const onSubmit = data => {
    console.log(data)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label>Name</label>
        <input {...register('name', { required: true })} />
        {errors.name && <p>Name is required.</p>}
      </div>
      <div>
        <label>Email</label>
        <input {...register('email', { 
          required: "Email is required",
          pattern: {
            value: /^\S+@\S+$/i,
            message: "Invalid email address"
          }
        })} />
        {errors.email && <p>{errors.email.message}</p>}
      </div>
      <button type="submit">Submit</button>
    </form>
  )
}

export default MyForm


```
- The library makes it easy to apply validation rules directly to input fields.
-  We can use the register function to define these rules and provide meaningful error messages.
```js 
 import React from 'react'
import { useForm } from 'react-hook-form'

const MyForm = () => {
  const { register, handleSubmit, formState: { errors } } = useForm()
  
  const onSubmit = data => {
    console.log(data)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label>Name</label>
        <input {...register('name', { required: true })} />
        {errors.name && <p>Name is required.</p>}
      </div>
      <div>
        <label>Email</label>
        <input 
          {...register('email', { 
            required: "Email is required",
            pattern: {
              value: /^\S+@\S+$/i,
              message: "Invalid email address"
            }
          })} 
        />
        {errors.email && <p>{errors.email.message}</p>}
      </div>
      <button type="submit">Submit</button>
    </form>
  )
}

export default MyForm



```
- In the above example if we submit the form to the server and we get validation errors we can set the errors using the setError function provided by react-hook-form 
```js 
 import React from 'react'
import { useForm } from 'react-hook-form'

const MyForm = () => {
  const { register, handleSubmit, setError, formState: { errors } } = useForm()

  const onSubmit = async data => {
    try {
      const response = await fetch('/api/submit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
      })

      const result = await response.json()

      if (!response.ok) {
        // Assuming the server returns an object with error messages keyed by field names
        Object.keys(result.errors).forEach(field => {
          setError(field, {
            type: 'server',
            message: result.errors[field]
          })
        })
      } else {
        console.log('Form submitted successfully:', result)
      }
    } catch (error) {
      console.error('Error submitting form:', error)
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label>Name</label>
        <input {...register('name', { required: true })} />
        {errors.name && <p>{errors.name.message}</p>}
      </div>
      <div>
        <label>Email</label>
        <input 
          {...register('email', { 
            required: "Email is required",
            pattern: {
              value: /^\S+@\S+$/i,
              message: "Invalid email address"
            }
          })} 
        />
        {errors.email && <p>{errors.email.message}</p>}
      </div>
      <button type="submit">Submit</button>
    </form>
  )
}

export default MyForm


```
### We use the register function of react-hook-form to register our inputs to the form and then react-hook-form will track those inputs 
- We can track the formState as well. 
```js 
 'use client'
import {FieldValues, useForm} from "react-hook-form";
import {Button, TextInput} from "flowbite-react";


export default function AuctionForm() {
    const {register, handleSubmit, setFocus, formState:{isSubmitting,isValid,isDirty,errors}} = useForm();
    function onSubmit(data: FieldValues) {
        console.log(data);
    }
    return (
        <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
            <div className='mb-3 block'>
                <TextInput
                    {...register('make', {required: 'Make is required'})}
                    placeholder='Make'
                    color={errors?.make && 'failure'}
                    helperText={errors?.make?.message as string}
                />
            </div>
            <div className='mb-3 block'>
                <TextInput
                    {...register('model', {required: 'Model is required'})}
                    placeholder='Model'
                    color={errors?.model && 'failure'}
                    helperText={errors?.model?.message as string}
                />
            </div>
            <div className='flex justify-between'>
                <Button outline color='gray'>Cancel</Button>
                <Button
                    isProcessing={isSubmitting}
                    //disabled={!isValid}
                    type='submit'
                    outline
                    color='success'>Submit</Button>
            </div>
        </form>
    )
}



```

## Creating a reusable form input 
- The useController hook is part of the react-hook-form library and provides a way to control an individual form field, giving you more granular control over the field's state and behavior. 
- It's particularly useful when you need to integrate custom components with your form.
- Create a custom input component: 
```js 
 import React from 'react'
import { useController } from 'react-hook-form'

const CustomInput = ({ control, name, rules }) => {
  const {
    field: { onChange, onBlur, value, ref },
    fieldState: { error }
  } = useController({
    name,
    control,
    rules
  })

  return (
    <div>
      <input
        name={name}
        ref={ref}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
      />
      {error && <p>{error.message}</p>}
    </div>
  )
}


```

- Use custom input within a form like this 
```js 
 import React from 'react'
import { useForm, Controller } from 'react-hook-form'
import CustomInput from './CustomInput'

const MyForm = () => {
  const { control, handleSubmit } = useForm()

  const onSubmit = data => {
    console.log(data)
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Controller
        name="customInput"
        control={control}
        render={({ field }) => (
          <CustomInput {...field} control={control} name="customInput" rules={{ required: 'This field is required' }} />
        )}
      />
      <button type="submit">Submit</button>
    </form>
  )
}

export default MyForm



```
- We created a custom input element like this :
```js 
 import {useController, UseControllerProps} from "react-hook-form";
import {Label, TextInput} from "flowbite-react";

type Props = {
    label: string;
    type?:string;
    showLabel?: boolean;
} & UseControllerProps
export default function Input(props: Props) {
    const {fieldState,field} = useController({...props, defaultValue:''});

    return (
        <div className='mb-3'>
            {props.showLabel && (
                <div className='mb-2 block'>
                <Label htmlFor={field.name} value={props.label}/>
                </div>
            )}
            <TextInput
                {...props}
                {...field}
                type = {props.type || 'text'}
                placeholder={props.label}
                color={fieldState.error ? 'failure' : !fieldState.isDirty ? '':'success'}
                helperText={fieldState.error?.message}
            />

        </div>
    )
}



```
- We used it inside the AuctionForm.tsx like this 
```js 
 'use client'
import {FieldValues, useForm} from "react-hook-form";
import {Button, TextInput} from "flowbite-react";
import Input from "@/app/components/Input";


export default function AuctionForm() {
    // const {register, handleSubmit, setFocus, formState:{isSubmitting,isValid,isDirty,errors}} = useForm();
    const {control, handleSubmit, setFocus, formState:{isSubmitting,isValid,isDirty,errors}} = useForm();
    function onSubmit(data: FieldValues) {
        console.log(data);
    }
    return (
        <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
           <Input label='Make'
                  name='make'
                  control={control}
                  rules={{required:'Make is required'}}/>
            <Input label='Model'
                   name='model'
                   control={control}
                   rules={{required:'Model is required'}}/>
            <div className='flex justify-between'>
                <Button outline color='gray'>Cancel</Button>
                <Button
                    isProcessing={isSubmitting}
                    //disabled={!isValid}
                    type='submit'
                    outline
                    color='success'>Submit</Button>
            </div>
        </form>
    )
}


```

## Creating a custom reusable Date Input
- We will make use of react-datepicker package 
```js 
 import {useController, UseControllerProps} from "react-hook-form";
import {Label} from "flowbite-react";
import 'react-datepicker/dist/react-datepicker.css'
import DatePicker, {DatePickerProps} from "react-datepicker";

type Props = {
    label: string;
    type?:string;
    showLabel?: boolean;
} & UseControllerProps & DatePickerProps;
export default function DateInput(props: Props) {
    const {fieldState,field} = useController({...props, defaultValue:''});

    return (
        <div className='mb-3'>
            {props.showLabel && (
                <div className='mb-2 block'>
                    <Label htmlFor={field.name} value={props.label}/>
                </div>
            )}
            <DatePicker
                {...props}
                {...field}
                placeholderText={props.label}
                selected={field.value}
                className={`
                rounded-lg 
                w-[100%]
                flex flex-col 
                ${fieldState.error ? 'bg-red-50 border-red-500 text-red-900' :
                    (!fieldState.invalid && fieldState.isDirty) ?
                        'bg-green-50 border-green-500 text-green-900':
                        ''
                }
                `}
            />
            {fieldState.error && (
                <div className='text-red-500 text-sm'>
                    {fieldState.error.message}
                </div>
            )}
        </div>
    )
}


```
- We can use this datepicker like this 
```js 
  <DateInput
                    label='Auction End Date/Time'
                    name='auctionEnd'
                    dateFormat={'dd MMMM yyyy h:mm a'}
                    showTimeSelect
                    control={control}
                    rules={{required: 'Auction End Date is required'}}
                />

```

## Creating a fetch Wrapper 
- We will use server actions using Next.js 
- Client will send request to Next.js server which will send request to the API resource server. 
- There are libraries like axios and use-swr to do this 
- However we want to make server side based fetch requests
- The code below is a custom fetch wrapper that we have developed for our GET/POST/PUT/DELETE requests.
```js 
 import {auth} from "@/auth";

const baseUrl = 'http://localhost:6001/';

async function handleResponse(response: Response) {
    const text = await response.text();
    const data = text && JSON.parse(text);
    if(response.ok) {
        return data || response.statusText;
    } else {
        const error = {
            status: response.status,
            message: response.statusText
        }
        return error;
    }
}

async function getHeaders() {
    const session = await auth();
    const headers = {
        'Content-Type': 'application/json'
    } as any;
    if(session?.accessToken) {
        headers.Authorization = `Bearer ${session?.accessToken}`;
    }
    return headers;
}

async function get(url: string) {
    const requestOptions = {
        method: 'GET',
        headers: await getHeaders()
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function post(url: string, body: {}) {
    const requestOptions = {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(body)
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function put(url: string, body: {}) {
    const requestOptions = {
        method: 'PUT',
        headers: await getHeaders(),
        body: JSON.stringify(body)
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

async function del(url: string) {
    const requestOptions = {
        method: 'DELETE',
        headers: await getHeaders()
    }
    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

export const fetchWrapper = {
    get,post,put,del
}


```
- We can use it like this: 
```js 
export async function getData(query:string): Promise<PagedResult<Auction>> {

    return await fetchWrapper.get(`search${query}`);
}

export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 10000) + 1,
    }

   return await fetchWrapper.put('auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c',data);
}

```
- Similar to above we will components for creating new auction, updating an auction and deleting an auction 

### Creating the Auction 
```js 
//Create Auction Page
import Heading from "@/app/components/Heading";
import AuctionForm from "@/app/auctions/AuctionForm";


export default function Create() {
    return (
        <div className="mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg">
            <Heading title = 'Sell your car!' subTitle='Please enter the details of your car' />
            <AuctionForm/>
        </div>
    )
}


//Auction Form 
'use client'
import {FieldValues, useForm} from "react-hook-form";
import {Button} from "flowbite-react";
import Input from "@/app/components/Input";
import {useEffect} from "react";
import DateInput from "@/app/components/DateInput";
import {createAuction, updateAuction} from "@/app/actions/auctionActions";
import {usePathname, useRouter} from "next/navigation";
import toast from "react-hot-toast";
import {Auction} from "@/types";



type Props = {
    auction?: Auction
}

export default function AuctionForm({auction}:Props) {
    // const {register, handleSubmit, setFocus, formState:{isSubmitting,isValid,isDirty,errors}} = useForm();
    const {control, handleSubmit, setFocus,reset,
        formState:{isSubmitting,isValid,isDirty,errors}} =
        useForm({
        mode:'onTouched'
    });
    const router = useRouter();
    const pathName = usePathname();
    useEffect(()=>{
        if(auction){
            const {make,model, year, mileage, color} = auction;
            reset({make,model,year,mileage,color})
        }
        setFocus('make')
    },[setFocus]);
    async function onSubmit(data: FieldValues) {
        try {
            console.log(data);
            let id = '';
            let res;
            if(pathName === '/auctions/create') {
                 res =  await createAuction(data);
                id = res.id;
            } else {
                if(auction){
                    res = await updateAuction(data, auction.id);
                    id = auction.id;
                }
            }

           if(res.error){
               throw res.error;
           }
           router.push(`/auctions/details/${id}`);
        }
        catch(error:any){
            toast.error(error.status + ' '+ error.message);
        }
    }
    return (
        <form className='flex flex-col mt-3' onSubmit={handleSubmit(onSubmit)}>
            <Input label='Make'
                   name='make'
                   control={control}
                   rules={{required: 'Make is required'}}/>
            <Input label='Model'
                   name='model'
                   control={control}
                   rules={{required: 'Model is required'}}/>
            <Input label='Color'
                   name='color'
                   control={control}
                   rules={{required: 'Color is required'}}/>
            <div className="grid grid-cols-2 gap-3">
                <Input label='Year'
                       name='year'
                       control={control}
                       type='number'
                       rules={{required: 'Year is required'}}/>
                <Input label='Mileage'
                       name='mileage'
                       type='number'
                       control={control}
                       rules={{required: 'Mileage is required'}}/>
            </div>

            {
                pathName === '/auctions/create' &&
            <>

            <Input label='Image Url'
                   name='imageUrl'
                   control={control}
                   rules={{required: 'Image Url is required'}}/>
            <div className="grid grid-cols-2 gap-3">
                <Input label='Reserve Price(enter 0 if no reserve)'
                       name='reservePrice'
                       control={control}
                       type='number'
                       rules={{required: 'Reserve Price is required'}}/>
                <DateInput
                    label='Auction End Date/Time'
                    name='auctionEnd'
                    dateFormat={'dd MMMM yyyy h:mm a'}
                    showTimeSelect
                    control={control}
                    rules={{required: 'Auction End Date is required'}}
                />
            </div>
            </>
            }
            <div className='flex justify-between'>
                <Button outline color='gray'>Cancel</Button>
                <Button
                    isProcessing={isSubmitting}
                    disabled={!isValid}
                    type='submit'
                    outline
                    color='success'>Submit</Button>
            </div>
        </form>
    )
}



```
### Updating an Auction 
```js 
//Updating Auction Page 
import Heading from "@/app/components/Heading";
import AuctionForm from "@/app/auctions/AuctionForm";
import {getDetailedViewData} from "@/app/actions/auctionActions";


export default async function Update({params}:{params:{id:string}}) {
    const data = await getDetailedViewData(params.id);

    return (
        <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
        <Heading title='Update your auction' subTitle='Please update details of your car'/>
            <AuctionForm auction = {data}/>
        </div>
    )
}

```

### Deleting an Auction 
```js 
//Deleting an Auction 
//This will just be a button to which we will pass our id of auction to delete 
import Heading from "@/app/components/Heading";
import AuctionForm from "@/app/auctions/AuctionForm";
import {getDetailedViewData} from "@/app/actions/auctionActions";


export default async function Update({params}:{params:{id:string}}) {
    const data = await getDetailedViewData(params.id);

    return (
        <div className='mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg'>
        <Heading title='Update your auction' subTitle='Please update details of your car'/>
            <AuctionForm auction = {data}/>
        </div>
    )
}



```

## Updating the Auction Actions 
- We will update our auction Actions server actions to support creating/updating/deleting an auction 
- Note that in case of updating an action we are revalidating the auction path for that specific id 
```js 
 'use server'
import {Auction, PagedResult} from "@/types";
import {auth} from "@/auth";
import {fetchWrapper} from "@/lib/fetchWrapper";
import {FieldValue, FieldValues} from "react-hook-form";
import {revalidatePath} from "next/cache";

export async function getData(query:string): Promise<PagedResult<Auction>> {
    //Caches the data coming from the API
    // const res = await fetch(`http://localhost:6001/search${query}`);
    // if (!res.ok) {
    //     throw new Error('Failed to fetch data');
    // }
    // return res.json();

    return await fetchWrapper.get(`search${query}`);
}

export async function getDetailedViewData(id:string): Promise<Auction> {
    return await fetchWrapper.get(`auctions/${id}`);
}

export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 10000) + 1,
    }

   return await fetchWrapper.put('auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c',data);
}

export async function createAuction(data:FieldValues) {
    return await fetchWrapper.post('auctions',data);
}

export async function updateAuction(data:FieldValues,id:string) {
    const res =  await fetchWrapper.put(`auctions/${id}`,data);
    revalidatePath(`/auctions/${id}`);
    return res;
}

export async function deleteAuction(id:string) {
    return await fetchWrapper.del(`auctions/${id}`);
}

```

## Creating the Bidding Service 
- We will take a look at the following :
- Background Services 
- gRPC 
- We will create a Bidding Service Project and will configure JwtBearer Authentication and RabbitMq inside of it. 
-  We will also create a BidsController that has 2 methods inside of it: PlaceBid() and GetBidsForAuction()
-  We will also create a consumer called AuctionCreatedConsumer inside the BiddingService to consume the AuctionCreated event 
- Inside the PlaceBid() method we will also publish a BidPlaced event which will be consumed by BidConsumer inside the AuctionService and SearchService 
```c#
//Auction Created Consumer 
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var auction = new Auction
        {
            ID = context.Message.Id.ToString(),
            Seller = context.Message.Seller,
            AuctionEnd = context.Message.AuctionEnd,
            ReservePrice = context.Message.ReservePrice,
        };

        await auction.SaveAsync();
    }
}


//BidsController.cs 
using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController(IMapper mapper, IPublishEndpoint publishEndpoint):ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            //TODO: check with auction service if that has auction
            return NotFound();
        }

        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You cannot place bids on your own auction");
        }

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User.Identity.Name,
        };
        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highBid = await DB.Find<Bid>()
                .Match(x => x.AuctionId == auctionId)
                .Sort(y=>y.Descending(z=>z.Amount))
                .ExecuteFirstAsync();

            if (highBid != null && amount > highBid.Amount || highBid == null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted
                    : BidStatus.AcceptedBelowReserve;
            }

            if (highBid != null && bid.Amount <= highBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        
        await DB.SaveAsync(bid);
        
        //Publish Bid Placed Event
        await publishEndpoint.Publish(mapper.Map<BidPlaced>(bid));
        
        return Ok(mapper.Map<BidDto>(bid));
    }

    [HttpGet]
    [Route("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(a => a.AuctionId == auctionId)
            .Sort(y => y.Descending(a => a.BidTime))
            .ExecuteAsync();
        
        return bids.Select(mapper.Map<BidDto>).ToList();
    }
}


```
- We need to create a background service that will check the auctions that have been finished  and mark them as finished. 
- It will also then publish an Auction Finished Event. 
- However please note this background service will run in singleton mode, but the MassTransit Publish Endpoint is a scoped service. 
- To use a scoped service inside a singleton service, we need to use IServiceProvider interface. 
- We can do it like this 
```c# 
 public interface IScopedService
{
    string GetScopedServiceID();
}

public class ScopedService : IScopedService
{
    private readonly string _scopedServiceID;

    public ScopedService()
    {
        _scopedServiceID = Guid.NewGuid().ToString();
    }

    public string GetScopedServiceID()
    {
        return _scopedServiceID;
    }
}

public interface ISingletonService
{
    string GetScopedServiceID();
}

public class SingletonService : ISingletonService
{
    private readonly IServiceProvider _serviceProvider;

    public SingletonService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string GetScopedServiceID()
    {
        var scopedService = _serviceProvider.GetRequiredService<IScopedService>();
        return scopedService.GetScopedServiceID();
    }
}


//Register these services in Program.cs file like this 
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IScopedService, ScopedService>();
        services.AddSingleton<ISingletonService, SingletonService>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure your application pipeline
    }
}



```
- IScopedService and ScopedService: This is your scoped service. Each HTTP request will get a new instance of this service.
- ISingletonService and SingletonService: This is your singleton service. It will be instantiated once and shared across all HTTP requests.
- IServiceProvider: The IServiceProvider instance is injected into the singleton service to resolve the scoped service whenever it's needed. 
- This approach ensures that the scoped service's lifetime is respected while being used in a singleton service.

- We will create a Background service called CheckAuctionFinished as follows: 
  
  ```c# 
  using BiddingService.Models;
    using Contracts;
    using MassTransit;
    using MongoDB.Entities;

    namespace BiddingService.Services;

    public class CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services) : BackgroundService
    {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting check for finished auctions");
        stoppingToken.Register(() => logger.LogInformation(" ==> Stopping check for finished auctions"));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);
            await Task.Delay(5000,stoppingToken);
        }
     }

         private async Task CheckAuctions(CancellationToken stoppingToken)
        {
         var finishedAuctions = await DB.Find<Auction>()
            .Match(x=>x.AuctionEnd <= DateTime.UtcNow)
            .Match(x=>!x.Finished)
            .ExecuteAsync(stoppingToken);
        
        if(finishedAuctions.Count() == 0) return;
        logger.LogInformation($"==>Found Finished auctions: {finishedAuctions.Count()}");
        using var scope = services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null,stoppingToken);
            
            var winningBid = await DB.Find<Bid>()
                .Match(x=>x.AuctionId == auction.ID)
                .Match(b=>b.BidStatus == BidStatus.Accepted)
                .Sort(x=>x.Descending(s=>s.Amount))
                .ExecuteFirstAsync(stoppingToken);
            
            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount ?? 0,
                Seller = auction.Seller
            },stoppingToken);
        }
        
        }
     }
  ```  



## Using GRPC to communicate with Auction Service from Bidding Service. 
- Another way our services can communicate with each other 
- Google Remote Procedure Call 
- HTTP/2 protocol to transport binary messages(over TLS)
- HTTP/2 uses TLS by default. 
- If we want to do GRPC over HTTP, we will have to tweak it. 
- Focussed on High Performance 
- 7 times faster than REST API communication. 
- Messages are binary in nature. No need for HTTP Request/Response messages. 
- Used for synchronous communication between microservices. 
- Relies on Protocol buffers(contracts between services)
- Multi-language support. 
- Used for service to service synchronous communication 
- Not used for communication between browser and server(have grpc web for that)
- ![alt text](image-63.png)

## Implementing GRPC Services with ASP.NET Core 
- Kestrel doesnot support HTTP/2 with TLS on MACOS before .NET 8. 
- The ASP.NET Core grpc template and samples use TLS by default. 
- Kestrel GRPC Endpoints require HTTP/2 and it should be secured with Transport Layer Security (TLS)

### Configuring the GRPC Server in the Auction Service 
- First step is to install Grpc.AspnetCore package in Auction Service 
- Next step is to create an auctions.proto file like this 
```js
 syntax = "proto3";
option csharp_namespace = "AuctionService";
        
service GrpcAuction {
  rpc GetAuction(GetAuctionRequest) returns (GrpcAuctionResponse);
}

message GetAuctionRequest {
  string id = 1;
}

message GrpcAuctionModel {
  string id = 1;
  string seller = 2;
  string auctionEnd = 3;
  int32 reservePrice = 4;
}

message GrpcAuctionResponse {
  GrpcAuctionModel auction = 1;
}

```
- Next step is to update AuctionService.csproj to tell it about the proto file
- Add the below mentioned code. 
```c# 
<ItemGroup>
  <Protobuf Include = "protos/auctions.proto" GrpcServices = "Server" />
  </ItemGroup>

```
- Next Step is to create an GrpcAuctionService that will implement the contracts defined in the auctions.proto file 
```c# 
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

```
- Finally we need to configure Kestrel to start a Grpc connection over HTTP/2 at a different port and run the AuctionService WebApi Application in a different port over HTTP 
- We will specify the Kestrel configuration in appsettings.Development.json file 
```js 
  "Kestrel": {
    "Endpoints": {
      "Grpc": {
        "Protocols": "Http2",
        "Url": "http://localhost:7777"
      },
      "WebApi": {
        "Protocols": "Http1",
        "Url": "http://localhost:7001"
      }
    }
  }


```
- Finally we need to let the application know about Grpc Routes in Program.cs file of AuctionService like this 
```c#
 builder.Services.AddGrpc();
 app.MapGrpcService<GrpcAuctionService>();

```

## Configuration of the Grpc Client 
- Need to install different packages for Grpc.Client 
- google.protobuf 
- Grpc.Net.Client
- Grpc.Tools 

- We will create a proto/auctions.proto file same as the auction service above and copy over the contents of that file into this file. 
- We will configure the BiddingService.csproj file as follows: 
```c# 
 <ItemGroup>
    <Protobuf Include = "protos/auctions.proto" GrpcServices = "Client" />
  </ItemGroup>

```
- Next we will create a GrpcAuctionClient.cs file as follows: 
```c# 
 using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
{
    public Auction GetAuction(string id)
    {
        logger.LogInformation($"Calling Grpc Service");
        var channel = GrpcChannel.ForAddress(config["GrpcAuction"]);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest
        {
            Id = id
        };

        try
        {
            var reply = client.GetAuction(request);
            var auction = new Auction
            {
                ID = reply.Auction.Id,
                AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                Seller = reply.Auction.Seller,
                ReservePrice = reply.Auction.ReservePrice
            };
            return auction;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not call Grpc Server");
            return null;
        }
    }
    
}

```
- Next step is to setup the GrpcClient in Program.cs file as follows :
```c#
 builder.Services.AddScoped<GrpcAuctionClient>();

```
- Now we will utilize this GrpcAuctionClient to call the GrpcAuctionServer Service and get the Auction as follows: 
```c#
 using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcClient):ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            //TODO: check with auction service if that has auction
            //return NotFound();
            auction = grpcClient.GetAuction(auctionId);
            if (auction == null)
            {
                return BadRequest("Cannot accept bids on this auction at this time");
            }
        }

        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You cannot place bids on your own auction");
        }

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User.Identity.Name,
        };
        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highBid = await DB.Find<Bid>()
                .Match(x => x.AuctionId == auctionId)
                .Sort(y=>y.Descending(z=>z.Amount))
                .ExecuteFirstAsync();

            if (highBid != null && amount > highBid.Amount || highBid == null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted
                    : BidStatus.AcceptedBelowReserve;
            }

            if (highBid != null && bid.Amount <= highBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        
        await DB.SaveAsync(bid);
        
        //Publish Bid Placed Event
        await publishEndpoint.Publish(mapper.Map<BidPlaced>(bid));
        
        return Ok(mapper.Map<BidDto>(bid));
    }

    [HttpGet]
    [Route("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(a => a.AuctionId == auctionId)
            .Sort(y => y.Descending(a => a.BidTime))
            .ExecuteAsync();
        
        return bids.Select(mapper.Map<BidDto>).ToList();
    }
}

```

## Updating the Gateway Service
- Update appsettings.json file as follows 
```json 
   "bidsWrite": {
        "ClusterId": "bids",
        "AuthorizationPolicy": "default",
        "Match": {
          "Path": "/bids",
          "Methods": ["POST"]
        },
        "Transforms": [
          {
            "PathPattern": "api/bids"
          }
        ]
      },
      "bidsRead": {
        "ClusterId": "bids",
        "Match": {
          "Path": "/bids/{**catch-all}",
          "Methods": ["GET"]
        },
        "Transforms": [
          {
            "PathPattern": "api/bids/{**catch-all}"
          }
        ]
      }

```
- Update appSettings.Development.json and appSettings.Docker.json as follows: 
```json 
//appsettings.Docker.json 
 "bids": {
        "Destinations": {
          "bidApi": {
            "Address": "http://bid-svc"
          }
        }
      }

//appsettings.Development.json 
 "bids": {
        "Destinations": {
          "bidApi": {
            "Address": "http://localhost:7003"
          }
        }
      }

```

## Dockerizing the Bidding Service 
- We will create a docker file for Bidding Service as follows: 
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
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore package dependencies
RUN dotnet restore Carsties.sln

# Copy the app folders over 
COPY src/BiddingService src/BiddingService
COPY src/Contracts src/Contracts

WORKDIR /app/src/BiddingService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet","BiddingService.dll"]

```
- Next we will update docker compose file as follows :
```shell 
//Add the bid-svc 
  bid-svc:
    image: nishant198509/bid-svc:latest
    build:
      context: .
      dockerfile: src/BiddingService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - RabbitMq__Host=rabbitmq
      - ConnectionStrings__BidDbConnection=mongodb://root:mongopw@mongodb
      - IdentityServiceUrl=http://identity-svc
      - GrpcAuction=http://auction-svc:7777
    ports:
      - 7003:80
    depends_on:
      - mongodb
      - rabbitmq


//Update the auction-svc to support Grpc Server configuration 

  auction-svc:
    image: nishant198509/auction-svc:latest
    build:
      context: .
      dockerfile: src/AuctionService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ASPNETCORE_URLS=http://+:7777
      - RabbitMq__Host=rabbitmq
      - ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=auctions
      - IdentityServiceUrl=http://identity-svc
      - Kestrel__Endpoints__Grpc__Protocols=Http2
      - Kestrel__Endpoints__Grpc__Url=http://+:7777
      - Kestrel__Endpoints__WebApi__Protocols=Http1
      - Kestrel__Endpoints__WebApi__Url=http://+:80
    ports:
      - 7001:80
      - 7777:7777
    depends_on:
      - postgres
      - rabbitmq


```

## Adding a SignalR Service 
- ![alt text](image-64.png)
- Provides real time communication between client app and Backend services 
- ![alt text](image-65.png)
- Uses WebSockets, SSR or Long Polling 
- We will create a NotificationService Project and install MassTransit.RabbitMq package inside it. 
- Then we will create a NotificationHub inside it 
```c# 
 using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class NotificationHub : Hub
{
    
}

```
- Then we will update Program.cs file to know where the Hub is and map it 
```c#
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");
app.Run();
```
- Then in this project we will create our consumers to consume the AuctionCreated, AuctionFinished and BidPlaced events. 
- Our notification service will consume these events, then using SignalR send out these events to all the clients connected to the hub. 

```c#
//Auction Created Consumer
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext):IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"==> auction created message received: {context.Message.Id}");
        await hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}

//Auction Finished Consumer 
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext):IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine($"==> auction finished message received: {context.Message.AuctionId}");
        await hubContext.Clients.All.SendAsync("AuctionFinished", context.Message);
    }
}

//Bid Placed Consumer 
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidPlacedConsumer(IHubContext<NotificationHub> hubContext):IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"==> Bid placed message received: {context.Message.AuctionId}");
        await hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
    }
}


```
- We will also configure RabbitMq inside Program.cs file of NotificationService. 

### Configuring CORS for NotificationService in Gateway Service 
- So far we havent needed to configure CORS for any of our services. 
- This is because our client browser connects to the Next.js server to get the data which is of the same origin. 
- However, in case of notification hub, our client browser will make a direct TCP/IP connection to our NotificationHub using websockets. 
- So our browser will connect to a cross origin and we need to configure it to allow cross origin requests. 
- Since our notification service will be accessed through gateway service, we will have to configure it inside our gateway service. 
- We will add the following code to Program.cs file of GatewayService: 
```c#
 builder.Services.AddCors(opt =>
{
    opt.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(builder.Configuration["ClientApp"]);
    });
});
var app = builder.Build();

app.UseCors();
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

```
- We will also add the Cors Policy inside our Yarp Reverse Proxy configuration in appSettings.json like this 
```json 
  "notifications": {
        "ClusterId": "notifications",
        "CorsPolicy": "customPolicy",
        "Match": {
          "Path": "/notifications/{**catch-all}"
        }
      }

```
- Then we will create a Dockerfile for NotificationService similar to all the other services and add it to docker compose file as well. 

## Adding Bids/Notifications to the Client App 
- We will create an AuctionStore first to get the list of auctions and set the current price to the current high bid 
```js 
 import {Auction, PagedResult} from "@/types";
import {create} from "zustand/react";

type State = {
    auctions : Auction[]
    totalCount: number
    pageCount: number
}

type Actions = {
    setData: (data: PagedResult<Auction>) => void
    setCurrentPrice : (auctionId: string, amount: number) => void
}

const initialState: State = {
    auctions: [],
    pageCount: 0,
    totalCount: 0
}

export const useAuctionStore = create<State & Actions>((set) => ({
    ...initialState,
    setData: (data: PagedResult<Auction>) => {
        set(() => ({
            auctions: data.results,
            totalCount: data.totalCount,
            pageCount: data.pageCount
        }))
    },
    setCurrentPrice: (auctionId, amount) => {
        set((state) => ({
            auctions: state.auctions.map((auction) => auction.id === auctionId
            ? {...auction,currentHighBid: amount} : auction)
        }))
    }
}))

```
- We will also update the auctionActions server actions to get the List of Bids for the Current Auction 
```js 
 export async function getBidsForAuction(id:string): Promise<Bid[]>{
    return await fetchWrapper.get(`bids/${id}`);
}

```
- We will create a type for Bid as follows:
```js 
  export type Bid = {
    id:string
    auctionId:string
    bidder:string
    bidType: string
    amount: number
    bidStatus: string
    bidTime: string
}

```
- In the Bid Details Page, we will get the list of bids for the Auction like this :
```js 
 export default async function Details({params}:{params:{id:string}}) {
    const data = await getDetailedViewData(params.id);
    const user = await getCurrentUser();
    const bids = await getBidsForAuction(params.id);

    return (
        <div>
            <div className="flex justify-between">
                <div className="flex items-center gap-3">
                    <Heading title={`${data.make} ${data.model}`}/>
                    {user?.username === data.seller && (
                        <>
                            <EditButton id ={data.id}/>
                            <DeleteButton id ={data.id}/>
                        </>

                    )}
                </div>

            <div className='flex gap-3'>
                <h3 className='text-2xl font-semibold'>Time remaining:</h3>
                <CountdownTimer auctionEnd={data.auctionEnd}/>
            </div>
            </div>
            <div className='grid grid-cols-2 gap-6 mt-3'>
                <div className='w-full bg-gray-200 relative aspect-[4/3] rounded-lg overflow-hidden'>
                    <CarImage imageUrl={data.imageUrl} />
                </div>
                <div className='border-2 rounded-lg p-2 bg-gray-100'>
                    <Heading title="Bids"/>
                    {bids.map(bid => (
                        <BidItem bid={bid} key={bid.id}/>
                    ))}
                </div>
            </div>
            <div className='mt-3 grid grid-cols-1 rounded-lg '>
            <DetailedSpecs auction={data}/>
            </div>

        </div>
    )
}



//BidItem component 
 import React from 'react'
import {Bid} from "@/types";
import {format} from "date-fns/format";
import {numberWithCommas} from "@/app/lib/numberWithComma";
type Props = {
    bid: Bid
}
export default function BidItem({bid}: Props) {

    function getBidInfo(){
        let bgColor = '';
        let text = '';
        switch (bid.bidStatus) {
            case 'Accepted':
                bgColor = 'bg-green-200';
                text = 'Bid accepted';
                break;
            case 'AcceptedBelowReserve':
                bgColor = 'bg-amber-200';
                text = 'Reserve not met';
                break;
            case 'TooLow':
                bgColor = 'bg-red-200';
                text = 'Bid was too low';
                break;
            default:
                bgColor = 'bg-red-200';
                text = 'Bid placed after auction finished';
                break;
        }
        return {bgColor,text};
    }

    return (
        <div className={`
        border-gray-300 border-2 px-3 py-2 rounded-lg
        flex justify-between items-center mb-2
        ${getBidInfo().bgColor}
        `}>
            <div className='flex flex-col'>
                <span>Bidder: {bid.bidder}</span>
                <span className='text-gray-700 text-sm'>Time: {format(new Date(bid.bidTime), 'dd MMM yyyy h:mm a') }</span>
            </div>
            <div className='flex flex-col text-right'>
                <div className='text-xl font-semibold'>
                    ${numberWithCommas(bid.amount)}
                </div>
                <div className='flex flex-row items-center'>
                    <span>{getBidInfo().text}</span>
                </div>
            </div>
        </div>
    )
}


```

## Creating a Bid Store
- Next step is to put our bids somewhere so we will create our store for the bids 
- This store will have the list of bids as well as the methods to set the List of Bids and add a bid to the List of bids 
```js 
 import {Bid} from "@/types";
import {create} from "zustand/react";

type State = {
    bids : Bid[]
}

type Actions = {
    setBids: (bids : Bid[]) => void
    addBid: (bid: Bid) => void
}

export const useBidStore = create<State & Actions>((set)=>({
    bids:[],
    setBids:(bids:Bid[]) =>{
        set(()=>({
            bids:bids
        }))
    },
    addBid:(bid: Bid) =>{
        set((state)=>({
            //Check if the bid already exists in the list of bids, if not add it to the top of the bids[] else just return existing list of bids
            bids: !state.bids.find(x=>x.id==bid.id) ? [bid,...state.bids]: [...state.bids]
    }))
    },
}))

```
- Now we will display the list of bids on the Auction Details page as its own client component. 
- This component will accept the props of user and the auction and get the list of bids from the server auction,set them inside the bids store and display them accordingly. 
```js 
 'use client'

import {User} from "next-auth";
import {Auction, Bid} from "@/types";
import {useBidStore} from "@/hooks/useBidStore";
import {useEffect, useState} from "react";
import {getBidsForAuction} from "@/app/actions/auctionActions";
import toast from "react-hot-toast";
import Heading from "@/app/components/Heading";
import BidItem from "@/app/auctions/details/[id]/BidItem";

type Props = {
    user: User | null
    auction: Auction
}
export default function BidList({user, auction}: Props) {
    const [loading,setLoading] = useState<boolean>(true);
    const bids = useBidStore(state => state.bids);
    const setBids = useBidStore(state => state.setBids);

    useEffect(() => {
        getBidsForAuction(auction.id)
            .then((res:any) => {
                if(res.error) {
                    throw res.error;
                }
                setBids(res as Bid[]);
            }).catch(err => {
                toast.error(err.message);
        }).finally(() => setLoading(false));
    }, [auction.id, setLoading,setBids]);

    if(loading){
        return <span>Loading Bids...</span>;
    }
    return (
            <div className='border-2 rounded-lg p-2 bg-gray-100'>
                <Heading title="Bids"/>
                {bids.map(bid => (
                    <BidItem bid={bid} key={bid.id}/>
                ))}
            </div>
    )
}



```

## Creating a bid form to place bids 
- First we will create the server action to place a bid 
```js 
 export async function placeBidForAuction(auctionId:string, amount:number) {
    return await fetchWrapper.post(`bids?auctionId=${auctionId}&amount=${amount}`,{});
}

```
- Then we will create a bid form like this 
```js 
 'use client'
import {FieldValues, useForm} from "react-hook-form";
import {useBidStore} from "@/hooks/useBidStore";
import {placeBidForAuction} from "@/app/actions/auctionActions";
import {numberWithCommas} from "@/app/lib/numberWithComma";

type Props = {
    auctionId: string
    highBid: number
}
export default function BidForm({ auctionId,highBid }: Props) {
    const {register,handleSubmit,reset,formState:{errors}} = useForm();
    const addBid = useBidStore(state =>state.addBid);

    function onSubmit(data:FieldValues)
    {
        placeBidForAuction(auctionId,+data.amount).then(bid => {
            addBid(bid);
            reset();
        })
    }
    return (
        <form onSubmit={handleSubmit(onSubmit)} className='flex items-center border-2 rounded-lg py-2'>
            <input type='number' {...register('amount')}
                   className='input-custom text-sm text-gray-600'
                   placeholder={`Enter your bid (minimum bid is $ ${numberWithCommas(highBid + 1)})`}
                   name='amount' />
        </form>
    )
}


```
- Then we will display the Add Bid Form inside the Bid List component 
```js 
 'use client'

import {User} from "next-auth";
import {Auction, Bid} from "@/types";
import {useBidStore} from "@/hooks/useBidStore";
import {useEffect, useState} from "react";
import {getBidsForAuction} from "@/app/actions/auctionActions";
import toast from "react-hot-toast";
import Heading from "@/app/components/Heading";
import BidItem from "@/app/auctions/details/[id]/BidItem";
import {numberWithCommas} from "@/app/lib/numberWithComma";
import EmptyFilter from "@/app/components/EmptyFilter";
import BidForm from "@/app/auctions/details/[id]/BidForm";

type Props = {
    user: User | null
    auction: Auction
}
export default function BidList({user, auction}: Props) {
    const [loading,setLoading] = useState<boolean>(true);
    const bids = useBidStore(state => state.bids);
    const setBids = useBidStore(state => state.setBids);

//function to get the highest bid from the list of bids,here we reduce the list of bids to a single bid.
    const highBid = bids.reduce(
        (prev,current) =>
        prev > current.amount
            ? prev
            : current.amount, 0);

    useEffect(() => {
        getBidsForAuction(auction.id)
            .then((res:any) => {
                if(res.error) {
                    throw res.error;
                }
                setBids(res as Bid[]);
            }).catch(err => {
                toast.error(err.message);
        }).finally(() => setLoading(false));
    }, [auction.id, setLoading,setBids]);

    if(loading){
        return <span>Loading Bids...</span>;
    }
    return (
            <div className='rounded-lg shadow-md'>
                <div className='py-2 px-4 bg-white'>
                    <div className='sticky top-0 bg-white p-2'>
                        <Heading title={`Current high bid is $ ${numberWithCommas(highBid)}`}/>
                    </div>
                </div>

                <div className='overflow-auto h-[400px] flex flex-col-reverse px-2'>
                    {bids.length  == 0 ?
                        (<EmptyFilter title='No bids for this item' subtitle='Please feel free to make a bid'/>)
                        : ( <>
                            {bids.map(bid => (
                                <BidItem bid={bid} key={bid.id}/>
                            ))}
                            </>
                            )
                    }
                </div>

                <div className='px-2 pb-2 text-gray-500'>
                    <BidForm auctionId={auction.id} highBid={highBid}/>
                </div>

            </div>
    )
}



```

## Aside How to create custom Tailwind CSS classes to use inside our code 
- We can go to globals.css file and add our custom tailwind css classes like this 
- Below we are creating a custom input class 
```css 
 @tailwind base;
@tailwind components;
@tailwind utilities;

.react-datepicker-wrapper {
    width: 100%;
}

@layer components {
    .input-custom {
        @apply  flex-grow pl-5 bg-transparent focus:outline-none border-transparent focus:border-transparent focus:ring-0
    }
}

```
- We can then use it like any other regular tailwind css class 
```js 
  <input type='number' {...register('amount')}
                   className='input-custom text-sm text-gray-600'
                   placeholder={`Enter your bid (minimum bid is $ ${numberWithCommas(highBid + 1)})`}
                   name='amount' />

```

## Handling Errors while adding Bids 
  - We will make changes to the handleError method inside fetchWrapper like this: 
```js 
 async function handleResponse(response: Response) {
    const text = await response.text();
    //const data = text && JSON.parse(text);
    let data;
    try {
        data = text && JSON.parse(text);
    }
    catch (error)
    {
        data = text;
    }
    if(response.ok) {
        return data || response.statusText;
    } else {
        const error = {
            status: response.status,
            message: typeof(data) === 'string' ? data : response.statusText
        }
        return {error};
    }
}


```
- We will also make changes to the onSubmit method inside the BidForm as follows: 
```js 
  function onSubmit(data:FieldValues)
    {
        placeBidForAuction(auctionId,+data.amount).then(bid => {
            if(bid.error) throw bid.error;
            addBid(bid);
            reset();
        }).catch(err=> toast.error(err.message));
    }

```

## Adding SignalR to the client application 
- We should all live updates to our application to get the live list of bids being placed on an auction. 
- We should add client side signalR to our application. 
- We need to install the following package to our application: 
```shell 
npm install @microsoft/signalr
```
- We need to add a SignalR provider which will make a connection to the signalR Hub and provide that connection to all the child components in our application.
- What we can do is add the children prop to this provider(of type React.ReactNode) effectively wrapping our {children} inside this provider. 
- We can then use SignalR provider inside the layout.tsx file. 
- As you can see that we are using the AuctionStore and BidStore inside it 
- This provider opens a connection to the notification hub of notification service. 
- It is then consuming the 'BidPlaced' message from the hub coming from the BidPlacedConsumer.cs file. 
- It is then using that bid data to update the current price for an auction using setCurrentPrice method of the useAuctionStore. 

```js 
//SignalRProvider.tsx 

'use client'
import React, {useEffect, useRef} from 'react'
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {useAuctionStore} from "@/hooks/useAuctionStore";
import {useBidStore} from "@/hooks/useBidStore";
import {useParams} from "next/navigation";
import {Bid} from "@/types";
type Props = {
    children: React.ReactNode
}
export default function SignalRProvider({children}: Props) {
    const connection = useRef<HubConnection | null>(null);
    const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
    const addBid = useBidStore(state => state.addBid);
    const params = useParams<{id:string}>();

    useEffect(() => {
        if(!connection.current)
        {
            connection.current = new HubConnectionBuilder()
                .withUrl('http://localhost:6001/notifications')
                .withAutomaticReconnect()
                .build();
            connection.current.start()
                .then(()=> 'Connected to notification hub')
                .catch(err => console.error(err));

            connection.current.on('BidPlaced',(bid:Bid) => {
                setCurrentPrice(bid.auctionId, bid.amount);
            })
        }
    },[setCurrentPrice])

    return (
        children
    )
}



```
- We can then use it in layout.tsx file like this 
```js 
export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
      >
      <ToasterProvider/>
      <Navbar />
      <main className="container mx-auto px-5 pt-10">
          <SignalRProvider>
              {children}
          </SignalRProvider>
      </main>
      </body>
    </html>
  );
}

```

## Adding a new bid to SignalR
- Here on the Auction Details page in the BidList component, we want to display the bids 'live' as they are coming in from various users. 
- In the SignalR provider component we are updating the bids being displayed for an auction as it comes in.
- For that purpose we have a handleBidsPlaced method. 
- When a component is re-rendered, all its functions get recreated as well. 
- We only want handleBidPlaced method to be recreated when the current price is updated, or a new bid is added or if the route params change i.e the user navigates to a different auction. 
- So we will placed this method inside a useCallback hook. 
- Picture this: you're juggling tasks, and you want to make sure you're not repeating yourself unnecessarily. 
- In the React world, that's where the useCallback hook swoops in. It's like giving your functions a sticky note that says, "Only change if these things change." 
- Essentially, useCallback memoizes your callback functions, preventing them from being recreated on every render unless their dependencies change.
- ![alt text](image-66.png)
- By doing this, you help React optimize performance, especially when passing callbacks to child components that rely on reference equality to prevent unnecessary re-renders.
- Overusing useCallback can add complexity without significant benefits. Think of it like saving every single receipt—you can, but do you really need to?
- Also when we switch auctions we want to switch off the SignalR hub connection and recreate it again for the new auction. 
- So we will have to use a cleanup function inside useEffect hook 
- Now if we go and check, as the bids come in for an auction, if that particular auction page is open for any user, the user can see 'live' list of bids. 
- If the user is on some other auction, he wont see anything come in inside the BidList component.  
```js 

'use client'
import React, {useCallback, useEffect, useRef} from 'react'
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {useAuctionStore} from "@/hooks/useAuctionStore";
import {useBidStore} from "@/hooks/useBidStore";
import {useParams} from "next/navigation";
import {Bid} from "@/types";
type Props = {
    children: React.ReactNode
}
export default function SignalRProvider({children}: Props) {
    const connection = useRef<HubConnection | null>(null);
    const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
    const addBid = useBidStore(state => state.addBid);
    const params = useParams<{id:string}>();

    const handleBidPlaced = useCallback((bid:Bid) =>{
        if(bid.bidStatus.includes('Accepted')){
            setCurrentPrice(bid.auctionId, bid.amount);
        }

        //check what is there in the router parameters using the useParams hook
        if(params.id === bid.auctionId)
        {
            addBid(bid);
        }
    },[setCurrentPrice,addBid,params.id]);


    useEffect(() => {
        if(!connection.current)
        {
            connection.current = new HubConnectionBuilder()
                .withUrl('http://localhost:6001/notifications')
                .withAutomaticReconnect()
                .build();
            connection.current.start()
                .then(()=> 'Connected to notification hub')
                .catch(err => console.error(err));
        }
        connection.current.on('BidPlaced',handleBidPlaced);

        //Cleanup the connection
        return () => {
            connection.current?.off('BidPlaced',handleBidPlaced);
        }
    },[handleBidPlaced])

    return (
        children
    )
}



```
- Similar to the BidPlaced event, we will consume 'AuctionCreated' and 'AuctionFinished' events as well 
- We will create their own separate toast components and then subscribe to these events using client side signalR and show these toasts accordingly 

```js 
//AuctionCreatedToast
import {Auction} from "@/types";
import Link from "next/link";
import Image from "next/image";

type Props = {
   auction: Auction
}

export default function AuctionCreatedToast({auction}: Props) {
    return (
        <Link href={`/auctions/details/${auction.id}`} className='flex flex-col items-center'>
           <div className='flex flex-row items-center gap-2'>
               <Image
               src={auction.imageUrl}
               alt='Image of car'
               height={80}
               width={80}
               className='rounded-lg w-auto h-auto'
               />
               <span>New Auction! {auction.make} {auction.model} has been added</span>
           </div>
        </Link>
    )
}


//AuctionFinishedToast
import {Auction, AuctionFinished} from "@/types";
import Link from "next/link";
import Image from "next/image";
import {numberWithCommas} from "@/app/lib/numberWithComma";

type Props = {
    finishedAuction: AuctionFinished,
    auction: Auction,
}

export default function AuctionFinishedToast({auction,finishedAuction}: Props) {
    return (
        <Link href={`/auctions/details/${auction.id}`} className='flex flex-col items-center'>
            <div className='flex flex-row items-center gap-2'>
                <Image
                    src={auction.imageUrl}
                    alt='Image of car'
                    height={80}
                    width={80}
                    className='rounded-lg w-auto h-auto'
                />
                <div className='flex flex-col'>
                    <span>Auction for {auction.make} {auction.model} has finished</span>
                    {finishedAuction.itemSold && finishedAuction.amount  ? (
                        <p>Congrats to {finishedAuction.winner} who has won this auction for $${numberWithCommas(finishedAuction.amount)}</p>
                    ):(
                        <p>This item did not sell</p>
                    )}
                </div>

            </div>
        </Link>
    )
}



//SignalRProvider code to handle these toasts
const handleAuctionCreated = useCallback((auction:Auction) => {
        if(user?.username !== auction.seller)
        {
            return toast(<AuctionCreatedToast auction={auction} />,{
                duration: 10000,
            })
        }
    },[user?.username])

    const handleAuctionFinished = useCallback((finishedAuction:AuctionFinished) => {
        const auction = getDetailedViewData(finishedAuction.auctionId);
        return toast.promise(auction, {
            loading:'loading',
            success: (auction) =>
                <AuctionFinishedToast
                    finishedAuction={finishedAuction} auction={auction} />
            , error: (err) => 'Auction Finished'
        },{success: {duration:10000,icon:null}})
    },[])

 useEffect(() => {
        if(!connection.current)
        {
            connection.current = new HubConnectionBuilder()
                .withUrl('http://localhost:6001/notifications')
                .withAutomaticReconnect()
                .build();
            connection.current.start()
                .then(()=> 'Connected to notification hub')
                .catch(err => console.error(err));
        }
        connection.current.on('BidPlaced',handleBidPlaced);
        connection.current.on('AuctionCreated',handleAuctionCreated);
        connection.current.on('AuctionFinished',handleAuctionFinished);

        //Cleanup the connection
        return () => {
            connection.current?.off('BidPlaced',handleBidPlaced);
            connection.current?.off('AuctionCreated',handleAuctionCreated);
            connection.current?.off('AuctionFinished',handleAuctionFinished);
        }
    },[handleBidPlaced,handleAuctionCreated,handleAuctionFinished])


```

## Disabling the auction finished form when auction finishes 
- We will use the useBidStore to store the state of whether the auction is open or not 
```js 
 type State = {
    bids : Bid[]
    open: boolean
}

type Actions = {
    setBids: (bids : Bid[]) => void
    addBid: (bid: Bid) => void
    setOpen: (value:boolean) => void
}

export const useBidStore = create<State & Actions>((set)=>({
    bids:[],
    open: true,
    setOpen:(value:boolean) =>{
        set(()=>({
            open: value
        }))
    }
}))


```
- Next step is to update this state inside the Countdown timer component 
```js 

 export default function CountdownTimer({auctionEnd}:Props) {
    const setOpen = useBidStore(state => state.setOpen);
    const pathName = usePathname();

    function AuctionFinished(){
        console.log("Auction Finished triggered")

        if(pathName.startsWith('/auction/details')){
            setOpen(false);
        }
    }
    return (
        <div>
            <Countdown date={auctionEnd} renderer={renderer} onComplete={AuctionFinished} />
        </div>
    )
}

```
- Finally we will read the 'open' state property inside the BidList component like this 
- So when the state updates, this component also updates. 
```js 
const open = useBidStore(state => state.open);
const setOpen = useBidStore(state => state.setOpen);
const openForBids = new Date(auction.auctionEnd) > new Date();

useEffect(() => {
        setOpen(openForBids);
    }, [openForBids,setOpen]);

<div className='px-2 pb-2 text-gray-500'>
                    {!open ? (
                        <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                            This auction has finished
                        </div>
                    ):(
                        !user ? (
                        <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                        Please login to make a bid
                        </div>
                        ): user && user.username === auction.seller ? (
                        <div className='flex items-center justify-center p-2 text-lg font-semibold'>
                        You cannot bid on your own auction
                        </div>
                        ) : (
                        <BidForm auctionId={auction.id} highBid={highBid}/>
                            )
                    )}


                </div>


```

## Publishing App to Production Locally 
- ![alt text](image-67.png)
- We need an Ingress into our application. 
- This can be 'nginx' that can receive external traffic and forward it based on rules to the correct location. 
- We will have a rule that will allow our client browsers to maintain connection to NotificationService. 
- We will run the following command: 
```shell 
npm run build 
```
- We will also create a file .env.local to store our environment variables for any URLs we use 
```shell 
AUTH_SECRET="as19ZchDsmc3ZqU5oHq5or4uhtPZtE6sZyIEUYh+s3M="
API_URL=http://localhost:6001/
ID_URL=http://localhost:5000
AUTH_URL=http://localhost:3000
NOTIFY_URL=http://localhost:6001/notifications


```
- Then inside code we can use them like this:
```js 
 const baseUrl = process.env.API_URL;
```
- We can create a Dockerfile for our Next.js webapp as follows: 
```shell 
 FROM node:18-alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

COPY frontend/web-app/package*.json ./
RUN  npm install --omit-dev

FROM node:18-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY frontend/web-app ./

ENV NEXT_TELEMETRY_DISABLED 1

RUN npm run build

# production image, copy all files and run next
FROM node:18-alpine AS runner
WORKDIR /app

ENV NODE_ENV production

ENV NEXT_TELEMETRY_DISABLED 1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

#COPY --from=builder /app/public ./public
COPY --from=builder /app/package.json ./package.json
COPY --from=builder /app/next.config.mjs ./next.config.mjs

COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000

CMD ["node", "server.js"]


```
- Note that we are building our app as a standalone webapp. 
- For this purpose we need to install a package sharp for this in our webapp. 
- Then we can add it to our docker compose file as follows: 
```shell 
   web-app:
    image: nishant198509/web-app
    build:
      context: .
      dockerfile: frontend/web-app/Dockerfile
    volumes:
      - /var/lib/web/data
    ports:
      - 3000:3000
    environment:
      - AUTH_SECRET="as19ZchDsmc3ZqU5oHq5or4uhtPZtE6sZyIEUYh+s3M="
      - API_URL=http://gateway-svc/
      - ID_URL=http://localhost:5000
      - ID_URL_INTERNAL=http://identity-svc
      - AUTH_URL=http://localhost:3000
      - AUTH_URL_INTERNAL:http://web-app:3000
      - NOTIFY_URL=http://localhost:6001/notifications

```

## Fixing problems with identity server
- When we first run the webapp in production mode in Docker and try to login by using the identity server it will not work. 
- This is because our next js server running inside docker container will try to connect over localhost:5000 to our identity server to validate the issuer. 
- If we specify the issuer to be http://identity-svc then also it will not work because in HostingExtensions.cs file our issuer is a hardcoded value which we have to move to appsettings.json 
- Similarly in Config.cs file the callback url for next.js Client also need to be moved in appsettings.json 
```c#
//Changes in Config.cs file 
new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256()) },
                //ID Token and Access Token can be shared without any browser involvement
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                //Pkce is required in case of mobile applications not for web applications
                RequirePkce = false,
                RedirectUris = {config["ClientApp"]+"/api/auth/callback/id-server"},
                //Allows us to use Refresh Token Functionality
                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile","auctionApp"},
                AccessTokenLifetime = 3600*24*30,
                AlwaysIncludeUserClaimsInIdToken = true
            }

//Changes in HostingExtensions.cs file 
builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.IssuerUri = builder.Configuration["IssuerUri"];
                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients(builder.Configuration))
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

```
- So we will need to configure 2 kinds of URLs: internal and external 
- We will configure another environment variable ID_URL_INTERNAL which our Next.js server will have to connect to get the token for 'authorize' and 'userinfo' endpoints. 
- We will make the following changes in auth.ts file in the Providers array for Duende Identity Server. 
```js 
 providers: [
        DuendeIDS6Provider({
            id:'id-server',
            clientId: "nextApp",
            clientSecret: "secret",
            issuer: process.env.ID_URL,
            authorization:{
                params:{scope:'openid profile auctionApp'},
                url: process.env.ID_URL + '/connect/authorize'
            },
            token:{
                url: `${process.env.ID_URL_INTERNAL}/connect/token`,
            },
            userinfo: {
                url: `${process.env.ID_URL_INTERNAL}/connect/token`,
            },
            idToken:true
        } as OIDCConfig<Omit<Profile,'username'>>),
    ],
    callbacks:{
        async redirect ({url,baseUrl}){
            return url.startsWith(baseUrl) ? url : baseUrl
        },


```
- Once we do the above changes our Next.js server will be able to connect to Identity Server. 

## Adding an Nginx ingress controller. 
- It is going to be a reverse proxy. 
- Client browser will go to a URL and then its going to hit the nginx proxy.
- It is the proxy's job to forward it to the appropriate service. 
- We will add the nginx-proxy container to docker compose like this: 
```shell 
   nginx-proxy:
    image: nginxproxy/nginx-proxy
    container_name: nginx-proxy
    ports:
      - "80:80"
    volumes:
      - /var/run/docker.sock:/tmp/docker.sock:ro

```
- We will then add the following entry in hosts file 
```shell 
 127.0.0.1 id.carsties.local app.carsties.local api.carsties.local

```
- We will then update the docker compose file for the identity svc, gateway svc and web-app to include Virtual host and port number like this: 
```shell
  web-app:
    image: nishant198509/web-app
    build:
      context: .
      dockerfile: frontend/web-app/Dockerfile
    volumes:
      - /var/lib/web/data
#    ports:
#      - 3000:3000
    environment:
      - AUTH_SECRET="as19ZchDsmc3ZqU5oHq5or4uhtPZtE6sZyIEUYh+s3M="
      - API_URL=http://gateway-svc/
      - ID_URL=http://id.carsties.local
      - ID_URL_INTERNAL=http://identity-svc
      - AUTH_URL=http://app.carsties.local
      - AUTH_URL_INTERNAL:http://web-app:3000
      - NOTIFY_URL=http://api.carsties.local/notifications
      - VIRTUAL_HOST=app.carsties.local
      - VIRTUAL_PORT=3000

  gateway-svc:
    image: nishant198509/gateway-svc:latest
    build:
      context: .
      dockerfile: src/GatewayService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:80
      - ClientApp=http://app.carsties.local
      - VIRTUAL_HOST=api.carsties.local

  identity-svc:
    image: nishant198509/identity-svc:latest
    build:
      context: .
      dockerfile: src/IdentityService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:80
      - ClientApp=http://app.carsties.local
      - IssuerUri=http://id.carsties.local
      - ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=identity
      - VIRTUAL_HOST=id.carsties.local

```

## Adding SSL to the ingress controller 
- SSL is supported using single host, wildcard and SAN certificates using naming conventions for certificates or optionally specifying a cert name as an environment variable.
- To enable SSL use the following command: 
```shell 
docker run -d -p 80:80 -p 443:443 -v /path/to/certs:/etc/nginx/certs -v /var/run/docker.sock:/tmp/docker.sock:ro nginxproxy/nginx-proxy
```
- The contents of /path/to/certs should contain the certificates and private keys for any virtual hosts in use. The certificate and keys should be named after the virtual host with a .crt and .key extension. For example, a container with VIRTUAL_HOST=foo.bar.com should have a foo.bar.com.crt and foo.bar.com.key file in the certs directory.
- mkcert is a simple tool for making locally-trusted development certificates. It requires no configuration.
- mkcert automatically creates and installs a local CA in the system root store, and generates locally-trusted certificates.
- use the following commands to install and use mkcert 
```shell 
# Install mkcert tool
choco install mkcert 
# Install local Certification Authority
mkcert -install
# Create a new folder devcert 
mkdir devcert
#Go into that folder
cd devcert
#Generate a new key file and crt file with the same name as the domain we will use i.e carsties.local to create a new certificate which is valid for app.carsties.local, api.carsties.local and id.carsties.local
 mkcert -key-file carsties.local.key -cert-file carsties.local.crt app.carsties.local api.carsties.local id.carsties.local

```
- The above commands will generate carsties.local.crt and carsties.local.key in the devcert folder. 
- Then we will update nginx configuration in the docker compose file and also update web-app, identity-svc and gateway-svc to use the secure https urls like this 

```shell 
  nginx-proxy:
    image: nginxproxy/nginx-proxy
    container_name: nginx-proxy
    ports:
      - 80:80
      - 443:443
    volumes:
      - /var/run/docker.sock:/tmp/docker.sock:ro
      # Path of the folder where to find the certificates key and crt files
      - ./devcert:/etc/nginx/certs

  web-app:
    image: nishant198509/web-app
    build:
      context: .
      dockerfile: frontend/web-app/Dockerfile
    volumes:
      - /var/lib/web/data
#    ports:
#      - 3000:3000
    environment:
      - AUTH_SECRET="as19ZchDsmc3ZqU5oHq5or4uhtPZtE6sZyIEUYh+s3M="
      - API_URL=http://gateway-svc/
    # update to use https
      - ID_URL=https://id.carsties.local
      - ID_URL_INTERNAL=http://identity-svc
      - AUTH_URL=https://app.carsties.local
      - AUTH_URL_INTERNAL:http://web-app:3000
      - NOTIFY_URL=https://api.carsties.local/notifications
      - VIRTUAL_HOST=app.carsties.local
      - VIRTUAL_PORT=3000

  gateway-svc:
    image: nishant198509/gateway-svc:latest
    build:
      context: .
      dockerfile: src/GatewayService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:80
      - ClientApp=https://app.carsties.local
      - VIRTUAL_HOST=api.carsties.local

  identity-svc:
    image: nishant198509/identity-svc:latest
    build:
      context: .
      dockerfile: src/IdentityService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:80
      - ClientApp=https://app.carsties.local
      - IssuerUri=https://id.carsties.local
      - ConnectionStrings__DefaultConnection=Server=postgres:5432;User Id=postgres;Password=postgrespw;Database=identity
      - VIRTUAL_HOST=id.carsties.local

```

## Unit and Integration Testing
- ![alt text](image-68.png)
- Unit Tests are faster to write and are cheap 
- Integration tests test more than one thing. 
- We will test against a fake database. 
- Tests provide us documentation against our code
- They provide us loosely coupled code. 
- A good test is Fast, Isolated, Repeatable, Self-Checking and Timely(takes less time to write)
- We will use xUnit
- ![alt text](image-69.png)
- ![alt text](image-70.png)
- ![alt text](image-71.png)
- Stub doesnot have implementation but they can return a pre-programmed value. 
- ![alt text](image-72.png)
- We will add an xUnit test project to our solution: 
```shell 
dotnet new xunit -o tests/AuctionService.UnitTests


```
- Next step is to add a Unit Test. We will name our unit tests in the following manner: 
```c#
namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void Method_Scenario_ExpectedResult()
    {

    }
}

```
- It is very difficult to mock AuctionDbContext in AuctionController. 
- We need to create an IRepository and AuctionRepository to be able to test our AuctionsController.
- It is easy to mock IMapper and IPublishEndpoint since they are interfaces, but AuctionDbContext is not an interface. 
- So we will move the database related changes into IAuctionRepository and its implementation AuctionRepository. 
- Then we can create Unit Tests cases for the AuctionController.cs file like this: 
```c#
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

```
- We may also have to pass the claims principal into the HTTP Request like this. 
- We will create a folder Utils with file Helpers.cs like this: 
- This will pass the claims principal into our AuctionControllerTests constructor 
```c#
 using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class Helpers
{
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        var claims = new List<Claim>
        {
            new Claim("username", "test"),
            new Claim(ClaimTypes.Name, "test"),
        };
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}

```
- Pass it to the AuctionControllerTests constructor like this: 
```c#
  _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };

```

## Integration Testing (TestContainers and Fake JwtBearer and Asp.Net Core WebApi)
- We will create a new project for AuctionService.IntegrationTests 
```shell
dotnet new xunit -o tests/AuctionService.IntegrationTests
dotnet sln add tests/AuctionService.IntegrationTests
dotnet add reference ../../src/AuctionService

```
- We will add the following packages: 
- Microsoft.AspNetCore.Mvc.Testing (For functional testing for our Asp.net core webapi project)
- TestContainers.PostgresSql (For testing our postgresSql database without actually connecting to a real database)
- WebMotions.Fake.Authentication.JwtBearer (For testing authentication with JwtBearer tokens without connecting to an actual identity server)

- Then we will create a class CustomWebAppFactory that implements WebApplicationFactory of Microsoft.AspNetCore.Mvc.Testing and we will create a partial class for Program.cs like this 
```c#
public partial class Program {}
```
- Custom Web Application Factory is used to create a test instance of our WebApplication and we can add test services inside it like this: 
```c#
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuctionService.IntegrationTests.Fixtures;

// Create a test instance of our WebApplication and we can add test services inside here
public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    
}

```
- We now need to implement IAsyncLifeTime interface which is provided by xUnit which provides asynchronous lifetime functionality. 
- We will override a method ConfigureWebHost().
- When this CustomWebAppFactory runs, it basically executes everything inside Program.cs file of AuctionService. 
- We need to then replace DbContext from Program.cs with the one from the Test Container. 
- We also need to add a DbContext to use the DbContext of PostgresSqlContainer. 
- Then we also need to replace the MassTransit initialized from Program.cs file to be replaced with MassTransitTestHarness. 
- We also need to ensure our database is created inside our postgreSql test container. 
- We will also need to create a DbHelper class that will initialize the database for us, but we will also need to method to reset the db after each test.
- This is because each test will cause data to be in some kind of wrong state. So we need to reset it back. 
- We will also need to implement a ServiceCollectionExtension class that will have methods to remove the DbContext and ensure the database is created. 
- The methods inside this class are used inside the CustomWebAppFactory class. 
```c#
//DbHelper.cs 
using AuctionService.Data;
using AuctionService.Entities;

namespace AuctionService.IntegrationTests.Util;

public static class DbHelper
{
    public static void InitDbForTests(AuctionDbContext db)
    {
        db.Auctions.AddRange(GetAuctionsForTest());
        db.SaveChanges();
    }

    public static void ReInitDbForTests(AuctionDbContext db)
    {
        //Remove all auctions from table
        db.Auctions.RemoveRange(db.Auctions);
        db.SaveChanges();
        InitDbForTests(db);
    }

    private static List<Auction> GetAuctionsForTest()
    {
        return new List<Auction>
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
            }
        };
    }
}



//Service Collection Extensions.cs 

using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
        if(descriptor != null) services.Remove(descriptor);
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AuctionDbContext>();
        db.Database.Migrate();
            
        DbHelper.InitDbForTests(db);
    }
}



//CustomWebAppFactory.cs file 
using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

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

```
## Creating Unit Tests for GET/POST/UPDATE
- We can create such tests as follows: 
- Please note for POST/PUT methods we are using FakeBearerToken and this works well with FakeJwtBearerDefaults Authentication Scheme we have specified in CustomWebAppFactory.cs file 
```c#
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;

namespace AuctionService.IntegrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string AuctionId = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        //arrange
        
        
        //act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("/api/auctions");
        
        //assert
        Assert.Equal(3, response.Count);
    }
    
    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        //arrange
        
        
        //act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"/api/auctions/{AuctionId}");
        
        //assert
        Assert.Equal("GT", response.Model);
    }
    
    [Fact]
    public async Task GetAuctionById_WithInvalidId_ShouldReturn404()
    {
        //arrange
        
        
        //act
        var response = await _httpClient.GetAsync($"/api/auctions/{Guid.NewGuid()}");
        
        //assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturn401()
    {
        //arrange
        var auction = new CreateAuctionDto(){Make = "Test"};
        
        //act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);
        
        //assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        //arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        //act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);
        
        //assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob",createdAuction.Seller);
    }
    
    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        //arrange
        var auction = GetAuctionForCreate();
        auction.Make = null;
        auction.Mileage = 0;
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        //act
        var response = await _httpClient.PostAsJsonAsync($"/api/auctions", auction);
        
        //assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        //arrange
        var auction = GetAuctionForUpdate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        //act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{AuctionId}", auction);
        
        //assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        var auction = GetAuctionForUpdate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("alice"));
        
        //act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{AuctionId}", auction);
        
        //assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReInitDbForTests(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            Color = "testColor",
            ImageUrl = "testImageUrl",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
    
    private UpdateAuctionDto GetAuctionForUpdate()
    {
        return new UpdateAuctionDto
        {
            Make = "Updated",
            Model = "Updated",
            Color = "Updated",
            Mileage = 20,
            Year = 20
        };
    }
}

```

## Testing the Service Bus 
- We can use the MassTransitTestHarness to test the events being published
- We can make use of ITestHarness which can be instantiated to get the TestHarness from MassTransit and then we can check whether the event has been published using this TestHarness.
```c#
 using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

//Each of our test classes uses an instance of our IClassFixture
//This implies that we have 2 databases instantiated
public class AuctionBusTests: IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreatedEvent()
    {
        //arrange 
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
        
        //act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        
        //assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
    }
    
    
    
    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReInitDbForTests(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            Color = "testColor",
            ImageUrl = "testImageUrl",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}


```

## Using Collection Fixtures to share DB across test classes. 
- Need to ensure our tests run as fast as possible. 
- We should try and use the same database server for each test.
- Since we now have 2 test classes: AuctionControllerTests and AuctionServiceBusTests
- Both need access to database and both are going to startup a new database.
- So we need to create a Shared Fixture. 
- We can create it like this: 
```c#
 [CollectionDefinition("Shared Collection")]
public class SharedFixture: ICollectionFixture<CustomWebAppFactory>
{
    
}


```
- Now we will specify it in our Test Classes like this 
```c#
 [Collection("Shared Collection")]
public class AuctionBusTests:  IAsyncLifetime
{
}

[Collection("Shared Collection")]
public class AuctionControllerTests : IAsyncLifetime
{
}

```

## Publishing our App to (local) Kubernetes
- Enable Kubernetes in Docker Desktop 
- ![alt text](image-73.png)
- This K8s cluster will have a single node.
- Node is just a virtual machine that is capable of running containers. 
- Identity Service would not be part of this cluster and will live outside the cluster. 
- Typically Identity Providers live outside the cluster of our application. 
- We will take advantage of single sign on and keep this separate from the application that is consuming it.
- ![alt text](image-74.png)
- K8s eliminates many of the manual processes involved in deploying and scaling containerized applications. 
- K8s helps us to cluster together group of hosts running Linux containers and K8s helps us to easily and efficiently manage those clusters. 
- K8s is declarative. 
- We just specify what is the state we want. 
- It provides service discovery and load balancing. 
- Allows for storage orchestration and allows us to mount a storage system of our choice. 
- It also automates rollouts and rollbacks. 
- It is self-healing
- It also provides us with secret and config management 
- ![alt text](image-75.png)
- We have a master node to control these nodes. 
- Node is a VM or physical machine that can run containerized applications. 
- Smallest unit of deployment in K8s is a pod 
- ![alt text](image-76.png)
- Sometimes pods and container have a one to one relationship. 
- Pods can have more than one container also . 
- We can have services running inside our K8s as well. 
- We will provide a cluster-ip to each of our services 
- ![alt text](image-77.png)
- In the above example Auction Service can find Search Service using its cluster IP name and thats how these services communicate internally. 
- Externally we use a Load Balancer or NodePort to communicate to outside world. 
- For K8s we need to provide manifest files. 
- ![alt text](image-78.png)
- We have apiVersion, kind, metadata, spec. 
- We will use kubectl tool. 
- Alternative to using K8s on Docker Desktop is to use minikube. 
- Minikube doesnot use NodePort.
- Kubernetes doesnot have depends on feature like docker compose. 
- We need to use Polly instead in our code to wait for our postgres container to start or rabbitmq container to start. 
- We need to handle exceptions in code and use retry policies. 
- So for Auction Service and Search Service we will install Polly nuget package and handle it in a retry policy in Program.cs file like this: 
- Similar sort of code will be implemented for IdentityService and Bidding Service. 
```c#
//Auction Service 
var retryPolicy = Policy
    .Handle<NpgsqlException>()
    .WaitAndRetry(5,retryAttempt => TimeSpan.FromSeconds(10));

retryPolicy.ExecuteAndCapture(() => DbInitializer.InitDb(app));

//Search Service 
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await Policy.Handle<TimeoutException>()
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitDb(app));
});


```

## Creating Deployment Manifests for K8s 
- We will use kubectl tool 
- ![alt text](image-79.png)
- ![alt text](image-80.png)
- First we will create a deployment for postgres sql database. 
- We will also create a persistent volume claim for the same. 
- A Persistent Volume Claim (PVC) in Kubernetes is a request for storage by a user. 
- It's similar to a Pod in that it consumes storage resources
- Persistent Volume (PV): A piece of storage in the cluster that has been provisioned by an administrator or dynamically provisioned using StorageClasses. It's a resource in the cluster, just like a node.
- Persistent Volume Claim (PVC): A request for storage by a user. It specifies the size and access modes (e.g., ReadWriteOnce, ReadOnlyMany).
- Imagine your applications have different storage needs and you want to ensure they have consistent and reliable access to storage across your Kubernetes cluster. That's where Persistent Volume Claims (PVCs) come in handy.
- PVCs decouple the storage from the Pods, ensuring that the lifecycle of the storage is not tied to the lifecycle of the Pod. This means your data remains intact even if the Pod is deleted or restarted.
- We can create a PVC like this: 
```yaml 
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-claim
spec:
  resources:
    requests:
      storage: 200Mi
  volumeMode: Filesystem
  accessModes:
    - ReadWriteOnce #Volume can be read from or written to, from one node at one time.

```
- Then we can use this PVC inside our K8s deployment file like this 
```yaml 
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
spec:
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
        - name: postgres
          image: postgres
          env:
            - name: POSTGRES_PASSWORD
              value: postgrespw
          ports:
            - containerPort: 5432
          volumeMounts:
            - mountPath: /var/data/postgres
              name: postgresdata
      volumes:
        - name: postgresdata
          persistentVolumeClaim:
            claimName: postgres-claim

```
- We can apply these changes using Kubectl command like this: 
```shell 
kubectl apply -f postgres-depl.yml

```

## Adding a NodePort
- In Kubernetes, a NodePort is a type of Service that exposes your application to the external world, making it accessible outside the Kubernetes cluster.
- How NodePort Works
- NodePort Service: This service opens a specific port on all the nodes in your cluster and forwards traffic to the appropriate Pod based on the defined selector. The port range is usually between 30000 and 32767.
- ClusterIP: Behind the scenes, a NodePort service still uses a ClusterIP service, which is the default type of service that exposes the service on an internal IP in the cluster.
- External Access: By using NodePort, you can access your application from outside the cluster using the node's IP address and the specific port.
- We can create one for our postgres container like this: 
- This is type of NodePort Service. 
```yaml 
 ---
apiVersion: v1
kind: Service
metadata:
  name: postgres-np
spec:
  type: NodePort
  selector:
    app: postgres
  ports:
    - port: 5432
      targetPort: 5432
      nodePort: 30001

```
## Adding a ClusterIP Address 
- In Kubernetes, ClusterIP is the default type of service that provides internal access to your application within the cluster.
- Internal Communication: ClusterIP assigns an internal IP address to the service, making it accessible only within the Kubernetes cluster.
- Service Discovery: Other services and Pods in the cluster can use this internal IP address to communicate with the service.
- Default Service Type: When you create a service without specifying a type, it defaults to ClusterIP.
- Internal Networking: Facilitates communication between different services and Pods within the cluster.
- Service Discovery: Simplifies the discovery of services by providing a stable internal IP address and DNS name.
- Security: Limits exposure to the external network, reducing potential attack vectors.
- Microservices Architecture: Ideal for internal communication between microservices.
- Private Applications: For applications that don't need to be exposed outside the cluster.
- Testing and Development: Useful for testing and development environments where external access isn't required.
- Equivalent for this in docker compose is using the name like auction.svc, gateway-svc, search-svc 
- ![alt text](image-81.png)
- Sample clusterIp is like this: 
```yaml 
 # Create a ClusterIP for the postgres pod to make it available internally to other services
apiVersion: v1
kind: Service
metadata:
  name: postgres-clusterip
spec:
  type: ClusterIP
  selector:
    app: postgres
  ports:
    - port: 5432
      targetPort: 5432
```
## Adding a deployment for rabbitMq 
- We will add a PVC for rabbitmq like this 
```shell 
 ---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: rabbit-claim
spec:
  resources:
    requests:
      storage: 200Mi
  volumeMode: Filesystem
  accessModes:
    - ReadWriteOnce #Volume can be read from or written to, from one node at one time.

```
- Then we will create a deployment for rabbitmq like this 
```yaml 
apiVersion: apps/v1
kind: Deployment
metadata:
  name: rabbitmq
spec:
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
        - name: rabbitmq
          image: rabbitmq:3-management
          env:
            - name: RABBITMQ_DEFAULT_USER
              value: rabbit
            - name: RABBITMQ_DEFAULT_PASS
              value: rabbitpw
          ports:
            - containerPort: 15672
              name: rbmq-mgmt-port
            - containerPort: 5672
              name: rbmq-msg-port
          volumeMounts:
            - mountPath: /var/lib/rabbitmq
              name: rabbitdata
      volumes:
        - name: rabbitdata
          persistentVolumeClaim:
            claimName: rabbit-claim
---
# Create a ClusterIP for the rabbitmq pod to make it available internally to other services
apiVersion: v1
kind: Service
metadata:
  name: rabbit-clusterip
spec:
  type: ClusterIP
  selector:
    app: rabbitmq
  ports:
    - port: 5672
      targetPort: 5672
---

# Create a service type of NodePort to expose this rabbitmq pod to the outside world
apiVersion: v1
kind: Service
metadata:
  name: rabbit-np
spec:
  type: NodePort
  selector:
    app: rabbitmq
  ports:
    - port: 15672
      targetPort: 15672
      nodePort: 30002

```

## Creating a MongoDb Deployment 
- We will first create a PVC for this 
```yaml 
 apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: mongo-claim
spec:
  resources:
    requests:
      storage: 200Mi
  volumeMode: Filesystem
  accessModes:
    - ReadWriteOnce 
```
- Then we will create a deployment for mongodb as follows: 
```yaml 
 apiVersion: apps/v1
kind: Deployment
metadata:
  name: mongo
spec:
  selector:
    matchLabels:
      app: mongo
  template:
    metadata:
      labels:
        app: mongo
    spec:
      containers:
        - name: mongo
          image: mongo
          env:
            - name: MONGO_INITDB_ROOT_USERNAME
              value: root
            - name: MONGO_INITDB_ROOT_PASSWORD
              value: mongopw
          ports:
            - containerPort: 27017
          volumeMounts:
            - mountPath: /data/db
              name: mongodata
      volumes:
        - name: mongodata
          persistentVolumeClaim:
            claimName: mongo-claim
---
# Create a ClusterIP for the rabbitmq pod to make it available internally to other services
apiVersion: v1
kind: Service
metadata:
  name: mongo-clusterip
spec:
  type: ClusterIP
  selector:
    app: mongo
  ports:
    - port: 27017
      targetPort: 27017
---

# Create a service type of NodePort to expose this rabbitmq pod to the outside world
apiVersion: v1
kind: Service
metadata:
  name: mongo-np
spec:
  type: NodePort
  selector:
    app: mongo
  ports:
    - port: 27017
      targetPort: 27017
      nodePort: 30003

```

## Auction Service Deployment 
- If we remember during docker compose this had a lot of environment variables. 
- It would be noisy to include those environment variables within the same auction svc deployment file. 
- So we can can create a configMap file like this 
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: auction-svc-config
data:
  ASPNETCORE_URLS: http://+:80;http://+:7777
  RabbitMq__Host: rabbit-clusterip
  RabbitMq__Username: rabbit
  RabbitMq__Password: rabbitpw
  ConnectionStrings__DefaultConnection: Server=postgres-clusterip:5432;User Id=postgres;Password=postgrespw;Database=auctions
  IdentityServiceUrl: http://identity-svc
  Kestrel__Endpoints__Grpc__Protocols: Http2
  Kestrel__Endpoints__Grpc__Url: http://+:7777
  Kestrel__Endpoints__WebApi__Protocols: Http1
  Kestrel__Endpoints__WebApi__Url: http://+:80
```

- Then we can create our auction-svc deployment file and use it inside it by using its name: auction-svc-config. 
```yaml 
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auction-svc
spec:
  selector:
    matchLabels:
      app: auction-svc
  template:
    metadata:
      labels:
        app: auction-svc
    spec:
      containers:
        - name: auction-svc
          image: nishant198509/auction-svc
          imagePullPolicy: Never #To use local images and not pull them down from repository
          envFrom:
            - configMapRef:
                name: auction-svc-config
          ports:
            - containerPort: 80
              name: web
            - containerPort: 7777
              name: grpc  
---
# Create a ClusterIP for the auction service pod to make it available internally to other services
apiVersion: v1
kind: Service
metadata:
  name: auction-clusterip
spec:
  type: ClusterIP
  selector:
    app: auction-svc
  ports:
    - port: 80
      targetPort: 80
      name: web
    - port: 7777
      targetPort: 7777
      name: grpc
      

```
- If we make changes to the config file and just need to restart our pod we can do it using this kubectl command: 
```shell 
 kubectl rollout restart deployment auction-svc  

```
- Please note that since we are deploying them inside our local K8s cluster running in docker, our images are already available. 
- In Production, we would have to pull them from a container repository like Azure Container Repository. 


## Creating the Search Service Deployment 
- We will create a configmap for this first:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: search-svc-config
data:
  ASPNETCORE_URLS: http://+:80
  RabbitMq__Host: rabbit-clusterip
  RabbitMq__Username: rabbit
  RabbitMq__Password: rabbitpw
  ConnectionStrings__MongoDbConnection: mongodb://root:mongopw@mongo-clusterip
  AuctionServiceUrl: http://auction-clusterip

```
- We will now create search svc deployment file like this 
```yaml 
 apiVersion: apps/v1
kind: Deployment
metadata:
  name: search-svc
spec:
  selector:
    matchLabels:
      app: search-svc
  template:
    metadata:
      labels:
        app: search-svc
    spec:
      containers:
        - name: search-svc
          image: nishant198509/search-svc
          imagePullPolicy: Never #To use local images and not pull them down from repository
          envFrom:
            - configMapRef:
                name: search-svc-config
          ports:
            - containerPort: 80
---
# Create a ClusterIP for the auction service pod to make it available internally to other services
apiVersion: v1
kind: Service
metadata:
  name: search-clusterip
spec:
  type: ClusterIP
  selector:
    app: search-svc
  ports:
    - port: 80
      targetPort: 80
      

```
- Similar to the above format we will add configs and then individual deployment files for bid service, notify service, gateway service, identity service and the webapp. 


## Adding an ingress controller for K8s
- There are multiple ways to install the Ingress-Nginx Controller:
- with Helm, using the project repository chart;
- with kubectl apply, using YAML manifests;
- with specific addons (e.g. for minikube or MicroK8s).
- On most Kubernetes clusters, the ingress controller will work without requiring any extra configuration.
- Use this command to install nginx controller in k8s 
```shell 
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.0/deploy/static/provider/cloud/deploy.yaml
```
- We will copy paste the code from the above link into its ingress-depl yaml file 
- We will create another ingress-svc yaml file and we will specify the rules for forwarding the traffic from the outside world within the cluster.
- That file will look like this: 
```yaml 
 # https://kubernetes.io/docs/concepts/services-networking/ingress/#the-ingress-resource

apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ingress-svc
  labels:
    name: ingress-svc
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - app.carsties.local
        - api.carsties.local
        - id.carsties.local
      secretName: carsties-app-tls
  rules:
  - host: app.carsties.local
    http:
      paths:
      - pathType: Prefix
        path: "/"
        backend:
          service:
            name: webapp-clusterip
            port:
              number: 3000
  - host: api.carsties.local
    http:
      paths:
      - pathType: Prefix
        path: "/"
        backend:
          service:
            name: gateway-clusterip
            port:
              number: 80
  - host: id.carsties.local
    http:
      paths:
      - pathType: Prefix
        path: "/"
        backend:
          service:
            name: identity-clusterip
            port:
              number: 80

```
- Next step is to add SSL support to our application.
- So we will make use of MkCert tool to generate our key and certificate inside the devcerts folder like this 
```shell 
 mkcert -key-file server.key -cert-file server.crt app.carsties.local api.carsties.local id.carsties.local

```
- ![alt text](image-82.png)
- Next we will have to create a kubernetes secret to store this key and certificate like this 
```shell
  kubectl create secret tls carsties-app-tls --key server.key --cert server.crt

```
- Then we will use it in our ingress-svc yaml file to provide HTTPs support in our application 
```yaml 
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - app.carsties.local
        - api.carsties.local
        - id.carsties.local
      secretName: carsties-app-tls

```
- Then we can run our application.

## Publishing to Kubernetes(Internet)
- Deployment to hosted Kubernetes Cluster. 
- Continuous integration using Github Actions
- Creating Kubernetes Cluster on Digital Ocean/Azure Kubernetes Service or Amazon Elastic Kubernetes Service