var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors( options => 
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("https://h0n9.com",
                                 "https://www.h0n9.com",
                                 "https://admin.h0n9.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
