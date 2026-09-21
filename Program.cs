using IntroController;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace IntroController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOpenApi();



            // Add services to the container.

            builder.Services.AddControllers();

            //IOC - Container de injeção de dependência.

            builder.Services.AddScoped<IntroAPI.Repository.MySqlDbContext>();
            builder.Services.AddScoped<IntroAPI.Repository.AlunoRepository>();
            builder.Services.AddScoped<IntroAPI.Services.AlunoService>();


            builder.Services.AddAuthentication(x =>
            {
                    //Especificando o Padrão do Token

                    //para definir que o esquema de autenticação que queremos utilizar é o Bearer e o
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                    //Diz ao asp.net que utilizamos uma autenticação interna,
                    //ou seja, ela é gerada neste servidor e vale para este servidor apenas.
                    //Não é gerado pelo google/fb
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(x =>
                {
                    //Lendo o Token

                    // Obriga uso do HTTPs
                    x.RequireHttpsMetadata = false;

                    // Configurações para leitura do Token
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Chave que usamos para gerar o Token
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("minha-chave-secreta-minha-chave-secreta")),
                        ValidAudience = "Usuários da API",
                        ValidIssuer = "Unoeste",
                        ValidateLifetime = true, // Expiração do token
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(5)

                    };
                });

            //política
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

            app.UseAuthorization();
            app.MapOpenApi();
            app.MapScalarApiReference("/doc");


            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}
