using MinimalApi.Dominio.Enuns;

namespace MinimalApi.Dominios.Models;

public record AdministradorLogado
{
    public string Email { get; set; } = default!;
    public string Perfil { get; set; } = default!;
    public string Token { get; set; } = default!;
}