using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple grid-based A* pathfinding for 2D navigation
/// </summary>
public class SimplePathfinding : MonoBehaviour
{
    public static SimplePathfinding Instance { get; private set; }
    
    [Header("Grid Settings")]
    [Tooltip("Size of each grid cell (smaller = more precise but slower)")]
    public float cellSize = 0.3f;
    
    [Tooltip("How far to extend the grid in each direction")]
    public Vector2 gridWorldSize = new Vector2(50f, 50f);
    
    [Tooltip("Center of the pathfinding grid")]
    public Vector2 gridCenter = Vector2.zero;
    
    [Tooltip("Layers that block movement")]
    public LayerMask unwalkableMask;
    
    [Tooltip("Check radius for walkability (smaller = more precise)")]
    public float walkabilityCheckRadius = 0.25f;
    
    [Tooltip("Extra padding around obstacles (helps guards not clip edges)")]
    public float obstaclePadding = 0.1f;
    
    [Header("Debug")]
    public bool showGrid = false;
    
    private Node[,] grid;
    private int gridSizeX, gridSizeY;
    
    // Node class for A* algorithm
    private class Node
    {
        public bool walkable;
        public Vector2 worldPosition;
        public int gridX;
        public int gridY;
        
        public int gCost; // Distance from start
        public int hCost; // Distance to target (heuristic)
        public Node parent;
        
        public int fCost { get { return gCost + hCost; } }
        
        public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        CreateGrid();
    }
    
    public bool IsReady()
    {
        return grid != null;
    }
    
    void CreateGrid()
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / cellSize);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / cellSize);
        grid = new Node[gridSizeX, gridSizeY];
        
        Vector2 worldBottomLeft = (Vector2)transform.position + gridCenter - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * cellSize + cellSize / 2) + Vector2.up * (y * cellSize + cellSize / 2);
                
                // Check if this cell is blocked by obstacles (with padding)
                bool walkable = !Physics2D.OverlapCircle(worldPoint, walkabilityCheckRadius + obstaclePadding, unwalkableMask);
                
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
        
        Debug.Log($"Pathfinding grid created: {gridSizeX}x{gridSizeY} cells");
    }
    
    /// <summary>
    /// Find a path from start to target position
    /// </summary>
    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);
        
        if (startNode == null || targetNode == null || !targetNode.walkable)
        {
            return null;
        }
        
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            // Path found
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }
            
            // Check neighbors
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }
                
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        
        // No path found
        return null;
    }
    
    List<Vector2> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        
        // Convert to world positions and simplify path
        List<Vector2> waypoints = new List<Vector2>();
        Vector2 directionOld = Vector2.zero;
        
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i].gridX - startNode.gridX, path[i].gridY - startNode.gridY);
            if (directionNew != directionOld || i == path.Count - 1)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        
        return waypoints;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        
        return neighbors;
    }
    
    Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector2 worldBottomLeft = (Vector2)transform.position + gridCenter - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;
        Vector2 localPos = worldPosition - worldBottomLeft;
        
        int x = Mathf.RoundToInt(localPos.x / cellSize);
        int y = Mathf.RoundToInt(localPos.y / cellSize);
        
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y];
        }
        
        return null;
    }
    
    void OnDrawGizmos()
    {
        if (!showGrid || grid == null) return;
        
        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable ? Color.white : Color.red;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (cellSize - 0.05f));
        }
    }
}

