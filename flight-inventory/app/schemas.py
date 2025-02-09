from pydantic import BaseModel
from datetime import date

class FlightScheduleCreate(BaseModel):
    flight_number: str
    departure_date: date
    origin: str
    destination: str
    business_seat_capacity: int
    economy_seat_capacity: int

class InventoryUpdate(BaseModel):
    flight_number: str
    departure_date: date
    booked_business_seats: int
    booked_economy_seats: int