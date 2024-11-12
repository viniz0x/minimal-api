using minimal_api.Migrations;
using MinimalApi.DTOs;
using MinimalApi.Entidades.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoServico
{
    List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

    Veiculo? BuscarPorId(int id);

    void Incluir(Veiculo veiculos);

    void Atualizar(Veiculo veiculo);

    void Apagar(Veiculo veiculo);
}