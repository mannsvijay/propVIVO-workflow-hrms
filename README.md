# propVIVO HRMS - Workflow Management System

A modern, modular Human Resource Management System (HRMS) with integrated workflow capabilities. Built with a scalable microservices architecture on the backend and a responsive Next.js frontend.

## 📋 Table of Contents
- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [Features](#features)
- [API Documentation](#api-documentation)
- [Contributing](#contributing)

## 🎯 Project Overview

propVIVO HRMS is a comprehensive Human Resource Management System designed to streamline HR operations including:
- Service request management
- Task/Todo management
- User management and authentication
- Workflow automation
- Real-time notifications and monitoring

The system follows a modular monolithic architecture with a clear separation of concerns between domain, application, and infrastructure layers.

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 8+ / ASP.NET Core
- **API**: GraphQL with Hot Chocolate
- **Database**: PostgreSQL
- **Cloud Services**: Azure (Key Vault, Application Insights, Blob Storage)
- **Architecture**: Modular Monolith with Clean Architecture
- **Telemetry**: Application Insights

### Frontend
- **Framework**: Next.js 14+ with TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Redux
- **API Client**: Apollo Client (GraphQL)
- **UI Components**: Custom components with React

### Infrastructure
- **Authentication**: Azure Active Directory
- **Secrets Management**: Azure Key Vault
- **Monitoring**: Application Insights
- **Storage**: Azure Blob Storage

## 📁 Project Structure

```
HRMS_Modular_Monolithic_BolierPlate/
├── API/
│   └── HRMS.API/                 # Main API entry point
│       ├── Extensions/            # Configuration extensions
│       ├── Middleware/            # Custom middleware (error handling, auth, etc.)
│       └── RegisterDependencies/  # DI container setup
│
├── Modules/                       # Feature modules (modular structure)
│   ├── ServiceRequestFeature/     # Service request management
│   ├── TodoFeature/               # Task management
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── GraphQL/
│   │   └── Infrastructure/
│   └── UserFeature/               # User management
│
├── Shared/                        # Shared libraries
│   ├── HRMS.Shared.Application/   # Common DTOs, services, validators
│   ├── HRMS.Shared.Core/          # Core utilities (HTTP, KeyVault, DB, Telemetry)
│   ├── HRMS.Shared.Domain/        # Shared entities and enums
│   └── HRMS.Shared.Infrastructure/ # Common infrastructure
│
└── next-js-boilerplate/           # Frontend application
    ├── app/                       # Next.js app directory
    ├── components/                # React components
    ├── context/                   # React context
    ├── graphql/                   # GraphQL queries & mutations
    ├── lib/                       # Utilities (Apollo, Auth, etc.)
    └── store/                     # Redux store
```

## 📦 Prerequisites

### Backend
- .NET 8 SDK or higher
- PostgreSQL 12 or higher
- Azure Account (for Key Vault and other services)

### Frontend
- Node.js 18+ and npm/yarn
- Git

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/mannsvijay/propVIVO-workflow-hrms.git
cd propVIVO-workflow-hrms
```

### 2. Backend Setup

#### Configure Database
1. Create a PostgreSQL database named `HRMS`
2. Update connection string in `API/HRMS.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Postgres": {
      "ConnectionString": "Host=localhost;Port=5432;Database=HRMS;Username=postgres;Password=YOUR_PASSWORD",
      "DatabaseName": "HRMS"
    }
  }
}
```

#### Configure Azure Services
1. Set up Azure Key Vault and add credentials to `appsettings.Development.json`
2. Required Azure AD application registration for authentication
3. Update the following in `appsettings.Development.json`:
```json
{
  "KeyVaultURL": "YOUR_KEY_VAULT_URL",
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "TenantId": "YOUR_TENANT_ID"
}
```

#### Run Migrations
```bash
cd API/HRMS.API
dotnet ef database update
```

### 3. Frontend Setup

```bash
cd next-js-boilerplate
npm install
# or
yarn install
```

Configure the GraphQL endpoint in `lib/apolloClient.ts` if needed.

## ▶️ Running the Application

### Backend
```bash
cd API/HRMS.API
dotnet run
```
The API will start on `https://localhost:5001` and GraphQL endpoint at `https://localhost:5001/graphql`

### Frontend
```bash
cd next-js-boilerplate
npm run dev
# or
yarn dev
```
The frontend will be available at `http://localhost:3000`

## ✨ Features

### Service Request Management
- Create, read, update, and delete service requests
- Status tracking and workflow automation
- Integration with user management

### Todo/Task Management
- Personal task management
- Task status tracking
- User-specific task lists

### User Management
- User registration and authentication
- Role-based access control (RBAC)
- Azure AD integration

### GraphQL API
- Strongly typed API with GraphQL schema
- Efficient data fetching
- Real-time subscriptions support

## 📚 API Documentation

### GraphQL Endpoint
```
POST /graphql
```

### Sample Queries

**Get Todos:**
```graphql
query GetTodos {
  todos {
    id
    title
    description
    status
    createdAt
  }
}
```

**Create Todo:**
```graphql
mutation CreateTodo($input: CreateTodoInput!) {
  createTodo(input: $input) {
    id
    title
    status
  }
}
```

For full schema documentation, access the GraphQL Playground at `https://localhost:5001/graphql` (when enabled in development)

## 🔒 Security

- Secrets stored in Azure Key Vault (never committed to repository)
- Azure AD authentication for API access
- CORS policy configured in middleware
- Exception details hidden in production
- IP address filtering available

## 📝 Environment Configuration

### Development
- Use `appsettings.Development.json` for local secrets
- Enable GraphQL introspection and exception details
- Detailed logging enabled

### Production
- Use environment variables or Key Vault for secrets
- Disable GraphQL introspection
- Minimal exception details in responses

## 🤝 Contributing

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Commit your changes: `git commit -am 'Add new feature'`
3. Push to the branch: `git push origin feature/your-feature-name`
4. Submit a pull request

## 📄 License

This project is proprietary and confidential to propVIVO.

## 👥 Support

For issues and questions, please open an issue on GitHub or contact the development team.

---

**Last Updated**: June 2026
**Version**: 1.0.0
