from fastapi import FastAPI
from .database import Base, engine
from .api.routes import router
from .otel import setup_tracing  # Import OpenTelemetry setup

app = FastAPI()

# Setup OpenTelemetry
setup_tracing(app)

# Create database tables
Base.metadata.create_all(bind=engine)

app.include_router(router, prefix="/api/flight-inventory")
