# AuthNest

## A fully functional .Net project that is more focused on security and mitigation of some attacks such as Dos, brute force attacks while ensuring balance between smooth consumer experience and security.

This project is a real-world API simulation that focuses on secure user onboarding, account lifecycle management, and robust API governance. It includes a complete authentication and verification workflow to ensure security and reliability.

The API allows users to register, after which an email containing a **verification code** is sent. The verification process validates the submitted code against the one stored in the database. Upon successful verification, the system automatically assigns a unique login number and activates the user’s account.

To maintain system integrity, an admin must create at least one role before any registration attempt. If a user tries to register without any existing roles, the API throws an exception indicating that a role must be configured first.

Users who do not receive their verification code can request a new one, but requests are rate-limited to three attempts per day to prevent abuse. The API also provides secure features for **password updates**, **password resets**, **account deactivation**, and **account deletion**, **Login number request if forgotten**.

The project includes **API versioning**, advanced **Rate Limiting** mechanisms such as **Sliding Window and Token Bucket Policies**, and a full **JWT authentication** system with both **access and refresh tokens** for improved user experience.

For monitoring and observability, the system relies on **Serilog, with Seq and Console sinks** to provide structured logs, performance insights, and debugging support.

---

## Table of Contents

- [About](#about)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Project Structure](#project-structure)
- [.Env Configuration](#env-configuration)
- [Clone And Setup](#clone-and-setup)
- [Install Dependencies](#restore-dependencies)
- [Create And Apply Migrations](#create-and-apply-migrations)
- [Run Project](#run-the-project)
- [Run Test](#run-test)

---

## About

This project is a RESTful API built in .NET that provides authentication, secure data access, and CRUD endpoints. It follows clean architecture principles and includes JWT authentication, logging, and error handling.

---

## Tech Stack

#### Language and Runtime
- **C#**
- **.NET 8**

#### Database and ORM
- **Entity Framework Core**  
- **MySQL**

#### Authentication and Security
- **JWT Authentication**    
- **Swagger**
- **Rate limiting***
- **API Versioning**
- **Cookies**
- **Anti-Forgery Token**
- **Role Based Access Control (RBAC)**

#### API Features
- **Swagger**
- **API Versioning**
  
#### Logging
- **Serilog**

---

## Features

#### User and Admin Management
- Admin can register a role
- User can register, Login, Logout
- User can Delete, Retrieve, Disable and Enable account

#### Authentication and Security
- Jwt Token Blacklisting on logout
- Temporary Password Generation and Cleanup
- Anti-Forgery Token ( Yet to implement)
- Rate limiting

#### Background Services
- Automatic Email Sending
- Scheduled Cleanup of Expired/Used codes
- Scheduled Cleanup for Expired/Used Temporary Passwords

#### Databases and Infrastructure
- Entity Framework Integration
- MySql databases

#### Architecture and Maintainability
- Clean Architecture
- Centralized logging and error handling
- Configurable using environmental variables (`.env`)

---

## Project Structure

```txt
root/
│── src/
│   ├── BackgroundTask/
│   ├── Configurations/
│   ├── Controllers/
│   ├── Database/
│   ├── DTOs/
│   ├── Enums/
│   ├── Exceptions/
│   ├── Interfaces/
│   ├── Models/
│   ├── RegisterServices/
│   ├── Repositories/
│   ├── Security/
│   ├── Services/
│   ├── Templates/
│   ├── Utilities/
│   ├── .env
│   └── Program.cs
│
│── tests/
│── .env
│── .gitignore
│── README.md
│── unit_test.yml

```
---

## .Env Configuration

`.env` file contains sensitive environment variables required by the API.
Below is the structure you should follow:

```txt
#---DATABASE---
DB_PASSWORD=yourpassword

#---JWT---
# Must be at least 32 bytes for strong HMACSHA256 security
JWT_KEY=your-super-secretive-key(atleast 32 bytes)

#---AES---
# Must be EXACTLY 16 bytes for AES-128 encryption
AES_KEY=your-super-secretive-key(16 bytes)

#---EMAIL---
EMAIL_PASSWORD=yourgmailpassword-notpersonalpassword
FROM_EMAIL=youremail

```
---

## Clone And Setup

#### Clone the repository
```bash
git clone https://github.com/Jimmy-commits-hue/AuthNest.git
```

#### Navigate into the project
```bash
cd AuthNest
```
---

## Restore Dependencies
#### Restoring both project dependencies
```bash
- dotnet restore AuthNest/AuthApiBackend/AuthApiBackend.sln
```

#---Note--
- This will restore dependencies in both project (API project and API tests project)

#### Restore only API Project dependencies
```bash
- dotnet restore AuthNest/AuthApiBackend/AuthApiBackend.csproj
```

#---Note---
- This will restore only dependencies for API project
  
---

## Create And Apply Migrations
```bash
- dotnet ef migrations add "Initial"
- dotnet ef database update
```

---

## Run the Project
```bash
- dotnet run
```

#### Ports
- To run the project on different ports, please change "ASPNETCORE_URLS" environmental variables in launchsettings.json file.
- #### Default Ports
```json
  - ASPNETCORE_URLS: "https://localhost:7123;http://localhost:5267"
```

---

## Run Test
```bash
- dotnet test
```

---
