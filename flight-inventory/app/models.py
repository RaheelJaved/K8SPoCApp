from sqlalchemy import Column, Integer, String, Date, ForeignKey, UniqueConstraint
from sqlalchemy.orm import relationship
from .database import Base

class FlightSchedule(Base):
    __tablename__ = "flight_schedules"

    id = Column(Integer, primary_key=True, index=True)
    flight_number = Column(String, index=True)
    departure_date = Column(Date, index=True)
    origin = Column(String)
    destination = Column(String)
    business_seat_capacity = Column(Integer)
    economy_seat_capacity = Column(Integer)

    __table_args__ = (UniqueConstraint('flight_number', 'departure_date', name='_flight_date_uc'),)

class Inventory(Base):
    __tablename__ = "flight_inventory"

    id = Column(Integer, primary_key=True, index=True)
    flight_id = Column(Integer, ForeignKey("flight_schedules.id"))
    booked_business_seats = Column(Integer, default=0)
    booked_economy_seats = Column(Integer, default=0)
    flight = relationship("FlightSchedule")

    __table_args__ = (UniqueConstraint('flight_id', name='_inventory_flight_uc'),)

    def available_business_seats(self):
        return self.flight.business_seat_capacity - self.booked_business_seats

    def available_economy_seats(self):
        return self.flight.economy_seat_capacity - self.booked_economy_seats
