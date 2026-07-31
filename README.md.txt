# ABC Construction Website - Project Documentation

## 1. Project Overview

### Project Name

ABC Construction Website

### Project Type

Full-stack responsive web application for a construction company.

### Technology Stack

Backend:

* ASP.NET Core Web Application (.NET)
* C#
* Entity Framework Core
* PostgreSQL Database
* ASP.NET Core Identity
* REST API architecture

Frontend:

* ASP.NET Core MVC Views
* Razor Views
* HTML5
* CSS3
* JavaScript
* Responsive design for desktop, tablet, and mobile devices

Development Environment:

* Visual Studio Community 2022

---

# 2. Main Goal

Create a professional construction company website that represents ABC Construction online.

The website must not look like generic AI-generated content.

The design must feel like a real premium construction company website.

The UI/UX Pro Max skill must be used during frontend development.

Prioritize:

* Professional visual hierarchy
* Strong typography
* Clear navigation
* Real-world construction industry aesthetics
* High-quality spacing
* Responsive layouts
* Accessibility
* User-friendly interactions

---

# 3. Design Requirements

## Color Theme

The entire website must follow this color palette:

Primary:

* Construction Yellow

Secondary:

* Black

Supporting:

* White

The design should communicate:

* Strength
* Reliability
* Engineering quality
* Professional construction branding

Avoid:

* Random gradients
* Generic AI landing page patterns
* Excessive animations
* Template-like layouts

---

# 4. Website Pages

## Public Pages

### Home Page

Route:

```
/
```

The landing page is the most important page.

Requirements:

* Strong hero section
* Company introduction
* Professional construction imagery
* Clear call-to-action buttons
* Featured projects
* Company statistics
* Services overview
* Contact section

The first impression should feel like a premium construction company website.

---

## Projects Page

Route:

```
/projects
```

Purpose:

Display completed construction projects.

Features:

* Project cards
* Project images
* Project name
* Description
* Completion duration
* Completion date
* Construction category
* Expandable project details

Example:

```
Commercial Building Renovation

Duration:
8 months

Details:
Full renovation of a commercial property including structural improvements.
```

---

## Individual Project Details

Route:

```
/projects/{id}
```

Requirements:

Display:

* Full project description
* Gallery images
* Timeline
* Duration
* Technologies/materials used
* Project challenges
* Completion information

---

## Employees Page

Route:

```
/employees
```

Purpose:

Display company employees.

Information:

* Full name
* Position
* Biography
* Profile image

Examples:

* Project Manager
* Architect
* Engineer
* Construction Worker

---

## Contact Page

Route:

```
/contact
```

Contains:

Email:

```
unwritten
```

Phone:

```
unwritten
```

Address:

```
unwritten
```

These values must be editable later through the admin panel.

---

# 5. Admin Panel

## Requirement

The website must include a secure administration system.

Multiple users must be able to have administrator access.

---

## Authentication

Use:

ASP.NET Core Identity

Features:

* Login
* Logout
* Password hashing
* User management
* Role-based authorization

---

## Roles

Minimum roles:

### Admin

Can:

* Create projects
* Edit projects
* Delete projects
* Manage employees
* Modify company information
* Change contact information
* Upload images

### Future Roles

Prepare architecture so additional roles can be added later.

---

# 6. Admin Panel Routes

Example:

```
/admin
```

Dashboard:

```
/admin/dashboard
```

Projects:

```
/admin/projects
```

Create project:

```
/admin/projects/create
```

Edit project:

```
/admin/projects/edit/{id}
```

Employees:

```
/admin/employees
```

Company settings:

```
/admin/settings
```

---

# 7. Required Architecture

The application must strictly follow this flow:

```
Database
    ↓
Application DbContext
    ↓
Repositories
    ↓
Services
    ↓
Controllers
    ↓
Views/API
```

Do not directly access database from controllers.

---

# 8. Database

Database Engine:

PostgreSQL

ORM:

Entity Framework Core

---

# 9. Database Entities

## Project Entity

Fields:

```
Id
Title
Description
ShortDescription
Duration
CompletionDate
Category
ImageUrl
CreatedDate
UpdatedDate
IsActive
```

---

## Employee Entity

Fields:

```
Id
FullName
Position
Biography
ImageUrl
CreatedDate
```

---

## CompanyInformation Entity

Fields:

```
Id
Email
PhoneNumber
Address
Description
```

Initial values:

```
Email = unwritten
PhoneNumber = unwritten
Address = unwritten
```

---

## ProjectImage Entity

Fields:

```
Id
ProjectId
ImageUrl
```

