using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Plumbing;

namespace WebUI;

public static class OpenTelemetry
{
    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (string.IsNullOrEmpty(otlpEndpoint))
        {
            return builder;
        }
        
        builder.Logging.AddOpenTelemetry(l =>
        {
            l.IncludeFormattedMessage = true;
            l.IncludeScopes = true;
            l.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(otlpEndpoint);
            });
        });

        var otel = builder.Services.AddOpenTelemetry();
        otel.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddMeter("Microsoft.AspNetCore.Hosting");
            metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
        });

        otel.WithTracing(tracing =>
        {
            tracing.AddSource(Source.SourceName);
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation(opt =>
            {
                opt.EnrichWithHttpResponseMessage = async void (activity, httpResponseMessage) =>
                {
                    try
                    {
                        var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType;

                        // Only log supported content types (e.g., JSON)
                        if (contentType != null && contentType.Contains("application/json"))
                        {
                            // Limit request body size (e.g., 10 KB)
                            const int maxBodySize = 10 * 1024;
                            var body = await httpResponseMessage.Content.ReadAsStringAsync();

                            if (body.Length > maxBodySize)
                            {
                                activity.SetTag("response_body_truncated", true);
                                activity.SetTag("response_body", body.Substring(0, maxBodySize) + "...");
                            }
                            else
                            {
                                activity.SetTag("response_body", body);
                            }
                        }
                        else
                        {
                            activity.SetTag("response_body_skipped", "Unsupported content type or empty body.");
                        }
                    }
                    catch (Exception ex)
                    {
                        activity.SetTag("response_body_error", ex.Message);
                    }
                };
                opt.EnrichWithHttpRequestMessage = async void (activity, httpRequestMessage) =>
                {
                    // Set Display Name for the activity
                    activity.DisplayName = $"{httpRequestMessage.Method} {httpRequestMessage.RequestUri?.Host}{httpRequestMessage.RequestUri?.AbsolutePath}";

                    try
                    {
                        if (httpRequestMessage.Content != null)
                        {
                            var contentType = httpRequestMessage.Content.Headers.ContentType?.MediaType;
                    
                            // Only log supported content types (e.g., JSON)
                            if (contentType != null && contentType.Contains("application/json"))
                            {
                                // Limit request body size (e.g., 10 KB)
                                const int maxBodySize = 10 * 1024; 
                                var body = await httpRequestMessage.Content.ReadAsStringAsync();
                        
                                if (body.Length > maxBodySize)
                                {
                                    activity.SetTag("request_body_truncated", true);
                                    activity.SetTag("request_body", body.Substring(0, maxBodySize) + "...");
                                }
                                else
                                {
                                    activity.SetTag("request_body", body);
                                }
                            }
                            else
                            {
                                activity.SetTag("request_body_skipped", "Unsupported content type or empty body.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        activity.SetTag("request_body_error", ex.Message);
                    }
                };
            });
                
            tracing.AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri(otlpEndpoint);
            });
        });
        return builder;
    }
}