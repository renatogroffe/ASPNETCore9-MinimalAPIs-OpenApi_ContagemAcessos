using APIContagem;
using APIContagem.Models;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Contador>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();

app.MapGet("/contador",
    Results<Ok<ResultadoContador>, InternalServerError<string>> (Contador contador) =>
{
    int valorAtualContador;
    lock (contador)
    {
        contador.Incrementar();
        valorAtualContador = contador.ValorAtual;
    }
    app.Logger.LogInformation($"Contador - Valor atual: {valorAtualContador}");

    if (Convert.ToBoolean(Convert.ToBoolean(app.Configuration["SimularFalha"])) &&
        valorAtualContador % 2 == 0)
    {
        app.Logger.LogError(
            "Simulando uma falha para testar TypedResults.InternalServerError...");
        return TypedResults.InternalServerError("Simulação de falha");
    }

    return TypedResults.Ok(new ResultadoContador()
    {
        ValorAtual = contador.ValorAtual,
        Local = contador.Local,
        Kernel = contador.Kernel,
        Framework = contador.Framework,
        Mensagem = app.Configuration["Saudacao"]
    });
}).Produces<ResultadoContador>().Produces(StatusCodes.Status500InternalServerError).WithOpenApi();

app.MapGet("/contador-decrementar",
    Results<Ok<ResultadoContador>, InternalServerError<string>> (Contador contador) =>
    {
        int valorAtualContador;
        lock (contador)
        {
            contador.Decrementar();
            valorAtualContador = contador.ValorAtual;
        }
        app.Logger.LogInformation($"Contador (Decremento) - Valor atual: {valorAtualContador}");

        return TypedResults.Ok(new ResultadoContador()
        {
            ValorAtual = contador.ValorAtual,
            Local = contador.Local,
            Kernel = contador.Kernel,
            Framework = contador.Framework,
            Mensagem = $"{app.Configuration["Saudacao"]} (Decremento)"
        });
    }).Produces<ResultadoContador>().Produces(StatusCodes.Status500InternalServerError).WithOpenApi();

app.Run();