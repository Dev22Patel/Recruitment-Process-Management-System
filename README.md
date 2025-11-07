# Recruitment Process Management System - Backend

A RESTful Web API backend for managing recruitment processes, developed as a pre-internship assignment. This system provides secure endpoints for job management, candidate applications, user authentication, and asynchronous email processing using message queues.

## Project Overview

This is the backend API for a Recruitment Process Management System built with .NET Core. The application follows modern architectural patterns and implements asynchronous processing for email notifications using RabbitMQ message broker.

**Status**: Under Development  
**Purpose**: Pre-Internship Technical Assignment  
**Type**: Backend Web API

## Technology Stack

### Core Technologies
- **.NET Core** (ASP.NET Core Web API)
- **C#** - Primary programming language
- **Entity Framework Core** - ORM for database operations
- **SQL Server Express** - Relational database
- **RabbitMQ** - Message broker for asynchronous processing
- **Docker** - Containerization for RabbitMQ server

### Authentication & Security
- **JWT (JSON Web Tokens)** - Token-based authentication
- **BCrypt / Identity** - Password hashing and user management
- **CORS** - Cross-Origin Resource Sharing configuration

### Additional Libraries
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Request validation
- **Serilog / NLog** - Logging framework

## Architecture

The application follows a layered architecture pattern:
- **API Layer** - Controllers and routing
- **Business Logic Layer** - Service classes and business rules
- **Data Access Layer** - Repository pattern with Entity Framework Core
- **Message Queue Layer** - RabbitMQ producers and consumers for async operations

## Features Implemented

### Authentication & Authorization
- User registration with role assignment
- JWT-based authentication
- Role-based access control (Admin, Recruiter, Candidate)
- Secure password storage with hashing
- Token refresh mechanism

### Job Management
- Create, read, update, delete job postings
- Search and filter job listings
- Job status management (Active, Closed, Draft)
- Pagination for job listings

### Application Management
- Submit job applications
- Track application status
- Update application workflow
- File upload for resumes/documents
- Application history tracking

### User Management
- User profile management
- Role-based user operations
- User authentication and authorization
- Password reset functionality

### Email Notifications (Asynchronous)
- RabbitMQ message queue integration
- Background email processing
- Application status update notifications
- Welcome emails for new registrations
- Interview scheduling notifications

### API Documentation
- Swagger/OpenAPI documentation
- Endpoint descriptions and request/response models
- Authentication requirements documented

## Features Under Development

- Advanced search with multiple filters
- Interview scheduling system
- Calendar integration
- Bulk operations for applications
- Analytics and reporting endpoints
- File storage optimization
- Email template management
- Notification preferences
- Advanced logging and monitoring

## Prerequisites

- .NET SDK 6.0 or higher
- SQL Server Express (or SQL Server)
- Docker Desktop (for RabbitMQ)
- Visual Studio 2022 / VS Code / Rider
- Git

## Installation and Setup

### 1. Clone the Repository
```bash
git clone https://github.com/Dev22Patel/Recruitment-Process-Management-System.git
cd Recruitment-Process-Management-System
```

### 2. Setup SQL Server Database

Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RecruitmentDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Setup RabbitMQ with Docker

Pull and run RabbitMQ container:
```bash
docker pull rabbitmq:3-management
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Access RabbitMQ Management UI at `http://localhost:15672`
- Default username: `guest`
- Default password: `guest`

Update RabbitMQ configuration in `appsettings.json`:
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "email-queue"
  }
}
```

### 4. Install Dependencies

```bash
dotnet restore
```

### 5. Apply Database Migrations

```bash
dotnet ef database update
```

Or if migrations don't exist:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 7. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`

### 8. Access Swagger Documentation

Navigate to: `https://localhost:7001/swagger`

## Project Structure

```
RecruitmentSystem/
├── Controllers/              # API Controllers
│   ├── AuthController.cs
│   ├── JobsController.cs
│   ├── ApplicationsController.cs
│   └── UsersController.cs
├── Models/                   # Domain models
│   ├── Job.cs
│   ├── Application.cs
│   └── User.cs
├── DTOs/                     # Data Transfer Objects
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   └── JobDto.cs
├── Services/                 # Business logic services
│   ├── AuthService.cs
│   ├── JobService.cs
│   ├── EmailService.cs
│   └── RabbitMQService.cs
├── Repositories/             # Data access layer
│   ├── IRepository.cs
│   └── Repository.cs
├── Data/                     # Database context
│   └── ApplicationDbContext.cs
├── Middleware/              # Custom middleware
│   ├── ExceptionMiddleware.cs
│   └── JwtMiddleware.cs
├── Helpers/                 # Utility classes
├── Migrations/              # EF Core migrations
├── appsettings.json         # Configuration
└── Program.cs               # Application entry point
```

## API Endpoints

### Authentication
```
POST   /api/auth/register      - Register new user
POST   /api/auth/login         - User login
POST   /api/auth/refresh       - Refresh JWT token
```

