using Microsoft.EntityFrameworkCore;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.DTOs;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominios.Models;
using MinimalApi.Entidades.DTOs;
using MinimalApi.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;



#region Builder
var builder = WebApplication.CreateBuilder(args);

// Configuração da chave JWT
var key = builder.Configuration.GetValue<string>("Jwt");
if (string.IsNullOrEmpty(key)) 
    key = "123456";

// Configuração de autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = true,
        ValidateIssuer = false, // Atualize se tiver um emissor configurado
        ValidateAudience = false // Atualize se tiver uma audiência configurada
    };
});

// Configuração de autorização
builder.Services.AddAuthorization();

// Serviços da aplicação
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // Nome correto para cabeçalho
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuração do banco de dados
builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion


#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

List<Claim> GetClaims() { return new List<Claim>(); }
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador) {
    if(string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>() 
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => { 
    var adm = administradorServico.Login(loginDTO);

    if(adm != null) {
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdministradorLogado {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    } else {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores");



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
}).RequireAuthorization().WithTags("Administradores");



app.MapGet("/Administradores/{id}/buscar", ([FromRoute] int id, IAdministradorServico administradorServico) => { 
    
    var administrador = administradorServico.BuscarPorId(id);

    if(administrador == null) return Results.NotFound();

    return Results.Ok(new AdministradorModelView {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().WithTags("Administradores");



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
}).RequireAuthorization().WithTags("Administradores");
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

}).RequireAuthorization().WithTags("Veiculos");



app.MapGet("/veiculos/páginas", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => { 
    
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);

}).RequireAuthorization().WithTags("Veiculos");



app.MapGet("/veiculos/{id}/buscar", ([FromRoute] int id, IVeiculoServico veiculoServico) => { 
    
    var veiculo = veiculoServico.BuscarPorId(id);

    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);

}).RequireAuthorization().WithTags("Veiculos");



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
}).RequireAuthorization().WithTags("Veiculos");



app.MapDelete("/veiculos/{id}/deletar", ([FromRoute] int id, IVeiculoServico veiculoServico) => { 
    
    var veiculo = veiculoServico.BuscarPorId(id);

    if(veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).RequireAuthorization().WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion