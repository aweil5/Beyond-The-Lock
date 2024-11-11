using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator
{
    private int dungeonWidth;
    private int dungeonLength;
    List<RoomNode> allNodesCollection = new List<RoomNode>();

    public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
    }

    public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth)
    {
        // Binary Space Partitioning to generate rooms
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
        allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        List<Node> roomSpaces = StrucutureHelper.TraverseGraphToExtractLowestLeafs(bsp.RootNode);

        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier, roomTopCornerModifier, roomOffset);
        
        // Handles Connecting the Rooms

        CorridorsGenerator corridorsGenerator = new CorridorsGenerator();
        var corridorList = corridorsGenerator.CreateCorridor(allNodesCollection, corridorWidth);
        
        return new List<Node>(roomList).Concat(corridorList).ToList();
    }
}
