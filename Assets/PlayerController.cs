using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Tilemap tilemap;
    public RuleTile wallTile;
    public RuleTile endTile;
    public RuleTile doorTile;
    public RuleTile keyTile;
    public RuleTile floorTile;
    public UIManager uiManager;

    private Vector3Int currentTilePos;
    private Vector3 targetPos;
    private Vector3Int moveDirection = Vector3Int.zero;
    private bool isMoving = false;

    private Vector3 startingPosition;
    private bool hasKey = false;

    void Start()
    {
        startingPosition = tilemap.GetCellCenterWorld(tilemap.WorldToCell(transform.position));
        ResetPlayer();
    }

    void Update()
    {
        if (!isMoving)
        {
            moveDirection = Vector3Int.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveDirection = new Vector3Int(0, 1, 0);
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveDirection = new Vector3Int(0, -1, 0);
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveDirection = new Vector3Int(-1, 0, 0);
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveDirection = new Vector3Int(1, 0, 0);

            if (moveDirection != Vector3Int.zero)
            {
                Vector3Int nextTile = currentTilePos + moveDirection;
                TileBase nextTileBase = tilemap.GetTile(nextTile);

                if (nextTileBase != null)
                {
                    string tileName = nextTileBase.name;

                    if (tileName == wallTile.name)
                    {
                        // Wall - no movement
                    }
                    else if (tileName == doorTile.name)
                    {
                        if (hasKey)
                        {
                            tilemap.SetTile(nextTile, floorTile); // Unlock the door
                            currentTilePos = nextTile;
                            targetPos = tilemap.GetCellCenterWorld(currentTilePos);
                            isMoving = true;
                            Debug.Log("Door unlocked.");
                        }
                        else
                        {
                            Debug.Log("The door is locked. Find the key.");
                        }
                    }
                    else
                    {
                        currentTilePos = nextTile;
                        targetPos = tilemap.GetCellCenterWorld(currentTilePos);
                        isMoving = true;
                    }
                }
            }
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;

                if (!IsAnyMoveKeyHeld())
                {
                    moveDirection = Vector3Int.zero;
                }

                CheckIfPlayerReachedEnd();
                CheckForKeyPickup();
            }
        }
    }

    public void ResetPlayer()
    {
        currentTilePos = tilemap.WorldToCell(startingPosition);
        targetPos = tilemap.GetCellCenterWorld(currentTilePos);
        transform.position = targetPos;

        moveDirection = Vector3Int.zero;
        isMoving = false;
        hasKey = false;
    }

    private void CheckIfPlayerReachedEnd()
    {
        TileBase tileAtPlayer = tilemap.GetTile(currentTilePos);
        if (tileAtPlayer != null && tileAtPlayer.name == endTile.name)
        {
            Debug.Log("Maze Solved! You've reached the end!");
            uiManager.GameOver();
        }
    }

    private void CheckForKeyPickup()
    {
        TileBase tileAtPlayer = tilemap.GetTile(currentTilePos);
        if (tileAtPlayer != null && tileAtPlayer.name == keyTile.name && !hasKey)
        {
            hasKey = true;
            uiManager.ShowKeyIcon();
            tilemap.SetTile(currentTilePos, floorTile);
            Debug.Log("Key collected!");
        }
    }

    private bool IsAnyMoveKeyHeld()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ||
               Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
               Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
    }
}
