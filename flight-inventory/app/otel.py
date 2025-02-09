from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.sdk.resources import Resource
from app.config import OTEL_EXPORTER_OTLP_ENDPOINT  # Import from config.py
import os

def setup_tracing(app):
    resource = Resource.create({"service.name": "flight-inventory"})
    provider = TracerProvider(resource=resource)
    provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=OTEL_EXPORTER_OTLP_ENDPOINT)))
    
    FastAPIInstrumentor.instrument_app(app, tracer_provider=provider)
