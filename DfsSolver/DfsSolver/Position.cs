namespace DfsSolver
{
    public class Position
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return $"{Name}({Id})";
        }
    }
}
