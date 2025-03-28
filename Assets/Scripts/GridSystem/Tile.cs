using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Board Board;
    public int col, row;
    public Vector2Int BoardPos { get { return new Vector2Int(col, row); } }

    public static readonly float SIDELEN = 4f;
    private bool isHighlighted = false;

    public GameObject Balloon = null;
    public void Highlight()
    {
        isHighlighted = true;
        SetCorrectMaterial();
    }

    public void Unhighlight()
    {
        isHighlighted = false;
        SetCorrectMaterial();
    }

    #region TileSetup
    public Tile(Board boardRef, int rowNum, int colNum)
    {
        SetupTile(boardRef, rowNum, colNum);
        this.Board = boardRef;
        this.col = colNum;
        this.row = rowNum;
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
        GetComponent<MeshRenderer>().material.color = color;
    }

    public void SetCorrectMaterial()
    {
        if (isHighlighted) {
            GetComponent<MeshRenderer>().sharedMaterial = Board.selectedTileMat;
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
}