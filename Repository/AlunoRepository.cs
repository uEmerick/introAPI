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
                        cmd.CommandText = $@"insert into Aluno(Nome, CPF)
                                       values (@Nome, @CPF)";
                    }
                    else
                    {
                        cmd.CommandText = $@"update Aluno
                                     set Nome = @Nome, 
                                         CPF = @CPF
                                     where Id = @Id";

                        cmd.Parameters.AddWithValue("@Id", aluno.Id);
                    }

                    cmd.Parameters.AddWithValue("@Nome", aluno.Nome);
                    cmd.Parameters.AddWithValue("@CPF", aluno.CPF);

                    //INSERT, DELETE, UPDATE E SP
                    cmd.ExecuteNonQuery();

                    if (aluno.Id == 0)
                        aluno.Id = (int)cmd.LastInsertedId;

                    sucesso = true;
                }

            }
            catch (Exception ex)
            {
                throw;
                //serilog...
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
                    cmd.CommandText = $@"select count(*) 
                                     from Aluno
                                     where CPF = @CPF";

                    //funções de agregação/ count/min/sum
                    existente = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    //DateTime data = Convert.ToDateTime(cmd.ExecuteScalar());
                }

            }
            catch (Exception ex)
            {
                throw;
                //serilog...
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


                    cmd.CommandText = $@"select Id, Nome, CPF
                                     from Aluno
                                     where Id = " + id;


                    var dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        aluno = new IntroController.Entidades.Aluno();
                        //aluno.Id = Convert.ToInt32(dr["Id"]);
                        //aluno.Nome = dr["Nome"].ToString();
                        //aluno.CPF = dr["CPF"].ToString();

                        aluno.Id = dr.GetInt32("Id");
                        aluno.Nome = dr.GetString("Nome");
                        aluno.CPF = dr.GetString("CPF");

                        //aluno.Id = Convert.ToInt32(dr[0]);
                        //aluno.Nome = dr[1].ToString();
                        //aluno.CPF = dr[2].ToString();
                    }

                }

            }
            catch (Exception ex)
            {
                throw;
                //serilog...
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

                    cmd.CommandText = $@"select Id, Nome, CPF
                                     from Aluno
                                     where Nome like @Nome";

                    cmd.Parameters.AddWithValue("@Nome", "%" + nome + "%");
                    var dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        var aluno = new IntroController.Entidades.Aluno();
                        aluno.Id = dr.GetInt32("Id");
                        aluno.Nome = dr.GetString("Nome");
                        aluno.CPF = dr.GetString("CPF");
                        alunos.Add(aluno);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
                //serilog...
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

                    cmd.CommandText = $@"delete from Aluno
                                         where Id = " + id;

                    cmd.ExecuteNonQuery();
                    excluido = true;
                }
               
            }
            catch (Exception ex)
            {
                throw;
                //serilog...
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
                        if (aluno.Id == 0)
                        {
                            cmd.CommandText = $@"insert into Aluno(Nome, CPF)
                                       values (@Nome, @CPF)";
                        }
                        else
                        {
                            cmd.CommandText = $@"update Aluno
                                     set Nome = @Nome, 
                                         CPF = @CPF
                                     where Id = @Id";

                            cmd.Parameters.AddWithValue("@Id", aluno.Id);
                        }

                        cmd.Parameters.AddWithValue("@Nome", aluno.Nome);
                        cmd.Parameters.AddWithValue("@CPF", aluno.CPF);

                        //INSERT, DELETE, UPDATE E SP
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
                transacao.Rollback();
                throw;
                //serilog...
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

                    cmd.CommandText = $@"select count(*) 
                                         from Aluno";
                    conta = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw;
                //serilog...
            }

            return conta;
        }


    }
}
