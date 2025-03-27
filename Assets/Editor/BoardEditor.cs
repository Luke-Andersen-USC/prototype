using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor
{
    Board myBoard;

    public override void OnInspectorGUI()
    {
        //Adds default inspector elements (data members from Board.cs)
        DrawDefaultInspector();

        //Target is default ref to the base class
        myBoard = (Board)target;

        //Button to spawn a tile at -2, -2
        GUILayout.Space(25);

        //EditorGUILayout.HelpBox("Use this to spawn tiles instead of the prefab (not sure why but prefab breaks it)\nObjective tiles are locations for capture/defend objectives", MessageType.Info);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Default Tile"))
        {
            SpawnDefaultTile();
        }
        if (GUILayout.Button("Spawn Objective Tile"))
        {
            SpawnObjectiveTile();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(25);

        //Button to create generic map
        GUILayout.Space(25);
        EditorGUILayout.HelpBox("Creates a " + Board.NUM_ROWS + "x" + Board.NUM_COLS + " rectangular map", MessageType.Info);
        if (GUILayout.Button("Generate Default Map"))
        {
            //Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("BoardManager"), "Clear All Tiles");
            GenerateMap();
        }

        //Button to save level dat to BoardTools.cs
        GUILayout.Space(25);
        EditorGUILayout.HelpBox("This saves the level data in scripting, will also be called automatically if scene is saved", MessageType.Info);
        if (GUILayout.Button("Save Level Data"))
        {
            myBoard.SaveLevelData();
        }
    }

    //Spawns a tile at -2, -2
    public void SpawnDefaultTile()
    {
        Vector3 pos = new Vector3(Tile.SIDELEN * -2, 0, Tile.SIDELEN * -2);

        // Use PrefabUtility.InstantiatePrefab to maintain prefab link
        GameObject tilePrefab = myBoard.TilePrefab as GameObject; // Ensure this is the prefab you want to spawn
        GameObject tileObj = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
        tileObj.transform.position = pos;
        tileObj.transform.rotation = Quaternion.identity;
        tileObj.transform.SetParent(myBoard.transform); // Set the parent to myBoard

        Undo.RegisterCreatedObjectUndo(tileObj, "Spawned Tile");
    }

    //Spawns an objective tile at -2, -2
    public void SpawnObjectiveTile()
    {
        Vector3 pos = new Vector3(Tile.SIDELEN * -2, 0, Tile.SIDELEN * -2);

        // Use PrefabUtility.InstantiatePrefab to maintain prefab link
        GameObject tilePrefab = myBoard.TilePrefab as GameObject; // Ensure this is the prefab you want to spawn
        GameObject tileObj = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
        tileObj.transform.position = pos;
        tileObj.transform.rotation = Quaternion.identity;
        tileObj.transform.SetParent(myBoard.transform); // Set the parent to myBoard

        // Set tile type and material
        tileObj.GetComponent<Tile>().Type = TileType.OBJECTIVE;
        tileObj.GetComponent<MeshRenderer>().material = myBoard.objectiveTileMat;

        Undo.RegisterCreatedObjectUndo(tileObj, "Spawned Objective Tile");
    }

    // Generating a random map as placeholder so i can see if map works, will need new system when levels are created
    // Currently cued by an editor button
    public void GenerateMap()
    {
        myBoard.Tiles = new Tile[Board.NUM_ROWS, Board.NUM_COLS];

        for (int row = 0; row < Board.NUM_ROWS; row++)
        {
            for (int col = 0; col < Board.NUM_COLS; col++)
            {
                Vector3 pos = new Vector3(Tile.SIDELEN * row, 0, Tile.SIDELEN * col);

                // Use PrefabUtility to maintain a link to the prefab
                GameObject tileObj = PrefabUtility.InstantiatePrefab(myBoard.TilePrefab, myBoard.transform) as GameObject;
                if (tileObj != null)
                {
                    tileObj.transform.position = pos;
                    tileObj.name = string.Format("TILE: {0},{1}", row, col);

                    Tile tile = tileObj.GetComponent<Tile>();
                    tile.row = row;
                    tile.col = col;
                    tile.Board = myBoard;
                }
            }
        }
    }

    //Creates an option in top-screen options menu to delete all tile objects in scene
    [MenuItem("Tools/Level Design/Clear Tiles")]
    private static void ClearAllTiles()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Board");
        Undo.RegisterFullObjectHierarchyUndo(GameObject.FindGameObjectWithTag("BoardManager"), "Clear All Tiles");
        foreach(GameObject go in tiles)
        {
            DestroyImmediate(go);
        }
    }
}