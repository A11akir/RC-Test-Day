using Domain;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class CheckerboardBackground : MonoBehaviour
{
    [Header("Tilemap & Colors")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Color colorLight;
    [SerializeField] private Color colorDark;
    private int pixelsPerTile = 32;
    
    
    private GridMap _gridMap;
    
    public GridMap GridMap => _gridMap;
    private void Awake()
    {
        GenerateCheckerboard();
    }

    
    public void ForceInitGridMap()
    {
        if (_gridMap != null) return;

        BoundsInt bounds = tilemap.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;

        _gridMap = new GridMap(width, height);
    }
    
    private void GenerateCheckerboard()
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;

        int texWidth = width * pixelsPerTile;
        int texHeight = height * pixelsPerTile;

        Texture2D texture = new Texture2D(texWidth, texHeight);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                if (!tilemap.HasTile(cellPos)) continue;

                bool isLight = (x + y) % 2 == 0;
                Color c = isLight ? colorLight : colorDark;
                
                for (int py = 0; py < pixelsPerTile; py++)
                {
                    for (int px = 0; px < pixelsPerTile; px++)
                    {
                        int texX = x * pixelsPerTile + px;
                        int texY = y * pixelsPerTile + py;
                        texture.SetPixel(texX, texY, c);
                    }
                }
            }
        }

        texture.Apply();
        
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texWidth, texHeight),
            new Vector2(0.5f, 0.5f),
            pixelsPerTile
        );

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }
}