namespace IntroAPI.Entidades
{
    /// <summary>
    /// Representa a entidade de domínio Cidade (município) cadastrada no banco de dados da aplicação.
    /// </summary>
    public class Cidade
    {
        /// <summary>
        /// Identificador único (Chave Primária) da cidade no banco de dados.
        /// </summary>
        public int CidadeId { get; set; }

        /// <summary>
        /// Nome oficial do município (ex: "Presidente Prudente", "São Paulo").
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Sigla do estado/UF ao qual a cidade pertence (ex: "SP", "RJ").
        /// </summary>
        public string Sigla { get; set; }

        /// <summary>
        /// Código oficial do município segundo a tabela do IBGE (Instituto Brasileiro de Geografia e Estatística).
        /// </summary>
        public int IBGEMunicipio { get; set; }

        /// <summary>
        /// Coordenada geográfica de latitude do centro ou ponto de referência da cidade.
        /// Nulo caso não cadastrado.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Coordenada geográfica de longitude do centro ou ponto de referência da cidade.
        /// Nulo caso não cadastrado.
        /// </summary>
        public double? Longitude { get; set; }
    }
}