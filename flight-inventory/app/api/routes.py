from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from ..database import get_db
from ..models import FlightSchedule, Inventory
from ..schemas import FlightScheduleCreate
from ..schemas import InventoryUpdate
from ..events import publish_event

router = APIRouter()

@router.post("/schedule")
def upsert_flight_schedule(flight: FlightScheduleCreate, db: Session = Depends(get_db)):
    """
    Create or update a flight schedule.
    If the flight (flight_number + departure_date) exists, update it; otherwise, insert a new record.
    """
    existing_flight = db.query(FlightSchedule).filter_by(
        flight_number=flight.flight_number, departure_date=flight.departure_date
    ).first()

    if existing_flight:
        existing_flight.origin = flight.origin
        existing_flight.destination = flight.destination
        existing_flight.business_seat_capacity = flight.business_seat_capacity
        existing_flight.economy_seat_capacity = flight.economy_seat_capacity
    else:
        existing_flight = FlightSchedule(**flight.dict())
        db.add(existing_flight)

    db.commit()
    db.refresh(existing_flight)

    # Publish event with full flight details
    publish_event("ScheduleCreated", {
        "flight_id": existing_flight.id,
        "flight_number": existing_flight.flight_number,
        "departure_date": existing_flight.departure_date.isoformat(),
        "origin": existing_flight.origin,
        "destination": existing_flight.destination,
        "business_seat_capacity": existing_flight.business_seat_capacity,
        "economy_seat_capacity": existing_flight.economy_seat_capacity
    })

    return existing_flight



@router.get("/schedule")
def get_schedules(db: Session = Depends(get_db)):
    return db.query(FlightSchedule).all()

@router.put("/inventory")
def upsert_inventory(inventory_update: InventoryUpdate, db: Session = Depends(get_db)):
    """
    Updates or creates the inventory for a flight.
    If the inventory exists, update booked seats. If it doesn't, create a new inventory entry.
    Ensures that booked seats do not exceed the seat capacity.
    """
    flight = db.query(FlightSchedule).filter_by(
        flight_number=inventory_update.flight_number,
        departure_date=inventory_update.departure_date
    ).first()
    
    if not flight:
        raise HTTPException(status_code=404, detail="Flight not found")

    inventory = db.query(Inventory).filter_by(flight_id=flight.id).first()

    # Perform capacity validation before creating or updating the inventory
    if inventory_update.booked_business_seats > flight.business_seat_capacity:
        raise HTTPException(status_code=400, detail="Booked business seats exceed capacity")
    
    if inventory_update.booked_economy_seats > flight.economy_seat_capacity:
        raise HTTPException(status_code=400, detail="Booked economy seats exceed capacity")

    if not inventory:
        # Create new inventory with capacity validation applied
        inventory = Inventory(
            flight_id=flight.id,
            booked_business_seats=inventory_update.booked_business_seats,
            booked_economy_seats=inventory_update.booked_economy_seats
        )
        db.add(inventory)
    else:
        # Update existing inventory with validated seat counts
        inventory.booked_business_seats = inventory_update.booked_business_seats
        inventory.booked_economy_seats = inventory_update.booked_economy_seats

    db.commit()
    db.refresh(inventory)

    # Publish event with full inventory details
    publish_event("InventoryUpdated", {
        "inventory_id": inventory.id,
        "flight_id": flight.id,
        "flight_number": flight.flight_number,
        "departure_date": flight.departure_date.isoformat(),
        "booked_business_seats": inventory.booked_business_seats,
        "booked_economy_seats": inventory.booked_economy_seats,
        "available_business_seats": flight.business_seat_capacity - inventory.booked_business_seats,
        "available_economy_seats": flight.economy_seat_capacity - inventory.booked_economy_seats
    })

    return {
        "message": "Inventory updated successfully",
        "flight_number": flight.flight_number,
        "departure_date": flight.departure_date.isoformat(),
        "booked_business_seats": inventory.booked_business_seats,
        "booked_economy_seats": inventory.booked_economy_seats,
        "available_business_seats": flight.business_seat_capacity - inventory.booked_business_seats,
        "available_economy_seats": flight.economy_seat_capacity - inventory.booked_economy_seats
    }




@router.get("/inventory/{flight_number}")
def get_flight_inventory(flight_number: str, db: Session = Depends(get_db)):
    flight = db.query(FlightSchedule).filter_by(flight_number=flight_number).first()
    if not flight:
        return {"error": "Flight not found"}
    
    inventory = db.query(Inventory).filter_by(flight_id=flight.id).first()
    if not inventory:
        return {"error": "Inventory not found"}
    
    return {
        "flight_number": flight.flight_number,
        "available_business_seats": inventory.available_business_seats(),
        "available_economy_seats": inventory.available_economy_seats()
    }
