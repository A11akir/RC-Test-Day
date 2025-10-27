namespace Domain
{
    public class Tile
    {
        public GridPosition Position { get; }
        public bool IsOccupied { get; set; }

        public Tile(int x, int y)
        {
            Position = new GridPosition(x, y);
            IsOccupied = false;
        }
    }
}

