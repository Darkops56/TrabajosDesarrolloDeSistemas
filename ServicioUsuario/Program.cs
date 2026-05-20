var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var usuarios = new Dictionary<int, decimal>
{
    {1, 1500.00m },
    {2, 200.00m}
};

app.MapGet("/api/usuarios", () => 
{
    if(usuarios.TryGetValue(id, out var saldo))
        return Results.Ok(new {UsuarioId = id, Saldo = saldo});
    return Results.NotFound(new { Mensaje = "Usuario inexistente." });
})
.WithName("Usuarios");



app.MapPost("/api/usuarios/{id}/debitar", (int id, decimal valor) =>
{
    if(usuarios.TryGetValue(id, out var saldo))
    {
        if(saldo >= valor)
        {
            usuarios[id] = saldo - valor;
            return Results.Ok(new {UsuarioId = id, Saldo = usuarios[id], Mensaje = "Se debito correctamente."});
        }
        else 
        {
            return Results.BadRequest(new {Error = "Saldo insuficiente"});
        }
    }
    return Results.NotFound(new { Mensaje = "Usuario inexistente." });
})
.WithName("Usuarios");


app.Run("https://localhost:5001");