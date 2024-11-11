using MinimalApi.DTOs;
using MinimalApi.Entidades.DTOs;
using MinimalApi.Dominio.Interfaces;
using System.Data.Common;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorServicio : IAdministradorServico
{
    private readonly DbContexto _contexto;

    public AdministradorServicio(DbContexto contexto) 
    {
        _contexto = contexto;
    }


    public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return adm;
    }
}