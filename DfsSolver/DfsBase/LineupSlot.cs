namespace DfsBase
{
    public class LineupSlot
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return $"{Name}, Count = {Count}";
        }
    }
}
