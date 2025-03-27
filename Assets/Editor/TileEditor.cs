using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(Tile)), CanEditMultipleObjects]
public class TileEditor : Editor
{
    List<Tile> tiles;

    public override void OnInspectorGUI()
    {
        tiles = new List<Tile>();
        foreach(Object obj in targets)
            tiles.Add((Tile)obj);

        EditorGUI.BeginChangeCheck();
        TileType oldType = tiles[0].Type;

        DrawDefaultInspector();

        //tile = (Tile)target;

        //If type type changed, update material
        if (EditorGUI.EndChangeCheck())
        {
            if (oldType != tiles[0].Type)
            {
                TileType newType = tiles[0].Type;
                List<Object> origMats = new List<Object>();
                foreach (Tile tile in tiles)
                    origMats.Add(tile.gameObject.GetComponent<MeshRenderer>());
                    
                Undo.RecordObjects(origMats.ToArray(), "Changed Tile Type");

                foreach (Tile tile in tiles)
                    tile.UpdateMaterial();
            }
        }
    }
}