### Jobs
```
GET    /api/jobs               - Get all jobs (with pagination)
GET    /api/jobs/{id}          - Get job by ID
POST   /api/jobs               - Create new job (Recruiter/Admin)
PUT    /api/jobs/{id}          - Update job (Recruiter/Admin)
DELETE /api/jobs/{id}          - Delete job (Admin)
GET    /api/jobs/search        - Search jobs with filters
```

### Applications
```
GET    /api/applications       - Get all applications
GET    /api/applications/{id}  - Get application by ID
POST   /api/applications       - Submit application
PUT    /api/applications/{id}  - Update application status
DELETE /api/applications/{id}  - Delete application
```

### Users
```
GET    /api/users              - Get all users (Admin)
GET    /api/users/{id}         - Get user by ID
PUT    /api/users/{id}         - Update user profile
DELETE /api/users/{id}         - Delete user (Admin)
```

## RabbitMQ Message Queue Implementation

### Email Queue Architecture

The system uses RabbitMQ for asynchronous email processing to improve API response times and ensure reliable email delivery.

**Producer (API)**
- Publishes email messages to RabbitMQ queue when events occur
- Non-blocking operations for better performance

**Consumer (Background Service)**
- Continuously listens to the email queue
- Processes email messages asynchronously
- Handles retry logic for failed deliveries

**Message Structure**
```json
{
  "To": "user@example.com",
  "Subject": "Application Status Update",
  "Body": "Your application has been reviewed...",
  "Type": "ApplicationUpdate"
}
```

### Queue Operations

**Publishing Messages**
```csharp
await _rabbitMQService.PublishMessage(emailMessage, "email-queue");
```

**Consuming Messages**
```csharp
// Background service continuously processes queue messages
// Implemented in EmailConsumerService.cs
```

## Authentication Flow

1. User registers or logs in through `/api/auth` endpoints
2. Server validates credentials and generates JWT token
3. Client stores token and includes it in Authorization header
4. Server validates token for protected endpoints using JWT middleware
5. Role-based authorization checks for specific operations

## Email Notification Workflow

1. User action triggers email requirement (e.g., application submitted)
2. Email message is published to RabbitMQ queue
3. API responds immediately without waiting for email
4. Background consumer service picks up message from queue
5. Email is sent via SMTP
6. Success/failure is logged for monitoring

## Security Measures

- Password hashing using BCrypt/Identity
- JWT token expiration and refresh
- HTTPS enforcement
- SQL injection prevention via parameterized queries
- CORS policy configuration
- Input validation using FluentValidation
- Role-based access control

## Environment Configuration

### Development
- Debug logging enabled
- Detailed error messages
- Swagger UI available

### Production (Planned)
- Error logging only
- Generic error messages to clients
- Swagger disabled
- HTTPS required
- Production database connection

## Testing

### Run Unit Tests
```bash
dotnet test
```

### API Testing
- Use Swagger UI for manual testing

## Current Limitations

- Email templates are hardcoded (needs template engine)
- File upload size restrictions need configuration
- Advanced search filters partially implemented
- Rate limiting not yet implemented
- Comprehensive logging needs enhancement
- Redis caching not yet integrated

## Docker Support

### RabbitMQ Container Management

**Start RabbitMQ**
```bash
docker start rabbitmq
```

**Stop RabbitMQ**
```bash
docker stop rabbitmq
```

**View RabbitMQ Logs**
```bash
docker logs rabbitmq
```

## Monitoring and Debugging

### RabbitMQ Queue Monitoring
- Access management UI to view queue status
- Monitor message rates and queue length
- Check consumer connections

### Application Logs
- Console logging during development
- Exception tracking and error logs

## Learning Outcomes

This project demonstrates proficiency in:
- RESTful API design and implementation
- Entity Framework Core and database design
- JWT authentication and authorization
- Asynchronous message processing with RabbitMQ
- Docker containerization for services
- Clean architecture and SOLID principles
- Dependency injection and IoC containers
- Repository pattern implementation
- Background services and hosted services

## Future Enhancements

- Redis caching implementation
- SignalR for real-time notifications
- Microservices architecture migration
- API versioning
- Rate limiting and throttling
- Advanced analytics endpoints
- Comprehensive unit and integration tests
- CI/CD pipeline setup
- Health check endpoints
- Performance monitoring and APM

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database exists or migrations are applied

### RabbitMQ Connection Issues
- Verify Docker container is running: `docker ps`
- Check RabbitMQ ports are not blocked
- Verify credentials in configuration

### Email Not Sending
- Check RabbitMQ queue for messages
- Verify SMTP configuration
- Check consumer service is running
- Review application logs for errors

## Author

**Dev Patel**  
GitHub: [@Dev22Patel](https://github.com/Dev22Patel)

## Related Repository

Frontend Client: [Recruitment-Process-Management-System-Client](https://github.com/Dev22Patel/Recruitment-Process-Management-System-Client)

## Acknowledgments

This project was developed as part of a pre-internship assessment to demonstrate technical capabilities in:
- Backend API development with .NET Core
- Database design and ORM usage
- Message queue integration for asynchronous processing
- Modern software architecture patterns
- Docker containerization

