# Home Service Application

A comprehensive multi-region platform for on-demand home services in Saudi Arabia and Egypt, built with .NET 8, Semantic Kernel, and Angular 18.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [License](#license)

## 🌟 Overview

The Home Service Application connects customers with professional service providers for various home services including cleaning, plumbing, electrical work, carpentry, and more. The platform supports multiple languages (Arabic and English) and is optimized for the Saudi Arabian and Egyptian markets.

## ✨ Features

### Core Features
- **User Management**: Multi-role authentication (Customer, Service Provider, Admin)
- **Service Catalog**: Hierarchical service categories with multi-language support
- **Booking System**: Real-time booking with smart provider matching
- **Payment Processing**: Multiple payment methods including regional gateways
- **Rating & Reviews**: 5-star rating system with AI-powered sentiment analysis
- **Real-time Notifications**: Push, SMS, Email, and In-App notifications

### AI-Powered Features (Semantic Kernel)
- **Intelligent Chatbot**: 24/7 multilingual customer support
- **Smart Recommendations**: Personalized service suggestions
- **Dynamic Pricing**: AI-driven surge pricing optimization
- **Semantic Search**: Natural language service discovery
- **Predictive Analytics**: Demand forecasting and fraud detection

### Multi-Region Support
- **Saudi Arabia**: SAR currency, 15% VAT, Mada payments, Hijri calendar
- **Egypt**: EGP currency, 14% VAT, Fawry payments

## 🛠 Technology Stack

### Backend
- **.NET 8** - Modern web API framework
- **Microsoft Semantic Kernel** - AI orchestration
- **Entity Framework Core 8** - ORM
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Hangfire** - Background job processing
- **SQL Server / PostgreSQL** - Primary database
- **Redis** - Caching and session management

### Frontend
- **Angular 18** - Modern web framework with standalone components
- **TypeScript 5.0+** - Type-safe development
- **Angular Material** - UI component library
- **RxJS 7+** - Reactive programming
- **ngx-translate** - Internationalization
- **TailwindCSS** - Utility-first CSS

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD pipeline
- **Azure / AWS** - Cloud hosting

## 🏗 Architecture

The application follows **Clean Architecture** principles with clear separation of concerns:

```
backend/
├── src/
│   ├── HomeService.Domain/          # Core business entities
│   ├── HomeService.Application/     # Business logic & CQRS
│   ├── HomeService.Infrastructure/  # Data access & external services
│   └── HomeService.API/             # Web API endpoints
└── tests/                           # Unit & integration tests

frontend/
└── src/
    ├── app/
    │   ├── core/                    # Singleton services
    │   ├── features/                # Feature modules
    │   └── shared/                  # Shared components
    └── assets/                      # Static assets & i18n
```

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 22+
- Docker & Docker Compose (recommended)
- SQL Server or PostgreSQL
- Redis (optional, for caching)

### Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/homeservice.git
   cd homeservice
   ```

2. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start all services**
   ```bash
   docker-compose up -d
   ```

4. **Access the applications**
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

### Manual Setup

#### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection strings** in `appsettings.json`

4. **Run database migrations**
   ```bash
   cd src/HomeService.API
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```

#### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Update API endpoint** in `src/environments/environment.ts`

4. **Start development server**
   ```bash
   npm start
   ```

5. **Open browser**
   ```
   http://localhost:4200
   ```

## 📁 Project Structure

```
BAYTAEK/
├── backend/                    # .NET 8 Backend
│   ├── src/
│   │   ├── HomeService.Domain/
│   │   ├── HomeService.Application/
│   │   ├── HomeService.Infrastructure/
│   │   └── HomeService.API/
│   ├── tests/
│   └── HomeService.sln
├── frontend/                   # Angular 18 Frontend
│   ├── src/
│   │   ├── app/
│   │   ├── assets/
│   │   └── environments/
│   └── package.json
├── docs/                       # Documentation
│   └── SRS.md                 # Software Requirements Specification
├── docker-compose.yml         # Docker orchestration
├── .env.example              # Environment variables template
└── README.md                 # This file
```

## 📚 Documentation

- [Software Requirements Specification (SRS)](./docs/SRS.md)
- [Backend Documentation](./backend/README.md)
- [Frontend Documentation](./frontend/README.md)
- [API Documentation](http://localhost:5000/swagger) (when running)

## 🔧 Configuration

### Backend Configuration

Key configuration files:
- `appsettings.json` - Main configuration
- `appsettings.Development.json` - Development overrides

Important settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your database connection string",
    "Redis": "Your Redis connection string"
  },
  "JwtSettings": {
    "SecretKey": "Your JWT secret",
    "Issuer": "HomeServiceAPI",
    "Audience": "HomeServiceClient"
  },
  "SemanticKernel": {
    "OpenAI": {
      "ApiKey": "your-openai-api-key"
    }
  }
}
```

### Frontend Configuration

Environment files:
- `environment.ts` - Development
- `environment.prod.ts` - Production

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## 🚢 Deployment

### Using Docker

1. Build images:
   ```bash
   docker-compose build
   ```

2. Deploy:
   ```bash
   docker-compose up -d
   ```

### Manual Deployment

#### Backend
```bash
cd backend/src/HomeService.API
dotnet publish -c Release -o ./publish
```

#### Frontend
```bash
cd frontend
npm run build
# Deploy contents of dist/ to your web server
```

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Team

- Development Team - Initial work

## 🙏 Acknowledgments

- Microsoft Semantic Kernel team
- Angular team
- Open source community

## 📞 Support

For support, email support@homeservice.com or open an issue in the repository.

---

**Version**: 1.0.0
**Last Updated**: November 2025
