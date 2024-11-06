using Microsoft.EntityFrameworkCore;
using MinimalApi.Entidades.DTOs;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext{

    private readonly IConfiguration _configurationAppSettings;
    public DbContexto(IConfiguration configurationAppSettings) 
    {
        _configurationAppSettings = configurationAppSettings;
    }
    public DbSet<Administrador> Administradores { get; set; } =default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador {
                Id = 1,
                Email = "administrador@example.com",
                Senha = "admin123",
                Perfil = "adm"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) 
        {
            var stringConexao = _configurationAppSettings.GetConnectionString("mysql")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(stringConexao, 
                ServerVersion.AutoDetect(stringConexao));
            }
        }
    }
}