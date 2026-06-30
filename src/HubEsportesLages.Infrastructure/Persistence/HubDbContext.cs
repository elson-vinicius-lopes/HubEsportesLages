using HubEsportesLages.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Persistence;

/// <summary>Contexto EF Core do Hub Esportes Lages (provider SQLite).</summary>
public class HubDbContext(DbContextOptions<HubDbContext> options) : DbContext(options)
{
    public DbSet<Modalidade> Modalidades => Set<Modalidade>();
    public DbSet<Local> Locais => Set<Local>();
    public DbSet<Equipe> Equipes => Set<Equipe>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Inscricao> Inscricoes => Set<Inscricao>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<JogadorEvento> JogadoresEvento => Set<JogadorEvento>();
    public DbSet<VotoMvp> VotosMvp => Set<VotoMvp>();
    public DbSet<Enquete> Enquetes => Set<Enquete>();
    public DbSet<OpcaoEnquete> OpcoesEnquete => Set<OpcaoEnquete>();
    public DbSet<VotoEnquete> VotosEnquete => Set<VotoEnquete>();
    public DbSet<MensagemTorcida> MensagensTorcida => Set<MensagemTorcida>();
    public DbSet<EquipeFavorita> EquipesFavoritas => Set<EquipeFavorita>();
    public DbSet<Ingresso> Ingressos => Set<Ingresso>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Modalidade>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            e.Property(x => x.Icone).HasMaxLength(16);
            e.Property(x => x.CorHex).HasMaxLength(9);
            e.Property(x => x.Descricao).HasMaxLength(400);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<Local>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Endereco).HasMaxLength(200);
            e.Property(x => x.Bairro).HasMaxLength(80);
            e.Property(x => x.Cidade).HasMaxLength(80);
            e.Property(x => x.Uf).HasMaxLength(2);
            e.Ignore(x => x.MapaUrl);
        });

        b.Entity<Equipe>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Sigla).HasMaxLength(8);
            e.Property(x => x.Escudo).HasMaxLength(16);
            e.Property(x => x.CorPrimaria).HasMaxLength(9);
            e.HasOne(x => x.Modalidade)
                .WithMany(m => m.Equipes)
                .HasForeignKey(x => x.ModalidadeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Evento>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(160).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(180).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(2000);
            e.Property(x => x.Campeonato).HasMaxLength(160);
            e.Property(x => x.ImagemUrl).HasMaxLength(400);
            e.Property(x => x.PrecoIngresso).HasPrecision(10, 2);
            e.Ignore(x => x.EhConfronto);
            e.Ignore(x => x.Placar);
            e.Ignore(x => x.AceitaInteracao);

            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.Inicio);
            e.HasIndex(x => x.Status);

            e.HasOne(x => x.Modalidade)
                .WithMany(m => m.Eventos)
                .HasForeignKey(x => x.ModalidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Local)
                .WithMany(l => l.Eventos)
                .HasForeignKey(x => x.LocalId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EquipeCasa)
                .WithMany()
                .HasForeignKey(x => x.EquipeCasaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EquipeVisitante)
                .WithMany()
                .HasForeignKey(x => x.EquipeVisitanteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Inscricao>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(160).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(40);
            e.HasIndex(x => x.Email);

            e.HasOne(x => x.Modalidade)
                .WithMany()
                .HasForeignKey(x => x.ModalidadeId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Equipe)
                .WithMany()
                .HasForeignKey(x => x.EquipeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Notificacao>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(160).IsRequired();
            e.Property(x => x.Mensagem).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.CriadoEm);

            e.HasOne(x => x.Evento)
                .WithMany(ev => ev.Notificacoes)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Modalidade)
                .WithMany()
                .HasForeignKey(x => x.ModalidadeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ----------------------------------------------------- Interação da torcida
        b.Entity<JogadorEvento>(e =>
        {
            e.Property(x => x.Nome).HasMaxLength(120).IsRequired();
            e.HasIndex(x => x.EventoId);

            e.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Equipe)
                .WithMany()
                .HasForeignKey(x => x.EquipeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<VotoMvp>(e =>
        {
            e.Property(x => x.TorcedorId).HasMaxLength(64).IsRequired();
            // 1 voto de MVP por torcedor por evento.
            e.HasIndex(x => new { x.EventoId, x.TorcedorId }).IsUnique();

            e.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Jogador)
                .WithMany()
                .HasForeignKey(x => x.JogadorEventoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Enquete>(e =>
        {
            e.Property(x => x.Pergunta).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.EventoId);

            e.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Opcoes)
                .WithOne(o => o.Enquete!)
                .HasForeignKey(o => o.EnqueteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<OpcaoEnquete>(e =>
        {
            e.Property(x => x.Texto).HasMaxLength(160).IsRequired();
        });

        b.Entity<VotoEnquete>(e =>
        {
            e.Property(x => x.TorcedorId).HasMaxLength(64).IsRequired();
            // 1 voto por torcedor por enquete.
            e.HasIndex(x => new { x.EnqueteId, x.TorcedorId }).IsUnique();

            e.HasOne(x => x.Enquete)
                .WithMany()
                .HasForeignKey(x => x.EnqueteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Opcao)
                .WithMany()
                .HasForeignKey(x => x.OpcaoEnqueteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MensagemTorcida>(e =>
        {
            e.Property(x => x.TorcedorId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Autor).HasMaxLength(80).IsRequired();
            e.Property(x => x.Texto).HasMaxLength(140).IsRequired();
            e.HasIndex(x => new { x.EventoId, x.CriadoEm });

            e.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<EquipeFavorita>(e =>
        {
            e.Property(x => x.TorcedorId).HasMaxLength(64).IsRequired();
            // 1 favorito por torcedor por equipe.
            e.HasIndex(x => new { x.TorcedorId, x.EquipeId }).IsUnique();

            e.HasOne(x => x.Equipe)
                .WithMany()
                .HasForeignKey(x => x.EquipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ----------------------------------------------------- Ingresso (QR + Pix simulado)
        b.Entity<Ingresso>(e =>
        {
            e.Property(x => x.CompradorId).HasMaxLength(160).IsRequired();
            e.Property(x => x.CompradorNome).HasMaxLength(120).IsRequired();
            e.Property(x => x.Preco).HasPrecision(10, 2);
            e.Property(x => x.Token).HasMaxLength(200);
            e.Property(x => x.TxidPix).HasMaxLength(64);
            e.Property(x => x.ValidadoPor).HasMaxLength(160);

            e.HasIndex(x => x.CompradorId);
            // Token único quando presente (ingressos pendentes ainda não têm token).
            e.HasIndex(x => x.Token).IsUnique();

            e.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
