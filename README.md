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

