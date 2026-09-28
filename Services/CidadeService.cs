using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using IntroAPI.Repository;
using IntroAPI.Entidades;

namespace IntroAPI.Services
{
    /// <summary>
    /// Classe de serviço responsável por aplicar as regras de negócio, validações de dados
    /// e orquestrar as operações relacionadas à entidade <see cref="Cidade"/>, incluindo a importação via arquivos CSV.
    /// </summary>
    public class CidadeService
    {
        private readonly CidadeRepository _cidadeRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de cidades injetando a camada de repositório.
        /// </summary>
        /// <param name="cidadeRepository">Instância de <see cref="CidadeRepository"/> para acesso ao banco de dados.</param>
        public CidadeService(CidadeRepository cidadeRepository)
        {
            _cidadeRepository = cidadeRepository;
        }

        /// <summary>
        /// Processa a leitura, validação e importação em lote de registros de cidades a partir de um arquivo CSV.
        /// </summary>
        /// <param name="arquivo">O arquivo CSV enviado na requisição HTTP (<see cref="IFormFile"/>).</param>
        /// <returns><c>true</c> se os dados forem lidos, validados e salvos no banco de dados com sucesso.</returns>
        /// <exception cref="ArgumentException">
        /// Lançada quando o arquivo é nulo, possui extensão incorreta, excede o limite de tamanho (10MB),
        /// não possui dados válidos, contém inconsistências nos campos obrigatórios ou falha no parse do CSV.
        /// </exception>
        public bool ImportarCsv(IFormFile arquivo)
        {
            // Validação 1: Verifica se o arquivo foi enviado e se possui conteúdo
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("O arquivo CSV é inválido ou está vazio.");

            // Validação 2: Restringe o tipo do arquivo pela extensão .csv
            if (!arquivo.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O arquivo deve ter extensão .csv");

            // Validação 3: Limita o tamanho do upload a no máximo 10 Megabytes
            const long maxFileSize = 10 * 1024 * 1024; // 10MB em bytes
            if (arquivo.Length > maxFileSize)
                throw new ArgumentException("O arquivo não pode exceder 10MB.");

            try
            {
                // Configuração de parsing da biblioteca CsvHelper
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,    // Indica que a primeira linha do arquivo é o cabeçalho
                    Delimiter = ",",            // Caractere separador das colunas
                    IgnoreBlankLines = true,     // Ignora linhas totalmente em branco
                    TrimOptions = TrimOptions.Trim // Remove espaços sobressalentes nas extremidades dos textos
                };

                using var stream = arquivo.OpenReadStream();
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                using var csv = new CsvReader(reader, config);

                // Mapeia síncronamente as colunas do CSV para instâncias do modelo Cidade
                List<Cidade> cidades = csv.GetRecords<Cidade>().ToList();
                
                // Trata a lista removendo duplicatas do arquivo com base no identificador único CidadeId
                cidades = cidades.GroupBy(c => c.CidadeId).Select(g => g.First()).ToList();
                
                // Validação 4: Garante que a lista resultante não ficou vazia após o parse
                if (!cidades.Any())
                    throw new ArgumentException("O arquivo CSV não contém dados válidos.");

                // Validação 5: Filtra registros que possuem campos obrigatórios inválidos
                var cidadesInvalidas = cidades.Where(c => 
                    c.CidadeId <= 0 || 
                    string.IsNullOrWhiteSpace(c.Nome) || 
                    string.IsNullOrWhiteSpace(c.Sigla) || 
                    c.IBGEMunicipio <= 0).ToList();

                if (cidadesInvalidas.Any())
                    throw new ArgumentException($"Encontradas {cidadesInvalidas.Count} linhas com dados incompletos ou inválidos.");

                // Envia a lista tratada e validada para persistência no banco de dados
                return _cidadeRepository.Importar(cidades);
            }
            catch (HeaderValidationException ex)
            {
                throw new ArgumentException($"Erro ao ler cabeçalho do CSV: {ex.Message}", ex);
            }
            catch (ReaderException ex)
            {
                throw new ArgumentException($"Erro ao processar linhas do CSV: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém a listagem completa de cidades cadastradas no sistema.
        /// </summary>
        /// <returns>Lista de objetos <see cref="Cidade"/>.</returns>
        public List<Cidade> ObterTodas()
        {
            return _cidadeRepository.ObterTodas();
        }

        /// <summary>
        /// Recupera a contagem total de cidades cadastradas no banco de dados.
        /// </summary>
        /// <returns>A quantidade total de cidades registradas.</returns>
        public int ObterTotal()
        {
            return _cidadeRepository.ObterTotal();
        }

        /// <summary>
        /// Busca os detalhes de uma cidade específica informando o seu identificador.
        /// </summary>
        /// <param name="id">Identificador único (CidadeId) da cidade buscada.</param>
        /// <returns>Objeto <see cref="Cidade"/> correspondente ou <c>null</c> caso não seja encontrada.</returns>
        /// <exception cref="ArgumentException">Lançada caso o ID informado seja menor ou igual a zero.</exception>
        public Cidade? ObterPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID deve ser maior que zero.");

            return _cidadeRepository.ObterPorId(id);
        }

        /// <summary>
        /// Retorna todas as siglas de estados (UFs) distintas cadastradas na base de dados.
        /// </summary>
        /// <returns>Lista com as siglas únicas dos estados.</returns>
        public List<string> ObterEstados()
        {
            return _cidadeRepository.ObterEstados();
        }

        /// <summary>
        /// Busca todas as cidades pertencentes a uma determinada sigla de estado (UF).
        /// </summary>
        /// <param name="uf">Sigla do estado com 2 caracteres (ex: "SP", "RJ").</param>
        /// <returns>Lista de cidades pertencentes ao estado informado.</returns>
        /// <exception cref="ArgumentException">Lançada se a sigla informada estiver em branco ou não possuir exatamente 2 caracteres.</exception>
        public List<Cidade> ObterPorEstado(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
                throw new ArgumentException("A sigla do estado deve conter 2 caracteres.");

            return _cidadeRepository.ObterPorEstado(uf.ToUpper());
        }

        /// <summary>
        /// Valida os campos obrigatórios de uma cidade e solicita a atualização dos dados no repositório.
        /// </summary>
        /// <param name="id">Identificador único da cidade a ser atualizada.</param>
        /// <param name="cidade">Instância de <see cref="Cidade"/> contendo os novos dados.</param>
        /// <returns><c>true</c> se a atualização for realizada com sucesso.</returns>
        /// <exception cref="ArgumentException">
        /// Lançada se o ID for inválido, o nome estiver em branco, a sigla for diferente de 2 caracteres
        /// ou o código IBGE for menor ou igual a zero.
        /// </exception>
        public bool Atualizar(int id, Cidade cidade)
        {
            if (id <= 0)
                throw new ArgumentException("O ID deve ser maior que zero.");

            if (string.IsNullOrWhiteSpace(cidade.Nome))
                throw new ArgumentException("O nome da cidade não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(cidade.Sigla) || cidade.Sigla.Length != 2)
                throw new ArgumentException("A sigla deve conter exatamente 2 caracteres.");

            if (cidade.IBGEMunicipio <= 0)
                throw new ArgumentException("O código IBGE deve ser maior que zero.");

            return _cidadeRepository.Atualizar(id, cidade);
        }

        /// <summary>
        /// Executa as validações prévias e solicita a exclusão de uma cidade pelo identificador.
        /// </summary>
        /// <param name="id">Identificador único da cidade a ser removida.</param>
        /// <returns><c>true</c> se o registro foi excluído no banco de dados.</returns>
        /// <exception cref="ArgumentException">Lançada caso o ID fornecido seja menor ou igual a zero.</exception>
        public bool Excluir(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID deve ser maior que zero.");

            return _cidadeRepository.Excluir(id);
        }
    }
}