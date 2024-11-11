using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator
{
    private int maxIterations;
    private int roomLengthMin;
    private int roomWidthMin;

    public RoomGenerator(int maxIterations, int roomLengthMin, int roomWidthMin)
    {
        this.maxIterations = maxIterations;
        this.roomLengthMin = roomLengthMin;
        this.roomWidthMin = roomWidthMin;
    }

    public List<RoomNode> GenerateRoomsInGivenSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset)
    {
        List<RoomNode> listToReturn = new List<RoomNode>();
        foreach (var space in roomSpaces)
        {

            // Last two Parameters are the Offset and Point Modifiers to make the rooms more random
            Vector2Int newBottomLeftPoint = StrucutureHelper.GenerateBottomLeftCornerBetween(
                space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomBottomCornerModifier, roomOffset);
            Vector2Int newTopRightPoint = StrucutureHelper.GenerateTopRightCornerBetween(space.BottomLeftAreaCorner,
                space.TopRightAreaCorner, roomTopCornerModifier, roomOffset);

            // Reassign bottom left and right points
            space.BottomLeftAreaCorner = newBottomLeftPoint;
            space.TopRightAreaCorner = newTopRightPoint;

            // Assign the two new points
            space.BottomRightAreaCorner = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
            space.TopLeftAreaCorner = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);

            listToReturn.Add((RoomNode)space);
        }
        return listToReturn;
    }
}