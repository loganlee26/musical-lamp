using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour
{
    public Tilemap tilemap;  // The Tilemap to place tiles in
    public RuleTile wallTile;  // The Rule Tile for walls
    public RuleTile floorTile; // The Rule Tile for floors
    public RuleTile startTile; // The Rule Tile for the start point
    public RuleTile endTile;   // The Rule Tile for the end point

    public int width = 10;
    public int height = 10;

    private int[,] maze;

    void Start()
    {
        maze = new int[width + 2, height + 2];  // Increase the size of the maze to accommodate the boundary
        GenerateMaze(1, 1);  // Start the maze generation inside the new boundary
        DrawMaze();
    }

    public void GenerateMaze(int startX, int startY)
    {
        // Initialize maze: set all cells to walls
        for (int x = 0; x < width + 2; x++)  // Adjust for new maze size
        {
            for (int y = 0; y < height + 2; y++)  // Adjust for new maze size
            {
                maze[x, y] = 0;  // 0 represents wall
            }
        }

        // Create the inner maze using DFS
        DFS(startX, startY);

        // Ensure start and end points are open
        maze[1, 1] = 1; // Start point (bottom-left inside the boundary)
        maze[width, height] = 1; // End point (top-right inside the boundary)

        // Ensure that top-right corner is connected to open paths
        EnsurePathToTopRight();
    }

    void DFS(int x, int y)
    {
        maze[x, y] = 1;  // Mark the current cell as part of the path (1 = path)

        // Randomly shuffle the directions to add randomness
        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 2), // Up
            new Vector2Int(2, 0), // Right
            new Vector2Int(0, -2), // Down
            new Vector2Int(-2, 0) // Left
        };

        // Shuffle the directions to ensure randomness
        directions = ShuffleDirections(directions);

        foreach (var dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            // Check if the new position is within bounds and is a wall
            if (IsInBounds(newX, newY) && maze[newX, newY] == 0)
            {
                maze[x + dir.x / 2, y + dir.y / 2] = 1; // Carve the path between current and new cell
                DFS(newX, newY); // Recursively visit the new cell
            }
        }
    }

    Vector2Int[] ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int j = Random.Range(0, directions.Length);
            Vector2Int temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
        return directions;
    }

    bool IsInBounds(int x, int y)
    {
        // Adjust for boundary: the player can't move outside the boundaries of the maze
        return x > 0 && x < width + 1 && y > 0 && y < height + 1;
    }

    public void DrawMaze()
    {
        // Draw the maze on the tilemap
        for (int x = 0; x < width + 2; x++)  // Adjust for new maze size
        {
            for (int y = 0; y < height + 2; y++)  // Adjust for new maze size
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (maze[x, y] == 0)
                    tilemap.SetTile(cellPosition, wallTile);  // Place wall
                else
                {
                    tilemap.SetTile(cellPosition, floorTile); // Place floor

                    // Mark start and end points
                    if (x == 1 && y == 1)
                        tilemap.SetTile(cellPosition, startTile);  // Start point (bottom-left inside the boundary)
                    else if (x == width && y == height)
                        tilemap.SetTile(cellPosition, endTile);    // End point (top-right inside the boundary)
                }
            }
        }
    }

    // Ensures the top-right corner has a valid path to at least one adjacent tile
    void EnsurePathToTopRight()
    {
        int endX = width;
        int endY = height;

        // If there is a wall to the left, clear it
        if (IsInBounds(endX - 1, endY) && maze[endX - 1, endY] == 0)
        {
            maze[endX - 1, endY] = 1;  // Clear path to the left
        }
        // If there is a wall below, clear it
        else if (IsInBounds(endX, endY - 1) && maze[endX, endY - 1] == 0)
        {
            maze[endX, endY - 1] = 1;  // Clear path below
        }
    }
}
