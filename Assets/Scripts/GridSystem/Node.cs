using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Tile Tile { get; private set; } // Tile can be read publicly but set privately
    public List<Node> Neighbors { get; private set; }

    public Node Parent { get; set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost { get { return GCost + HCost; } }

    void Awake()
    {
        Neighbors = new List<Node>();
    }

    public void Initialize(Tile tile)
    {
        Tile = tile;
    }

    public void AddNeighbor(Node neighbor)
    {
        if (!Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }
}

