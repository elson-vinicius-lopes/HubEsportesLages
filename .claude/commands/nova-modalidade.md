---
description: Adiciona uma nova modalidade esportiva ao seed do Hub Esportes Lages.
argument-hint: "<nome da modalidade> (ex.: \"Skate\")"
---

Adicione a modalidade **$ARGUMENTS** ao cenário de dados do hub, seguindo o padrão existente.

1. Em `src/HubEsportesLages.Infrastructure/Persistence/DataSeeder.cs`, crie a `Modalidade` na seção
   de modalidades com: `Nome`, `Slug` (kebab-case, sem acentos), `Icone` (emoji adequado), `CorHex`
   (hex coerente com a paleta) e `Descricao` curta. Inclua-a no array `modalidades`.
2. Se fizer sentido, crie 1–2 `Equipe`(s) e ao menos **um evento** dessa modalidade (use o helper
   local `Novo(...)`), garantindo um item futuro para a agenda — e, se quiser, um encerrado com placar
   para os resultados.
3. O banco só semeia quando está vazio. Para ver o resultado, **apague** `src/HubEsportesLages.Web/hubesportes.db`
   e rode a aplicação (`/rodar`).
4. Compile (`dotnet build HubEsportesLages.slnx`) e confirme via
   `curl -s http://localhost:5210/api/catalogo/modalidades` que a nova modalidade aparece.

Mantenha o estilo do código vizinho e os textos em pt-BR.
