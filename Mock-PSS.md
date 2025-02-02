# Passenger Service System (PSS) - System Design

This document outlines the design and implementation plan for the Passenger Service System (PSS) composed of microservices to be deployed on Kubernetes. Each service, its APIs, domain events, and technical stack are detailed below. This system is designed for scalability, maintainability, and modularity.

---

## **1. Services Overview**

### **1.1 Inventory Service**
- **Name:** `FlightInventory`
- **Functionality:**
  - Manage flight schedules and inventory (business and economy class).
  - APIs and events for creating schedules and updating inventory.
- **APIs:**
  - `POST /schedule` - Add a new flight schedule.
  - `GET /schedule` - Retrieve flight schedules (filterable by date, route, etc.).
  - `PUT /inventory` - Update inventory (e.g., seat availability).
  - `GET /inventory` - Check current inventory for a flight.
- **Events:**
  - `ScheduleCreated`
  - `InventoryUpdated`
- **Technology:**
  - **Language:** Python
  - **Database:** PostgreSQL

### **1.2 Booking Service**
- **Name:** `FlightBooking`
- **Functionality:**
  - Allow customers to search and book flights.
  - Support booking cancellation.
- **APIs:**
  - `GET /flights` - Search available flights.
  - `POST /bookings` - Book a flight.
  - `GET /bookings/{pnr}` - Retrieve booking details by PNR.
  - `DELETE /bookings/{pnr}` - Cancel a booking.
- **Events:**
  - `BookingCreated`
  - `BookingCancelled`
- **Technology:**
  - **Language:** Node.js
  - **Database:** MongoDB

### **1.3 Passenger Service**
- **Name:** `PassengerService`
- **Functionality:**
  - Centralized handling of passenger-related operations and validations.
  - Shared operations for OLCI and DCS apps.
- **APIs:**
  - `GET /passengers/{flightId}` - List passengers for a flight.
  - `GET /passenger/{pnr}` - Get passenger details by PNR.
  - `POST /checkin` - Check-in a passenger.
  - `POST /offload` - Offload a passenger.
  - `POST /board` - Board a passenger.
- **Events:**
  - `PassengerCheckedIn`
  - `PassengerOffloaded`
  - `PassengerBoarded`
- **Technology:**
  - **Language:** .NET 8
  - **Database:** PostgreSQL

### **1.4 OLCI (Online Check-In)**
- **Name:** `OLCI`
- **Functionality:**
  - Customer-facing backend with tailored APIs.
  - Relies on `PassengerService` for passenger operations.
- **APIs:**
  - `GET /checkin/{pnr}` - Retrieve booking details for check-in.
  - `POST /checkin` - Perform online check-in.
- **Technology:**
  - **Language:** .NET 8
  - **Database:** PostgreSQL (shared with PassengerService)

### **1.5 DCS (Departure Control System)**
- **Name:** `DCS`
- **Functionality:**
  - Agent-facing backend for airport check-in workflows.
  - Relies on `PassengerService` for passenger operations.
- **APIs:**
  - `GET /flights` - View flights with passenger details.
  - `POST /checkin/{passengerId}` - Check-in a passenger.
  - `POST /offload/{passengerId}` - Offload a passenger.
  - `POST /board/{passengerId}` - Board a passenger.
- **Technology:**
  - **Language:** .NET 8
  - **Database:** PostgreSQL (shared with PassengerService)

### **1.6 Airport Ops Service**
- **Name:** `FlightOps`
- **Functionality:**
  - Manage flight statuses (scheduled, departed, canceled).
- **APIs:**
  - `GET /flights` - List all flights with statuses.
  - `POST /flights/{flightId}/status` - Update flight status.
- **Events:**
  - `FlightStatusUpdated`
- **Technology:**
  - **Language:** Python
  - **Database:** PostgreSQL

### **1.7 Reporting and Analytics Service**
- **Name:** `FlightAnalytics`
- **Functionality:**
  - Handle events from RabbitMQ to generate reports and real-time analytics.
- **APIs:**
  - `GET /reports/{reportId}` - Retrieve a pre-generated report.
  - `GET /stats` - View real-time statistics.
- **Events Consumed:**
  - `ScheduleCreated`, `BookingCreated`, `PassengerCheckedIn`, `FlightStatusUpdated`, etc.
- **Technology:**
  - **Language:** Java
  - **Database:** Apache Cassandra

---

## **2. System Features**

### **2.1 OpenTelemetry Instrumentation**
- All services will implement OpenTelemetry for:
  - **Logging**
  - **Tracing**
  - **Metrics**
- OpenTelemetry Collector will aggregate telemetry data.
- Visualization tools:
  - **Jaeger** for tracing.
  - **Prometheus** and **Grafana** for metrics.
  - **Loki** for log aggregation.

### **2.2 RabbitMQ Deployment**
- RabbitMQ will be deployed in a container and used for event-driven communication between services.

### **2.3 API Documentation**
- All APIs will be documented using OpenAPI.
- Web-based documentation via Swagger UI or Redoc.

### **2.4 Web-Based UIs**
- Each service will include a lightweight, React.js-based web UI served via Nginx.

### **2.5 Simulation Scripts**
- Scripts will simulate operations like schedule creation, bookings, and check-ins.
- Used for load testing and auto-scaling validation in Kubernetes.

---

## **3. Development and Deployment Plan**

### **3.1 Development**
- Use Docker on Windows (WSL) for local development and testing.
- Develop APIs, domain events, and UIs for all services.

### **3.2 Deployment**
- Local deployment using Docker Compose.
- Gradual migration to Kubernetes (AWS EKS) for PoC.

---

## **4. Summary**
This system design provides a modular, scalable, and maintainable architecture for a Passenger Service System. It leverages modern technologies, microservices principles, and best practices for observability and event-driven communication. The planned PoC will validate the system's scalability and operational robustness on Kubernetes.

---

## URLs and Commands

### Change otel collector config
	after "docker compose up -d" copy the otel-collector-config.yaml file to the otel-config volume, and restart the otel-collector
	$ docker cp ./otel-collector-config.yaml otel-collector:/etc/otel-collector-config/otel-collector-config.yaml
	$ docker restart otel-collector

### Otel Collector metrics exposed on
    http://localhost:8889/metrics
	
### Jager UI
    http://localhost:16686
	
### Graphana
	http://localhost:3000
	username: admin
	password: admin

### Prometheus
	http://prometheus:9090 (from cluster)
	or
	http://localhost:9090 (from host)