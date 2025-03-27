using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public enum TileType
{
    //Placeholders for now cause idk what we need
    GROUND,
    WALL,
    HAZARD,
    OBJECTIVE,
    EXTRACT,
    OBSTACLE,
    DEPLOY
}

public class Tile : MonoBehaviour
{
    public Board Board;
    public int col, row;
    public Vector2Int BoardPos { get { return new Vector2Int(col, row); } }

    public TileType Type;

    public static readonly float SIDELEN = 4f;
    private bool isHighlighted = false;

    public bool IsSelected = false;
    private bool isCustomHighlighted = false;

    #region MouseEvents
    public void OnSelect()
    {
        IsSelected = true;
        GetComponent<MeshRenderer>().material = Board.selectedTileMat;
    }

    public void OnDeselect()
    {
        IsSelected = false;
        
        SetCorrectMaterial();
    }
    public void OnHover()
    {
        if (IsSelected || isCustomHighlighted)
        {
            return;
        }
        GetComponent<MeshRenderer>().material = Board.hoveredTileMat;
    }

    public void OnUnhover()
    {
        if (IsSelected || isCustomHighlighted)
        {
            return;
        }

        SetCorrectMaterial();
    }

    public void Highlight()
    {
        isHighlighted = true;
        SetCorrectMaterial();
    }

    public void Unhighlight()
    {
        isHighlighted = false;
        isCustomHighlighted = false;
        SetCorrectMaterial();
    }
    #endregion`

    #region TileSetup
    //Setup tile by passing in info
    public Tile(Board boardRef, int rowNum, int colNum)
    {
        SetupTile(boardRef, rowNum, colNum);
        this.Board = boardRef;
        this.col = colNum;
        this.row = rowNum;

        Type = TileType.GROUND;
    }

    public void SetupTile(Board boardRef, int rowNum, int colNum)
    {
        this.Board = boardRef;
        this.col = colNum;
        this.row = rowNum;

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }
    public Tile()
    {
        this.Board = null;
        this.col = -1;
        this.row = -1;

        Type = TileType.GROUND;
    }
    #endregion

    #region UnitHelperFunctions

    public Vector3 WorldPosition()
    {
        return new Vector3(SIDELEN * row, 0, SIDELEN * col);
    }

    public void HighlightWithColor(Color color)
    {
        isHighlighted = true;
        isCustomHighlighted = true;
        GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetCorrectMaterial()
    {
        if (isHighlighted) {
            GetComponent<MeshRenderer>().sharedMaterial = Board.selectedTileMat;
        }
        else if (Type == TileType.OBJECTIVE)
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.objectiveTileMat;
        }
        else if (Type == TileType.EXTRACT)
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.extractTileMat;
        }
        else if (Type == TileType.OBSTACLE)
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.obstacleTileMat;
        }
        else if (Type == TileType.HAZARD)
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.hazardTileMat;
        }
        else if (Type == TileType.DEPLOY)
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.deployTileMat;
        }
        else
        {
            GetComponent<MeshRenderer>().sharedMaterial = Board.defaultTileMat;
        }
    }
    #endregion

    public void UpdateMaterial(){
        SetCorrectMaterial();
    }

    public void ConfigMeshRenderer()
    {
        if (GetComponent<MeshRenderer>().enabled == true)
        {
            if (Type == TileType.GROUND)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else
        {
            if (Type != TileType.GROUND)
            {
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
}