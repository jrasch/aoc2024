namespace CeresSearch
{
    public readonly struct Cell(int value, int x, int y)
    {
        public int Value { get; } = value;
        public int X { get; } = x;
        public int Y { get; } = y;

        public override string ToString() => Value.ToString();
    }
}
