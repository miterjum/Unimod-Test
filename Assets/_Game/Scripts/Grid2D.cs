using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Grid2D : MonoBehaviourSingleton<Grid2D>
{
    public Vector3 gridWorldSize;
    public float nodeRadius;
    public Node2D[,] Grid;
    public Tilemap obstaclemap;
    public List<Node2D> path;
    Vector3 worldBottomLeft;

    float nodeDiameter;
    public int gridSizeX, gridSizeY;

    //void Awake()
    //{
    //    nodeDiameter = nodeRadius * 2;
    //    gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
    //    gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    //    CreateGrid();
    //}

    //private void Start()
    //{
    //    find.FindPath(find.seeker.position, find.target.position);

    //}

    [Button]
    void CreateGrid()
    {
        nodeDiameter = nodeDiameter = nodeRadius * 2;
        Grid = new Node2D[gridSizeX, gridSizeY];
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                Grid[x, y] = new Node2D(false, worldPoint, x, y);

                if (obstaclemap.HasTile(obstaclemap.WorldToCell(Grid[x, y].worldPosition)))
                    Grid[x, y].SetObstacle(true);
                else
                    Grid[x, y].SetObstacle(false);


            }
        }
    }


    public List<Node2D> GetNeighbors(Node2D node)
    {
        List<Node2D> neighbors = new List<Node2D>();

        //checks and adds top neighbor
        if (node.GridX >= 0 && node.GridX < gridSizeX && node.GridY + 1 >= 0 && node.GridY + 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX, node.GridY + 1]);

        //checks and adds bottom neighbor
        if (node.GridX >= 0 && node.GridX < gridSizeX && node.GridY - 1 >= 0 && node.GridY - 1 < gridSizeY)
            neighbors.Add(Grid[node.GridX, node.GridY - 1]);

        //checks and adds right neighbor
        if (node.GridX + 1 >= 0 && node.GridX + 1 < gridSizeX && node.GridY >= 0 && node.GridY < gridSizeY)
            neighbors.Add(Grid[node.GridX + 1, node.GridY]);

        //checks and adds left neighbor
        if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSizeX && node.GridY >= 0 && node.GridY < gridSizeY)
            neighbors.Add(Grid[node.GridX - 1, node.GridY]);

        return neighbors;
    }


    public Node2D NodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - Grid[0,0].worldPosition.x) / (nodeRadius * 2));
        int y = Mathf.RoundToInt((worldPosition.y - Grid[0,0].worldPosition.y) / (nodeRadius * 2));

        return Grid[Mathf.Abs(x), Mathf.Abs(y)];
    }


    
    //Draws visual representation of grid
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (Grid != null)
        {
            foreach (Node2D n in Grid)
            {
                if (n.obstacle)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.white;

                if (path != null && path.Contains(n))
                    Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeRadius));

            }
        }
    }

    #region Editor
    [Header("------------Editor------------")]
    public int key;

    public List<DataPath> allPaths;

    public Transform startPos;

    public Transform endPos;

    [Button]
    public void FindPath()
    {
        var seekerNode = NodeFromWorldPoint(new Vector2(startPos.position.x, startPos.position.y));
        var targetNode = NodeFromWorldPoint(new Vector2(endPos.position.x, endPos.position.y));

        List<Node2D> openSet = new List<Node2D>();
        HashSet<Node2D> closedSet = new HashSet<Node2D>();
        openSet.Add(seekerNode);

        //calculates path for pathfinding
        while (openSet.Count > 0)
        {

            //iterates through openSet and finds lowest FCost
            Node2D node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost <= node.FCost)
                {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            //If target found, retrace path
            if (node == targetNode)
            {
                RetracePath(seekerNode, targetNode);
                return;
            }

            //adds neighbor nodes to openSet
            foreach (Node2D neighbour in GetNeighbors(node))
            {
                if (neighbour.obstacle || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    [Button]
    public void ClearAllPath()
    {
        allPaths = new List<DataPath>();
    }
    void RetracePath(Node2D startNode, Node2D endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node2D currentNode = endNode;
        var test = new List<Node2D>();
        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            test.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        test.Reverse();
        this.path = test;
        allPaths.Add(new DataPath() 
        {
            startPos = startNode.worldPosition,
            endPos = endNode.worldPosition,
            path = path
        });
    }
    //gets distance between 2 nodes for calculating cost
    int GetDistance(Node2D nodeA, Node2D nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


    #endregion
}
[Serializable]
public class DataPath
{
    public Vector2 startPos;
    public Vector2 endPos;

    public List<Vector2> path;
}