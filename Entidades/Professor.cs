namespace IntroController.Entidades
{

    public class Professor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; } = true;

        public List<int> CursosIds { get; set; } = new List<int>();

    }


}
