Product and Order management Service 

- This application constructed using 2 independent .NET 9 Web API microservices that communicate with each otjer via HTTP. Products and Orders are handled by the system with order entry automatically verifying stock and updating inventory in the Product Service.

## Group Members:-
- Yatri Patel (8980053)
- Kunj Lakhani ()

## Services

- Product Service - 8081 - SQLite (product.db)
# Endpoint:
    - GET - /api/product - Get all product
    - GET - /api/produt/{id} - Get product by ID
    - POST - /api/product - Create new Product
    - PUT - /api/product/{id} - Update Product
    - DELETE - /api/product/{id} - delete Product
    - PATCH - /api/product/{id}/stock?auantity=N - update stock level
    - GET - /health - Health check 

- Order Service - 8082 - SQLitw (order.db)
# Endpoint:
    - GET - /api/order - Get all order
    - GET - /api/order/{id} - Get order by ID
    - POST - /api/order - Create new order
    - PUT - /api/order/{id}/status - Update order status
    - DELETE - /api/order/{id} - cancel order
    - GET - api/order/customer/{email} - Get order by customer email
    - GET - /health - Health check

## How they communicate

When an order is made, the Order Service makes a call to the Product Service to verify the product, stock and deduct the quantity. In the case of order cancellation, the stock is automatically returned.

Client -> Order Service -> Product service (validate + deduct stock)
            -> Save order -> Return to Client

## Run Project with Docker
```bash
docker-compose up --build
```

Product Service: http://localhost:8081/swagger
Order Service: http://localhost:8082/swagger

Client:- 
```bash
cd Client
node index.js
```

## Run Manually

```bash
#Terminal 1
cd ProductService 
dotnet run

#Terminal 2
cd OrderService
dotnet run

#Terminal 3 
cd Client
npm install
node index.js
```

## Cloud Deployment

The Product Service and Order Service deployed publicaly in Render.

- Note:- 
The GitHub Actions workflows in this repository have been initially authored to be deployed on Azure Web Apps. The Azure deployment, however, requires an active subscription in Azure with set up credentials, which was not provided. As a result, the services have been manually deployed to Render instead, which provides free-tier hosting of .Services through NET without subscriptions. The CI/CD pipeline still needs to be fully assembled and run on every push to main - only the deploy step is run to Azure (this might be customized to Render or any other provider).

- Live URL:
    - Product Service - https://product-service-api-52g6.onrender.com/Swagger 
    - Order Service - https://order-service-api-v1ts.onrender.com/swagger


## CI/CD

Actions pipelines in the form of files in the directory .github/workflows run when pushed to main: each service is restored, built, and published in Release configuration. The pipelines were initially developed on Azure, but converted to Render since Azure needs a paid subscription. The build and publish processes are complete.

## Unit Tests 

``` bash
cd FinalProject.Tests 


dotnet test
```
total 7 tests using xUnit with in-memory database.

## Github id
- https://github.com/YatriPatel17/FinalProject

## Vide Link
https://youtu.be/Q-VIidworiM


