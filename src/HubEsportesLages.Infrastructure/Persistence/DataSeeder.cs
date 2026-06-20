using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Persistence;

/// <summary>
/// Popula o banco com um cenário demonstrativo da cena esportiva de Lages/SC:
/// modalidades, locais, equipes e uma agenda com eventos passados (com placar),
/// ao vivo e futuros. As datas são relativas ao momento da carga, de modo que a
/// demonstração esteja sempre "atual".
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(HubDbContext db, CancellationToken ct = default)
    {
        if (await db.Modalidades.AnyAsync(ct))
            return; // já populado

        // ---------------------------------------------------------------- Modalidades
        var futebol = new Modalidade { Nome = "Futebol", Slug = "futebol", Icone = "FUT", CorHex = "#16a34a", Descricao = "Campeonatos de campo da Serra Catarinense." };
        var futsal = new Modalidade { Nome = "Futsal", Slug = "futsal", Icone = "FSL", CorHex = "#2563eb", Descricao = "A modalidade mais popular dos ginásios de Lages." };
        var basquete = new Modalidade { Nome = "Basquete", Slug = "basquete", Icone = "BSQ", CorHex = "#ea580c", Descricao = "Disputas de quadra no planalto serrano." };
        var volei = new Modalidade { Nome = "Vôlei", Slug = "volei", Icone = "VLI", CorHex = "#d97706", Descricao = "Vôlei de quadra masculino e feminino." };
        var handebol = new Modalidade { Nome = "Handebol", Slug = "handebol", Icone = "HND", CorHex = "#7c3aed", Descricao = "Handebol escolar e adulto." };
        var atletismo = new Modalidade { Nome = "Atletismo", Slug = "atletismo", Icone = "ATL", CorHex = "#dc2626", Descricao = "Corridas de rua e provas de pista." };
        var ciclismo = new Modalidade { Nome = "Ciclismo", Slug = "ciclismo", Icone = "CIC", CorHex = "#0891b2", Descricao = "Provas de estrada e mountain bike na serra." };
        var lutas = new Modalidade { Nome = "Artes Marciais", Slug = "artes-marciais", Icone = "LUT", CorHex = "#475569", Descricao = "Jiu-jitsu, judô e outras artes marciais." };

        var modalidades = new[] { futebol, futsal, basquete, volei, handebol, atletismo, ciclismo, lutas };

        // --------------------------------------------------------------------- Locais
        var vidalRamos = new Local { Nome = "Estádio Vidal Ramos Júnior", Endereco = "Rua Frei Rogério, s/n", Bairro = "Coral", Capacidade = 8000, Latitude = -27.8025, Longitude = -50.3192 };
        var jonesMinosso = new Local { Nome = "Ginásio Jones Minosso", Endereco = "Av. Marechal Floriano, 200", Bairro = "Centro", Capacidade = 3000, Latitude = -27.8167, Longitude = -50.3259 };
        var cme = new Local { Nome = "Centro Municipal de Esportes (CME)", Endereco = "Rua Belisário Ramos, 1500", Bairro = "Habitação", Capacidade = 1500, Latitude = -27.8094, Longitude = -50.3361 };
        var uniplac = new Local { Nome = "Ginásio da UNIPLAC", Endereco = "Av. Castelo Branco, 170", Bairro = "Universitário", Capacidade = 1200, Latitude = -27.8210, Longitude = -50.3100 };
        var jonasRamos = new Local { Nome = "Parque Jonas Ramos (Tanque)", Endereco = "Av. Belisário Ramos, s/n", Bairro = "Centro", Capacidade = 5000, Latitude = -27.8156, Longitude = -50.3289 };
        var sesi = new Local { Nome = "Ginásio do SESI Lages", Endereco = "Rua Nereu Ramos, 850", Bairro = "Coral", Capacidade = 800, Latitude = -27.8048, Longitude = -50.3221 };

        var locais = new[] { vidalRamos, jonesMinosso, cme, uniplac, jonasRamos, sesi };

        // --------------------------------------------------------------------- Equipes
        Equipe Eq(string nome, string sigla, Modalidade m, string escudo, string cor) =>
            new() { Nome = nome, Sigla = sigla, Modalidade = m, Escudo = escudo, CorPrimaria = cor };

        var lagesFc = Eq("Lages Futebol Clube", "LAG", futebol, "LAG", "#16a34a");
        var guarani = Eq("Guarani de Lages", "GUA", futebol, "GUA", "#1d4ed8");
        var internacional = Eq("Internacional de Lages", "INT", futebol, "INT", "#dc2626");
        var vilaNova = Eq("Vila Nova EC", "VIL", futebol, "VIL", "#334155");

        var acel = Eq("ACEL Futsal", "ACE", futsal, "ACE", "#16a34a");
        var lagesFutsal = Eq("Lages Futsal", "LGF", futsal, "LGF", "#2563eb");
        var aabb = Eq("AABB Lages", "AAB", futsal, "AAB", "#d97706");
        var serraFutsal = Eq("Serra Futsal", "SER", futsal, "SER", "#ea580c");

        var lagesBasquete = Eq("Lages Basquete", "LBC", basquete, "LBC", "#ea580c");
        var planalto = Eq("Planalto Basquete", "PLA", basquete, "PLA", "#b45309");

        var voleiSerrano = Eq("Vôlei Serrano", "VSE", volei, "VSE", "#d97706");
        var uniplacVolei = Eq("UNIPLAC Vôlei", "UNI", volei, "UNI", "#2563eb");

        var handebolLages = Eq("Handebol Lages", "HLG", handebol, "HLG", "#7c3aed");
        var serraHandebol = Eq("Serra Handebol", "SHA", handebol, "SHA", "#6d28d9");

        var equipes = new[]
        {
            lagesFc, guarani, internacional, vilaNova,
            acel, lagesFutsal, aabb, serraFutsal,
            lagesBasquete, planalto, voleiSerrano, uniplacVolei,
            handebolLages, serraHandebol
        };

        await db.Modalidades.AddRangeAsync(modalidades, ct);
        await db.Locais.AddRangeAsync(locais, ct);
        await db.Equipes.AddRangeAsync(equipes, ct);

        // --------------------------------------------------------------------- Eventos
        var hoje = DateTime.Today;
        var agora = DateTime.Now;
        var eventos = new List<Evento>();
        var contador = 0;

        Evento Novo(
            string titulo, string campeonato, Modalidade modalidade, Local local,
            DateTime inicio, StatusEvento status,
            Equipe? casa = null, Equipe? visitante = null,
            int? placarCasa = null, int? placarVisitante = null,
            bool gratuito = true, decimal? preco = null, bool destaque = false,
            string descricao = "", int duracaoHoras = 2)
        {
            contador++;
            var ev = new Evento
            {
                Titulo = titulo,
                Slug = SlugGenerator.Gerar(titulo, contador),
                Campeonato = campeonato,
                Modalidade = modalidade,
                Local = local,
                Inicio = inicio,
                Fim = inicio.AddHours(duracaoHoras),
                Status = status,
                EquipeCasa = casa,
                EquipeVisitante = visitante,
                PlacarCasa = placarCasa,
                PlacarVisitante = placarVisitante,
                Gratuito = gratuito,
                PrecoIngresso = gratuito ? null : preco,
                Destaque = destaque,
                Descricao = string.IsNullOrWhiteSpace(descricao)
                    ? $"{titulo} — acompanhe ao vivo pelo Bora pro Jogo."
                    : descricao,
                CriadoEm = agora.AddDays(-20),
                AtualizadoEm = agora
            };
            eventos.Add(ev);
            return ev;
        }

        // ---- Encerrados (alimentam a página de Resultados) ----
        var jogoFutebol = Novo("Lages FC x Guarani de Lages", "Campeonato Citadino de Futebol 2026", futebol, vidalRamos,
            hoje.AddDays(-7).AddHours(15), StatusEvento.Encerrado, lagesFc, guarani, 2, 1,
            descricao: "Clássico da rodada de abertura com casa cheia no Vidal Ramos.");
        var jogoFutsal1 = Novo("ACEL x Lages Futsal", "Liga Serrana de Futsal", futsal, jonesMinosso,
            hoje.AddDays(-5).AddHours(20), StatusEvento.Encerrado, acel, lagesFutsal, 4, 3);
        var jogoBasquete = Novo("Lages Basquete x Planalto", "Copa Planalto de Basquete", basquete, uniplac,
            hoje.AddDays(-3).AddHours(19), StatusEvento.Encerrado, lagesBasquete, planalto, 78, 65);
        Novo("Vôlei Serrano x UNIPLAC", "Liga Serrana de Vôlei", volei, sesi,
            hoje.AddDays(-2).AddHours(20), StatusEvento.Encerrado, voleiSerrano, uniplacVolei, 3, 1);
        Novo("Handebol Lages x Serra Handebol", "Copa Serra de Handebol", handebol, cme,
            hoje.AddDays(-1).AddHours(19), StatusEvento.Encerrado, handebolLages, serraHandebol, 27, 24);
        Novo("4ª Corrida da Serra Catarinense", "Circuito Serrano de Corridas", atletismo, jonasRamos,
            hoje.AddDays(-10).AddHours(8), StatusEvento.Encerrado,
            descricao: "Prova de 5km e 10km com mais de 600 corredores pelas ruas do centro.");

        // ---- Ao vivo (acontecendo agora) ----
        var aoVivo = Novo("AABB Lages x Serra Futsal", "Liga Serrana de Futsal", futsal, jonesMinosso,
            agora.AddMinutes(-25), StatusEvento.AoVivo, aabb, serraFutsal, 2, 2, destaque: true,
            descricao: "Jogo da rodada acontecendo agora no Ginásio Jones Minosso.");

        // ---- Futuros (alimentam a Agenda) ----
        Novo("Internacional de Lages x Vila Nova EC", "Campeonato Citadino de Futebol 2026", futebol, vidalRamos,
            hoje.AddDays(2).AddHours(16), StatusEvento.Agendado, internacional, vilaNova, destaque: true,
            descricao: "Duelo direto pela liderança do grupo A no Estádio Vidal Ramos Júnior.");
        Novo("Lages Futsal x ACEL", "Liga Serrana de Futsal", futsal, jonesMinosso,
            hoje.AddDays(3).AddHours(20), StatusEvento.Agendado, lagesFutsal, acel, destaque: true);
        Novo("Planalto x Lages Basquete", "Copa Planalto de Basquete", basquete, uniplac,
            hoje.AddDays(4).AddHours(19), StatusEvento.Agendado, planalto, lagesBasquete);
        Novo("UNIPLAC x Vôlei Serrano", "Liga Serrana de Vôlei", volei, sesi,
            hoje.AddDays(5).AddHours(20), StatusEvento.Agendado, uniplacVolei, voleiSerrano);
        Novo("Serra Futsal x AABB Lages", "Liga Serrana de Futsal", futsal, jonesMinosso,
            hoje.AddDays(7).AddHours(20), StatusEvento.Agendado, serraFutsal, aabb);
        Novo("Guarani de Lages x Lages FC", "Campeonato Citadino de Futebol 2026", futebol, vidalRamos,
            hoje.AddDays(8).AddHours(15), StatusEvento.Agendado, guarani, lagesFc, destaque: true,
            descricao: "O maior clássico da cidade em jogo de volta. Ingresso solidário: 1kg de alimento.");
        Novo("5ª Corrida Noturna de Lages", "Circuito Serrano de Corridas", atletismo, jonasRamos,
            hoje.AddDays(9).AddHours(19), StatusEvento.Agendado, gratuito: false, preco: 60m, destaque: true,
            descricao: "Percurso iluminado de 5km e 10km saindo do Parque Jonas Ramos. Kit do atleta incluso.");
        Novo("Serra Handebol x Handebol Lages", "Copa Serra de Handebol", handebol, cme,
            hoje.AddDays(6).AddHours(19), StatusEvento.Agendado, serraHandebol, handebolLages);
        Novo("Copa Lages de Jiu-Jitsu", "Federação Catarinense de Jiu-Jitsu", lutas, jonesMinosso,
            hoje.AddDays(11).AddHours(9), StatusEvento.Agendado, gratuito: false, preco: 40m,
            descricao: "Competição com chaves do branca ao preta, infantil ao master.", duracaoHoras: 8);
        Novo("Lages Basquete x Planalto (volta)", "Copa Planalto de Basquete", basquete, uniplac,
            hoje.AddDays(13).AddHours(19), StatusEvento.Agendado, lagesBasquete, planalto);
        Novo("Desafio MTB Serra Catarinense", "Circuito Catarinense de MTB", ciclismo, jonasRamos,
            hoje.AddDays(14).AddHours(8), StatusEvento.Agendado, gratuito: false, preco: 80m, destaque: true,
            descricao: "Prova de mountain bike com percursos de 30km e 50km pela zona rural de Lages.", duracaoHoras: 5);
        Novo("Vôlei Serrano x UNIPLAC (volta)", "Liga Serrana de Vôlei", volei, sesi,
            hoje.AddDays(15).AddHours(20), StatusEvento.Agendado, voleiSerrano, uniplacVolei);
        Novo("Vila Nova EC x Internacional de Lages", "Campeonato Citadino de Futebol 2026", futebol, vidalRamos,
            hoje.AddDays(16).AddHours(16), StatusEvento.Agendado, vilaNova, internacional);

        await db.Eventos.AddRangeAsync(eventos, ct);
        await db.SaveChangesAsync(ct);

        // ------------------------------------------------- Interação da torcida (ao vivo)
        // Escalação (candidatos a MVP), enquete e mural para o jogo acontecendo agora.
        var escalacao = new[]
        {
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = aabb.Id, Nome = "Bruno (GOL)" },
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = aabb.Id, Nome = "Diego Ala" },
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = aabb.Id, Nome = "Rafael Pivô" },
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = serraFutsal.Id, Nome = "Léo Fixo" },
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = serraFutsal.Id, Nome = "Marcos Ala" },
            new JogadorEvento { EventoId = aoVivo.Id, EquipeId = serraFutsal.Id, Nome = "Tiago Pivô" },
        };
        await db.JogadoresEvento.AddRangeAsync(escalacao, ct);

        await db.Enquetes.AddAsync(new Enquete
        {
            EventoId = aoVivo.Id,
            Pergunta = "Qual será o resultado final?",
            Ativa = true,
            CriadoEm = agora.AddMinutes(-20),
            Opcoes = new List<OpcaoEnquete>
            {
                new() { Texto = "Vitória do AABB Lages" },
                new() { Texto = "Empate" },
                new() { Texto = "Vitória do Serra Futsal" },
            }
        }, ct);

        var muralSeed = new[]
        {
            new MensagemTorcida { EventoId = aoVivo.Id, TorcedorId = "seed-1", Autor = "Serra Azul", Texto = "Vamo AABB!", CriadoEm = agora.AddMinutes(-12) },
            new MensagemTorcida { EventoId = aoVivo.Id, TorcedorId = "seed-2", Autor = "Torcida Serra", Texto = "Que jogão!", CriadoEm = agora.AddMinutes(-7) },
            new MensagemTorcida { EventoId = aoVivo.Id, TorcedorId = "seed-3", Autor = "Lageano", Texto = "Empate justo até aqui", CriadoEm = agora.AddMinutes(-2) },
        };
        await db.MensagensTorcida.AddRangeAsync(muralSeed, ct);

        await db.SaveChangesAsync(ct);

        // --------------------------------- Interação: Jogo 1 — Lages FC x Guarani (futebol)
        var jogadoresFutebol = new[]
        {
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = lagesFc.Id, Nome = "Rodrigo (GOL)" },
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = lagesFc.Id, Nome = "Anderson Volante" },
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = lagesFc.Id, Nome = "Caio Atacante" },
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = guarani.Id, Nome = "Fábio (GOL)" },
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = guarani.Id, Nome = "Henrique Meia" },
            new JogadorEvento { EventoId = jogoFutebol.Id, EquipeId = guarani.Id, Nome = "Paulo Zagueiro" },
        };
        await db.JogadoresEvento.AddRangeAsync(jogadoresFutebol, ct);
        await db.SaveChangesAsync(ct);

        // MVP votes — 12 torcedores distintos
        var votosFutebol = new[]
        {
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[2].Id, TorcedorId = "futseed-01", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-30) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[2].Id, TorcedorId = "futseed-02", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-28) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[2].Id, TorcedorId = "futseed-03", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-25) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[1].Id, TorcedorId = "futseed-04", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-20) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[1].Id, TorcedorId = "futseed-05", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-18) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[0].Id, TorcedorId = "futseed-06", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-15) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[4].Id, TorcedorId = "futseed-07", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-12) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[4].Id, TorcedorId = "futseed-08", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-10) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[5].Id, TorcedorId = "futseed-09", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-8) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[3].Id, TorcedorId = "futseed-10", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-5) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[2].Id, TorcedorId = "futseed-11", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-3) },
            new VotoMvp { EventoId = jogoFutebol.Id, JogadorEventoId = jogadoresFutebol[2].Id, TorcedorId = "futseed-12", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(-1) },
        };
        await db.VotosMvp.AddRangeAsync(votosFutebol, ct);

        var enqueteFutebol = new Enquete
        {
            EventoId = jogoFutebol.Id,
            Pergunta = "Quem foi o craque da partida?",
            Ativa = false,
            CriadoEm = jogoFutebol.Inicio.AddMinutes(10),
            Opcoes = new List<OpcaoEnquete>
            {
                new() { Texto = "Caio Atacante (Lages FC)" },
                new() { Texto = "Anderson Volante (Lages FC)" },
                new() { Texto = "Henrique Meia (Guarani)" },
            }
        };
        await db.Enquetes.AddAsync(enqueteFutebol, ct);
        await db.SaveChangesAsync(ct);

        var opcoesFutebol = enqueteFutebol.Opcoes.ToArray();
        await db.VotosEnquete.AddRangeAsync(new[]
        {
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[0].Id, TorcedorId = "futseed-01" },
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[0].Id, TorcedorId = "futseed-02" },
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[0].Id, TorcedorId = "futseed-03" },
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[1].Id, TorcedorId = "futseed-04" },
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[2].Id, TorcedorId = "futseed-07" },
            new VotoEnquete { EnqueteId = enqueteFutebol.Id, OpcaoEnqueteId = opcoesFutebol[2].Id, TorcedorId = "futseed-08" },
        }, ct);

        await db.MensagensTorcida.AddRangeAsync(new[]
        {
            new MensagemTorcida { EventoId = jogoFutebol.Id, TorcedorId = "futseed-01", Autor = "Fã do Lages FC", Texto = "Que golaço do Caio!", CriadoEm = jogoFutebol.Inicio.AddMinutes(35) },
            new MensagemTorcida { EventoId = jogoFutebol.Id, TorcedorId = "futseed-05", Autor = "Guardião Verde", Texto = "Rodrigo salvou o jogo!", CriadoEm = jogoFutebol.Inicio.AddMinutes(60) },
            new MensagemTorcida { EventoId = jogoFutebol.Id, TorcedorId = "futseed-09", Autor = "Guarani Fiel", Texto = "Honra do Guarani sempre!", CriadoEm = jogoFutebol.Inicio.AddMinutes(80) },
            new MensagemTorcida { EventoId = jogoFutebol.Id, TorcedorId = "futseed-12", Autor = "Lageano da Gema", Texto = "Cidade toda vibrou!", CriadoEm = jogoFutebol.Fim!.Value.AddMinutes(5) },
        }, ct);
        await db.SaveChangesAsync(ct);

        // --------------------------------- Interação: Jogo 2 — ACEL x Lages Futsal
        var jogadoresFutsal1 = new[]
        {
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = acel.Id, Nome = "Marcos (GOL)" },
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = acel.Id, Nome = "Felipe Fixo" },
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = acel.Id, Nome = "Renato Ala" },
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = lagesFutsal.Id, Nome = "Gustavo (GOL)" },
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = lagesFutsal.Id, Nome = "Thiago Pivô" },
            new JogadorEvento { EventoId = jogoFutsal1.Id, EquipeId = lagesFutsal.Id, Nome = "Bruno Ala" },
        };
        await db.JogadoresEvento.AddRangeAsync(jogadoresFutsal1, ct);
        await db.SaveChangesAsync(ct);

        var votosFutsal1 = new[]
        {
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[2].Id, TorcedorId = "fseed-01", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-25) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[2].Id, TorcedorId = "fseed-02", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-22) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[1].Id, TorcedorId = "fseed-03", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-20) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[1].Id, TorcedorId = "fseed-04", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-18) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[4].Id, TorcedorId = "fseed-05", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-15) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[4].Id, TorcedorId = "fseed-06", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-12) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[5].Id, TorcedorId = "fseed-07", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-10) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[0].Id, TorcedorId = "fseed-08", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-8) },
            new VotoMvp { EventoId = jogoFutsal1.Id, JogadorEventoId = jogadoresFutsal1[3].Id, TorcedorId = "fseed-09", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(-5) },
        };
        await db.VotosMvp.AddRangeAsync(votosFutsal1, ct);

        await db.MensagensTorcida.AddRangeAsync(new[]
        {
            new MensagemTorcida { EventoId = jogoFutsal1.Id, TorcedorId = "fseed-01", Autor = "Fã do ACEL", Texto = "Renato voou essa noite!", CriadoEm = jogoFutsal1.Inicio.AddMinutes(15) },
            new MensagemTorcida { EventoId = jogoFutsal1.Id, TorcedorId = "fseed-04", Autor = "Torcida Serrana", Texto = "Jogo pegado demais!", CriadoEm = jogoFutsal1.Inicio.AddMinutes(30) },
            new MensagemTorcida { EventoId = jogoFutsal1.Id, TorcedorId = "fseed-06", Autor = "LagesFutsal+1", Texto = "Thiago decidiu no final!", CriadoEm = jogoFutsal1.Inicio.AddMinutes(50) },
            new MensagemTorcida { EventoId = jogoFutsal1.Id, TorcedorId = "fseed-09", Autor = "Ginásio Cheio", Texto = "Que jogo histórico no Jones Minosso!", CriadoEm = jogoFutsal1.Fim!.Value.AddMinutes(3) },
        }, ct);
        await db.SaveChangesAsync(ct);

        // --------------------------------- Interação: Jogo 3 — Lages Basquete x Planalto
        var jogadoresBasquete = new[]
        {
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = lagesBasquete.Id, Nome = "Alex (PG)" },
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = lagesBasquete.Id, Nome = "Victor (SF)" },
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = lagesBasquete.Id, Nome = "Daniel (C)" },
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = planalto.Id, Nome = "Rafael (PG)" },
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = planalto.Id, Nome = "Igor (SG)" },
            new JogadorEvento { EventoId = jogoBasquete.Id, EquipeId = planalto.Id, Nome = "Lucas (PF)" },
        };
        await db.JogadoresEvento.AddRangeAsync(jogadoresBasquete, ct);
        await db.SaveChangesAsync(ct);

        var votosBasquete = new[]
        {
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[1].Id, TorcedorId = "bseed-01", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-20) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[1].Id, TorcedorId = "bseed-02", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-18) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[1].Id, TorcedorId = "bseed-03", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-15) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[1].Id, TorcedorId = "bseed-04", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-12) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[2].Id, TorcedorId = "bseed-05", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-10) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[2].Id, TorcedorId = "bseed-06", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-8) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[0].Id, TorcedorId = "bseed-07", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-6) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[4].Id, TorcedorId = "bseed-08", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-4) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[3].Id, TorcedorId = "bseed-09", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-2) },
            new VotoMvp { EventoId = jogoBasquete.Id, JogadorEventoId = jogadoresBasquete[1].Id, TorcedorId = "bseed-10", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(-1) },
        };
        await db.VotosMvp.AddRangeAsync(votosBasquete, ct);

        var enqueteBasquete = new Enquete
        {
            EventoId = jogoBasquete.Id,
            Pergunta = "Quem merece o MVP do jogo?",
            Ativa = false,
            CriadoEm = jogoBasquete.Inicio.AddMinutes(5),
            Opcoes = new List<OpcaoEnquete>
            {
                new() { Texto = "Victor - Lages Basquete" },
                new() { Texto = "Daniel - Lages Basquete" },
                new() { Texto = "Igor - Planalto" },
            }
        };
        await db.Enquetes.AddAsync(enqueteBasquete, ct);
        await db.SaveChangesAsync(ct);

        var opcoesBasquete = enqueteBasquete.Opcoes.ToArray();
        await db.VotosEnquete.AddRangeAsync(new[]
        {
            new VotoEnquete { EnqueteId = enqueteBasquete.Id, OpcaoEnqueteId = opcoesBasquete[0].Id, TorcedorId = "bseed-01" },
            new VotoEnquete { EnqueteId = enqueteBasquete.Id, OpcaoEnqueteId = opcoesBasquete[0].Id, TorcedorId = "bseed-02" },
            new VotoEnquete { EnqueteId = enqueteBasquete.Id, OpcaoEnqueteId = opcoesBasquete[0].Id, TorcedorId = "bseed-03" },
            new VotoEnquete { EnqueteId = enqueteBasquete.Id, OpcaoEnqueteId = opcoesBasquete[1].Id, TorcedorId = "bseed-05" },
            new VotoEnquete { EnqueteId = enqueteBasquete.Id, OpcaoEnqueteId = opcoesBasquete[2].Id, TorcedorId = "bseed-08" },
        }, ct);

        await db.MensagensTorcida.AddRangeAsync(new[]
        {
            new MensagemTorcida { EventoId = jogoBasquete.Id, TorcedorId = "bseed-01", Autor = "Quadra Cheia", Texto = "Victor com 30 pontos! Absurdo!", CriadoEm = jogoBasquete.Inicio.AddMinutes(25) },
            new MensagemTorcida { EventoId = jogoBasquete.Id, TorcedorId = "bseed-04", Autor = "Planalto Fiel", Texto = "Igor ainda honrou o Planalto!", CriadoEm = jogoBasquete.Inicio.AddMinutes(50) },
            new MensagemTorcida { EventoId = jogoBasquete.Id, TorcedorId = "bseed-07", Autor = "UNIPLAC Arena", Texto = "Ginásio pegando fogo!", CriadoEm = jogoBasquete.Inicio.AddMinutes(70) },
            new MensagemTorcida { EventoId = jogoBasquete.Id, TorcedorId = "bseed-10", Autor = "LagesBsk+1", Texto = "Título vai ficar em casa!", CriadoEm = jogoBasquete.Fim!.Value.AddMinutes(2) },
        }, ct);
        await db.SaveChangesAsync(ct);

        // ----------------------------------------------------------------- Notificações
        var notificacoes = new List<Notificacao>
        {
            new()
            {
                Titulo = "Bem-vindo ao Bora pro Jogo!",
                Mensagem = "Acompanhe a agenda completa do esporte da cidade e ative as notificações da sua equipe.",
                Tipo = TipoNotificacao.NovoEvento,
                Importante = true,
                CriadoEm = agora.AddDays(-12)
            }
        };

        // Resultados dos jogos encerrados viram notificações no feed.
        foreach (var ev in eventos.Where(e => e.Status == StatusEvento.Encerrado && e.Placar is not null))
        {
            notificacoes.Add(new Notificacao
            {
                Titulo = $"Resultado: {ev.Titulo}",
                Mensagem = $"Final pelo {ev.Campeonato}: {ev.Titulo.Replace(" x ", $" {ev.PlacarCasa} x {ev.PlacarVisitante} ")}.",
                Tipo = TipoNotificacao.Resultado,
                EventoId = ev.Id,
                ModalidadeId = ev.ModalidadeId,
                CriadoEm = ev.Inicio.AddHours(2)
            });
        }

        // Destaques futuros viram avisos de "novo evento".
        foreach (var ev in eventos.Where(e => e.Destaque && e.Inicio > agora))
        {
            notificacoes.Add(new Notificacao
            {
                Titulo = $"Novo na agenda: {ev.Titulo}",
                Mensagem = $"{ev.Inicio:dd/MM 'às' HH'h'} no {ev.Local!.Nome}. Não perca!",
                Tipo = TipoNotificacao.NovoEvento,
                EventoId = ev.Id,
                ModalidadeId = ev.ModalidadeId,
                Importante = true,
                CriadoEm = agora.AddDays(-1)
            });
        }

        await db.Notificacoes.AddRangeAsync(notificacoes, ct);
        await db.SaveChangesAsync(ct);
    }
}
