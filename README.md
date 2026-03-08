# Dataverse API

A RESTful API built with Azure Functions (.NET 8) that provides CRUD operations for Microsoft Dataverse entities. This API acts as a middleware layer between client applications and Dataverse, enabling standardized HTTP operations on Dataverse data.

## 📖 Table of Contents

- [Overview](#overview)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Contributing](#contributing)

## Overview

This project exposes Dataverse entities through a clean REST API, allowing applications to:

- **Create** new records in Dataverse
- **Read** existing records by ID
- **Update** records with partial updates (PATCH)
- **Delete** records by ID

### Key Features

- ✅ Built on Azure Functions (Isolated Worker Model)
- ✅ .NET 8 with C# 12
- ✅ OpenAPI/Swagger documentation
- ✅ Bearer token authentication
- ✅ Application Insights integration
- ✅ Modular architecture for multiple entities

## API Documentation

### Swagger UI

Interactive API documentation is available at:

🔗 **[https://api.ethanbalgobin.com/swagger/ui](https://api.ethanbalgobin.com/swagger/ui)**

### OpenAPI Specification

The OpenAPI JSON specification can be accessed at:

🔗 **[https://api.ethanbalgobin.com/swagger.json](https://api.ethanbalgobin.com/swagger.json)**

## Authentication

This API uses **OAuth 2.0 Bearer Token** authentication.

### Obtaining a Token

Request an access token from the Microsoft Identity Platform authorization endpoint:

```http
POST https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={client-id}
&client_secret={client-secret}
&scope=https://orgXXXXX.crm.dynamics.com/.default
```

### Using the Token

Include the token in all API requests using the `Authorization` header:

```http
GET https://api.ethanbalgobin.com/contacts/{id}
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...
Content-Type: application/json
```

## API Endpoints

### Contacts

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/contacts/{id}` | Retrieve a contact by ID |
| `POST` | `/contacts` | Create a new contact |
| `PATCH` | `/contacts` | Update an existing contact |
| `DELETE` | `/contacts/{id}` | Delete a contact by ID |

### Example Requests

#### Create Contact

```http
POST /contacts
Content-Type: application/json

{
    "accountId": "9e788fcc-0119-f111-8341-000d3a5b1564",
    "firstName": "John",
    "lastName": "Doe",
    "emailAddress": "john.doe@example.com",
    "gender": 1,
    "mobilePhone": "+447821201204",
    "address1Line1": "123 Main Street",
    "address1City": "London",
    "address1Country": "England",
    "address1Type": 3
}
```

#### Get Contact

```http
GET /contacts/9e788fcc-0119-f111-8341-000d3a5b1564
```

#### Update Contact (Partial)

```http
PATCH /contacts
Content-Type: application/json

{
    "contactId": "9e788fcc-0119-f111-8341-000d3a5b1564",
    "firstName": "Jane",
    "mobilePhone": "+447821999999"
}
```

#### Delete Contact

```http
DELETE /contacts/9e788fcc-0119-f111-8341-000d3a5b1564
```

## Architecture

```
DataverseAPI/
├── Configuration/
│   └── OpenApiConfigurationOptions.cs    # Swagger/OpenAPI branding
├── Functions/
│   └── ContactFunction.cs                # HTTP trigger endpoints
├── Models/
│   ├── DataverseSettings.cs              # Configuration model
│   ├── ErrorResponse.cs                  # Standard error response
│   └── ContactModels/
│       ├── CreateContactRequest.cs
│       ├── CreateContactResponse.cs
│       ├── GetContactResponse.cs
│       ├── UpdateContactRequest.cs
│       ├── UpdateContactResponse.cs
│       └── DeleteContactResponse.cs
├── Services/
│   └── Contacts/
│       ├── IContactDataverseService.cs   # Service interface
│       └── ContactDataverseService.cs    # Dataverse SDK operations
├── Program.cs                            # Application entry point & DI
├── host.json                             # Azure Functions host configuration
└── local.settings.json                   # Local development settings
```

### How It Works

1. **HTTP Request** → Azure Function receives the request
2. **Function Layer** → Validates input, handles errors, returns responses
3. **Service Layer** → Business logic and Dataverse SDK operations
4. **Dataverse** → Data is persisted in Microsoft Dataverse

### Key Components

| Component | Purpose |
|-----------|---------|
| `ContactFunction` | HTTP endpoints with OpenAPI decorators |
| `IContactDataverseService` | Service abstraction for testability |
| `ContactDataverseService` | Dataverse SDK integration using `ServiceClient` |
| `OpenApiConfigurationOptions` | Swagger UI branding and metadata |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Access to a Microsoft Dataverse environment as System Administrator. If you do not have one, sign up via the (Power Apps Developer Plan)[https://learn.microsoft.com/en-us/power-platform/developer/plan] providing you have an Azure tenant and a configured user in said tenant.

### Installation

1. **Clone the repository**

   ```bash
   git clone https://dev.azure.com/ethanbalgobinpersonal/Azure%20Functions/_git/Contact%20Function
   cd ContactFunction
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure local settings**

   Create or update `local.settings.json`:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "DataverseSettings:DataverseUrl": "https://yourorg.crm.dynamics.com/",
       "DataverseSettings:ClientId": "your-client-id",
       "DataverseSettings:ClientSecret": "your-client-secret",
       "DataverseSettings:TenantId": "your-tenant-id",
       "OpenApi__AuthLevel__Document": "Anonymous",
       "OpenApi__AuthLevel__UI": "Anonymous"
     }
   }
   ```

4. **Run locally**

   ```bash
   func start
   ```

   Or press `F5` in Visual Studio.

5. **Access Swagger UI**

   Navigate to: `http://localhost:7071/swagger/ui`

## Configuration

### Application Settings

| Setting | Description |
|---------|-------------|
| `DataverseSettings:DataverseUrl` | Your Dataverse environment URL |
| `DataverseSettings:ClientId` | Azure AD App Registration Client ID |
| `DataverseSettings:ClientSecret` | Azure AD App Registration Client Secret |
| `DataverseSettings:TenantId` | Azure AD Tenant ID |
| `OpenApi__AuthLevel__Document` | Auth level for swagger.json (`Anonymous` recommended) |
| `OpenApi__AuthLevel__UI` | Auth level for Swagger UI (`Anonymous` recommended) |

### Dataverse App Registration

1. Register an application in Azure AD
2. Grant **Dataverse API** permissions (`user_impersonation`)
3. Create a client secret
4. Add the application user to your Dataverse environment with appropriate security roles

## Deployment

This API is deployed as an **Azure Function App** with the following infrastructure:

| Resource | Purpose |
|----------|---------|
| Azure Function App | Hosts the API (Consumption or Premium plan) |
| Application Insights | Monitoring, logging, and diagnostics |
| Azure Key Vault | Secure storage for secrets (recommended) |

### Deploy via Azure DevOps

The repository includes CI/CD pipelines for automated deployment to Azure.

### Deploy via CLI

```bash
# Build the project
dotnet publish -c Release -o ./publish

# Deploy to Azure
func azure functionapp publish <your-function-app-name>
```

### Azure Portal Configuration

After deployment, configure the following **Application Settings** in the Azure Portal:

- `DataverseSettings:DataverseUrl`
- `DataverseSettings:ClientId`
- `DataverseSettings:ClientSecret`
- `DataverseSettings:TenantId`
- `OpenApi__AuthLevel__Document` = `Anonymous`
- `OpenApi__AuthLevel__UI` = `Anonymous`

## Contributing

This repository is **closed** for contributions as it interacts with a Dataverse environment and consumes well monitored Azure resources.

### Adding New Entities

To add support for a new Dataverse entity:

1. Create models in `Models/{EntityName}Models/`
2. Create service interface and implementation in `Services/{EntityName}/`
3. Create function endpoints in `Functions/{EntityName}Function.cs`
4. Register the service in `Program.cs`

## License

This project is proprietary and confidential.

## Contact

**Ethan Balgobin**  
📧 ethanbalgo@hotmail.com  
🔗 [https://www.ethanbalgobin.com](https://www.ethanbalgobin.com)