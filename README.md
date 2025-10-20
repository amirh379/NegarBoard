🚀 Dynamic Data Engine (Full‑Stack)

A full‑stack project that provides a dynamic CRUD + schema management engine on top of SQL Server, with a modern frontend for interacting with it. This project allows you to:

Create tables dynamically with schema definitions (columns, types, constraints, relationships).

Insert, update, delete, and query data in a generic, schema‑driven way.

Manage relationships, indexes, and constraints via API requests.

Explore and interact with your data visually through the frontend.

📂 Project Structure

Code
/src
  /backend        -> ASP.NET Core Web API (Dynamic CRUD Engine)
  /frontend       -> 
  
⚙️ Backend Features

Dynamic Table Creation Define tables at runtime with JSON requests (RequestNewTableModel).

Column Management Add, drop, or alter columns dynamically.

Relationships Define foreign keys and cascade rules between tables.

CRUD Operations

Insert rows dynamically with parameterized queries.

Update and delete rows by Id.

Query tables and columns safely with metadata validation.

Metadata Endpoints

Get all table names.

Get detailed column metadata (type, length, nullability, PK, identity).

🛠️ Tech Stack

Backend: ASP.NET Core Web API + Dapper + SQL Server

Frontend: 

Database: SQL Server

Tooling: Swagger/OpenAPI for API docs, Docker for containerization

🚀 Getting Started

Prerequisites
.NET 9 SDK
SQL Server (local or Docker)

Tip: to use project you must create database and set connection string in appsetting 
