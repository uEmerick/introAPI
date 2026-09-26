using IntroController.Entidades;
using IntroAPI.Repository;

namespace IntroAPI.Services
{
    public class AlunoService
    {
        private readonly AlunoRepository _alunoRepository;
        private readonly ILogger<AlunoService> _logger;

        public AlunoService(AlunoRepository alunoRepository, ILogger<AlunoService> logger)
        {
            _alunoRepository = alunoRepository;
            _logger = logger;
        }

        public bool Criar(Aluno aluno)
        {

            if (aluno == null)
            {
                throw new ArgumentNullException("Aluno não fornecido");
            }

            if (string.IsNullOrWhiteSpace(aluno.CPF))
            {
                throw new ArgumentException("O CPF do aluno não pode ser vazio.");
            }

            if (_alunoRepository.AlunoExistente(aluno.CPF))
            {
                throw new InvalidOperationException("Já existe um aluno com este CPF.");
            }

            //regras, regras...

            return _alunoRepository.Salvar(aluno);

        }

        public bool Alterar(Aluno aluno)
        {

            //aplica regra de negócio
            return _alunoRepository.Salvar(aluno);

        }

        public Aluno Obter(int id)
        {
            return _alunoRepository.Obter(id);
        }

        public IEnumerable<Aluno> Consultar(string nome)
        {
            return _alunoRepository.Consulta(nome);
        }

        public int TotalAlunos()
        {
            return _alunoRepository.Contar();
        }


        public void Excluir(int id)
        {
            _alunoRepository.Excluir(id);

        }

        public bool AlunoExistente(string cpf)
        {
            return _alunoRepository.AlunoExistente(cpf);
        }

        /// <summary>
        /// Faz upload da foto de um aluno
        /// </summary>
        public bool SalvarFoto(int alunoId, IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("O arquivo de foto é inválido.");

            // Validação: aceitar apenas imagens
            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
                throw new ArgumentException("Apenas imagens (JPG, PNG, GIF, BMP) são permitidas.");

            // Validação: tamanho máximo 5MB
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (arquivo.Length > maxFileSize)
                throw new ArgumentException("A foto não pode exceder 5MB.");

            try
            {
                using var stream = new MemoryStream();
                arquivo.CopyTo(stream);
                byte[] fotoBytes = stream.ToArray();

                _logger.LogInformation("Foto do aluno {alunoId} carregada com sucesso. Tamanho: {tamanho} bytes", alunoId, fotoBytes.Length);

                return _alunoRepository.SalvarFoto(alunoId, fotoBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar foto do aluno {alunoId}", alunoId);
                throw;
            }
        }

        /// <summary>
        /// Obtém a foto de um aluno em base64
        /// </summary>
        public string? ObterFotoBase64(int alunoId)
        {
            try
            {
                var aluno = _alunoRepository.Obter(alunoId);

                if (aluno == null)
                    throw new ArgumentException("Aluno não encontrado.");

                if (aluno.Foto == null || aluno.Foto.Length == 0)
                    return null;

                string base64 = Convert.ToBase64String(aluno.Foto);
                _logger.LogInformation("Foto do aluno {alunoId} retornada em base64", alunoId);

                return base64;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter foto do aluno {alunoId}", alunoId);
                throw;
            }
        }

        /// <summary>
        /// Deleta a foto de um aluno
        /// </summary>
        public bool DeletarFoto(int alunoId)
        {
            try
            {
                _logger.LogInformation("Foto do aluno {alunoId} deletada", alunoId);
                return _alunoRepository.DeletarFoto(alunoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar foto do aluno {alunoId}", alunoId);
                throw;
            }
        }
    }
}
