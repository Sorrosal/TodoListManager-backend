// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.Application.Handlers;
using TodoListManager.Application.Services;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Repositories;
using TodoListManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repository as singleton (in-memory)
builder.Services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();

// Register TodoList aggregate as singleton (in-memory state)
builder.Services.AddSingleton<TodoList>();
builder.Services.AddSingleton<ITodoList>(sp => sp.GetRequiredService<TodoList>());

// Register command handlers
builder.Services.AddScoped<AddTodoItemCommandHandler>();
builder.Services.AddScoped<UpdateTodoItemCommandHandler>();
builder.Services.AddScoped<RemoveTodoItemCommandHandler>();
builder.Services.AddScoped<RegisterProgressionCommandHandler>();

// Register query handlers
builder.Services.AddScoped<GetAllTodoItemsQueryHandler>();

// Register application service
builder.Services.AddScoped<TodoListService>();

var app = builder.Build();

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
