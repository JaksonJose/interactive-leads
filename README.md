# 🚀 Interactive Leads

**Interactive Leads** is a **multi-tenant SaaS core platform** designed for scalable business applications — combining **lead management, sales pipelines, scheduling, chat, analytics, and inventory modules**.  
It provides a solid foundation that can be **extended to any business niche** through parametrization and tenant-level customization.

---

## 🧭 Overview

This project serves as a **core CRM and operations management system**, offering the flexibility to support multiple industries and configurations.  
It’s built as a **modular and extensible SaaS foundation**, enabling:

- Multi-company and multi-user environments (Owner, Manager, Agent)  
- Centralized control of leads, chats, pipelines, and appointments  
- Generic inventory management applicable across business niches  
- Dynamic configuration and parameterization per tenant  
- Future AI integration for smart assistance and data insights  

> The long-term goal is to use this core as the backbone for vertical SaaS solutions or custom enterprise implementations.

---

## 🧠 Core Features

- **Multi-tenant architecture** with isolated data per company  
- **Lead management** and customer communication hub  
- **Sales pipeline** with dynamic stages and automation  
- **Scheduling** for tasks and appointments  
- **Chat system** (future integration with WhatsApp and Instagram)  
- **Dashboards, charts, and analytics** for business insights  
- **Generic inventory management** — adaptable to any business domain  
- **Extensible parametrization** for tenant-level customization  
- **AI-ready design** with planned RAG + Semantic Kernel integration  

---

## ⚙️ Tech Stack

| Layer | Technologies |
|-------|---------------|
| **Backend** | .NET 8 Web API, Entity Framework Core, PostgreSQL |
| **Caching / Messaging** | Redis, RabbitMQ |
| **Frontend (Main)** | Angular 18 |
| **Infrastructure** | Docker & Docker Compose |
| **AI / Agentic SDK (Planned)** | Semantic Kernel |
| **Testing & Quality** | xUnit, FluentAssertions, MediatR for CQRS pattern |

---

## 🧱 Core Architecture

The system is based on a **clean, modular, and scalable architecture**:

- **Clean Architecture / DDD-inspired** domain structure  
- **CQRS + MediatR** pattern for request handling  
- **Tenant isolation** handled at the database and application levels  
- **Redis** for distributed caching and performance  
- **RabbitMQ** for event-driven messaging and background workflows  
- **Angular** frontends consuming the same RESTful API  
- **Dockerized deployment** for simplified orchestration and scaling  

### 🧩 Modular Design
- `Core` → Domain and infrastructure abstractions  
- `Backend` → REST API + Background Services  
- `Frontends` → Angular
- `Shared` → DTOs and utilities across services  
- `Docker` → Environment setup for local and production builds  

---

## 📁 Project Structure

```bash
interactive-leads/
│
├── backend/                  # .NET 8 Web API + Background Services
│   ├── src/
│   ├── tests/
│   ├── Dockerfile
│   └── README.md
│
├── frontend-angular/          # Angular app (main frontend)
│   ├── src/
│   └── Dockerfile
│
├── shared/                    # Shared DTOs, utilities, models
│
├── docker/                    # Compose and environment setup
│   ├── docker-compose.yml
│   └── README.md
│
└── README.md                  # Main documentation



