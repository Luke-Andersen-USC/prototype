using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum BoardState
{
    DEFAULT,
    PLAYERSELECTED
}

public enum DistanceType
{
    MANHATTAN,
    CHESSBOARD,
    EUCLIDEAN
}


public class Board : MonoBehaviour
{
    [HideInInspector] public static Board Instance;

    // Tile obj to spawn
    public GameObject TilePrefab;

    // Materials for debug purposes
    public Material defaultTileMat;
    public Material selectedTileMat;

    //Max number of rows and cols a level can have
    public static readonly int NUM_ROWS = 20;
    public static readonly int NUM_COLS = 10;
    
    // Data to store tiles properly
    public Tile[,] Tiles;
    public Dictionary<Tile, GameObject> tileToGameObjectMap;
    public Dictionary<GameObject, Tile> gameObjectToTileMap;   

    [SerializeField] private GameObject outlinePrefab;

    private void Awake()
    {
        Instance = this;
        ReadInBoardData();
    }
    public void ReadInBoardData()
    {
        Tiles = new Tile[NUM_ROWS, NUM_COLS];
        
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Board");
        foreach (GameObject tileObj in tiles)
        {
            Tile tile = tileObj.GetComponent<Tile>();
            Tiles[tile.row, tile.col] = tile;
        }
    }

    public Tile GetTileAt(int row, int col)
    {
        try
        {
            return Tiles[row, col];
        }
        catch
        {
            return null;
        }
    }

    public Tile GetTileAt(Vector2Int pos)
    {
        return GetTileAt(pos.y, pos.x);
    }

    // Searches for the corresponding Tile given a GameObject
    public Tile GetTileFromGameObject(GameObject tileObj)
    {
        if (gameObjectToTileMap.ContainsKey(tileObj))
        {
            return gameObjectToTileMap[tileObj];
        }
        else
        {
            Debug.LogError("Tile not found from GameObject!");
        }

        return null;
    }

    // Searches for the corresponding GameObject given a Tile
    public GameObject GetGameObjectFromTile(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Input tile is null!");
            return null;
        }

        if (tileToGameObjectMap.ContainsKey(tile))
        {
            return tileToGameObjectMap[tile];
        }
        else
        {
            Debug.LogError("GameObject not found from Tile!");
        }

        return null;
    }

    // Saves all tiles to the BoardTools.cs
    // Looks at every location between (0,0) and (Board.NUM_ROWS, Board.NUM_COLS) for objects, then getting their tiles and saving

    #if UNITY_EDITOR
    public void SaveLevelData()
    {
        // Saves tile information
        BoardTools tools = gameObject.GetComponent<BoardTools>();

        tools.ResetData();
        for (int row = 0; row < Board.NUM_ROWS; row++)
        {
            for (int col = 0; col < NUM_COLS; col++)
            {
                if (Physics.CheckSphere(new Vector3(row * Tile.SIDELEN, 0, col * Tile.SIDELEN), Tile.SIDELEN / 4))
                {
                    Collider[] collidersOnTile = Physics.OverlapSphere(new Vector3(row * Tile.SIDELEN, 0, col * Tile.SIDELEN), Tile.SIDELEN / 4);
                    foreach (Collider collider in collidersOnTile)
                    {
                        GameObject tileObj = collider.gameObject;

                        if (tileObj.tag != "Board")
                        {
                            continue;
                        }

                        Tile tile = tileObj.GetComponent<Tile>();

                        tile.SetupTile(this, row, col);
                        tile.UpdateMaterial();

                        tools.AddTileToData(tile);
                    }
                }
            }
        }

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(tools);
    }
    #endif
}