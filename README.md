# AuthNest

## A fully functional .Net project that is more focused on security and mitigation of some attacks such as Dos, brute force attacks while ensuring balance between smooth consumer experience and security.

This project is a real-world API simulation that focuses on secure user onboarding, account lifecycle management, and robust API governance. It includes a complete authentication and verification workflow to ensure security and reliability.

The API allows users to register, after which an email containing a **verification code** is sent. The verification process validates the submitted code against the one stored in the database. Upon successful verification, the system automatically assigns a unique login number and activates the user’s account.

To maintain system integrity, an admin must create at least one role before any registration attempt. If a user tries to register without any existing roles, the API throws an exception indicating that a role must be configured first.

Users who do not receive their verification code can request a new one, but requests are rate-limited to three attempts per day to prevent abuse. The API also provides secure features for **password updates**, **password resets**, **account deactivation**, and **account deletion**, **Login number request if forgotten**.

The project includes **API versioning**, advanced **Rate Limiting** mechanisms such as **Sliding Window and Token Bucket Policies**, and a full **JWT authentication** system with both **access and refresh tokens** for improved user experience.

For monitoring and observability, the system relies on **Serilog, with Seq and Console sinks** to provide structured logs, performance insights, and debugging support.

---
Table of Contents

[About](#about)

[Tech Stack](#tech-stack)

[Features](#features)

[Project Structure](#project-structure)

[Environment Variable](#enviromental-variables)

[Clone the repository](#clone-the-repository)

[Running Inside Docker](#running-inside-docker)

[Running Outside Docker](#running-outside-docker)

[Testing](#run-test)

[Contact](#contact)

---

## About

A RESTful API built in .NET providing authentication, secure data access, and CRUD endpoints. Follows clean architecture principles with JWT authentication, logging, and error handling.

---
## Tech Stack

### Language and Runtime

- C#

- .NET 8

- Database and ORM

- Entity Framework Core

- MySQL


### Authentication & Security

- JWT Authentication (Access & Refresh Tokens)

- Rate limiting (Sliding Window & Token Bucket)

- Role-Based Access Control (RBAC)

- Anti-Forgery Tokens

- Cookies

### API Features

- Swagger

- API Versioning

- Logging

- Serilog (Console & Seq)

### Testing

- xUnit & Moq

- Postman

### Others

- MailKit

- RazorLight

- Docker

- Documentation (**Coming soon**)

## Features

### User and Admin Management

- Admin can register roles

- User registration, login, logout

- Account deletion, retrieval, disable/enable


### Authentication & Security

- JWT Token blacklisting on logout

- Temporary password generation & cleanup

- Anti-Forgery Token (planned)

- Rate limiting

- Background Services

- Automatic email sending

- Scheduled cleanup of expired/used codes & temporary passwords


### Database & Infrastructure

- EF Core integration

- MySQL databases

- Architecture & Maintainability

- Clean architecture

- Centralized logging & error handling

- Configurable using (**.env**)

## Project Structure
```txt
AuthNest/
│── .env                     # Root environment variables (DB_PASSWORD for Docker)
│── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
│
│── 
│   ├── AuthApiBackend/
│   │   ├── .env              # API-specific environment variables (JWT, AES, Email, etc.)
│   │   ├── BackgroundTask/
│   │   ├── Configurations/
│   │   ├── Controllers/
│   │   ├── Database/
│   │   ├── DTOs/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── RegisterServices/
│   │   ├── Repositories/
│   │   ├── Security/
│   │   ├── Services/
│   │   ├── Templates/
│   │   ├── Utilities/
│   │   └── Program.cs
│   │
│   └── AuthApiBackendTest/
│       ├── TestHelpers/
│       ├── UnitTests/
│       └── AuthApiBackendTest.csproj
│
│── .gitignore
│── README.md
│── unit_test.yml

```
---

## Enviromental Variables

- .env in root: mainly for Docker
- Location: AuthNest/.env
  
```txt
DB_PASSWORD=yourdbpassword
```

- .env in AuthApiBackend/: contains API-specific sensitive keys (JWT_KEY, AES_KEY, EMAIL_PASSWORD, FROM_EMAIL).
- Location: AuthNest/AuthApiBackend/.env
  
```txt
JWT_KEY=your-super-secretive-key(atleast 32 bytes)
AES_KEY=your-super-secretive-key(16 bytes)
FROM_EMAIL=youremail
EMAIL_PASSWORD=yourinappgmailpassword-not-your-true-personal-password
```

- Docker files are under docker/ for easier maintenance.

---

## Clone the Repository

```bash
git clone https://github.com/Jimmy-commits-hue/AuthNest.git
cd AuthNest
```

---

## Running Outside Docker

### Prerequisites
- [Visual Studio 2022 or later: Download](https://visualstudio.microsoft.com/downloads/)

- .NET 8 SDK installed

### Environment Variable

#### **Create .env files:**
- [See-> AuthNest\AuthApiBackend\.env](#enviromental-variables)


## Restore Dependencies

- Navigate to **..\AuthNest**
  
### All projects:

```terminal
dotnet restore AuthApiBackend.sln
```
---

### API project only:

```terminal
dotnet restore AuthApiBackend/AuthApiBackend.csproj
```
---

### Run API

```terminal
dotnet run --project AuthApiBackend/AuthApiBackend.csproj
```
---

### Default ports in launchsettings.json:

```json
"ASPNETCORE_URLS": "https://localhost:7123;http://localhost:5267"
```
---

## Running Inside Docker

- Install Docker Desktop: [Download](https://docs.docker.com/desktop/)

- Check if docker was installed successfully
  
  ```powershell
   docker --version
  ```
  
- Ensure WSL installed on Windows:
  
```powershell
wsl --version
```

- If not installed:
  
```powershell
wsl install
```

- Start docker-desktop
  
- Navigate to project root:
  
```terminal
cd AuthNest
```

- Create .env in root (AuthNest\.env) and in src\AuthApiBackend\.env (JWT, AES, Email). [Click me](#enviromental-variables)

- Build and start containers in detached mode:
```terminal
docker compose --env-file .env up -d --build
```

- Check running containers:
  
```terminal
docker ps
```

- Stop containers:
  
```terminal
docker compose down
```

- Ports
  
  ```Ports
     ASPNETCORE_HTTP=5000 (http://localhost:5000/swagger/index.html)
     ASPNETCORE_HTTPS=5001 (https://localhost:5001/swagger/index.html)
  ```
  
- For more docker commands, [click me](https://docs.docker.com/get-started/docker_cheatsheet.pdf)
  
--- 

## Run Test

---
Prerequisite
- Create .env file (AuthNest\AuthApiBackend\.env): [See this](#enviromental-variables)
  
```terminal
cd AuthNest
dotnet test AuthApiBackendTest/AuthApiBackendTest.csproj
```

---

## Contact

- Author: Khabana Jabulani Jimmy
- [Email Me](jabulanikhabana0@gmail.com)
 
---
