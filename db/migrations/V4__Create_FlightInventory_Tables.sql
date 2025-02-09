CREATE TABLE flight_schedules (
    id SERIAL PRIMARY KEY,
    flight_number VARCHAR UNIQUE NOT NULL,
    departure_date DATE NOT NULL,
    origin VARCHAR NOT NULL,
    destination VARCHAR NOT NULL,
    business_seat_capacity INTEGER NOT NULL,
    economy_seat_capacity INTEGER NOT NULL
);

CREATE TABLE flight_inventory (
    id SERIAL PRIMARY KEY,
    flight_id INTEGER REFERENCES flight_schedules(id),
    booked_business_seats INTEGER DEFAULT 0 NOT NULL,
    booked_economy_seats INTEGER DEFAULT 0 NOT NULL
);
