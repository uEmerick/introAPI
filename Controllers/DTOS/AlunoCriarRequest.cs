namespace IntroController.Controllers.DTOS
{
    public class AlunoCriarRequest
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public int Idade { get; set; }
        public int CidadeId { get; set; }
    }
}
