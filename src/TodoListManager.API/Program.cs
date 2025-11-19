// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Identity;
using TodoListManager.API.Extensions;
using TodoListManager.Infrastructure.Data;
using TodoListManager.Infrastructure.Identity;
using TodoListManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCorsPolicy();

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register application layers
builder.Services.AddValidation();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDomain();
builder.Services.AddApplication();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerConfiguration();

// Add MVC Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Seed database
await app.SeedDatabaseAsync();

// Configure the HTTP request pipeline
app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
