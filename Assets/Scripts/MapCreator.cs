using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreator : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int occupiedWidth;
    [SerializeField] int occupiedHeight;
    [SerializeField] GameObject cubePrefab;

    Tile[,] tiles;

    // Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[height,width];
        CreateArea(width, height);
        SetOccupiedArea(occupiedWidth, occupiedHeight);
    }

    // Creating grid map with cubes
    void CreateArea(int width, int height)
    {
        int startX = -width/2;
        int startZ = -height/2;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = new Vector3(startX + x, 0, startZ + z);
                GameObject currentTile = Instantiate(cubePrefab, position, Quaternion.identity);
                currentTile.transform.parent = transform;
                tiles[z, x] = currentTile.GetComponent<Tile>();
                tiles[z, x].Initialize(x, z);
            }
        }
    }

    // Setting occupied area into the middle of the map
    private void SetOccupiedArea(int occupiedWidth, int occupiedHeight)
    {
        int startX = Mathf.FloorToInt((width - occupiedWidth) / 2.0f);
        int startZ = Mathf.FloorToInt((height - occupiedHeight) / 2.0f);

        for (int z = startZ; z < startZ + occupiedHeight; z++)
        {
            for (int x = startX; x < startX + occupiedWidth; x++)
            {
                tiles[z, x].GetComponent<MeshRenderer>().material = ResourceHolder.Instance.occupiedMaterial;
                tiles[z, x].SetTileType(TileType.Occupied);
                // Set walls around the border of the occupied area
                if (z == startZ || z == startZ + occupiedHeight - 1 || x == startX || x == startX + occupiedWidth - 1)
                {
                    // Changing current tile to wall
                    tiles[z, x].AddComponent<Wall>();
                    tiles[z, x].SetTileType(TileType.Wall);
                }
                
            }
        }
        
    }



}
