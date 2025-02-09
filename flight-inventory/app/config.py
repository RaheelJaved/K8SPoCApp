import os
from dotenv import load_dotenv

# Load environment variables from .env file if available
load_dotenv()

DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://admin:password@localhost:5432/pss")
RABBITMQ_URI = os.getenv("RABBITMQ_URI", "amqp://admin:admin@localhost:5672")
OTEL_EXPORTER_OTLP_ENDPOINT = os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://otel-collector:4317")