Relationship:

One Project → Many Images

---

# 10. Application DbContext

Create:

```
ApplicationDbContext
```

Responsibilities:

* Database configuration
* Entity relationships
* Entity mappings
* Identity integration

---

# 11. Repository Layer

Every entity must have:

Interface:

Example:

```
IProjectRepository
```

Implementation:

```
ProjectRepository
```

Responsibilities:

* Database queries
* CRUD operations
* Filtering
* Pagination

---

Example:

```
IProjectRepository
        |
        |
ProjectRepository
```

---

# 12. Service Layer

Every business operation must happen here.

Examples:

```
IProjectService

ProjectService
```

Responsibilities:

* Business logic
* Validation
* DTO mapping
* Cache handling

Controllers should never contain business logic.

---

# 13. Controllers

Controllers receive requests and communicate with services through Dependency Injection.

Example:

```
ProjectController

depends on:

IProjectService
```

---

# 14. API Endpoints

All APIs must follow REST conventions.

---

## Projects

GET:

```
/api/projects
```

Get all projects.

---

GET:

```
/api/projects/{id}
```

Get specific project.

---

POST:

```
/api/projects
```

Create project.

Admin only.

---

PUT:

```
/api/projects/{id}
```

Update project.

Admin only.

---

DELETE:

```
/api/projects/{id}
```

Delete project.

Admin only.

---

## Employees

GET:

```
/api/employees
```

POST:

```
/api/employees
```

PUT:

```
/api/employees/{id}
```

DELETE:

```
/api/employees/{id}
```

---

## Company Information

GET:

```
/api/company
```

PUT:

```
/api/company
```

Admin only.

---

# 15. View Models

Do not use database entities directly inside Views.

Create separate ViewModels.

Example:

Database:

```
Project
```

View:

```
ProjectViewModel
```

---

Examples:

```
ProjectViewModel

EmployeeViewModel

ContactViewModel

AdminDashboardViewModel
```

---

# 16. Frontend Requirements

The frontend must be:

* Mobile responsive
* Laptop compatible
* Tablet compatible

Required:

* Mobile navigation menu
* Responsive project cards
* Responsive images
* Touch-friendly buttons
* Good loading performance

---

# 17. Caching

The application must implement caching.

Use:

ASP.NET Core Distributed Cache or Memory Cache.

Cache:

* Projects list
* Employee list
* Company information

Example:

```
GET /projects
```

should use cache.

When admins update data:

Invalidate related cache.

Example:

Admin edits project:

```
Update database

↓

Remove project cache

↓

New data is loaded
```

---

# 18. API Rate Limiting

The application must include request limits.

Purpose:

Prevent:

* Server overload
* Abuse
* Too many requests

Implement:

ASP.NET Core Rate Limiting Middleware.

Example:

Anonymous users:

```
100 requests per minute
```

Authenticated admins:

```
300 requests per minute
```

---

# 19. Security Requirements

Implement:

* Authentication
* Authorization
* Input validation
* Anti-forgery protection
* Secure password storage
* File upload validation
* Protection against SQL injection
* Proper exception handling

---

# 20. File Upload Requirements

Admins should be able to upload:

* Project images
* Employee images

Validate:

* File type
* File size
* Extension

Allowed:

```
.jpg
.jpeg
.png
.webp
```

---

# 21. Suggested Folder Structure

```
ABCConstruction

├── Controllers

├── Data
│   └── ApplicationDbContext.cs

├── Models

├── ViewModels

├── Repositories
│
├── Interfaces

├── Services

├── Views

├── wwwroot
│
│── css
│── js
│── images

├── Middleware

├── Migrations

└── Program.cs
```

---

# 22. Logging

Implement application logging.

Track:

* Errors
* Admin actions
* Failed requests

---

# 23. Performance Requirements

Optimize:

* Database queries
* Image loading
* API responses
* Static files

Use:

* Pagination
* Caching
* Lazy loading where appropriate

---

# 24. Development Instructions for Claude Code

Before writing code:

1. Analyze the architecture.
2. Follow the repository-service-controller pattern.
3. Use dependency injection everywhere.
4. Use UI/UX Pro Max skill.
5. Create professional UI.
6. Avoid AI-generated looking designs.
7. Write clean maintainable code.
8. Explain major architectural decisions.
9. Build features incrementally.

---

# Final Requirement

The finished application should look and behave like a professional commercial construction company website, not a generated demo.

The priority order is:

1. Professional UX/UI
2. Clean architecture
3. Secure backend
4. Admin flexibility
5. Performance
6. Mobile responsiveness
