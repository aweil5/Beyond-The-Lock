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

    public GameObject teleporter;

    public GameObject player;
    public GameObject playerCam;

    List<Vector3Int> possibleWallHorizontalPosition;

    List<Vector3Int> possibleWallVerticalPosition;

    public List<GameObject> randomWorldSpawns;


    [Header("Start Room Prefabs")]
    public GameObject roomPillar1;
    public GameObject office;
    public GameObject clerkDesk;
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

        // Create teleporters that teleport a user from one room to the next
        GameObject currSender = null;
        GameObject currReceiver = null;
        GameObject temp = null;
        RoomType roomType;
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
            buildRoomInterior(room, roomParent, roomType);
               


            // This will be for every room
            Vector2 bottomLeft = room.BottomLeftAreaCorner;
            Vector2 topRight = room.TopRightAreaCorner;

            float roomWidth = topRight.x - bottomLeft.x;
            float roomHeight = topRight.y - bottomLeft.y;

            Vector3 teleporterPosition1, teleporterPosition2;


            if (roomWidth > roomHeight)
            {
                // Place teleporters on the left and right walls
                teleporterPosition1 = new Vector3(bottomLeft.x + 3, 0, (bottomLeft.y + topRight.y) / 2);
                teleporterPosition2 = new Vector3(topRight.x - 3, 0, (bottomLeft.y + topRight.y) / 2);
            }
            else
            {
                // Place teleporters on the top and bottom walls
                teleporterPosition1 = new Vector3((bottomLeft.x + topRight.x) / 2, 0, bottomLeft.y + 3);
                teleporterPosition2 = new Vector3((bottomLeft.x + topRight.x) / 2, 0, topRight.y - 3);
            }
            // Ensure both teleporters have BoxColliders with isTrigger enabled


            currReceiver = Instantiate(teleporter, teleporterPosition1, Quaternion.identity, transform);
            temp = Instantiate(teleporter, teleporterPosition2, Quaternion.identity, transform);

            if (!currReceiver.TryGetComponent<BoxCollider>(out BoxCollider receiverCollider))
            {
                receiverCollider = currReceiver.AddComponent<BoxCollider>();
            }
            receiverCollider.isTrigger = true;

            if (!temp.TryGetComponent<BoxCollider>(out BoxCollider senderCollider))
            {
                senderCollider = temp.AddComponent<BoxCollider>();
            }
            senderCollider.isTrigger = true;
            // Ensure both teleporters have BoxColliders with isTrigger enabled

            if (currSender != null)
            {
                currSender.AddComponent<Teleport>();
                currSender.GetComponent<Teleport>().reciever = currReceiver;
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
            playerPosition = new Vector3(firstRoomSpawn.x, 1, firstRoomSpawn.y);

            
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
        
        
        Vector3 lookAtPosition = new Vector3((bottomLeftFirst.x + topRightFirst.x) / 2, playerPosition.y, (bottomLeftFirst.y + topRightFirst.y) / 2);
        lookRotation = Quaternion.LookRotation(lookAtPosition - playerPosition);

        GameObject playerInstance = Instantiate(player, playerPosition, lookRotation, transform);

    }

    private void buildRoomInterior(Node room, GameObject roomParent, RoomType roomType)
    {
        if (roomType == RoomType.Start)
        {
            // Add specific logic for Start room
            buildStartRoom(room, roomParent);
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
    private void buildStartRoom(Node room, GameObject roomParent)
    {
        // Spawn in the 
        RoomGrid gridRoom = new RoomGrid((RoomNode)room);
        int rowCount = gridRoom.grid.GetLength(0);
        int colCount = gridRoom.grid.GetLength(1);

        int xStart;
        int xEnd;

        int yStart;
        int yEnd;
        bool ColBigger = colCount > rowCount; 
        
        // Building Office Rooms
        if (rowCount > colCount)
        {
            xStart = rowCount / 2 - 1;
            xEnd = rowCount;
            yStart = colCount - 1;
            yEnd = colCount / 2;
        }else{

            xStart = colCount / 2 - 1;
            xEnd = colCount;
            yStart = rowCount - 1;
            yEnd = rowCount / 2;
        }
       
        
        for (int row = xStart; row < xEnd; row++)
        {
            for (int col = yStart; col > yEnd; col -= 2)
            {
                if (ColBigger)
                {
                    
                    Vector3 position = new Vector3(gridRoom.grid[col, row].Center.x, 0, gridRoom.grid[col, row].Center.y);
                    Instantiate(office, position, Quaternion.identity, roomParent.transform);
                }
                else{
                    Vector3 position = new Vector3(gridRoom.grid[row, col].Center.x, 0, gridRoom.grid[row, col].Center.y);
                    Instantiate(office, position, Quaternion.identity, roomParent.transform);
                }

            
            }
        }


        // Building Clerk Desk
        // NEEED TO FIX THIS
        GameObject clerkOffice;
        Vector3 clerkPosition;
        Quaternion clerkLookRotation = Quaternion.identity;
        Vector3 lookAtPosition;
        if (ColBigger)
        {
            int colLocation = colCount / 2;
            int rowLocation = 0;
            clerkPosition = new Vector3(gridRoom.grid[rowLocation, colLocation].Center.x, 0, gridRoom.grid[yEnd, xEnd - 1].Center.y);
            lookAtPosition = new Vector3(gridRoom.grid[rowLocation + 1, colLocation].Center.x, 0 , gridRoom.grid[rowLocation + 1, colLocation].Center.y);
        }
        else
        {
            int colLocation = 0;
            int rowLocation = rowCount / 2;
            clerkPosition = new Vector3(gridRoom.grid[rowLocation, colLocation].Center.x, 0, gridRoom.grid[xStart, yStart].Center.y);
            lookAtPosition = new Vector3(gridRoom.grid[rowLocation, colLocation + 1].Center.x, 0 , gridRoom.grid[rowLocation, colLocation + 1].Center.y);

        }
        clerkLookRotation = Quaternion.LookRotation(lookAtPosition - clerkPosition);
        clerkOffice = Instantiate(clerkDesk, clerkPosition, clerkLookRotation, roomParent.transform);
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


        // Instantiate the room rug at the lower right corner
        // Scale the floor to the size of the room
        // floorInstance.transform.localScale = new Vector3(topRightCorner.x - bottomLeftCorner.x, floorInstance.transform.localScale.y, topRightCorner.y - bottomLeftCorner.y);


        // Create ceiling mesh
        Vector3[] ceilingVertices = new Vector3[]
        {
            topLeftV + Vector3.up * 6,
            topRightV + Vector3.up * 6,
            bottomLeftV + Vector3.up * 6,
            bottomRightV + Vector3.up * 6
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