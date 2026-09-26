using IntroController;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

namespace IntroController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configurar Serilog incluindo a saída no Console
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console() // <-- Adicionado para exibir as URLs e logs no terminal
                .WriteTo.File("logs/aplicacao-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            builder.Host.UseSerilog(logger);

            builder.Services.AddOpenApi();

            // Add services to the container
            builder.Services.AddControllers();

            // IoC - Container de injeção de dependência
            builder.Services.AddScoped<IntroAPI.Repository.MySqlDbContext>();

            // Repositórios
            builder.Services.AddScoped<IntroAPI.Repository.AlunoRepository>();
            builder.Services.AddScoped<IntroAPI.Repository.CidadeRepository>();

            // Serviços
            builder.Services.AddScoped<IntroAPI.Services.AlunoService>();
            builder.Services.AddScoped<IntroAPI.Services.CidadeService>();

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("minha-chave-secreta-minha-chave-secreta")),
                    ValidAudience = "Usuários da API",
                    ValidIssuer = "Unoeste",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("APIAuth", new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser().Build());
            });

            builder.Services.AddHttpContextAccessor();

            string stringConexao = builder.Configuration["StringConexao"];

            if (string.IsNullOrEmpty(stringConexao))
            {
                throw new Exception("STRING_CONEXAO não definida");
            }

            Environment.SetEnvironmentVariable("STRING_CONEXAO", stringConexao);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapOpenApi();
            app.MapScalarApiReference("/doc");

            // 2. Ordem correta dos middlewares: Autenticação SEMPRE antes da Autorização
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}