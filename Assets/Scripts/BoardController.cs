using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController instance;

    private int xSize, ySize;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArroy;

    private Tile oldSelectTile;
    private Vector2[] dirRay = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right};
    
    private bool isFindMatch = false;
    private bool isShift = false;
    private bool isSearchEmptyTile = false;
    private int count;


    public void SetValye(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite)
    {
        this.tileArroy = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
    }

    private void Awake()
    { 
        instance = this;
    }

    private void Update()
    {
        if (isSearchEmptyTile)
        {
            SearchEmptyTile();  
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if  (ray != false)
            {
                ChecSelectTile(ray.collider.gameObject.GetComponent<Tile>());   
            }
        }
    }
    private void SelectTile(Tile tile)
    {
        tile.isSelect = true;
        tile.spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        oldSelectTile = tile;   
    }

    private void DeselectTile(Tile tile)
    {
        tile.isSelect = false;
        tile.spriteRenderer.color = new Color(1, 1, 1);
        oldSelectTile = null;
    }

    private void ChecSelectTile(Tile tile)
    {
        if (tile.isEmpty || isShift)
            return;
        if (tile.isSelect)
        {
            DeselectTile(tile);
        }
        else
        {
            if (!tile.isSelect && oldSelectTile == null)
                SelectTile(tile);
            else
            {
                // если 2й выбранный тайл сосед предыдущего
                if (AdjacentTile().Contains(tile))
                {
                    SwapTwoTile(tile);
                    FindAllMatch(tile);
                    DeselectTile(oldSelectTile);
                }
                // новое выделение, забываем старый тайл
                else
                {
                    DeselectTile(oldSelectTile);
                    SelectTile(tile);

                }
            }
        }
    }

    private void SwapTwoTile(Tile tile)
    {
        if (oldSelectTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
            return;
        Sprite cashSprite = oldSelectTile.spriteRenderer.sprite;
        oldSelectTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
        tile.spriteRenderer.sprite = cashSprite;

        UI.Instance.Moves(1);
    }

    private List<Tile> AdjacentTile()
    {
        List<Tile> cashTile = new List<Tile>();
        for (int i = 0; i < dirRay.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(oldSelectTile.transform.position, dirRay[i]);
            if (hit.collider != null)
            {
                cashTile.Add(hit.collider.gameObject.GetComponent<Tile>());
            }
        }
        return cashTile;
    }

    private List<Tile> FindMatch(Tile tile, Vector2 dir)
    {
        List<Tile> cashFindTiles = new List<Tile>();
        RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, dir);
        while (hit.collider != null &&
            hit.collider.gameObject.GetComponent<Tile>().spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            cashFindTiles.Add(hit.collider.gameObject.GetComponent<Tile>());
            hit = Physics2D.Raycast(hit.collider.transform.position, dir);
        }
        return cashFindTiles;
    }

    private void DeleteSprite(Tile tile, Vector2[] dirArray)
    {
        List<Tile> cashFindSprite = new List<Tile>();
        for (int i = 0; i < dirArray.Length; i++)
        {
            cashFindSprite.AddRange(FindMatch(tile, dirArray[i]));
        }
        if (cashFindSprite.Count >= 2)
        {
            for (int i = 0; i < cashFindSprite.Count; i++)
            {
                cashFindSprite[i].spriteRenderer.sprite = null;
            }
            isFindMatch = true;
        }
    }

    private void FindAllMatch(Tile tile)
    {
        if (tile.isEmpty)
            return;
        DeleteSprite(tile, new Vector2[2] { Vector2.up, Vector2.down});
        DeleteSprite(tile, new Vector2[2] { Vector2.left, Vector2.right });
        if (isFindMatch)
        {
            isFindMatch = false;
            tile.spriteRenderer.sprite = null;
            isSearchEmptyTile = true;
        }
    }

    //метод поиска пустого тайла
    private void SearchEmptyTile()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = ySize - 1; y > -1; y--)
            {
                if (tileArroy[x, y].isEmpty)
                {
                    ShiftTileDown(x, y);
                }
            }
        }
        isSearchEmptyTile = false;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                FindAllMatch(tileArroy[x, y]);
            }
        }

    }
    
    //метод "падени€" тайлов сверху, заполн€ющих пустые клетки
    public void ShiftTileDown(int xPos, int yPos)
    {
        isShift = true;
        for (int y = yPos; y < ySize - 1; y++)
        {
            if (!tileArroy[xPos, y + 1].isEmpty)
            {
                Tile tile = tileArroy[xPos, y];
                tile.spriteRenderer.sprite = tileArroy[xPos, y + 1].spriteRenderer.sprite;
            }
        }
        UI.Instance.Score(50);
        tileArroy[xPos, ySize - 1].spriteRenderer.sprite = tileSprite[Random.Range(0, tileSprite.Count)];
        isShift = false;
    }

    private void SetNewSprite(int xPos, List<SpriteRenderer> renderer)
    {
        for (int y = 0; y < renderer.Count - 1; y++)
        {
            renderer[y].sprite = renderer[y + 1].sprite;
            renderer[y + 1].sprite = GetNewSprite(xPos, ySize - 1);
        }
    }
    private Sprite GetNewSprite(int xPos, int yPos)
    {
        List<Sprite> cashSprite = new List<Sprite>();
        cashSprite.AddRange(tileSprite);

        if (xPos > 0)
        {
            cashSprite.Remove(tileArroy[xPos - 1, yPos].spriteRenderer.sprite);
        }
        if (xPos < xSize - 1)
        {
            cashSprite.Remove(tileArroy[xPos + 1, yPos].spriteRenderer.sprite);
        }
        if (yPos > 0)
        {
            cashSprite.Remove(tileArroy[xPos, yPos - 1].spriteRenderer.sprite);
        }

        return cashSprite[Random.Range(0, cashSprite.Count)];
    }
}
