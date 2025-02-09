import pika
import json
import os
from datetime import date
from app.config import RABBITMQ_URI  # Import from config.py

def json_serializer(obj):
    """Custom serializer to handle date objects."""
    if isinstance(obj, date):
        return obj.isoformat()  # Converts date to string format "YYYY-MM-DD"
    raise TypeError(f"Type {type(obj)} not serializable")

def publish_event(event_name, payload):
    connection = pika.BlockingConnection(pika.URLParameters(RABBITMQ_URI))
    channel = connection.channel()
    
    channel.queue_declare(queue=event_name)
    channel.basic_publish(exchange='', routing_key=event_name, body=json.dumps(payload, default=json_serializer))

    connection.close()
