using IntroController.Entidades;
using IntroAPI.Repository;

namespace IntroAPI.Services
{
    public class AlunoService
    {
        private readonly AlunoRepository _alunoRepository;

        public AlunoService(AlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
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
    }
}
