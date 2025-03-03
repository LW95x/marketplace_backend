## Hosted Version

https://u2umarketplace-api.azurewebsites.net

## Project Overview
I designed this comprehensive **ASP.NET Core Web API** to simulate a user-to-user marketplace like eBay, where users can seamlessly buy and sell products. This project was built to handle complex, multifaceted entity interactions, including many-to-many relationships and cascading dependencies, providing me with a deeper understanding of interconnected database structures.

This API adheres strongly to **SOLID principles**, and incorporates many industry-standard design patterns to guarantee a **production-grade architecture**. It follows the **Controller-Service-Repository pattern** to enforce **Separation of Concerns**, with repositories abstracting data access, services encapsulating all business logic, and controllers managing API endpoints. It also enforces the **Unit of Work pattern**, ensuring database transactions are handled efficiently and safely, while **Data Transfer Objects (DTOs)** are extensively used for both request and response objects, preventing direct database access. **Dependency Injection** is leveraged throughout the application, reducing tight coupling and vastly improving testability.

By implementing these architectural best practices, this API is designed to be **scalable and maintainable**, with over 130 unit tests ensuring a **high level of reliability**. API Security is enforced through **JWT Authentication**, while **Docker containerisation** and an **automated GitHub Actions CI/CD pipeline** ensure a **streamlined deployment process** to an Azure-hosted App Service and SQL Server database.


## Key Features
- **Architecture-Led Design:** Implements SOLID principles, RESTful API design, and key architectural patterns to ensure scalability and maintainability.
- **Cloud Deployment & Security:** Dockerised and hosted on an Azure App Service, with secrets managed through the Azure Key Vault, and utilising JWT Bearer Token authentication for secure API access.
- **Automated CI/CD Pipeline:** GitHub Actions handles building, testing, and deploying to an Azure Container Registry (ACR) on each push and pull request.
- **Robust Codebase:** Built with an extensive xUnit testing suite comprised of over 130 unit tests, utilising Moq, AutoFixture, and a test data factory.
- **Thorough API Documentation:** Integrated with OpenAPI (Swagger), with detailed endpoint explanations and amended with response type annotations.

## Tech Stack

- **Backend:** C#, ASP.NET Core Web API, Entity Framework Core, SQL Server, Azure Identity, AutoMapper, JWT, OpenAPI (Swagger)
- **Hosting & Deployment:** Azure App Service, Azure SQL, Azure Key Vault
- **Cloud & DevOps:** Docker, Azure Container Registry, GitHub Actions (CI/CD)
- **Testing:** xUnit, Moq, AutoFixture
  


