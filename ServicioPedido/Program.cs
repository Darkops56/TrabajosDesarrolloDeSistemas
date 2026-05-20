using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapPost("api/checkout", async (CompareResquest pedido, HttpClient client) => 
{
    try
    {
        var responseCatologo = await client.GetAsync($"https://localhost:5002/api/productos/{pedido.ProductoId}");
        if(!responseCatologo.IsSuccessStatusCode)
            return Results.BadRequest(new {Error = "El producto no existe en el catologo"});
        
        var producto = await responseCatologo.Content.ReadFromJsonAsync<ProductoDTO>();
        
        var responseUsuario = await client.GetAsync($"https://localhost:5001/api/usuarios/{pedido.UsuarioId}");
        if(!responseUsuario.IsSuccessStatusCode)
            return Results.BadRequest(new {Error = "El usuario no existe en la base de datos"});
        
        var usuario = await responseUsuario.Content.ReadFromJsonAsync<UsuarioDTO>();

        if(usuario?.Saldo >= producto?.Precio)
        {
            var responseCatologoDescontar = await client.PostAsync($"https://localhost:5002/api/productos/{pedido.ProductoId}/descontar", new StringContent(""));
            if (!responseCatologoDescontar.IsSuccessStatusCode)
                return Results.BadRequest(new { Error = "El stock no se descontó."});
            
            var responseUsuarioDebitar = await client.PostAsync($"https://localhost:5001/api/usuarios/{pedido.UsuarioId}/debitar?valor={producto.Precio}", new StringContent(JsonSerializer.Serialize(new {valor = producto.Precio})));
            if (!responseUsuarioDebitar.IsSuccessStatusCode)
                return Results.BadRequest(new { Error = "El stock se descontó pero no se cobró."});

            return Results.Ok(
                        new
                        {
                            Estado = "Aprobado",
                            Mensaje = $"Compra exitosa. Se debitaron ${producto.Precio} de la cuenta del usuario {pedido.UsuarioId}"
                        }
                    );
        }
        else 
        {
            return Results.BadRequest(new {Estado = "Rechazado", Motivo = "Saldo insuficiente"});
        }
    }
    catch(HttpRequestException ex)
    {
        return Results.Json(new {
            Estado = "Error 503 (Service Unavailable)",
            Motivo = "Uno de los microservicios internos no responde. Intente mas tarde",
            DetalleTecnico = ex.Message
        }, statusCode: 503);
    }
});
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run("https://localhost:5003");

public record CompareResquest(int UsuarioId, int ProductoId);
public record UsuarioDTO(int UsuarioId, decimal Saldo);
public record ProductoDTO(int ProductoId, decimal Precio);