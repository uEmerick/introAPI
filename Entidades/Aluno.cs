namespace IntroController.Entidades
{

    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        /// <summary>
        /// Armazena a foto do aluno em base64
        /// </summary>
        public byte[]? Foto { get; set; }


    }


}
