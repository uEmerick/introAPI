using System.Security.Claims;

namespace IntroController
{
    public class UsuarioAutenticado
    {

        private readonly IHttpContextAccessor _contextAccessor;

        public string Nome { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }
        public string CPF { get; set; }

        public List<string> Permissoes { get; set; }
        public UsuarioAutenticado(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext != null)
            {
                //Nome = httpContext.User.Identity?.Name;
                Nome = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                Role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                Id = httpContext.User.Claims.FirstOrDefault(a => a.Type == "id")?.Value;
                CPF = httpContext.User.Claims.FirstOrDefault(a => a.Type == "cpf")?.Value;

            }

        }
    }
}
