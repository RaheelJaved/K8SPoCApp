# Next Steps for Implementing the Passenger Service System (PSS)

This document provides a step-by-step guide for implementing the Passenger Service System (PSS). Follow these steps to ensure a smooth development and deployment process.

---

## **Step 1: Set Up Development Environment**

### **1.1 Install Prerequisites**
1. **Development Tools:**
   - Docker Desktop (with WSL integration for Windows).
   - Visual Studio 2022 Community for .NET development.
   - Python (with package managers like `pip` or `pipenv`).
   - Node.js (for the Booking Service).
   - PostgreSQL and MongoDB (locally or in Docker containers).

2. **Version Control:**
   - Set up a Git repository to track your codebase.
   - Use the following folder structure:
     ```
     PSS/
     ├── passenger-service/  (Core Passenger Service)
     ├── flight-inventory/
     ├── flight-booking/
     ├── olci/  (Online Check-In)
     ├── dcs/  (Departure Control System)
     ├── flight-ops/
     ├── flight-analytics/
     ├── shared/  (shared libraries/configurations)
     └── docker-compose.yml
     ```
### **1.2 Observability and Deployment**

1. **OpenTelemetry Setup:**
   - Install the OpenTelemetry Collector in Docker.
   - Configure tracing, logging, and metrics collection.
   - While development each service, pay special attention to setup instrumentation at all important places, not just the default telemetry that comes with various packages.

2. **RabbitMQ Setup:**
   - Deploy RabbitMQ in a Docker container with management plugins enabled.

## **1.3: Dockerize Each Service**
1. Create **Dockerfiles** for each service.
2. Use **docker-compose.yml** to manage local deployment of all services, including RabbitMQ, PostgreSQL, and OpenTelemetry Collector.

---

## **Step 2: Start with Passenger Service**

### **2.1 Reason to Start Here**
The `PassengerService` provides core functionalities shared by other services. Implementing it first ensures other services can integrate easily later.

### **2.2 Tasks**
1. Create a new `.NET 8 Web API Project`.
2. Implement initial APIs:
   - `GET /passengers/{flightId}` - List passengers for a flight.
   - `GET /passenger/{pnr}` - Get passenger details by PNR.
   - `POST /checkin` - Check-in a passenger.
   - `POST /offload` - Offload a passenger.
   - `POST /board` - Board a passenger.
3. Integrate PostgreSQL for passenger data storage.
4. Add OpenTelemetry instrumentation for logging, tracing, and metrics.
5. Publish domain events (e.g., `PassengerCheckedIn`) to RabbitMQ.
6. Write unit and integration tests to validate functionality.

---
## **Step 3: Develop Flight Related Services**

### **3.1 Flight Inventory Service**
- Implement scheduling and inventory management APIs. After a light is scehduled and has inventory, it will be available for booking.
- Publish events like `ScheduleCreated` and `InventoryUpdated`.

### **3.2 Flight Booking Service**
- Develop APIs for booking, retrieving, and canceling flights. When a booking is created, `PassengerService` APIs will be able to act on those passengers in the booking.
- Publish events like `BookingCreated` and `BookingCancelled`.

---

## **Step 4: Develop Dependent BFFs**

### **4.1 Online Check-In (OLCI)**
1. Create a `.NET 8 Web API Project`.
2. Add tailored APIs for online check-in:
   - `GET /checkin/{pnr}` - Retrieve booking details for check-in.
   - `POST /checkin` - Perform online check-in.
3. Integrate with `PassengerService` APIs for passenger operations.

### **4.2 Departure Control System (DCS)**
1. Create a `.NET 8 Web API Project`.
2. Add tailored APIs for agent workflows:
   - `GET /flights` - View flights with passenger details.
   - `POST /checkin/{passengerId}` - Check-in a passenger.
   - `POST /offload/{passengerId}` - Offload a passenger.
   - `POST /board/{passengerId}` - Board a passenger.
3. Integrate with `PassengerService` APIs for passenger operations.

---

## **Step 5: Incrementally Add Other Services**

### **5.1 Flight Ops Service**
- Build APIs to manage flight statuses (scheduled, departed, canceled).
- Publish `FlightStatusUpdated` events.

### **5.2 Flight Analytics Service**
- Set up event consumption from RabbitMQ.
- Implement APIs to view reports and analytics.

---

## **Step 6: Test and Verify**
1. **API Testing:**
   - Test each service independently using Postman or Swagger UI.
   - Validate event publishing and consumption via RabbitMQ.

2. **Integration Testing:**
   - Deploy the full system locally using Docker Compose.
   - Simulate operations like creating schedules, bookings, and check-ins.

3. **Simulation Scripts**
   - Create simulation scripts for opertions
     -  Scheduling
	 -  Booking
	 
---

## **Step 7: Prepare for Kubernetes**
1. Define **Kubernetes manifests** (e.g., deployments, services, config maps) for each service.
2. Test deployment on **Minikube** or a Kubernetes cluster.
3. Create **Heml Chart** for the whole system. Deploy the the system using Helm Chart in a Kubernetes cluster.

---

## **Summary**
This step-by-step guide ensures a methodical approach to developing and deploying the PSS. Starting with the foundational `PassengerService` and incrementally adding dependent services will streamline the process and ensure reusability and scalability. The final system will be validated for performance and auto-scaling in Kubernetes.

---

