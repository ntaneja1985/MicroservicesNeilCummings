# Microservices by Neil Cummings
Microservices based Application using Next.js
- ![img.png](img.png)
- Microservices are loosely coupled and each service handles a specific function
- Each microservice does one thing, and it does it really well
- Each microservice have their own database
- ![img_2.png](img_2.png)
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
- ![img_3.png](img_3.png)
- We also need a gateway and identity provider for microservices
- Microservices are not the cheapest way to build an application.
- Not suitable for small teams
- Meant for large teams