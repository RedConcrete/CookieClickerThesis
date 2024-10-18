package middleware

import (
	"context"

	"go.opentelemetry.io/contrib/bridges/otelslog"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/exporters/otlp/otlplog/otlploghttp"
	"go.opentelemetry.io/otel/exporters/otlp/otlpmetric/otlpmetrichttp"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracehttp"
	"go.opentelemetry.io/otel/log/global"
	"go.opentelemetry.io/otel/sdk/log"
	"go.opentelemetry.io/otel/sdk/metric"
	"go.opentelemetry.io/otel/sdk/trace"
)

var (
	Tracer = otel.Tracer("cookie-server-tracer")
	Meter  = otel.Meter("cookie-server-meter")
	Logger = otelslog.NewLogger("cookie-server-logger")
)

func SetupMiddleware(context context.Context) error {
	if err := initTracer(context); err != nil {
		return err
	}
	if err := initMeter(context); err != nil {
		return err
	}
	return nil
}

func initTracer(context context.Context) error {
	traceExporter, err := otlptracehttp.New(context)
	if err != nil {
		return err
	}
	traceProvider := trace.NewTracerProvider(
		trace.WithBatcher(traceExporter))
	otel.SetTracerProvider(traceProvider)
	return nil
}

func initMeter(context context.Context) error {
	meterExporter, err := otlpmetrichttp.New(context)
	if err != nil {
		return err
	}
	meterProvider := metric.NewMeterProvider(
		metric.WithReader(metric.NewPeriodicReader(meterExporter)))
	otel.SetMeterProvider(meterProvider)
	return nil
}

func initLogger(context context.Context) error {
	loggerExporter, err := otlploghttp.New(context)
	if err != nil {
		return err
	}
	loggerProvider := log.NewLoggerProvider(
		log.WithProcessor(log.NewBatchProcessor(loggerExporter)),
	)
	global.SetLoggerProvider(loggerProvider)
	return nil
}