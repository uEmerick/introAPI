using IntroAPI.Entidades;
using MySql.Data.MySqlClient;
using System.Text;

namespace IntroAPI.Repository
{
    /// <summary>
    /// Repositório responsável por gerenciar as operações de persistência e consulta da entidade <see cref="Cidade"/> no banco MySQL.
    /// Utiliza ADO.NET com comandos SQL parametrizados.
    /// </summary>
    public class CidadeRepository
    {
        private readonly MySqlDbContext _context;

        /// <summary>
        /// Inicializa o repositório injetando o contexto de conexão com o banco de dados.
        /// </summary>
        /// <param name="context">Instância do contexto de banco de dados <see cref="MySqlDbContext"/>.</param>
        public CidadeRepository(MySqlDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Realiza a importação ou atualização em massa (Bulk Upsert) de uma lista de cidades usando transação e processamento em lotes.
        /// </summary>
        /// <param name="cidades">Lista de entidades <see cref="Cidade"/> a serem inseridas ou atualizadas.</param>
        /// <returns><c>true</c> se a importação de todos os lotes for concluída com sucesso.</returns>
        /// <exception cref="Exception">Lançada em caso de erro na execução SQL; aciona o Rollback da transação.</exception>
        public bool Importar(List<Cidade> cidades)
        {
            bool sucesso = false;
            MySqlTransaction? transaction = null;

            try
            {
                var conexao = _context.GetConnection();
                transaction = conexao.BeginTransaction();

                // Define o tamanho de cada lote para evitar estourar o limite de parâmetros por comando SQL
                int tamanhoLote = 1000;

                for (int i = 0; i < cidades.Count; i += tamanhoLote)
                {
                    var lote = cidades.Skip(i).Take(tamanhoLote).ToList();

                    using var cmd = conexao.CreateCommand();
                    cmd.Transaction = transaction;

                    var sqlBuilder = new StringBuilder();
                    sqlBuilder.AppendLine(@"INSERT INTO Cidade (CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude) VALUES ");

                    // Constrói dinamicamente as cláusulas VALUES com parâmetros indexados por item do lote
                    for (int j = 0; j < lote.Count; j++)
                    {
                        if (j > 0) sqlBuilder.Append(", ");
                        sqlBuilder.Append($"(@CidadeId{j}, @Nome{j}, @Sigla{j}, @IBGEMunicipio{j}, @Latitude{j}, @Longitude{j})");
                        
                        cmd.Parameters.AddWithValue($"@CidadeId{j}", lote[j].CidadeId);
                        cmd.Parameters.AddWithValue($"@Nome{j}", lote[j].Nome);
                        cmd.Parameters.AddWithValue($"@Sigla{j}", lote[j].Sigla);
                        cmd.Parameters.AddWithValue($"@IBGEMunicipio{j}", lote[j].IBGEMunicipio);
                        cmd.Parameters.AddWithValue($"@Latitude{j}", (object?)lote[j].Latitude ?? DBNull.Value);
                        cmd.Parameters.AddWithValue($"@Longitude{j}", (object?)lote[j].Longitude ?? DBNull.Value);
                    }

                    // Se a chave primária (CidadeId) já existir, atualiza os campos existentes (Upsert)
                    sqlBuilder.Append(@" ON DUPLICATE KEY UPDATE 
                                        Nome = VALUES(Nome), 
                                        Sigla = VALUES(Sigla), 
                                        IBGEMunicipio = VALUES(IBGEMunicipio), 
                                        Latitude = VALUES(Latitude), 
                                        Longitude = VALUES(Longitude);");
                        
                    cmd.CommandText = sqlBuilder.ToString();
                    cmd.ExecuteNonQuery();
                }

                // Confirma todas as alterações no banco de dados se todos os lotes foram executados sem erro
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                // Garante que as alterações sejam desfeitas em caso de erro, caso a transação ainda esteja aberta
                if (transaction != null && transaction.Connection != null && transaction.Connection.State == System.Data.ConnectionState.Open)
                {
                    transaction.Rollback();
                }
                throw;
            }

            return sucesso;
        }

        /// <summary>
        /// Recupera a lista completa de cidades cadastradas no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todas as instâncias de <see cref="Cidade"/> encontradas.</returns>
        /// <exception cref="Exception">Lançada caso ocorra falha ao executar a consulta no banco.</exception>
        public List<Cidade> ObterTodas()
        {
            var cidades = new List<Cidade>();

            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "SELECT CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude FROM Cidade";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cidades.Add(new Cidade
                            {
                                CidadeId = Convert.ToInt32(reader["CidadeId"]),
                                Nome = reader["Nome"].ToString() ?? string.Empty,
                                Sigla = reader["Sigla"].ToString() ?? string.Empty,
                                IBGEMunicipio = Convert.ToInt32(reader["IBGEMunicipio"]),
                                Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : null,
                                Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter cidades: {ex.Message}", ex);
            }

            return cidades;
        }

        /// <summary>
        /// Retorna a quantidade total de registros de cidades cadastrados no banco de dados.
        /// </summary>
        /// <returns>O número total de registros como inteiro.</returns>
        /// <exception cref="Exception">Lançada caso ocorra erro ao executar a contagem.</exception>
        public int ObterTotal()
        {
            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Cidade";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter total de cidades: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca os dados de uma cidade específica através de seu identificador único.
        /// </summary>
        /// <param name="id">Identificador único (CidadeId) da cidade.</param>
        /// <returns>A instância da <see cref="Cidade"/> encontrada ou <c>null</c> se o registro não existir.</returns>
        /// <exception cref="Exception">Lançada em caso de falha na consulta.</exception>
        public Cidade? ObterPorId(int id)
        {
            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "SELECT CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude FROM Cidade WHERE CidadeId = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Cidade
                            {
                                CidadeId = Convert.ToInt32(reader["CidadeId"]),
                                Nome = reader["Nome"].ToString() ?? string.Empty,
                                Sigla = reader["Sigla"].ToString() ?? string.Empty,
                                IBGEMunicipio = Convert.ToInt32(reader["IBGEMunicipio"]),
                                Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : null,
                                Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : null
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter cidade por ID: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Obtém a lista com todas as siglas das unidades federativas (UF/Estados) cadastradas, em ordem alfabética e sem duplicidades.
        /// </summary>
        /// <returns>Lista de strings com as siglas dos estados (ex: "AC", "AL", "SP").</returns>
        /// <exception cref="Exception">Lançada caso ocorra falha durante a execução da consulta.</exception>
        public List<string> ObterEstados()
        {
            var estados = new List<string>();

            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "SELECT DISTINCT Sigla FROM Cidade ORDER BY Sigla";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var sigla = reader["Sigla"].ToString();
                            if (!string.IsNullOrEmpty(sigla))
                                estados.Add(sigla);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter estados: {ex.Message}", ex);
            }

            return estados;
        }

        /// <summary>
        /// Busca todas as cidades pertencentes a um estado (UF) específico, ordenadas pelo nome.
        /// </summary>
        /// <param name="uf">Sigla do estado desejado (ex: "SP"). A busca é insensível a maiúsculas/minúsculas.</param>
        /// <returns>Lista de objetos <see cref="Cidade"/> pertencentes ao estado informado.</returns>
        /// <exception cref="Exception">Lançada em caso de erro na consulta SQL.</exception>
        public List<Cidade> ObterPorEstado(string uf)
        {
            var cidades = new List<Cidade>();

            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "SELECT CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude FROM Cidade WHERE UPPER(Sigla) = UPPER(@UF) ORDER BY Nome";
                    cmd.Parameters.AddWithValue("@UF", uf);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cidades.Add(new Cidade
                            {
                                CidadeId = Convert.ToInt32(reader["CidadeId"]),
                                Nome = reader["Nome"].ToString() ?? string.Empty,
                                Sigla = reader["Sigla"].ToString() ?? string.Empty,
                                IBGEMunicipio = Convert.ToInt32(reader["IBGEMunicipio"]),
                                Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : null,
                                Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : null
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter cidades por estado: {ex.Message}", ex);
            }

            return cidades;
        }

        /// <summary>
        /// Atualiza os dados de uma cidade já existente no banco de dados.
        /// </summary>
        /// <param name="id">Identificador único da cidade a ser modificada.</param>
        /// <param name="cidade">Objeto <see cref="Cidade"/> contendo as novas informações.</param>
        /// <returns><c>true</c> se o registro foi atualizado com sucesso; <c>false</c> se a cidade não for encontrada.</returns>
        /// <exception cref="Exception">Lançada em caso de falha na instrução SQL de UPDATE.</exception>
        public bool Atualizar(int id, Cidade cidade)
        {
            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Cidade 
                                        SET Nome = @Nome, Sigla = @Sigla, IBGEMunicipio = @IBGEMunicipio, 
                                            Latitude = @Latitude, Longitude = @Longitude 
                                        WHERE CidadeId = @CidadeId";

                    cmd.Parameters.AddWithValue("@CidadeId", id);
                    cmd.Parameters.AddWithValue("@Nome", cidade.Nome);
                    cmd.Parameters.AddWithValue("@Sigla", cidade.Sigla);
                    cmd.Parameters.AddWithValue("@IBGEMunicipio", cidade.IBGEMunicipio);
                    cmd.Parameters.AddWithValue("@Latitude", (object?)cidade.Latitude ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Longitude", (object?)cidade.Longitude ?? DBNull.Value);

                    int linhasAfetadas = cmd.ExecuteNonQuery();
                    return linhasAfetadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar cidade: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove o registro de uma cidade do banco de dados com base em seu ID.
        /// </summary>
        /// <param name="id">Identificador único (CidadeId) do registro a ser excluído.</param>
        /// <returns><c>true</c> se a cidade foi excluída; <c>false</c> se o registro não existir.</returns>
        /// <exception cref="Exception">Lançada em caso de falha na instrução SQL de DELETE (ex: restrição de chave estrangeira).</exception>
        public bool Excluir(int id)
        {
            try
            {
                var conexao = _context.GetConnection();

                using (var cmd = conexao.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Cidade WHERE CidadeId = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);

                    int linhasAfetadas = cmd.ExecuteNonQuery();
                    return linhasAfetadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir cidade: {ex.Message}", ex);
            }
        }
    }
}