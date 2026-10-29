# .NET 11: non chiamatelo un semplice upgrade

Le demo della sessione alla .NET Conference Italia 2026.

| Cartella | Cosa mostra |
|---|---|
| [`01-runtime-async`](01-runtime-async) | Lo stesso codice async compilato con le state machine classiche e con Runtime Async: 17 frame sullo stack contro 5. |
| [`02-union-types`](02-union-types) | Union types e closed hierarchy di C# 15, dagli switch esaustivi fino a System.Text.Json e OpenAPI in una Minimal API. |

Ogni cartella ha un README con i comandi da lanciare e cosa osservare, più una
sottocartella `output-atteso/` con i risultati di riferimento, utile se non avete
ancora installato l'SDK.

## Requisiti

- [SDK .NET 11](https://dotnet.microsoft.com/download/dotnet/11.0). Le demo sono state
  verificate con la 11.0.100-rc.3; `global.json` accetta questa versione o una successiva.
- Per aprire il codice: Visual Studio 2026, oppure VS Code con C# Dev Kit aggiornato
  (serve il supporto a C# 15).

```
git clone <url-del-repo>
cd demo-net11
dotnet build DemoNet11.slnx
```

## Note sulle preview

Runtime Async e union types sono feature nuove e la loro superficie può ancora
cambiare. Se con una versione più recente dell'SDK qualcosa non torna, confrontate
con `output-atteso/` e aprite pure una issue.

## Licenza

[MIT](LICENSE)
