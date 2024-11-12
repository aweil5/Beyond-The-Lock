using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGrid
{   
    public Vector2Int bottomLeftAreaCorner;
    public Vector2Int bottomRightAreaCorner;
    public Vector2Int topRightAreaCorner;
    public Vector2Int topLeftAreaCorner;

    public GridSquare[,] grid;



    public RoomGrid(RoomNode roomNode)
    {
        int roomWidth = Mathf.FloorToInt((roomNode.BottomRightAreaCorner.x - roomNode.BottomLeftAreaCorner.x) / 10f);
        int roomHeight = Mathf.FloorToInt((roomNode.TopRightAreaCorner.y - roomNode.BottomRightAreaCorner.y) / 10f);

        this.grid = new GridSquare[roomHeight, roomWidth];

        for (int y = 0; y < roomHeight; y++)
        {
            for (int x = 0; x < roomWidth; x++)
            {
            Vector2Int bottomLeft = new Vector2Int(roomNode.BottomLeftAreaCorner.x + x * 10, roomNode.BottomLeftAreaCorner.y + y * 10);
            Vector2Int bottomRight = new Vector2Int(bottomLeft.x + 10, bottomLeft.y);
            Vector2Int topLeft = new Vector2Int(bottomLeft.x, bottomLeft.y + 10);
            Vector2Int topRight = new Vector2Int(bottomLeft.x + 10, bottomLeft.y + 10);

            grid[y, x] = new GridSquare(bottomLeft, bottomRight, topRight, topLeft);
            }
        }


    }

}

public class GridSquare{
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }

    public GridSquare(Vector2Int bottomLeftAreaCorner, Vector2Int bottomRightAreaCorner, Vector2Int topRightAreaCorner, Vector2Int topLeftAreaCorner)
    {
        this.BottomLeftAreaCorner = bottomLeftAreaCorner;
        this.BottomRightAreaCorner = bottomRightAreaCorner;
        this.TopRightAreaCorner = topRightAreaCorner;
        this.TopLeftAreaCorner = topLeftAreaCorner;
    }

}