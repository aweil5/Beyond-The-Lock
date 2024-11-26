using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;

    // These need to have ranges or the entire world generation will be a mess

    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;

    [Range(0.7f, 1.0f)]
    public float roomTopCornerModifier;

    [Range(0, 2)]
    public int roomOffset;
    // Material for our meshes. Can prob split this into floor mesh wall mesh etc.
    public Material material;
    public Material ceilingMaterial;

    public GameObject wallVertical, wallHorizontal;
    public int wallScale;

    public GameObject teleporter;

    public GameObject player;

    List<Vector3Int> possibleWallHorizontalPosition;

    List<Vector3Int> possibleWallVerticalPosition;

    public List<GameObject> randomWorldSpawns;


    [Header("Start Room Prefabs")]
    public GameObject roomPillar1;
    public GameObject office;
    public GameObject clerkDesk;

    public GameObject doNotEnter;

    [Header("Vault Room Prefabs")]
    public GameObject diamond;

    [Header("Camera Room Prefabs")]
    public GameObject mainCam;
    public GameObject cameraPillar;



    // public GameObject clerkOffice;
    // public GameObject spaceWindows;


    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();

    }
    private void CreateDungeon()
    {
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);

        var listOfRooms = generator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, roomBottomCornerModifier, roomTopCornerModifier, roomOffset, corridorWidth);
        GameObject roomParent = new GameObject("Rooms");

        // Assign Each Room an index

                // FIRST ROOM SPAWN

        // Instantiate the player in the middle of the first room
        var firstRoom = listOfRooms[0];

        Vector2 bottomLeftFirst = firstRoom.BottomLeftAreaCorner;
        Vector2 topRightFirst = firstRoom.TopRightAreaCorner;

        float roomWidthFirst = topRightFirst.x - bottomLeftFirst.x;
        float roomHeightFirst = topRightFirst.y - bottomLeftFirst.y;

        Vector3 playerPosition;
        Quaternion lookRotation = Quaternion.identity;
        if (roomWidthFirst < roomHeightFirst)
        {
            // Place player in the middle of the room
            Vector2 firstRoomSpawn = new Vector2((bottomLeftFirst.x + topRightFirst.x) / 2, bottomLeftFirst.y + 3);
            playerPosition = new Vector3(firstRoomSpawn.x, 5f, firstRoomSpawn.y);


            Vector3 lookDirection = (topRightFirst - bottomLeftFirst).normalized;
            lookRotation = Quaternion.LookRotation(lookDirection);


        }
        else
        {
            // Place player in the middle of the room
            Vector2 firstRoomSpawn = new Vector2(bottomLeftFirst.x + 3, (bottomLeftFirst.y + topRightFirst.y) / 2);

            playerPosition = new Vector3(firstRoomSpawn.x, 1, firstRoomSpawn.y);
            Vector3 lookDirection = (topRightFirst - bottomLeftFirst).normalized;
            lookRotation = Quaternion.LookRotation(lookDirection);
        }


        Vector3 lookAtPosition = new Vector3((bottomLeftFirst.x + topRightFirst.x) / 2, 3f, (bottomLeftFirst.y + topRightFirst.y) / 2);
        lookRotation = Quaternion.LookRotation(lookAtPosition - playerPosition);

        GameObject playerInstance = Instantiate(player, playerPosition, lookRotation, transform);

        // Create teleporters that teleport a user from one room to the next
        GameObject currSender = null;
        GameObject currReceiver = null;
        GameObject temp = null;
        RoomType roomType;
        RoomOrientation roomOrientation;
        // Instantiate Teleporters in each Room
        foreach (var room in listOfRooms)
        {
            // Bulding main Event Rooms
            if (room != listOfRooms[0] && room != listOfRooms[listOfRooms.Count - 1])
            {
                roomType = (RoomType)UnityEngine.Random.Range(2, 5);
            }
            // Building Start Room
            else if (room == listOfRooms[0])
            {
                roomType = RoomType.Start;


            }
            else
            {
                roomType = RoomType.Treasure;
            }



            // This will be for every room
            Vector2 bottomLeft = room.BottomLeftAreaCorner;
            Vector2 topRight = room.TopRightAreaCorner;

            float roomWidth = topRight.x - bottomLeft.x;
            float roomHeight = topRight.y - bottomLeft.y;

            Vector3 teleporterPosition1, teleporterPosition2;


            if (roomWidth > roomHeight)
            {
                roomOrientation = RoomOrientation.Horizontal;
                // Place teleporters on the left and right walls
                teleporterPosition1 = new Vector3(bottomLeft.x + 3, 0, (bottomLeft.y + topRight.y) / 2);
                teleporterPosition2 = new Vector3(topRight.x - 3, 0, (bottomLeft.y + topRight.y) / 2);
            }
            else
            {
                roomOrientation = RoomOrientation.Vertical;
                // Place teleporters on the top and bottom walls
                teleporterPosition1 = new Vector3((bottomLeft.x + topRight.x) / 2, 0, bottomLeft.y + 3);
                teleporterPosition2 = new Vector3((bottomLeft.x + topRight.x) / 2, 0, topRight.y - 3);
            }
            // Ensure both teleporters have BoxColliders with isTrigger enabled

            buildRoomInterior(room, roomParent, roomType, roomOrientation, playerInstance);

            if (room != listOfRooms[0])
            {
                currReceiver = Instantiate(teleporter, teleporterPosition1, Quaternion.identity, transform);
                if (!currReceiver.TryGetComponent<BoxCollider>(out BoxCollider receiverCollider))
                {
                    receiverCollider = currReceiver.AddComponent<BoxCollider>();
                }
                receiverCollider.isTrigger = true;
            }


            if (room != listOfRooms[listOfRooms.Count - 1])
            {
                temp = Instantiate(teleporter, teleporterPosition2, Quaternion.identity, transform);
                if (!temp.TryGetComponent<BoxCollider>(out BoxCollider senderCollider))
                {
                    senderCollider = temp.AddComponent<BoxCollider>();
                }
                senderCollider.isTrigger = true;
            }




            // Ensure both teleporters have BoxColliders with isTrigger enabled

            if (currSender != null)
            {
                currSender.AddComponent<Teleport>();
                currSender.GetComponent<Teleport>().receiver = currReceiver;
            }

            // IF we want to add undirected teleportation
            // if (currReceiver != null)
            // {
            //     currReceiver.AddComponent<Teleport>();
            //     currReceiver.GetComponent<Teleport>().reciever = temp;
            // }

            currSender = temp;


            // NOW WE ARE GOING TO SPAWN ITEMS IN THE ROOMS

        }





        // // Instantiate the player camera and set its transform to the player
        // GameObject playerCamInstance = Instantiate(playerCam, playerPosition, Quaternion.identity, transform);
        // playerCamInstance.transform.SetParent(playerInstance.transform);


        GameObject wallParent = new GameObject("Walls");
        GameObject randomItemParent = new GameObject("RandomItems");
        wallParent.transform.parent = transform;
        // INSTEAD OF DOORS WE WILL DO TELEPORTERS LATER


        possibleWallHorizontalPosition = new List<Vector3Int>();

        possibleWallVerticalPosition = new List<Vector3Int>();
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
        }
        CreateWalls(wallParent);





    }

    private void buildRoomInterior(Node room, GameObject roomParent, RoomType roomType, RoomOrientation roomOrientation, GameObject player = null)
    {
        if (roomType == RoomType.Start)
        {
            // Add specific logic for Start room
            buildStartRoom(room, roomParent, roomOrientation, player);
            
        }
        else
        {
            return;
        }

    }


    // public GameObject roomPillar1;
    // public GameObject office;
    // public GameObject clerkOffice;
    // public GameObject spaceWindows;
    private void buildStartRoom(Node room, GameObject roomParent, RoomOrientation roomOrientation, GameObject player)
    {
        // Spawn in the 
        RoomGrid gridRoom = new RoomGrid((RoomNode)room);
        int rowCount = gridRoom.grid.GetLength(0);
        int colCount = gridRoom.grid.GetLength(1);

        int xStart;
        int xEnd;

        int yStart;
        int yEnd;





        // TESTING INFO
        // Instantiate the camera in the middle of the room
        Vector3 upperRightCorner = new Vector3(room.TopRightAreaCorner.x - 1, wallScale - 2, room.TopRightAreaCorner.y - 1);
        Quaternion cameraRotation = Quaternion.LookRotation(upperRightCorner - new Vector3(room.BottomLeftAreaCorner.x, wallScale - 2, room.BottomLeftAreaCorner.y)) * Quaternion.Euler(-28, 0, 0);
        GameObject cameraInstance = Instantiate(mainCam, upperRightCorner, cameraRotation, roomParent.transform);
        cameraInstance.transform.localScale = new Vector3(1, 1, 1);
        // Attach DetectPlayer component to the main player
        CameraDetectPlayer detectPlayer = cameraInstance.GetComponentInChildren<CameraDetectPlayer>();
        Transform playerChild = player.transform.Find("Player");
        if (playerChild != null)
        {
            detectPlayer.player = playerChild.gameObject;
        }
        else
        {
            detectPlayer.player = player;
            Debug.LogError("Child with name 'PLayer' not found in player GameObject.");
        }

        // Build DNE Room

        // Instantiate the DoNotEnter GameObject
        Vector3 doNotEnterPosition;
        Quaternion doNotEnterRotation = Quaternion.identity;
        Vector3 doNotEnterScale;

        if (roomOrientation == RoomOrientation.Horizontal)
        {
            doNotEnterPosition = new Vector3(gridRoom.grid[rowCount-1, colCount / 4].Center.x, 0, gridRoom.grid[rowCount-1, colCount / 4].Center.y);
            doNotEnterRotation = Quaternion.LookRotation(Vector3.right);
            doNotEnterScale = new Vector3((room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 2, wallScale, 1);
        }
        else
        {
            Debug.Log("Vertical Room");
            doNotEnterPosition = new Vector3(room.TopLeftAreaCorner.x + 16, -3, room.TopLeftAreaCorner.y - 32 );
            doNotEnterRotation = Quaternion.LookRotation(Vector3.back);
            doNotEnterScale = new Vector3(2.5f, 2.5f, 2.5f);
        }

        GameObject doNotEnterInstance = Instantiate(doNotEnter, doNotEnterPosition, doNotEnterRotation, roomParent.transform);
        // Normalize the GameObject before scaling
        doNotEnterInstance.transform.localScale = Vector3.one;
        doNotEnterInstance.transform.localScale = doNotEnterScale;


        // Building Offices
        if (roomOrientation == RoomOrientation.Vertical)
        {
            xStart = rowCount / 2 - 1;
            xEnd = rowCount;
            yStart = colCount - 1;
            yEnd = colCount / 2;
        }
        else
        {

            xStart = colCount / 2 - 1;
            xEnd = colCount;
            yStart = rowCount - 1;
            yEnd = rowCount / 2;
        }


        for (int row = xStart; row < xEnd; row++)
        {
            for (int col = yStart; col > yEnd; col -= 2)
            {
                if (roomOrientation == RoomOrientation.Horizontal)
                {

                    Vector3 position = new Vector3(gridRoom.grid[col, row].Center.x, 0, gridRoom.grid[col, row].Center.y);
                    Instantiate(office, position, Quaternion.identity, roomParent.transform);
                }
                else
                {
                    Vector3 position = new Vector3(gridRoom.grid[row, col].Center.x, 0, gridRoom.grid[row, col].Center.y);
                    Instantiate(office, position, Quaternion.identity, roomParent.transform);
                }


            }
        }


        // Building Clerk Desk
        // NEEED TO FIX THIS
        // It should be facing normal to its position plus a few in the x or z direction depending
        // the wall its placed on
        // GameObject clerkOffice;
        Vector3 clerkPosition;
        Quaternion clerkLookRotation = Quaternion.identity;
        Vector3 lookAtPosition;
        if (roomOrientation == RoomOrientation.Horizontal)
        {
            int colLocation = colCount / 2;
            int rowLocation = 0;
            for (int i = colLocation; i < colCount - 1; i += 2)
            {
                clerkPosition = new Vector3(gridRoom.grid[rowLocation, i].Center.x + 1, 0, gridRoom.grid[rowLocation, i].Center.y);
                lookAtPosition = clerkPosition + Vector3.forward * 0.1f; // Slightly offset forward
                clerkLookRotation = Quaternion.LookRotation(lookAtPosition - clerkPosition) * Quaternion.Euler(0, 180, 0);
                Instantiate(clerkDesk, clerkPosition, clerkLookRotation, roomParent.transform);
            }

        }
        else
        {
            int colLocation = 0;
            int rowLocation = rowCount / 2;

            for (int j = rowLocation; j < rowCount - 1; j += 2)
            {
                clerkPosition = new Vector3(gridRoom.grid[j, colLocation].Center.x + 1, 0, gridRoom.grid[j, colLocation].Center.y);
                lookAtPosition = clerkPosition + Vector3.right * 0.1f; // Slightly offset to the right
                clerkLookRotation = Quaternion.LookRotation(lookAtPosition - clerkPosition) * Quaternion.Euler(0, 180, 0);
                Instantiate(clerkDesk, clerkPosition, clerkLookRotation, roomParent.transform);
            }


        }


        // Bulding Pillars in bottom left and right quadrants of the room

        if (roomOrientation == RoomOrientation.Horizontal)
        {
            int colLocation = colCount / 4;
            int rowLocationOne = rowCount / 4;
            int rowLocationTwo = 3 * rowCount / 4;

            Vector3 positionOne = new Vector3(gridRoom.grid[rowLocationOne, colLocation].Center.x, 0, gridRoom.grid[rowLocationOne, colLocation].Center.y);
            Vector3 positionTwo = new Vector3(gridRoom.grid[rowLocationTwo, colLocation].Center.x, 0, gridRoom.grid[rowLocationTwo, colLocation].Center.y);
            GameObject pillarOne = Instantiate(roomPillar1, positionOne, Quaternion.identity, roomParent.transform);
            pillarOne.transform.localScale = new Vector3(10, wallScale, 10);

            GameObject pillarTwo = Instantiate(roomPillar1, positionTwo, Quaternion.identity, roomParent.transform);
            pillarTwo.transform.localScale = new Vector3(10, wallScale, 10);
        }
        else
        {


            // The front of the room is earlier in the grid
            int rowLocation = 1;


            Vector3 firstThirdPosition = new Vector3(room.BottomLeftAreaCorner.x + (room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 3, 0, room.BottomLeftAreaCorner.y + (room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y) / 3);
            Vector3 secondThirdPosition = new Vector3(room.BottomLeftAreaCorner.x + 2 * (room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 3, 0, room.BottomLeftAreaCorner.y + 2 * (room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y) / 3);
            for (int i = rowLocation; i < (rowCount / 2) - 1; i += 3)
            {
                Vector3 positionOne = new Vector3(firstThirdPosition.x, 0, gridRoom.grid[i, 0].Center.y);
                Vector3 positionTwo = new Vector3(secondThirdPosition.x, 0, gridRoom.grid[i, 0].Center.y);
                GameObject pillarOne = Instantiate(roomPillar1, positionOne, Quaternion.identity, roomParent.transform);
                pillarOne.transform.localScale = new Vector3(15, wallScale, 15);

                GameObject pillarTwo = Instantiate(roomPillar1, positionTwo, Quaternion.identity, roomParent.transform);
                pillarTwo.transform.localScale = new Vector3(15, wallScale, 15);
            }



        }



        // lookRotation = Quaternion.LookRotation(lookAtPosition - playerPosition);

        // Build Space Bank Sign

    }

    private void spawnRandomItems(Node room, GameObject randomItemParent)
    {
        // Spawn random items in the room
        // We will spawn a random number of items in the room
        // We need to turn rooms into grid. Then for each section determine an item to spawn in that section

        int numItems = UnityEngine.Random.Range(1, 10);
        for (int i = 0; i < numItems; i++)
        {
            // Randomly select a position in the room
            Vector2 randomPosition = new Vector2(UnityEngine.Random.Range(room.BottomLeftAreaCorner.x + 3, room.TopRightAreaCorner.x - 3), UnityEngine.Random.Range(room.BottomLeftAreaCorner.y + 3, room.TopRightAreaCorner.y + 3));
            Vector3 itemPosition = new Vector3(randomPosition.x, 0, randomPosition.y);
            // Randomly select an item to spawn
            int itemIndex = UnityEngine.Random.Range(0, randomWorldSpawns.Count);
            GameObject item = Instantiate(randomWorldSpawns[itemIndex], itemPosition, Quaternion.identity, randomItemParent.transform);
            item.transform.localScale *= 6;
            item.AddComponent<MeshCollider>();


            if (randomWorldSpawns[itemIndex].TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            {
                MeshCollider instantiatedCollider = item.GetComponent<MeshCollider>();
                instantiatedCollider.sharedMesh = meshCollider.sharedMesh;
            }
        }
    }

    private void CreateWalls(GameObject wallParent)
    {
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPosition, wallHorizontal);

        }
        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPosition, wallVertical);
        }


    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        Quaternion rotation = wallPrefab == wallHorizontal ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

        GameObject wall = Instantiate(wallPrefab, wallPosition, rotation, wallParent.transform);
        wall.transform.localScale = new Vector3(wall.transform.localScale.x, wallScale, wall.transform.localScale.z);
        // Add MeshCollider to the instantiated wall
        if (wallPrefab.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
        {
            MeshCollider instantiatedCollider = wallParent.transform.GetChild(wallParent.transform.childCount - 1).gameObject.AddComponent<MeshCollider>();
            instantiatedCollider.sharedMesh = meshCollider.sharedMesh;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        // Learn from codeMonkey Youtube how to create mesh programattically
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);


        // ORDER IMPORTANT HERE TO ENSURE VALID NORMALS
        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        // Create triangles in clockwise order
        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;

        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = material;
        dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;
        dungeonFloor.tag = "Ground";
        dungeonFloor.layer = LayerMask.NameToLayer("Ground");
        

        // Instantiate the room rug at the lower right corner
        // Scale the floor to the size of the room
        // floorInstance.transform.localScale = new Vector3(topRightCorner.x - bottomLeftCorner.x, floorInstance.transform.localScale.y, topRightCorner.y - bottomLeftCorner.y);


        // Create ceiling mesh
        Vector3[] ceilingVertices = new Vector3[]
        {
            topLeftV + Vector3.up * wallScale,
            topRightV + Vector3.up * wallScale,
            bottomLeftV + Vector3.up * wallScale,
            bottomRightV + Vector3.up * wallScale
        };

        Vector2[] ceilingUVs = new Vector2[ceilingVertices.Length];
        for (int i = 0; i < ceilingUVs.Length; i++)
        {
            ceilingUVs[i] = new Vector2(ceilingVertices[i].x, ceilingVertices[i].z);
        }

        int[] ceilingTriangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        Mesh ceilingMesh = new Mesh();
        ceilingMesh.vertices = ceilingVertices;
        ceilingMesh.uv = ceilingUVs;
        ceilingMesh.triangles = ceilingTriangles;

        GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        dungeonCeiling.transform.position = Vector3.zero;
        dungeonCeiling.transform.localScale = Vector3.one;

        dungeonCeiling.GetComponent<MeshFilter>().mesh = ceilingMesh;
        dungeonCeiling.GetComponent<MeshRenderer>().material = ceilingMaterial;
        dungeonCeiling.GetComponent<MeshCollider>().sharedMesh = ceilingMesh;

        // Add lighting

        // WE SHOULD MAKE THIS A FUNCTION AND THEN CHANGE LIGHTING BASED ON THE ROOM TYPE
        GameObject lightGameObject = new GameObject("RoomLight");
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.color = Color.white;
        lightComp.intensity = 1.0f;
        lightGameObject.transform.position = (bottomLeftV + topRightV) / 2 + Vector3.up * 2;

        // Add an offset to align everything to the grid
        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row + 2, 0, bottomLeftV.z);
            AddWallPositonToList(wallPosition, possibleWallHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row < (int)topRightV.x; row++)
        {
            var wallPosition = new Vector3(row + 2, 0, topRightV.z);
            AddWallPositonToList(wallPosition, possibleWallHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col + 2);
            AddWallPositonToList(wallPosition, possibleWallVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col + 2);
            AddWallPositonToList(wallPosition, possibleWallVerticalPosition);
        }
    }

    private void AddWallPositonToList(Vector3 wallPosition, List<Vector3Int> wallList)
    {
        // Rooms are surrounded by walls, if a corridor is added it is at the same position of the wall
        // Delete previous wall because we know that the corridor is there
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);

        wallList.Add(point);
    }
}



public enum RoomType
{
    Start = 0,
    Treasure = 1,
    CameraRoom = 2,
    LaserRoom = 3,
    FightRoom = 4,

}


// IF HORIZONTAL: The player spawns on the left and right walls
// IF VERTICAL: The player spawns on the top and bottom walls
public enum RoomOrientation
{
    Horizontal = 0,
    Vertical = 1
}