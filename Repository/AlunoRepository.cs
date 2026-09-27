using IntroAPI.Repository;

namespace IntroAPI.Repository
{
    public class AlunoRepository
    {
        private readonly MySqlDbContext _context;

        public AlunoRepository(MySqlDbContext context)
        {
            _context = context;
        }

        public bool Salvar(IntroController.Entidades.Aluno aluno)
        {
            bool sucesso = false;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    if (aluno.Id == 0)
                    {
                        cmd.CommandText = @"INSERT INTO Aluno(Nome, CPF) VALUES (@Nome, @CPF)";
                    }
                    else
                    {
                        cmd.CommandText = @"UPDATE Aluno SET Nome = @Nome, CPF = @CPF WHERE Id = @Id";
                        cmd.Parameters.AddWithValue("@Id", aluno.Id);
                    }

                    cmd.Parameters.AddWithValue("@Nome", aluno.Nome);
                    cmd.Parameters.AddWithValue("@CPF", aluno.CPF);

                    cmd.ExecuteNonQuery();

                    if (aluno.Id == 0)
                        aluno.Id = (int)cmd.LastInsertedId;

                    sucesso = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return sucesso;
        }

        public bool AlunoExistente(string cpf)
        {
            bool existente = false;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM Aluno WHERE CPF = @CPF";
                    cmd.Parameters.AddWithValue("@CPF", cpf);

                    existente = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return existente;
        }

        public IntroController.Entidades.Aluno Obter(int id)
        {
            IntroController.Entidades.Aluno aluno = null;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Nome, CPF FROM Aluno WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);

                    // 💡 CORREÇÃO: O 'using' fecha o DataReader logo após a leitura
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            aluno = new IntroController.Entidades.Aluno
                            {
                                Id = dr.GetInt32("Id"),
                                Nome = dr.GetString("Nome"),
                                CPF = dr.GetString("CPF")
                            };
                        }
                    } // <- O DataReader é fechado e a conexão fica livre!
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return aluno;
        }

        public IEnumerable<IntroController.Entidades.Aluno> Consulta(string nome)
        {
            List<IntroController.Entidades.Aluno> alunos = new List<IntroController.Entidades.Aluno>();

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Nome, CPF FROM Aluno WHERE Nome LIKE @Nome";
                    cmd.Parameters.AddWithValue("@Nome", "%" + nome + "%");

                    // 💡 CORREÇÃO: O 'using' garante o fechamento do leitor
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var aluno = new IntroController.Entidades.Aluno
                            {
                                Id = dr.GetInt32("Id"),
                                Nome = dr.GetString("Nome"),
                                CPF = dr.GetString("CPF")
                            };
                            alunos.Add(aluno);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return alunos;
        }

        public bool Excluir(int id)
        {
            bool excluido = false;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Aluno WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                    excluido = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return excluido;
        }

        public bool SalvarLote(List<IntroController.Entidades.Aluno> alunos)
        {
            bool sucesso = false;
            MySql.Data.MySqlClient.MySqlTransaction transacao = null;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    transacao = _context.GetConnection().BeginTransaction();

                    foreach (var aluno in alunos)
                    {
                        cmd.Parameters.Clear(); // Limpa parâmetros das iterações anteriores

                        if (aluno.Id == 0)
                        {
                            cmd.CommandText = @"INSERT INTO Aluno(Nome, CPF) VALUES (@Nome, @CPF)";
                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE Aluno SET Nome = @Nome, CPF = @CPF WHERE Id = @Id";
                            cmd.Parameters.AddWithValue("@Id", aluno.Id);
                        }

                        cmd.Parameters.AddWithValue("@Nome", aluno.Nome);
                        cmd.Parameters.AddWithValue("@CPF", aluno.CPF);

                        cmd.ExecuteNonQuery();

                        if (aluno.Id == 0)
                            aluno.Id = (int)cmd.LastInsertedId;
                    }

                    transacao.Commit();
                    sucesso = true;
                }
            }
            catch (Exception ex)
            {
                transacao?.Rollback();
                throw;
            }

            return sucesso;
        }

        public int Contar()
        {
            int conta = 0;

            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM Aluno";
                    conta = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return conta;
        }

        /// <summary>
        /// Salva a foto de um aluno
        /// </summary>
        public bool SalvarFoto(int alunoId, byte[] fotoBytes)
        {
            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Aluno SET Foto = @Foto WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", alunoId);
                    cmd.Parameters.AddWithValue("@Foto", fotoBytes);

                    int linhasAfetadas = cmd.ExecuteNonQuery();
                    return linhasAfetadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar foto do aluno: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deleta a foto de um aluno
        /// </summary>
        public bool DeletarFoto(int alunoId)
        {
            try
            {
                using (var cmd = _context.GetConnection().CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Aluno SET Foto = NULL WHERE Id = @Id";
                    cmd.Parameters.AddWithValue("@Id", alunoId);

                    int linhasAfetadas = cmd.ExecuteNonQuery();
                    return linhasAfetadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao deletar foto do aluno: {ex.Message}", ex);
            }
        }
    }
}