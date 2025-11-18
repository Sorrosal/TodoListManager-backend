// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCorsPolicy();

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure API Versioning
builder.Services.AddApiVersioningConfiguration();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerConfiguration();

// Register application layers
builder.Services.AddValidation();
builder.Services.AddInfrastructure();
builder.Services.AddDomain();
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

// Enable CORS - must be placed before Authentication and Authorization
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
