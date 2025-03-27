using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class exists to save the tile data for a level
public class BoardTools : MonoBehaviour
{
    public Board board;

    public List<Tile> boardData;
    public List<Tile> spawnTiles;

    public void AddTileToData(Tile t)
    {
        boardData.Add(t);
    }

    public void ResetData()
    {
        boardData = new List<Tile>();
        spawnTiles = new List<Tile>();
    }
}

public static class ExtensionFunctions
{
    public static Vector3 RoundXZ(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.z = Mathf.Round(v.z);

        return v;
    }

    public static Vector3 RoundXZResetY(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.z = Mathf.Round(v.z);

        v.y = 0;

        return v;
    }
}