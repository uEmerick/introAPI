using IntroAPI.Entidades;
using MySql.Data.MySqlClient;

namespace IntroAPI.Repository
{
    public class CidadeRepository
    {
        private readonly MySqlDbContext _context;

        public CidadeRepository(MySqlDbContext context)
        {
            _context = context;
        }

        public bool Importar(List<Cidade> cidades)
        {
            bool sucesso = false;
            MySqlTransaction? transaction = null;

            try
            {
                var conexao = _context.GetConnection();

                // 1. Inicia a transação na conexão
                transaction = conexao.BeginTransaction();

                using (var cmd = conexao.CreateCommand())
                {
                    // 2. OBRIGATÓRIO: Associa a transação ao comando MySqlCommand
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"INSERT INTO Cidade (CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude)
                                        VALUES (@CidadeId, @Nome, @Sigla, @IBGEMunicipio, @Latitude, @Longitude)";

                    foreach (var cidade in cidades)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@CidadeId", cidade.CidadeId);
                        cmd.Parameters.AddWithValue("@Nome", cidade.Nome);
                        cmd.Parameters.AddWithValue("@Sigla", cidade.Sigla);
                        cmd.Parameters.AddWithValue("@IBGEMunicipio", cidade.IBGEMunicipio);
                        cmd.Parameters.AddWithValue("@Latitude", (object?)cidade.Latitude ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Longitude", (object?)cidade.Longitude ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    // 3. Se todas as 5.570 linhas executaram sem erro, confirma no banco
                    transaction.Commit();
                    sucesso = true;
                }
            }
            catch (Exception)
            {
                // Proteção para o Rollback só ser executado se a conexão ainda estiver aberta
                if (transaction != null && transaction.Connection != null && transaction.Connection.State == System.Data.ConnectionState.Open)
                {
                    transaction.Rollback();
                }

                throw; // Relança o erro original
            }

            return sucesso;
        }

        /// <summary>
        /// Obtém todas as cidades do banco de dados
        /// </summary>
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
        /// Obtém o total de cidades no banco de dados
        /// </summary>
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
        /// Obtém uma cidade pelo ID
        /// </summary>
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
        /// Obtém todos os estados (UFs) únicos
        /// </summary>
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
        /// Obtém todas as cidades de um estado específico
        /// </summary>
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
        /// Atualiza uma cidade existente
        /// </summary>
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
        /// Exclui uma cidade pelo ID
        /// </summary>
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