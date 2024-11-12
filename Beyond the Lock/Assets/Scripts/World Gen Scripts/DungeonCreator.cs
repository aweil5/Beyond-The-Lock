using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();

    }
    private void CreateDungeon()
    {
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);

        var listOfRooms = generator.CalculateDungeon(maxIterations, roomWidthMin, roomLengthMin, roomBottomCornerModifier, roomTopCornerModifier, roomOffset, corridorWidth);
        
        // Assign Each Room an index

        // Create teleporters that teleport a user from one room to the next
        GameObject currSender = null;
        GameObject currReceiver = null;
        GameObject temp = null;
        // Instantiate Teleporters in each Room
        foreach (var room in listOfRooms)
        {
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
            
        }





        // // Instantiate the player camera and set its transform to the player
        // GameObject playerCamInstance = Instantiate(playerCam, playerPosition, Quaternion.identity, transform);
        // playerCamInstance.transform.SetParent(playerInstance.transform);


        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.parent = transform;
        // INSTEAD OF DOORS WE WILL DO TELEPORTERS LATER


        possibleWallHorizontalPosition = new List<Vector3Int>();

        possibleWallVerticalPosition = new List<Vector3Int>();
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
        }
        CreateWalls(wallParent);



        // Instantiate the player in the middle of the first room
        var firstRoom = listOfRooms[0];
        Vector2 firstRoomCenter = (firstRoom.BottomLeftAreaCorner + firstRoom.TopRightAreaCorner) / 2;
        Vector3 playerPosition = new Vector3(firstRoomCenter.x, 1, firstRoomCenter.y);
        GameObject playerInstance = Instantiate(player, playerPosition, Quaternion.identity, transform);
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

