var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

var productos = new Dictionary<int, decimal>
{
    { 10, 800.00m },
    { 20, 1200.00m },
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.MapGet("/api/productos/{id}", (int id) =>
{
    if(productos.TryGetValue(id, out var precio))
        return Results.Ok(new {ProductoId = id, Precio = precio});
    return Results.NotFound(new { Mensaje = "Producto sin stock o inexistente." });
})
.WithName("Productos");

app.Run("http://localhost:5002");