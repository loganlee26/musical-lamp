using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public RuleTile wallTile;
    public RuleTile floorTile;
    public RuleTile startTile;
    public RuleTile endTile;
    public RuleTile doorTile;
    public RuleTile keyTile;

    public int width = 10;
    public int height = 10;

    private int[,] maze;
    private List<Vector2Int> solutionPath = new List<Vector2Int>();
    private Vector2Int doorPos;
    private Vector2Int keyPos;

    void Start()
    {
        maze = new int[width + 2, height + 2];
        GenerateMaze(1, 1);
        FindSolutionPath();
        PlaceDoor();
        PlaceKey();
        DrawMaze();
    }

    public void GenerateMaze(int startX, int startY)
    {
        for (int x = 0; x < width + 2; x++)
            for (int y = 0; y < height + 2; y++)
                maze[x, y] = 0;

        DFS(startX, startY);

        maze[1, 1] = 1;
        maze[width, height] = 1;

        EnsurePathToTopRight();
    }

    void DFS(int x, int y)
    {
        maze[x, y] = 1;

        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 2),
            new Vector2Int(2, 0),
            new Vector2Int(0, -2),
            new Vector2Int(-2, 0)
        };

        directions = ShuffleDirections(directions);

        foreach (var dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            if (IsInBounds(newX, newY) && maze[newX, newY] == 0)
            {
                maze[x + dir.x / 2, y + dir.y / 2] = 1;
                DFS(newX, newY);
            }
        }
    }

    Vector2Int[] ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int j = Random.Range(0, directions.Length);
            var temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
        return directions;
    }

    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width + 1 && y > 0 && y < height + 1;
    }

    public void FindSolutionPath()
    {
        solutionPath.Clear();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = new Vector2Int(width, height);
        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;

            foreach (var dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left })
            {
                Vector2Int neighbor = current + dir;
                if (IsInBounds(neighbor.x, neighbor.y) &&
                    maze[neighbor.x, neighbor.y] == 1 &&
                    !cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Reconstruct path
        Vector2Int pathPos = end;
        while (pathPos != start)
        {
            solutionPath.Add(pathPos);
            pathPos = cameFrom[pathPos];
        }
        solutionPath.Add(start);
        solutionPath.Reverse();
    }

    public void PlaceDoor()
    {
        // Move the door closer to the end, e.g., one third of the way through the solution path
        int doorIndex = solutionPath.Count * 2 / 3; // Move door closer to the end
        doorPos = solutionPath[doorIndex];
        maze[doorPos.x, doorPos.y] = 0; // Wall until unlocked
    }

    public void PlaceKey()
    {
        // Try finding a valid key location, retry if it fails
        bool keyPlaced = false;
        int maxRetries = 10;  // Limit the number of retries to avoid an infinite loop
        int retries = 0;

        while (!keyPlaced && retries < maxRetries)
        {
            // BFS from start, avoid the door tile
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            List<Vector2Int> candidates = new List<Vector2Int>();

            Vector2Int start = new Vector2Int(1, 1);
            queue.Enqueue(start);
            visited.Add(start);

            // Perform BFS to find a valid location for the key
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                // Ensure the candidate is not on the solution path or door
                if (!solutionPath.Contains(current) && current != doorPos)
                    candidates.Add(current);

                foreach (var dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left })
                {
                    Vector2Int neighbor = current + dir;
                    if (IsInBounds(neighbor.x, neighbor.y) &&
                        maze[neighbor.x, neighbor.y] == 1 &&
                        !visited.Contains(neighbor) &&
                        neighbor != doorPos)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                // Choose a random candidate for the key's position
                keyPos = candidates[Random.Range(0, candidates.Count)];
                keyPlaced = true;
            }
            else
            {
                // If no valid place for the key was found, regenerate the maze and retry
                Debug.LogWarning("Could not find a valid place for the key, regenerating maze...");
                GenerateMaze(1, 1);  // Regenerate the maze
                FindSolutionPath();  // Find the solution path again
                PlaceDoor();  // Place the door again
                retries++;
            }
        }

        // If we exhausted retries, log a warning
        if (!keyPlaced)
        {
            Debug.LogError("Failed to place the key after multiple attempts.");
            keyPos = new Vector2Int(1, 1); // Fallback in case of failure
        }
    }


    public void DrawMaze()
    {
        for (int x = 0; x < width + 2; x++)
        {
            for (int y = 0; y < height + 2; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (x == doorPos.x && y == doorPos.y)
                {
                    tilemap.SetTile(cellPosition, doorTile);
                }
                else if (x == keyPos.x && y == keyPos.y)
                {
                    tilemap.SetTile(cellPosition, keyTile);
                }
                else if (maze[x, y] == 0)
                {
                    tilemap.SetTile(cellPosition, wallTile);
                }
                else
                {
                    tilemap.SetTile(cellPosition, floorTile);
                    if (x == 1 && y == 1)
                        tilemap.SetTile(cellPosition, startTile);
                    else if (x == width && y == height)
                        tilemap.SetTile(cellPosition, endTile);
                }
            }
        }
    }

    void EnsurePathToTopRight()
    {
        int endX = width;
        int endY = height;

        if (IsInBounds(endX - 1, endY) && maze[endX - 1, endY] == 0)
        {
            maze[endX - 1, endY] = 1;
        }
        else if (IsInBounds(endX, endY - 1) && maze[endX, endY - 1] == 0)
        {
            maze[endX, endY - 1] = 1;
        }
    }
}
