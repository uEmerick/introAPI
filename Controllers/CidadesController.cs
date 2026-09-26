using Microsoft.AspNetCore.Mvc;
using IntroAPI.Services;
using IntroAPI.Entidades;

namespace IntroAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CidadesController : ControllerBase
    {
        private readonly CidadeService _cidadeService;
        private readonly ILogger<CidadesController> _logger;

        public CidadesController(CidadeService cidadeService, ILogger<CidadesController> logger)
        {
            _cidadeService = cidadeService;
            _logger = logger;
        }

        /// <summary>
        /// Importa cidades a partir de um arquivo CSV
        /// </summary>
        /// <param name="arquivo">Arquivo CSV contendo as cidades (formato: CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude)</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPost("importar")]
        public IActionResult Importar([FromForm] IFormFile arquivo)
        {
            try
            {
                if (arquivo == null)
                {
                    _logger.LogWarning("Tentativa de importação com arquivo nulo");
                    return BadRequest(new { erro = "Arquivo não fornecido" });
                }

                bool resultado = _cidadeService.ImportarCsv(arquivo);

                if (resultado)
                {
                    _logger.LogInformation("Cidades importadas com sucesso do arquivo: {arquivo}", arquivo.FileName);
                    return Ok(new { mensagem = "Cidades importadas com sucesso!" });
                }

                _logger.LogWarning("Falha ao importar arquivo: {arquivo}", arquivo.FileName);
                return BadRequest(new { erro = "Falha ao importar o arquivo CSV." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação na importação: {mensagem}", ex.Message);
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao importar CSV");
                return StatusCode(500, new { erro = "Erro ao processar o arquivo CSV", detalhes = ex.Message });
            }
        }

        /// <summary>
        /// Retorna todas as cidades
        /// </summary>
        [HttpGet]
        public IActionResult ObterTodas()
        {
            try
            {
                var cidades = _cidadeService.ObterTodas();
                return Ok(cidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todas as cidades");
                return StatusCode(500, new { erro = "Erro ao obter cidades" });
            }
        }

        /// <summary>
        /// Retorna a quantidade total de cidades
        /// </summary>
        [HttpGet("total")]
        public IActionResult ObterTotal()
        {
            try
            {
                int total = _cidadeService.ObterTotal();
                return Ok(new { total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter total de cidades");
                return StatusCode(500, new { erro = "Erro ao obter total" });
            }
        }

        /// <summary>
        /// Retorna uma cidade pelo ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult ObterPorId(int id)
        {
            try
            {
                var cidade = _cidadeService.ObterPorId(id);
                if (cidade == null)
                {
                    _logger.LogWarning("Cidade com ID {id} não encontrada", id);
                    return NotFound(new { erro = "Cidade não encontrada" });
                }

                return Ok(cidade);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação: {mensagem}", ex.Message);
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter cidade por ID");
                return StatusCode(500, new { erro = "Erro ao obter cidade" });
            }
        }

        /// <summary>
        /// Retorna todos os estados (UFs) únicos
        /// </summary>
        [HttpGet("estados")]
        public IActionResult ObterEstados()
        {
            try
            {
                var estados = _cidadeService.ObterEstados();
                return Ok(new { estados = estados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estados");
                return StatusCode(500, new { erro = "Erro ao obter estados" });
            }
        }

        /// <summary>
        /// Retorna todas as cidades de um estado específico
        /// </summary>
        [HttpGet("estado/{uf}")]
        public IActionResult ObterPorEstado(string uf)
        {
            try
            {
                var cidades = _cidadeService.ObterPorEstado(uf);
                return Ok(cidades);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação: {mensagem}", ex.Message);
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter cidades por estado");
                return StatusCode(500, new { erro = "Erro ao obter cidades" });
            }
        }

        /// <summary>
        /// Atualiza uma cidade pelo ID
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] Cidade cidade)
        {
            try
            {
                if (cidade == null)
                    return BadRequest(new { erro = "Dados da cidade não fornecidos" });

                bool resultado = _cidadeService.Atualizar(id, cidade);
                if (resultado)
                {
                    _logger.LogInformation("Cidade {id} atualizada com sucesso", id);
                    return Ok(new { mensagem = "Cidade atualizada com sucesso" });
                }

                _logger.LogWarning("Falha ao atualizar cidade {id}", id);
                return NotFound(new { erro = "Cidade não encontrada" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação: {mensagem}", ex.Message);
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cidade");
                return StatusCode(500, new { erro = "Erro ao atualizar cidade" });
            }
        }

        /// <summary>
        /// Exclui uma cidade pelo ID
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Excluir(int id)
        {
            try
            {
                bool resultado = _cidadeService.Excluir(id);
                if (resultado)
                {
                    _logger.LogInformation("Cidade {id} excluída com sucesso", id);
                    return Ok(new { mensagem = "Cidade excluída com sucesso" });
                }

                _logger.LogWarning("Cidade {id} não encontrada para exclusão", id);
                return NotFound(new { erro = "Cidade não encontrada" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erro de validação: {mensagem}", ex.Message);
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cidade");
                return StatusCode(500, new { erro = "Erro ao excluir cidade" });
            }
        }
    }
}