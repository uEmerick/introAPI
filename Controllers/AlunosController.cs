using IntroAPI.Services;
using IntroController.Controllers.DTOS;
using IntroController.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntroController.Controllers
{

    /// <summary>
    /// Gerencimetno de alunos
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    //[Authorize("APIAuth")  ]
    public class AlunosController : ControllerBase
    {
        private readonly AlunoService _alunoService;

        public AlunosController(AlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        /// <summary>
        /// Obtém um aluno pelo Id
        /// </summary>
        /// <param name="id">Id do aluno</param>
        /// <returns>Aluno encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Obter(int id)
        {

            try
            {
                var aluno = new Aluno();

                if (id <= 0)
                {
                    return BadRequest("Id inválido");
                }

                var alunoExistente = _alunoService.Obter(id);

                if (alunoExistente != null)
                {
                    AlunoResponse response = new();
                    response.Id = alunoExistente.Id;
                    response.Nome = alunoExistente.Nome;

                    return Ok(response);
                }
                else
                {
                    return NotFound("Não encontrado");
                }
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }


        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ObterTodos()
        {
            try
            {
                List<AlunoResponse> response = new();

                foreach (var item in  _alunoService.Consultar("%"))
                {
                    AlunoResponse dto = new();
                    dto.Id = item.Id;
                    dto.Nome = item.Nome;
                    response.Add(dto);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }


        [HttpGet("consultar")]
        public IActionResult Consultar(string nome)
        {

            try
            {
                List<AlunoResponse> response = new();

                var alunos = _alunoService.Consultar(nome).ToList();

                foreach (var item in alunos)
                {
                    AlunoResponse dto = new();
                    dto.Id = item.Id;
                    dto.Nome = item.Nome;
                    response.Add(dto);
                }

                return Ok(alunos);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

        }



        [HttpGet("quantidade")]
        public IActionResult ObterQuantidade()
        {

            try
            {
                return Ok(_alunoService.TotalAlunos());
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Criar(DTOS.AlunoCriarRequest request)
        {
            try
            { 
                var aluno = new Aluno();
                aluno.Id = 0;
                aluno.Nome = request.Nome;
                aluno.CPF = request.CPF;
                _alunoService.Criar(aluno);
                return Created();
            }
            catch (ArgumentNullException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            catch (ArgumentException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (InvalidOperationException ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Alterar(int id, DTOS.AlunoAlterarRequest request)
        {

            try
            {
                if (id <= 0)
                {
                    return BadRequest("Id inválido");
                }

                var aluno = _alunoService.Obter(id);
                if (aluno == null)
                {
                    return NotFound();
                }
                else
                {
                    aluno.Nome = request.Nome;
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }



        //[HttpPatch("{id}")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public IActionResult AlterarParcial(int id, DTOS.AlunoAlterarParcialRequest request)
        //{
        //    try
        //    {
        //        if (id <= 0)
        //        {
        //            return BadRequest("Id inválido");
        //        }

        //        var aluno = FakeBD.FirstOrDefault(a => a.Id == id);
        //        if (aluno == null)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            if (!string.IsNullOrEmpty(request.Nome))
        //            {
        //                aluno.Nome = request.Nome;
        //            }

        //            return NoContent();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(
        //            title: "Erro inesperado",
        //            detail: ex.Message,
        //            statusCode: StatusCodes.Status500InternalServerError
        //        );
        //    }
        //}

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Excluir(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Id inválido");
                }

                _alunoService.Excluir(id);

               return NoContent();
               
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Erro inesperado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }


    }
}
