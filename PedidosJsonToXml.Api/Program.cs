using FluentValidation;
using PedidosJsonToXml.Api.Interfaces;
using PedidosJsonToXml.Api.Middlewares;
using PedidosJsonToXml.Api.Persistence;
using PedidosJsonToXml.Api.Services;
using PedidosJsonToXml.Api.Validators;
using Polly;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registro de los validadores de FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PedidoRequestValidator>();

// Registro del HttpClient para el consumo del Endpoint Externo
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Polly Resilience
builder.Services.AddResiliencePipeline("http-post-retry-pipeline", pipelineBuilder =>
{
    var delays = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) };

    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Constant,
        DelayGenerator = args =>
        {
            var delay = args.AttemptNumber < delays.Length
                ? delays[args.AttemptNumber]
                : delays.Last();
            return ValueTask.FromResult<TimeSpan?>(delay);
        },
        OnRetry = args =>
        {
            Console.WriteLine($"[Polly] Intento #{args.AttemptNumber + 1} fallido debido a: {args.Outcome.Exception?.Message}. Esperando {args.RetryDelay.TotalSeconds} segundos...");
            return ValueTask.CompletedTask;
        }
    });
});

// IOptions - POCO
builder.Services.Configure<AcmeSoapServiceSettings>(
    builder.Configuration.GetSection("AcmeSoapService")
);

builder.Services.AddScoped<IAcmeSoapClientService, AcmeSoapClientService>();
builder.Services.AddScoped<ISoapTransformerService, SoapTransformerService>();

var app = builder.Build();

// Registro del Middleware de errores 
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();