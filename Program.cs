using Microsoft.EntityFrameworkCore;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.DTOs;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominios.Models;
using MinimalApi.Entidades.DTOs;
using MinimalApi.Dominio.Enuns;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServicio>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
        );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => { 
    if(administradorServico.Login(loginDTO) != null){
        return Results.Ok("Login realizado com sucesso!");
    } else {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");



app.MapGet("/administradores/páginas", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => { 
    var adms = new List<AdministradorModelView>();

    var administradores = administradorServico.Todos(pagina);

    foreach (var adm in administradores) 
    {
        adms.Add(new AdministradorModelView {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }

    return Results.Ok(adms);
}).WithTags("Administradores");



app.MapGet("/Administradores/{id}/buscar", ([FromRoute] int id, IAdministradorServico administradorServico) => { 
    
    var administrador = administradorServico.BuscarPorId(id);

    if(administrador == null) return Results.NotFound();

    return Results.Ok(new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).WithTags("Administradores");



app.MapPost("/administradores/registrar", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => { 
    var validacao = new ErrosDeValidacao {
        Mensagens = new List<string>()  // Lista para armazenar as mensagens de erro
    };

    if(string.IsNullOrEmpty(administradorDTO.Email)) {
        validacao.Mensagens.Add("O Email não pode ser vazio");
    }
        if(string.IsNullOrEmpty(administradorDTO.Senha)) {
        validacao.Mensagens.Add("O Email não pode ser vazia");
    }
        if(administradorDTO.Perfil == null) {
        validacao.Mensagens.Add("O Email não pode ser vazio");
    }

    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var adm = new Administrador {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.editor.ToString()
    };

    administradorServico.Incluir(adm);

    return Results.Created($"/administrador/{adm.Perfil}", new AdministradorModelView {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
    });
}).WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao() {
        Mensagens = new List<string>()  // Lista para armazenar as mensagens de erro
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O Nome não pode ser vazio");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A Marca não pode ser vazio");

    if(veiculoDTO.Ano < 1886)
        validacao.Mensagens.Add("O primeiro veiculo a mortor foi inventado em 1886");

    return validacao;
}



app.MapPost("/veiculos/registrar", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => { 

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

}).WithTags("Veiculos");



app.MapGet("/veiculos/páginas", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => { 
    
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);

}).WithTags("Veiculos");



app.MapGet("/veiculos/{id}/buscar", ([FromRoute] int id, IVeiculoServico veiculoServico) => { 
    
    var veiculo = veiculoServico.BuscarPorId(id);

    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);

}).WithTags("Veiculos");



app.MapPut("/veiculos/{id}/editar", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => { 
    
    var veiculo = veiculoServico.BuscarPorId(id);
    if(veiculo == null) return Results.NotFound();

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");



app.MapDelete("/veiculos/{id}/deletar", ([FromRoute] int id, IVeiculoServico veiculoServico) => { 
    
    var veiculo = veiculoServico.BuscarPorId(id);

    if(veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion