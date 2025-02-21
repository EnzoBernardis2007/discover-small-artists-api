var builder = WebApplication.CreateBuilder(args);

// Adicione o serviço CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use o middleware CORS
app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Remover a linha abaixo caso você não queira usar HTTPS
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
