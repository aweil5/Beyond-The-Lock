using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator
{
    private int dungeonWidth;
    private int dungeonLength;
    List<RoomNode> allSpaceNodes = new List<RoomNode>();

    public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
    }

    public List<Node> CalculateRooms(int maxIterations, int roomWidthMin, int roomLengthMin)
    {
        // Binary Space Partitioning to generate rooms
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
        allSpaceNodes = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        return new List<Node>(allSpaceNodes);
    }
}