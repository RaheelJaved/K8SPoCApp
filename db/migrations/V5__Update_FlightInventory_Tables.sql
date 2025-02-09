-- Ensure flight_number + departure_date are unique in flight_schedules
ALTER TABLE flight_schedules
ADD CONSTRAINT _flight_date_uc UNIQUE (flight_number, departure_date);

-- Ensure inventory is uniquely tied to a flight schedule
ALTER TABLE flight_inventory
ADD CONSTRAINT _inventory_flight_uc UNIQUE (flight_id);
