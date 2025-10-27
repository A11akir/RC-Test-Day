using UnityEngine;

namespace Domain
{
    public class GridMap
    {
        private readonly Tile[,] _tiles;
        public int Width { get; set; }
        public int Height { get; }
        private readonly int _offsetX;
        private readonly int _offsetY;
        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            
            _offsetX = width / 2;
            _offsetY = height / 2;

            _tiles = new Tile[width, height];

            for (int x = -_offsetX; x < width - _offsetX; x++)
            for (int y = -_offsetY; y < height - _offsetY; y++)
            {
                _tiles[x + _offsetX, y + _offsetY] = new Tile(x, y);
            }
        }
        
        
        public bool IsOccupiedWorld(int worldX, int worldY)
        {
            int indexX = worldX + _offsetX;
            int indexY = worldY + _offsetY;

            if (indexX < 0 || indexX >= Width || indexY < 0 || indexY >= Height)
                return true; //

            return _tiles[indexX, indexY].IsOccupied;
        }

        public void SetOccupiedWorld(int worldX, int worldY, bool value)
        {
            int indexX = worldX + _offsetX;
            int indexY = worldY + _offsetY;

            if (indexX < 0 || indexX >= Width || indexY < 0 || indexY >= Height)
                return;

            _tiles[indexX, indexY].IsOccupied = value;
        }
    }
}