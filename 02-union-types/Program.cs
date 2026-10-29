using System.Globalization;
using UnionDemo;

// Importi in euro e date all'italiana, indipendentemente dal locale della macchina.
CultureInfo.DefaultThreadCurrentCulture = new("it-IT");

var builder = WebApplication.CreateBuilder(args);

// In .NET 11 il documento generato è OpenAPI 3.2 per default.
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi(); // -> /openapi/v1.json

Ordine[] ordini =
[
    new(1, "Rossi Srl",   120.50m, new CartaDiCredito("Visa", "4242"), new Spedito(new DateOnly(2026, 11, 20), "BRT")),
    new(2, "Bianchi Spa",  89.00m, new Bonifico("IT60X0542811101000000123456"), new InAttesa()),
    new(3, "Verdi & C.",   45.90m, new Contrassegno(4.00m), new Annullato("Richiesta del cliente")),
];

// La union attraversa lo stack: System.Text.Json la serializza,
// OpenAPI la descrive come anyOf dei tre casi.
// (Volutamente minimale: un id inesistente produce un 500, va bene per la demo.)
app.MapGet("/ordini/{id:int}/pagamento", (int id) =>
    ordini.Single(o => o.Id == id).Pagamento);

// Riepilogo leggibile, costruito con gli switch esaustivi.
app.MapGet("/ordini", () => ordini.Select(o => new
{
    o.Id,
    o.Cliente,
    o.Totale,
    Pagamento = Descrizioni.Descrivi(o.Pagamento),
    Stato = Descrizioni.Descrivi(o.Stato),
}));

app.Run();
