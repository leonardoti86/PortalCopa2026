using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Models.Copa;
using PortalCopa2026.Models.Simulacao;

namespace PortalCopa2026.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<Selecao> Selecoes => Set<Selecao>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<Jogo> Jogos => Set<Jogo>();
    public DbSet<RankingFifa> RankingsFifa => Set<RankingFifa>();
    public DbSet<Simulacao> Simulacoes => Set<Simulacao>();
    public DbSet<SimulacaoJogo> SimulacaoJogos => Set<SimulacaoJogo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.Property(g => g.Codigo).IsRequired().HasMaxLength(1);
            entity.HasIndex(g => g.Codigo).IsUnique();
        });

        modelBuilder.Entity<Selecao>(entity =>
        {
            entity.Property(s => s.Nome).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Codigo).IsRequired().HasMaxLength(3);
            entity.Property(s => s.Tecnico).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.Codigo).IsUnique();

            entity.HasOne(s => s.Grupo)
                .WithMany(g => g.Selecoes)
                .HasForeignKey(s => s.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Jogador>(entity =>
        {
            entity.Property(j => j.Nome).IsRequired().HasMaxLength(150);
            entity.Property(j => j.Posicao).IsRequired().HasMaxLength(50);

            entity.HasOne(j => j.Selecao)
                .WithMany(s => s.Jogadores)
                .HasForeignKey(j => j.SelecaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Jogo>(entity =>
        {
            entity.Property(j => j.Fase).IsRequired().HasMaxLength(50);
            entity.Property(j => j.Cidade).IsRequired().HasMaxLength(100);
            entity.Property(j => j.Estadio).IsRequired().HasMaxLength(100);

            entity.HasOne(j => j.Grupo)
                .WithMany()
                .HasForeignKey(j => j.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.SelecaoMandante)
                .WithMany()
                .HasForeignKey(j => j.SelecaoMandanteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.SelecaoVisitante)
                .WithMany()
                .HasForeignKey(j => j.SelecaoVisitanteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RankingFifa>(entity =>
        {
            entity.HasIndex(r => r.SelecaoId).IsUnique();

            entity.HasOne(r => r.Selecao)
                .WithOne(s => s.RankingFifa)
                .HasForeignKey<RankingFifa>(r => r.SelecaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Simulacao>(entity =>
        {
            entity.Property(s => s.DataCriacao).IsRequired();
        });

        modelBuilder.Entity<SimulacaoJogo>(entity =>
        {
            entity.HasOne(sj => sj.Simulacao)
                .WithMany(s => s.Jogos)
                .HasForeignKey(sj => sj.SimulacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sj => sj.Jogo)
                .WithMany()
                .HasForeignKey(sj => sj.JogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
