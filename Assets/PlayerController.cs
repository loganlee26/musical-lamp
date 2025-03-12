using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 moveDirection;

    public Tilemap tilemap;  // Reference to the tilemap
    public RuleTile endTile; // Reference to the end tile
    public UIManager uiManager;  // Reference to the UIManager to stop the timer

    private Vector3Int playerPosition;  // The player's position in tile coordinates

    void Update()
    {
        // Get player input
        moveDirection.x = Input.GetAxis("Horizontal");
        moveDirection.y = Input.GetAxis("Vertical");

        // Normalize direction to avoid diagonal speed increase
        moveDirection.Normalize();

        // Move player
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Convert player position to tilemap grid coordinates
        playerPosition = tilemap.WorldToCell(transform.position);

        // Check if the player is standing on the end tile
        CheckIfPlayerReachedEnd();
    }

    // Detect if the player is standing on the end tile
    void CheckIfPlayerReachedEnd()
    {
        // Get the tile at the player's position
        TileBase tileAtPlayer = tilemap.GetTile(playerPosition);

        // If the player is standing on the end tile, trigger the end of the game logic
        if (tileAtPlayer == endTile)
        {
            Debug.Log("Maze Solved! You've reached the end!");
            // You can trigger other actions here, such as:
            // - Display a victory message
            // - Load a new scene
            // - Stop player movement, etc.
            uiManager.GameOver();  // Notify UIManager to stop the timer
        }
    }
}
