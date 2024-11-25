using MinimalApi.Dominio.Enuns;

namespace MinimalApi.Dominios.Models;

public record AdministradorModelView 
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = "*******";
    public string Perfil { get; set; } = default!;
}