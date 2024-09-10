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

    private Tile[,] _tiles;
    private List<Tile> _wallTiles;

    // Start is called before the first frame update
    void Start()
    {
        _tiles = new Tile[height,width];
        _wallTiles = new List<Tile>();
        ResourceHolder.Instance.tiles = _tiles;
        ResourceHolder.Instance.wallTiles = _wallTiles;
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
                _tiles[z, x] = currentTile.GetComponent<Tile>();
                _tiles[z, x].Initialize(x, z);
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
                _tiles[z, x].SetTileType(TileType.Occupied);
            }
        }

        int counter = 0;
        // Build horizontal walls (bottom and top)
        for (int x = startX; x < startX + occupiedWidth; x++)
        {
            counter++;
            // Bottom wall
            Tile tileBot = _tiles[startZ, x];
            tileBot.SetTileType(TileType.Wall);
            tileBot.GetComponentInChildren<Wall>().RiseWall();
            _wallTiles.Add(tileBot);

            // Top wall
            Tile tileTop = _tiles[startZ + occupiedHeight - 1, x]; 
            tileTop.SetTileType(TileType.Wall);
            tileTop.GetComponentInChildren<Wall>().RiseWall();
            _wallTiles.Add(tileTop);
            
            // Building archer towers on the middle wall if there are 3 consecutive walls
            if(counter%3 == 0 && x != startX + occupiedWidth -1){
                tileBot.GetComponentInChildren<Wall>().BuildArcherTower();
                tileTop.GetComponentInChildren<Wall>().BuildArcherTower();
            }
        }
        counter=0;
        // Build vertical walls (left and right)
        for (int z = startZ+1; z < startZ + occupiedHeight - 1; z++)
        {
            counter++;
            // Left wall
            Tile tileLeft = _tiles[z, startX];
            tileLeft.SetTileType(TileType.Wall);
            tileLeft.GetComponentInChildren<Wall>().RiseWall();
            _wallTiles.Add(tileLeft);

            // Right wall
            Tile tileRight = _tiles[z, startX + occupiedWidth - 1];
            tileRight.SetTileType(TileType.Wall);
            tileRight.GetComponentInChildren<Wall>().RiseWall();
            _wallTiles.Add(tileRight);
            // Building archer towers on the middle wall if there are 3 consecutive walls
            if(counter%3 == 0 && z != startZ + occupiedHeight -1){
                tileLeft.GetComponentInChildren<Wall>().BuildArcherTower();
                tileRight.GetComponentInChildren<Wall>().BuildArcherTower();
            }
        }
        
    }



}
