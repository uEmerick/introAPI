namespace IntroAPI.Repository
{
    public class MySqlDbContext: IDisposable
    {

        private readonly MySql.Data.MySqlClient.MySqlConnection _conexao;

        public MySqlDbContext()
        {
            if (Environment.GetEnvironmentVariable("STRING_CONEXAO") == null)
                throw new Exception("Variável de ambiente STRING_CONEXAO não encontrada");


            string stringConexao = Environment.GetEnvironmentVariable("STRING_CONEXAO");
            _conexao = new MySql.Data.MySqlClient.MySqlConnection(stringConexao);
        }

        public MySql.Data.MySqlClient.MySqlConnection GetConnection() {

            if (_conexao.State != System.Data.ConnectionState.Open)
                _conexao.Open();

            return _conexao;
        }

        public void Dispose() { 
        
           if (_conexao.State == System.Data.ConnectionState.Open)
                _conexao.Close();

           _conexao.Dispose();
        
        }




    }
}
