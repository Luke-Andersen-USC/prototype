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

        DrawDefaultInspector();
    }
}