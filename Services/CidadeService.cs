using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using IntroAPI.Repository;
using IntroAPI.Entidades;

namespace IntroAPI.Services
{
    public class CidadeService
    {
        private readonly CidadeRepository _cidadeRepository;

        public CidadeService(CidadeRepository cidadeRepository)
        {
            _cidadeRepository = cidadeRepository;
        }

        public bool ImportarCsv(IFormFile arquivo)
        {
            // Validação: arquivo nulo ou vazio
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("O arquivo CSV é inválido ou está vazio.");

            // Validação: extensão do arquivo
            if (!arquivo.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O arquivo deve ter extensão .csv");

            // Validação: tamanho máximo (10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (arquivo.Length > maxFileSize)
                throw new ArgumentException("O arquivo não pode exceder 10MB.");

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",", // Altere para ";" se o seu CSV usar ponto e vírgula
                    IgnoreBlankLines = true,
                    TrimOptions = TrimOptions.Trim
                };

                using var stream = arquivo.OpenReadStream();
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                using var csv = new CsvReader(reader, config);

                // Mapeia síncronamente as linhas do CSV para a classe Cidade
                List<Cidade> cidades = csv.GetRecords<Cidade>().ToList();
                
                // Remove duplicatas com base no ID da cidade
                cidades = cidades.GroupBy(c => c.CidadeId).Select(g => g.First()).ToList();
                
                // Validação: verifica se há cidades para importar
                if (!cidades.Any())
                    throw new ArgumentException("O arquivo CSV não contém dados válidos.");

                // Validação: verifica integridade básica dos dados
                var cidadesInvalidas = cidades.Where(c => 
                    c.CidadeId <= 0 || 
                    string.IsNullOrWhiteSpace(c.Nome) || 
                    string.IsNullOrWhiteSpace(c.Sigla) || 
                    c.IBGEMunicipio <= 0).ToList();

                if (cidadesInvalidas.Any())
                    throw new ArgumentException($"Encontradas {cidadesInvalidas.Count} linhas com dados incompletos ou inválidos.");

                // Envia a lista tratada para o banco de dados
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

        // Métodos adicionais para CRUD
        public List<Cidade> ObterTodas()
        {
            return _cidadeRepository.ObterTodas();
        }

        public int ObterTotal()
        {
            return _cidadeRepository.ObterTotal();
        }

        public Cidade? ObterPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID deve ser maior que zero.");

            return _cidadeRepository.ObterPorId(id);
        }

        public List<string> ObterEstados()
        {
            return _cidadeRepository.ObterEstados();
        }

        public List<Cidade> ObterPorEstado(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf) || uf.Length != 2)
                throw new ArgumentException("A sigla do estado deve conter 2 caracteres.");

            return _cidadeRepository.ObterPorEstado(uf.ToUpper());
        }

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

        public bool Excluir(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID deve ser maior que zero.");

            return _cidadeRepository.Excluir(id);
        }
    }
}