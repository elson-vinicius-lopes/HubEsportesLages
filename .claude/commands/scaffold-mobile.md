---
description: Cria o esqueleto do app .NET MAUI (Arena Lages) consumindo a API do hub.
allowed-tools: Bash, Read, Edit, Write, Grep, Glob
---

Crie o app móvel **Arena Lages** em .NET MAUI, ligado à API do Hub Esportes Lages. Delegue ao
subagente **dev-mobile** quando precisar implementar telas/serviços.

Passos:

1. **Pré-requisito**: confirme o workload — `dotnet workload list`. Se faltar, avise que é preciso
   `dotnet workload install maui` (download grande) e pare para confirmação.
2. Scaffold: `dotnet new maui -n HubEsportesLages.Mobile -o src/HubEsportesLages.Mobile` e adicione ao
   solution: `dotnet sln HubEsportesLages.slnx add src/HubEsportesLages.Mobile` (use o `nuget.config` da raiz).
3. Pacotes: `CommunityToolkit.Mvvm` (e, opcionalmente, `CommunityToolkit.Maui`).
4. Estruture: `Models/` (DTOs espelhando o JSON camelCase da API), `Services/ArenaApiClient.cs`
   (HttpClient tipado + `System.Text.Json`), `ViewModels/`, `Views/` (Shell), `Resources/Styles`
   (paleta do `site.css`).
5. Telas iniciais: **Agenda** (lista + filtro), **Detalhe**, **Resultados**, **Notificações** e
   **Inscrição**. Cada lista com estados carregando/vazio/erro.
6. Configure `BaseUrl` (`10.0.2.2:5210` no Android emulador, `localhost:5210` no Windows) e registre os
   serviços/ViewModels no DI do `MauiProgram`.
7. Compile (`dotnet build src/HubEsportesLages.Mobile`) e reporte como rodar (Windows/Android).

Mantenha a identidade visual coerente com o site; se houver design do Figma, siga-o.
