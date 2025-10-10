using InteractiveLeads.Api.Middleware;
using InteractiveLeads.Application;
using InteractiveLeads.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddOpenApi();

builder.Services.AddInfraestructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Services.GetJwtSettings(builder.Configuration));

builder.Services.AddApplicationServices();

var app = builder.Build();

await app.Services.AddDatabaseInitializerAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseInfraestructure();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
