using Application.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<InsertEntriesService>();
builder.Services.AddTransient<InsertExitsService>();