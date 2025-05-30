# InvoiceServiceProvider

**InvoiceServiceProvider** is an ASP.NET Core microservice responsible for invoice management in a distributed system. It exposes both gRPC and REST APIs, uses Entity Framework Core for data persistence, and integrates with other services via Azure Service Bus.

---

## Table of Contents

- [Overview](#overview)  
- [Features](#features)  
- [Technologies Used](#technologies-used)  
- [Architecture Overview](#architecture-overview)  
- [Prerequisites](#prerequisites)  
- [Sequence Diagrams](#sequence-diagrams)  
- [Documentation](#documentation)  
- [Testing](#testing)  

---

## Overview

This service provides full CRUD operations and status management for invoices within an event/booking ecosystem. Operations include creating, retrieving (all or by filters), updating, deleting, and changing invoice status.

## Features

- **CreateInvoiceAsync**: Generate a new invoice using booking, event, and account data.  
- **GetAllAsync**: Retrieve all invoices.  
- **GetByIdAsync**: Retrieve a single invoice by its ID.  
- **GetByStatusIdAsync**: Retrieve invoices filtered by status.  
- **UpdateAsync**: Update an existing invoice.  
- **ChangeStatusAsync**: Change invoice status to Sent, Held, Paid or Unpaid.  
- **gRPC interface**: Defined in `invoice.proto` as `InvoiceManager`.  
- **REST API**: Implemented in `InvoicesController` with Swagger/OpenAPI support.  
- **Azure Service Bus**: Publishes messages to notify BookingService after invoice creation.  
- **PlantUML**: Sequence diagrams for core service methods.  
- **XUnit**: Unit tests for `InvoiceService`.

## Technologies Used

- **.NET 9 / ASP.NET Core**  
- **C# 12**  
- **Entity Framework Core** (SQL Server)  
- **gRPC** (Grpc.Net.Client & Grpc.Net.Server)  
- **Azure Service Bus**  
- **Swashbuckle / Swagger**  
- **XUnit**  
- **PlantUML**

## Architecture Overview

[Client]
   ↓  REST / gRPC
[InvoiceService]  
   ↓  Business Layer (IInvoiceService)  
[Data Layer (EF Core)]  
   ↓  
[InvoiceRepository] → [SQL Server]

InvoiceService also publishes messages to Azure Service Bus for asynchronous integration with BookingService.

## Prerequisites

- .NET SDK 9.0 or later
- SQL Server (local or Azure)
- Azure Service Bus namespace & access key

## Sequence Diagrams

### CreateInvoiceAsync
[CreateInvoiceAsync](https://github.com/user-attachments/assets/2200e51f-cd39-4f1a-9598-b90382010231)

### GetAllAsync
![GetAllAsync](https://github.com/user-attachments/assets/7d20fada-b91d-408b-8999-06c39a0df37d)

### GetByIdAsync
![GetByIdAsync](https://github.com/user-attachments/assets/d60561d4-2093-4215-afcb-7425e17ca9d5)

### GetByStatusIdAsync
![GetByStatusIdAsync](https://github.com/user-attachments/assets/54409d88-894e-41a4-8cca-67fe6c3e5d5f)

### UpdateAsync
![UpdateAsync](https://github.com/user-attachments/assets/b32e6156-917a-4537-93c8-f77a48f4e474)

### ChangeStatusAsync
![ChangeStatusAsync](https://github.com/user-attachments/assets/0a6275bc-a2c3-448e-85db-dc3356141753)

## Documentation

### Swagger UI

Open in browser:  
`https://localhost:<port>/swagger`

## Testing

Testet with xUnit testing.
