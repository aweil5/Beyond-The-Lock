using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CorridorNode : Node
{
    private Node structure1;
    private Node structure2;
    private int corridorWidth;
    // CHANGE TO PUBLIC LATER. THIS IS SO WE DONT MAKE CORRIDORS TOO CLOSE TO CORNERS
    private int modifierDistanceFromWall = 1;
    // Set parent to Null or else chance of connecting corridor to corridor, which we do not want
    public CorridorNode(Node startNode, Node endNode, int corridorWidth) : base(null)
    {
        this.structure1 = startNode;
        this.structure2 = endNode;
        this.corridorWidth = corridorWidth;

        GenerateCorridor();
    }

    private void GenerateCorridor()
    {
        // First find nodes realtive positioning
        var relativePositionOfStructure2 = CheckPositionStructure2AgainstStructure1();
        switch (relativePositionOfStructure2)
        {
            case RelativePosition.Up:
                ProcessRoomInRelationUpOrDown(this.structure1, this.structure2);
                break;
            case RelativePosition.Down:
                ProcessRoomInRelationUpOrDown(this.structure2, this.structure1);
                break;
            case RelativePosition.Left:
                ProcessRoomInRelationRightOrLeft(this.structure1, this.structure2);
                break;
            case RelativePosition.Right:
                ProcessRoomInRelationRightOrLeft(this.structure2, this.structure1);
                break;
        }
    }


    private void ProcessRoomInRelationRightOrLeft(Node structure1, Node structure2)
    {
        Node leftStructure = null;
        List<Node> leftStructuresChildren = StrucutureHelper.TraverseGraphToExtractLowestLeafs(structure1);
        Node rightStructure = null;
        List<Node> rightStructuresChildren = StrucutureHelper.TraverseGraphToExtractLowestLeafs(structure2);


        // Greedy Sort to Connect Closest Two Children

        // Extracted most right aligned from the left structures
        var sortedLeftStructure = leftStructuresChildren.OrderByDescending(ChildrenNodeList => ChildrenNodeList.TopRightAreaCorner.x).ToList();
        if (sortedLeftStructure.Count == 1)
        {
            leftStructure = sortedLeftStructure[0];
        }
        else
        {
            // IS THIS WORTH IT? MAYBE WE SHOULD JUST CONNECT THE CLOSEST TWO
            int maxX = sortedLeftStructure[0].TopRightAreaCorner.x;
            // Any close to the max X should still have a chance to be connected
            sortedLeftStructure = sortedLeftStructure.Where(child => Math.Abs(maxX - child.TopRightAreaCorner.x) < 10).ToList();
            // Randomly select the one to connect
            int index = UnityEngine.Random.Range(0, sortedLeftStructure.Count);
            leftStructure = sortedLeftStructure[index];
        }

        // Extracted most left aligned from the right structures
        var possibleNeighborsInRightStructureList = rightStructuresChildren.Where(
            child => GetValidYForNeighborLeftRight(
            leftStructure.TopRightAreaCorner,
            leftStructure.BottomRightAreaCorner,
            child.TopLeftAreaCorner,
            child.BottomLeftAreaCorner
            ) != -1).OrderBy(child => child.BottomRightAreaCorner.x).ToList();

        if (possibleNeighborsInRightStructureList.Count <= 0)
        {
            rightStructure = structure2;
        }
        else
        {
            rightStructure = possibleNeighborsInRightStructureList[0];
        }

        int y = GetValidYForNeighborLeftRight(
            leftStructure.TopLeftAreaCorner,
            leftStructure.BottomRightAreaCorner,
            rightStructure.TopLeftAreaCorner,
            rightStructure.BottomRightAreaCorner
        );


        // We can maybe change this so if there isnt a good fit ie more than n iterations
        // We just don't connect the two. Maybe connect via teleporter or something
        while (y == -1 && sortedLeftStructure.Count > 1)
        {
            sortedLeftStructure = sortedLeftStructure.Where(child => child.TopLeftAreaCorner.y != leftStructure.TopLeftAreaCorner.y).ToList();
            leftStructure = sortedLeftStructure[0];

            y = GetValidYForNeighborLeftRight(
            leftStructure.TopLeftAreaCorner,
            leftStructure.BottomRightAreaCorner,
            rightStructure.TopLeftAreaCorner,
            rightStructure.BottomRightAreaCorner
            );
        }

        BottomLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y);
        TopRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y + this.corridorWidth);

    }

    private void ProcessRoomInRelationUpOrDown(Node structure1, Node structure2)
    {
        Node bottomStructure = null;
        List<Node> bottomStructuresChildren = StrucutureHelper.TraverseGraphToExtractLowestLeafs(structure1);
        Node topStructure = null;
        List<Node> topStructuresChildren = StrucutureHelper.TraverseGraphToExtractLowestLeafs(structure2);

        var sortedBottomStructure = bottomStructuresChildren.OrderByDescending(child => child.TopRightAreaCorner.y).ToList();

        if (sortedBottomStructure.Count == 1)
        {
            bottomStructure = sortedBottomStructure[0];
        }
        else
        {
            // IS THIS WORTH IT? MAYBE WE SHOULD JUST CONNECT THE CLOSEST TWO
            int maxY = sortedBottomStructure[0].TopRightAreaCorner.y;
            // Any close to the max X should still have a chance to be connected
            sortedBottomStructure = sortedBottomStructure.Where(child => Math.Abs(maxY - child.TopRightAreaCorner.y) < 10).ToList();
            // Randomly select the one to connect
            int index = UnityEngine.Random.Range(0, sortedBottomStructure.Count);
            bottomStructure = sortedBottomStructure[index];
        }

        var possibleNeighborsInTopStructure = topStructuresChildren.Where(
            child => GetValidXForNeighborUpDown(
            bottomStructure.TopLeftAreaCorner,
            bottomStructure.TopRightAreaCorner,
            child.BottomLeftAreaCorner,
            child.BottomRightAreaCorner
            ) != -1).OrderBy(child => child.BottomRightAreaCorner.y).ToList();


        if (possibleNeighborsInTopStructure.Count == 0)
        {
            topStructure = structure2;
        }
        else
        {
            topStructure = possibleNeighborsInTopStructure[0];

        }
        int x = GetValidXForNeighborUpDown(
            bottomStructure.TopLeftAreaCorner,
            bottomStructure.TopRightAreaCorner,
            topStructure.BottomLeftAreaCorner,
            topStructure.BottomRightAreaCorner
            );

        while(x == -1 && sortedBottomStructure.Count > 1)
        {
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeftAreaCorner.x != topStructure.TopLeftAreaCorner.x).ToList();
            bottomStructure = sortedBottomStructure[0];

            x = GetValidXForNeighborUpDown(
            bottomStructure.TopLeftAreaCorner,
            bottomStructure.TopRightAreaCorner,
            topStructure.BottomLeftAreaCorner,
            topStructure.BottomRightAreaCorner
            );
        }
        BottomLeftAreaCorner = new Vector2Int(x, bottomStructure.TopLeftAreaCorner.y);
        TopRightAreaCorner = new Vector2Int(x + this.corridorWidth, topStructure.BottomLeftAreaCorner.y);
    }

    private int GetValidXForNeighborUpDown(Vector2Int bottomNodeLeft, Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
    {
        if (topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight .x < topNodeRight.x)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                bottomNodeRight - new Vector2Int(modifierDistanceFromWall + this.corridorWidth, 0)
            ).x;
        }
        if(topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                topNodeRight - new Vector2Int(modifierDistanceFromWall + this.corridorWidth, 0)
            ).x;
        }
        if (bottomNodeLeft.x >= topNodeLeft.x && bottomNodeLeft.x <= topNodeRight.x)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                topNodeRight - new Vector2Int(modifierDistanceFromWall + this.corridorWidth, 0)
            ).x;
        }
        if (bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                bottomNodeRight - new Vector2Int(modifierDistanceFromWall + this.corridorWidth, 0)
            ).x;
        }
        return -1;
    }

    private int GetValidYForNeighborLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
    {
        // Left Node Bigger in Both Cases
        if (rightNodeUp.y >= leftNodeUp.y && rightNodeDown.y <= leftNodeDown.y)
        {
            return StrucutureHelper.CalculateMiddlePoint(leftNodeDown + new Vector2Int(0, modifierDistanceFromWall), leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)).y;
        }
        // Left Node Smaller and in between Rights y vals
        else if (rightNodeUp.y <= leftNodeUp.y && rightNodeDown.y >= leftNodeDown.y)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
            ).y;
        }
        // Left Node Smaller and slightly below
        else if (leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                leftNodeUp - new Vector2Int(0, modifierDistanceFromWall)
            ).y;
        }
        // Left Node Smaller and slightly above
        else if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
        {
            return StrucutureHelper.CalculateMiddlePoint(
                leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                rightNodeUp + new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)).y;
        }
        else
        {
            return -1;
        }
    }

    private RelativePosition CheckPositionStructure2AgainstStructure1()
    {
        Vector2 middlePointStructure1Temp = ((Vector2)structure1.BottomLeftAreaCorner + (Vector2)structure1.TopRightAreaCorner) / 2;
        Vector2 middlePointStructure2Temp = ((Vector2)structure2.BottomLeftAreaCorner + (Vector2)structure2.TopRightAreaCorner) / 2;

        float angle = CalculateAngle(middlePointStructure1Temp, middlePointStructure2Temp);

        if (angle < 45 && angle > -45)
        {
            return RelativePosition.Right;
        }
        else if (angle < 135 && angle > 45)
        {
            return RelativePosition.Up;
        }
        else if (angle < -45 && angle > -135)
        {
            return RelativePosition.Down;
        }
        else
        {
            return RelativePosition.Left;
        }
    }

    private float CalculateAngle(Vector2 middlePointStructure1Temp, Vector2 middlePointStructure2Temp)
    {
        return Mathf.Atan2(middlePointStructure2Temp.y - middlePointStructure1Temp.y, middlePointStructure2Temp.x - middlePointStructure1Temp.x) * Mathf.Rad2Deg;
    }

}

public enum RelativePosition
{
    Up,
    Down,
    Left,
    Right
}