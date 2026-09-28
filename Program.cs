using IntroController;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

namespace IntroController
{
    /// <summary>
    /// Classe principal responsável por inicializar e configurar a aplicação Web API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Ponto de entrada (entry point) da aplicação ASP.NET Core.
        /// Configura os serviços de Injeção de Dependência, Logs, Autenticação, Autorização e o Pipeline HTTP.
        /// </summary>
        /// <param name="args">Argumentos de linha de comando passados na execução do sistema.</param>
        public static void Main(string[] args)
        {
            // ----------------------------------------------------------------------------------
            // 1. INICIALIZAÇÃO DO BUILDER DA APLICAÇÃO
            // ----------------------------------------------------------------------------------
            // Cria o construtor da aplicação Web, responsável por carregar configurações (appsettings.json,
            // variáveis de ambiente) e registrar serviços no Container de Injeção de Dependência (IoC).
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------------------------------------------------------------
            // 2. CONFIGURAÇÃO DO SERILOG (SISTEMA DE LOGS)
            // ----------------------------------------------------------------------------------
            // Define o comportamento e os destinos (Sinks) dos logs da aplicação.
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Define o nível mínimo para gravação de logs (Information, Warning, Error, etc.)
                .WriteTo.Console()          // Redireciona a saída de logs para o terminal/console
                .WriteTo.File(              // Configura a gravação de logs em arquivos de texto
                    path: "logs/aplicacao-.txt",                           // Caminho e prefixo dos arquivos de log
                    rollingInterval: RollingInterval.Day,                 // Cria um novo arquivo de log a cada dia
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}" // Formatação da mensagem
                )
                .CreateLogger();

            // Registra o Serilog como o provedor oficial de logs da hospedagem (Host) do ASP.NET Core
            builder.Host.UseSerilog(logger);

            // ----------------------------------------------------------------------------------
            // 3. REGISTRO DE SERVIÇOS E CONTROLADORES (IoC / DI)
            // ----------------------------------------------------------------------------------
            // Registra os geradores de documentação OpenAPI (Swagger/Scalar)
            builder.Services.AddOpenApi();

            // Habilita o suporte ao padrão MVC / Web API baseada em Controllers
            builder.Services.AddControllers();

            // Injeção de Dependência - Contexto do Banco de Dados MySQL (Tempo de vida: Scoped - por requisição HTTP)
            builder.Services.AddScoped<IntroAPI.Repository.MySqlDbContext>();

            // Injeção de Dependência - Camada de Repositórios (Acesso aos dados)
            builder.Services.AddScoped<IntroAPI.Repository.AlunoRepository>();
            builder.Services.AddScoped<IntroAPI.Repository.CidadeRepository>();

            // Injeção de Dependência - Camada de Serviços (Regras de negócio)
            builder.Services.AddScoped<IntroAPI.Services.AlunoService>();
            builder.Services.AddScoped<IntroAPI.Services.CidadeService>();

            // ----------------------------------------------------------------------------------
            // 4. CONFIGURAÇÃO DA AUTENTICAÇÃO VIA TOKENS JWT
            // ----------------------------------------------------------------------------------
            builder.Services.AddAuthentication(x =>
            {
                // Define o esquema padrão de autenticação para JWT Bearer
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false; // Permite requisições sem HTTPS (util para ambiente de desenvolvimento local)
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    // Chave secreta simétrica para validação da assinatura do token
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("minha-chave-secreta-minha-chave-secreta")),
                    ValidAudience = "Usuários da API", // Público esperado do token
                    ValidIssuer = "Unoeste",           // Emissor válido do token
                    ValidateLifetime = true,          // Garante que o token expirado seja recusado
                    ValidateIssuerSigningKey = true,  // Exige que a chave de assinatura seja válida
                    ClockSkew = TimeSpan.FromMinutes(5) // Tolerância máxima para diferença de horário entre servidores
                };
            });

            // ----------------------------------------------------------------------------------
            // 5. CONFIGURAÇÃO DAS POLÍTICAS DE AUTORIZAÇÃO
            // ----------------------------------------------------------------------------------
            builder.Services.AddAuthorization(options =>
            {
                // Cria a política de acesso "APIAuth" que exige um usuário autenticado via JwtBearer
                options.AddPolicy("APIAuth", new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build());
            });

            // Permite acessar as informações da requisição HTTP (HttpContext) em qualquer classe registrada no IoC
            builder.Services.AddHttpContextAccessor();

            // ----------------------------------------------------------------------------------
            // 6. LEITURA E VALIDAÇÃO DAS CONFIGURAÇÕES DE BANCO DE DADOS
            // ----------------------------------------------------------------------------------
            // Recupera a string de conexão declarada no appsettings.json
            string stringConexao = builder.Configuration["StringConexao"];

            // Valida se a string de conexão foi fornecida corretamente
            if (string.IsNullOrEmpty(stringConexao))
            {
                throw new Exception("STRING_CONEXAO não definida nas configurações do projeto.");
            }

            // Atribui o valor lido a uma variável de ambiente para acesso global do sistema
            Environment.SetEnvironmentVariable("STRING_CONEXAO", stringConexao);

            // ----------------------------------------------------------------------------------
            // 7. CONSTRUÇÃO DO PIPELINE DA APLICAÇÃO (BUILD)
            // ----------------------------------------------------------------------------------
            // Compila todas as configurações e serviços e instancia a aplicação Web
            var app = builder.Build();

            // ----------------------------------------------------------------------------------
            // 8. CONFIGURAÇÃO DA DOCUMENTAÇÃO INTERATIVA DA API
            // ----------------------------------------------------------------------------------
            // Expõe os endpoints com a especificação OpenAPI
            app.MapOpenApi();
            
            // Renderiza a interface gráfica do Scalar para teste e documentação das rotas na URL "/doc"
            app.MapScalarApiReference("/doc");

            // ----------------------------------------------------------------------------------
            // 9. MIDDLEWARES DE SEGURANÇA E EXECUÇÃO
            // ----------------------------------------------------------------------------------
            // ATENÇÃO: A ordem importa!
            // 1º Habilita a Autenticação (Quem é você?)
            app.UseAuthentication();
            
            // 2º Habilita a Autorização (O que você pode fazer?)
            app.UseAuthorization();

            // Mapeia as rotas dos Controllers declarados no projeto
            app.MapControllers();

            // Inicia a aplicação e escuta por requisições HTTP na porta configurada
            app.Run();
        }
    }
}