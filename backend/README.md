# Home Service Backend - .NET 8 API

Backend API for the Home Service Application, built with .NET 8, Clean Architecture, and Microsoft Semantic Kernel.

## 🏗 Architecture

This backend follows **Clean Architecture** principles with clear separation of concerns:

### Layers

1. **Domain Layer** (`HomeService.Domain`)
   - Core business entities
   - Value objects
   - Domain enums
   - Repository interfaces
   - No external dependencies

2. **Application Layer** (`HomeService.Application`)
   - Business logic
   - CQRS with MediatR
   - DTOs and mappings
   - Validators (FluentValidation)
   - Application interfaces

3. **Infrastructure Layer** (`HomeService.Infrastructure`)
   - Data access (EF Core)
   - Repository implementations
   - External service integrations
   - Semantic Kernel AI services
   - Caching (Redis)
   - Background jobs (Hangfire)

4. **API Layer** (`HomeService.API`)
   - RESTful controllers
   - Authentication & Authorization (JWT)
   - Swagger documentation
   - Middleware & filters

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server 2022 or PostgreSQL 15+
- Redis 7+ (optional)
- OpenAI API key or Azure OpenAI account

### Installation

1. **Clone and navigate**
   ```bash
   cd backend
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update configuration**

   Edit `src/HomeService.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HomeServiceDB;...",
       "Redis": "localhost:6379"
     },
     "JwtSettings": {
       "SecretKey": "your-secret-key-here"
     },
     "SemanticKernel": {
       "OpenAI": {
         "ApiKey": "your-openai-api-key"
       }
     }
   }
   ```

4. **Run migrations**
   ```bash
   cd src/HomeService.API
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   ```
   http://localhost:5000/swagger
   ```

## 📦 Project Structure

```
backend/
├── src/
│   ├── HomeService.Domain/
│   │   ├── Entities/           # Core business entities
│   │   ├── ValueObjects/       # Value objects (Money, Location)
│   │   ├── Enums/             # Domain enumerations
│   │   ├── Interfaces/        # Repository interfaces
│   │   └── Common/            # Base classes
│   │
│   ├── HomeService.Application/
│   │   ├── Commands/          # CQRS commands
│   │   ├── Queries/           # CQRS queries
│   │   ├── DTOs/              # Data transfer objects
│   │   ├── Validators/        # FluentValidation validators
│   │   ├── Mappings/          # AutoMapper profiles
│   │   └── Interfaces/        # Application interfaces
│   │
│   ├── HomeService.Infrastructure/
│   │   ├── Data/              # DbContext & configurations
│   │   ├── Repositories/      # Repository implementations
│   │   ├── AI/                # Semantic Kernel services
│   │   │   └── Services/      # AI service implementations
│   │   ├── Identity/          # Authentication & authorization
│   │   ├── Caching/           # Redis cache implementation
│   │   └── Messaging/         # Notification services
│   │
│   └── HomeService.API/
│       ├── Controllers/       # API controllers
│       ├── Middleware/        # Custom middleware
│       ├── Filters/           # Action filters
│       └── Program.cs         # Application entry point
│
└── tests/                     # Test projects
    ├── HomeService.Domain.Tests/
    ├── HomeService.Application.Tests/
    ├── HomeService.Infrastructure.Tests/
    └── HomeService.API.Tests/
```

## 🔑 Key Features

### Authentication & Authorization

- JWT-based authentication
- Role-based access control (RBAC)
- Refresh token mechanism
- Two-factor authentication support

### CQRS Pattern

Commands and queries are separated using MediatR:

```csharp
// Command
public record CreateBookingCommand : IRequest<Result<BookingDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ServiceId { get; init; }
    // ...
}

// Query
public record GetServicesQuery : IRequest<Result<PagedResult<ServiceDto>>>
{
    public Guid? CategoryId { get; init; }
    public Region? Region { get; init; }
    // ...
}
```

### AI Services (Semantic Kernel)

1. **Chatbot Service**
   - Multilingual customer support
   - Context-aware conversations

2. **Recommendation Service**
   - Personalized service recommendations
   - Dynamic pricing optimization

3. **Sentiment Analysis Service**
   - Review sentiment scoring
   - Theme extraction

4. **Semantic Search Service**
   - Natural language query understanding
   - Intent parsing

## 🔧 Configuration

### Database Connection

**SQL Server:**
```json
"DefaultConnection": "Server=localhost;Database=HomeServiceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

**PostgreSQL:**
```json
"DefaultConnection": "Host=localhost;Database=HomeServiceDB;Username=postgres;Password=YourPassword;"
```

Update `DependencyInjection.cs` in Infrastructure layer to use PostgreSQL:
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### JWT Settings

```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456789",
  "Issuer": "HomeServiceAPI",
  "Audience": "HomeServiceClient",
  "ExpiryMinutes": 60,
  "RefreshTokenExpiryDays": 7
}
```

### Semantic Kernel Configuration

**Using OpenAI:**
```json
"SemanticKernel": {
  "UseAzureOpenAI": false,
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "ModelId": "gpt-4"
  }
}
```

**Using Azure OpenAI:**
```json
"SemanticKernel": {
  "UseAzureOpenAI": true,
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "ApiKey": "your-azure-openai-key",
    "DeploymentName": "your-deployment-name"
  }
}
```

## 🗄 Database Migrations

### Create a new migration
```bash
cd src/HomeService.API
dotnet ef migrations add MigrationName
```

### Apply migrations
```bash
dotnet ef database update
```

### Rollback migration
```bash
dotnet ef database update PreviousMigrationName
```

## 📊 API Endpoints

### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh token
- `POST /api/v1/users/register` - User registration

### Services
- `GET /api/v1/services` - Get all services
- `GET /api/v1/services/{id}` - Get service by ID
- `POST /api/v1/services` - Create service (Provider/Admin)

### Bookings
- `GET /api/v1/bookings` - Get user bookings
- `POST /api/v1/bookings` - Create booking
- `PUT /api/v1/bookings/{id}` - Update booking
- `POST /api/v1/bookings/{id}/cancel` - Cancel booking

### Health Check
- `GET /health` - API health status

Full API documentation available at `/swagger` when running.

## 🧪 Testing

### Run all tests
```bash
dotnet test
```

### Run with coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory="./coverage"
```

### Run specific test project
```bash
dotnet test tests/HomeService.Application.Tests/
```

## 📈 Performance

- API response time: < 200ms for 95% of requests
- Database query optimization with EF Core
- Redis caching for frequently accessed data
- Async/await throughout for better scalability

## 🔒 Security

- HTTPS/TLS encryption
- JWT token-based authentication
- Input validation with FluentValidation
- SQL injection prevention via EF Core
- XSS prevention
- CORS configuration
- Rate limiting support

## 📝 Logging

Using Serilog for structured logging:

```csharp
Log.Information("Processing booking {BookingId} for customer {CustomerId}",
    bookingId, customerId);
```

Logs are written to:
- Console (development)
- File: `logs/homeservice-{Date}.log`
- Can be configured for Application Insights, Elasticsearch, etc.

## 🚀 Deployment

### Build for production
```bash
dotnet publish -c Release -o ./publish
```

### Using Docker
```bash
docker build -t homeservice-backend .
docker run -p 5000:80 homeservice-backend
```

## 🔗 Useful Links

- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [Semantic Kernel Documentation](https://learn.microsoft.com/semantic-kernel/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

## 📄 License

MIT License - see LICENSE file for details
