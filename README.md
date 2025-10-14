# 📋 Task Management System

A full-stack task management application built with .NET 8, Vue.js 3, and Docker.

In this project, I aimed to demonstrate as many technologies as I could. I hope it goes beyond expectations. Any feedback is welcome!

## 🛠️ Quick Start

### Running with Docker

1. **Clone and setup**: Clone this repository

2. **Execute with Docker:**

```bash
cd task-management
docker-compose up -d
```

3. **Access the application:**

   **Note**: Ichanged some default ports to avoide conflict

   - Frontend: http://localhost:3000
   - Backend API: http://localhost:7000
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - RabbitMQ: 5445
   - Postgres: 5444

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK (for local development)
- Node.js 18+ (for local development)

## 🚀 Features

- **Task Management**: Create, read, update, delete, and reorder tasks
- **User Authentication**: JWT-based authentication with role-based permissions
- **Real-time Notifications**:
  - RabbitMQ for task assignment notifications
  - SignalR for real-time frontend notifications
  - Live notifications: Toast notifications and notification center
- **Drag & Drop Interface**: Intuitive task organization across columns
- **Responsive Design**: Built with Tailwind CSS v4
- **Docker Support**: Complete containerization with Docker Compose

## 🏗️ Architecture

### Backend (.NET 8)

- **Framework**: ASP.NET Core 8 Web API
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Message Queue**: RabbitMQ for async notifications
- **Real-time Communication**: SignalR for WebSocket connections
- **Architecture**: Clean Architecture with Domain-Driven Design

### Frontend (Vue.js 3)

- **Framework**: Vue.js 3 with Composition API
- **Language**: TypeScript
- **State Management**: Pinia
- **Styling**: Tailwind CSS v4
- **Routing**: Vue Router
- **HTTP Client**: Axios with interceptors
- **Real-time Communication**: SignalR client for live updates

### Infrastructure

- **Containerization**: Docker & Docker Compose
- **Database**: PostgreSQL 18
- **Message Broker**: RabbitMQ 3.12
- **Web Server**: Nginx (frontend)

## 🔄 Real-time Notification System

### SignalR Integration

The application uses ASP.NET Core SignalR to provide real-time functionality:

### Notification Flow

- Task Assignment: When a task is assigned to a user
- RabbitMQ: Backend publishes message to RabbitMQ
- Message Consumer: Processes the message and saves to database
- SignalR: Sends real-time notification to specific user via WebSocket
- Frontend: Receives notification and shows toast instantly

### Features

- **Live Toast Notifications**: Instant popup notifications for new tasks
- **Notification Center**: Dedicated page to view all notifications
- **Real-time Updates**: Notifications appear without page refresh
- **User-specific**: Notifications are sent only to relevant users

Mark as Read: Users can mark notifications as read

### Notification Types

- **Task Assigned**: When a task is assigned to you
- **Task Updated**: When an assigned task is modified
- **Task Completed**: When a task is marked as completed
