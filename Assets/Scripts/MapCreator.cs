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

    public Tile[,] tiles;
    public List<Tile> walls;

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
                tiles[z, x].SetTileType(TileType.Occupied);
            }
        }

        int counter = 0;
        // Build horizontal walls (bottom and top)
        for (int x = startX; x < startX + occupiedWidth; x++)
        {
            counter++;
            // Bottom wall
            tiles[startZ, x].SetTileType(TileType.Wall);
            tiles[startZ, x].RiseTile();
            walls.Add(tiles[startZ, x]);

            // Top wall
            tiles[startZ + occupiedHeight - 1, x].SetTileType(TileType.Wall);
            tiles[startZ + occupiedHeight - 1, x].RiseTile();
            walls.Add(tiles[startZ + occupiedHeight - 1, x]);
            // Building archer towers on the middle wall if there are 3 consecutive walls
            if(counter%3 == 0 && x != startX + occupiedWidth -1){
                tiles[startZ, x].GetComponent<Wall>().BuildArcherTower();
                tiles[startZ + occupiedHeight - 1, x].GetComponent<Wall>().BuildArcherTower();
            }
        }
        counter=0;
        // Build vertical walls (left and right)
        for (int z = startZ; z < startZ + occupiedHeight; z++)
        {
            counter++;
            // Left wall
            tiles[z, startX].SetTileType(TileType.Wall);
            tiles[z, startX].RiseTile();
            walls.Add(tiles[z, startX]);

            // Right wall
            tiles[z, startX + occupiedWidth - 1].SetTileType(TileType.Wall);
            tiles[z, startX + occupiedWidth - 1].RiseTile();
            walls.Add(tiles[z, startX + occupiedWidth - 1]);
            // Building archer towers on the middle wall if there are 3 consecutive walls
            if(counter%3 == 0 && z != startZ + occupiedHeight -1){
                tiles[z, startX].GetComponent<Wall>().BuildArcherTower();
                tiles[z, startX + occupiedWidth - 1].GetComponent<Wall>().BuildArcherTower();
            }
        }
        
    }



}
