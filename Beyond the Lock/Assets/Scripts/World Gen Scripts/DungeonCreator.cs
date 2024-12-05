using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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

    public Material phaseMaterial;

    public GameObject wallVertical, wallHorizontal;
    public int wallScale;

    public GameObject teleporter;

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
    public AudioClip finalRoomAudio;

    [Header("Camera Room Prefabs")]
    public GameObject mainCam;
    public GameObject twoCameraPillar;
    public GameObject threeCameraPillar;

    public List<GameObject> cameraRoomItems;

    [Header("Laser Room Prefabs")]
    public GameObject laserStartPillar;
    public GameObject laserEndPillar;

    public GameObject laserSystem;

    [Header("Fight Room Prefabs")]
    public GameObject enemyPrefab;
    public List<GameObject> randRoomItems;

    private List<Vector3> enemySpawnPoints = new List<Vector3>();



    // public GameObject clerkOffice;
    // public GameObject spaceWindows;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateDungeonAndBuildNavMesh());
    }

    IEnumerator CreateDungeonAndBuildNavMesh()
    {
        CreateDungeon();

        // Wait for a frame to ensure all objects are instantiated
        yield return null;

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
        }
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        // Wait for NavMesh to complete building
        yield return new WaitForSeconds(0.5f);

        // Verify NavMesh is built before spawning enemies
        if (surface.navMeshData != null)
        {
            buildEnemies();
        }
        else
        {
            Debug.LogError("NavMesh failed to build!");
        }
    }

    private void buildEnemies()
    {
        foreach (var position in enemySpawnPoints)
        {
            // Check if the position is valid on the NavMesh
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);

                NavMeshAgent navMeshAgent = enemy.GetComponent<NavMeshAgent>();
                if (navMeshAgent == null)
                {
                    navMeshAgent = enemy.AddComponent<NavMeshAgent>();
                    Debug.Log("Added NavMeshAgent to enemy");
                }


            }
            else
            {
                Debug.LogWarning($"Invalid NavMesh position: {position}");
            }
        }
    }

    private void CreateDungeon()
    {
        // backgroundMusic.SetActive(true);
        // narration.SetActive(true);
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


        Vector3 lookAtPosition = new Vector3((bottomLeftFirst.x + topRightFirst.x) / 2, 20f, (bottomLeftFirst.y + topRightFirst.y) / 2);
        lookRotation = Quaternion.LookRotation(lookAtPosition - playerPosition);

        // GameObject playerInstance = Instantiate(player, playerPosition, lookRotation, transform);
        // Create teleporters that teleport a user from one room to the next
        GameObject playerInstance = GameObject.Find("MainPlayer3");
        GameObject playerSpawn = new GameObject("Player Spawn Reciever");
        playerSpawn.transform.position = playerPosition;
        playerSpawn.transform.rotation = lookRotation;
        GameObject firstTeleporter = GameObject.Find("UserSpawnPortal");
        if (firstTeleporter != null)
        {
            // currSender.AddComponent<Teleporter2>();
            //     currSender.GetComponent<Teleporter2>().targetTeleporter = currReceiver.transform;
            firstTeleporter.AddComponent<Teleporter2>();
            firstTeleporter.GetComponent<Teleporter2>().targetTeleporter = playerSpawn.transform;
        }
        else
        {
            Debug.LogError("UserSpawnPortal not found!");
        }
        if (playerInstance == null)
        {
            Debug.LogError("Player GameObject not found!");
            return;
        }
        // playerInstance.transform.position = playerPosition;
        // playerInstance.transform.rotation = lookRotation;



        // Teleportation spawning
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
                float higher = 5.4f;
                roomOrientation = RoomOrientation.Horizontal;
                // Place teleporters on the left and right walls
                teleporterPosition1 = new Vector3(bottomLeft.x + 8, higher, (bottomLeft.y + topRight.y) / 2);
                teleporterPosition2 = new Vector3(topRight.x - 8, higher, (bottomLeft.y + topRight.y) / 2);
            }
            else
            {
                float higher = 5.4f;
                roomOrientation = RoomOrientation.Vertical;
                // Place teleporters on the top and bottom walls
                teleporterPosition1 = new Vector3((bottomLeft.x + topRight.x) / 2, higher, bottomLeft.y + 8);
                teleporterPosition2 = new Vector3((bottomLeft.x + topRight.x) / 2, higher, topRight.y - 8);
            }
            // Ensure both teleporters have BoxColliders with isTrigger enabled

            buildRoomInterior(room, roomParent, roomType, roomOrientation, playerInstance);

            if (room != listOfRooms[0])
            {
                Vector3 directionToCenter = (new Vector3((bottomLeft.x + topRight.x) / 2, 0, (bottomLeft.y + topRight.y) / 2) - teleporterPosition1).normalized;
                Quaternion teleporterRotation = Quaternion.LookRotation(directionToCenter);
                currReceiver = Instantiate(teleporter, teleporterPosition1, teleporterRotation, transform);
                // currReceiver = Instantiate(teleporter, teleporterPosition1, Quaternion.identity, transform);
                if (!currReceiver.TryGetComponent<BoxCollider>(out BoxCollider receiverCollider))
                {
                    receiverCollider = currReceiver.AddComponent<BoxCollider>();
                }
                receiverCollider.isTrigger = true;
            }


            if (room != listOfRooms[listOfRooms.Count - 1])
            {
                Vector3 directionToCenter = (new Vector3((bottomLeft.x + topRight.x) / 2, 0, (bottomLeft.y + topRight.y) / 2) - teleporterPosition2).normalized;
                Quaternion teleporterRotation = Quaternion.LookRotation(directionToCenter);

                temp = Instantiate(teleporter, teleporterPosition2, teleporterRotation, transform);
                if (!temp.TryGetComponent<BoxCollider>(out BoxCollider senderCollider))
                {
                    senderCollider = temp.AddComponent<BoxCollider>();
                }
                senderCollider.isTrigger = true;
            }




            // Ensure both teleporters have BoxColliders with isTrigger enabled

            if (currSender != null)
            {
                currSender.AddComponent<Teleporter2>();
                currSender.GetComponent<Teleporter2>().targetTeleporter = currReceiver.transform;

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
        if (roomOrientation == RoomOrientation.Horizontal)
        {
            // Restart the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        if (roomType == RoomType.Start)
        {
            // Add specific logic for Start room
            buildStartRoom(room, roomParent, roomOrientation, player);

        }
        else if (roomType == RoomType.LaserRoom)
        {
            Debug.Log("Laser Room");
            buildLaserRoom(room, roomParent, roomOrientation);
        }
        else if (roomType == RoomType.CameraRoom)
        {
            Debug.Log("Camera Room");
            buildCameraRoom(room, roomParent, roomOrientation, player);
        }
        else if (roomType == RoomType.FightRoom)
        {
            buildFightRoom(room, roomParent, roomOrientation);
        }
        else
        {
            buildTreasureRoom(room, roomParent, roomOrientation);
        }

    }

    private void buildTreasureRoom(Node room, GameObject roomParent, RoomOrientation roomOrientation)
    {
        Vector3 roomCenter = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, 0, (room.BottomLeftAreaCorner.y + room.TopRightAreaCorner.y) / 2);
        GameObject diamondInstance = Instantiate(diamond, roomCenter, Quaternion.identity, roomParent.transform);
        diamondInstance.transform.localScale = new Vector3(1, 1, 1);
        diamondInstance.transform.localScale = new Vector3(3, 3, 3);

        AddLight(roomCenter + new Vector3(0, wallScale - 2, 0), roomParent, true);
    }

    private void AddLight(Vector3 position, GameObject parent, bool isCentral = false)
    {
        // Create a new light object
        GameObject lightGameObject = new GameObject("RoomLight");
        lightGameObject.transform.position = position;
        lightGameObject.transform.parent = parent.transform;

        // Add a Light component
        Light lightComponent = lightGameObject.AddComponent<Light>();
        lightComponent.type = LightType.Point; // Point light works well for indoor rooms
        lightComponent.intensity = isCentral ? 2.5f : 1.5f; // Brighter for central light
        lightComponent.range = 10f; // Adjust range as needed
        lightComponent.color = Color.yellow; // Customize light color
    }

    private void buildFightRoom(Node room, GameObject roomParent, RoomOrientation roomOrientation)
    {
        RoomGrid gridRoom = new RoomGrid((RoomNode)room);
        int rowCount = gridRoom.grid.GetLength(0);
        int colCount = gridRoom.grid.GetLength(1);

        List<GameObject> enemies = new List<GameObject>();
        enemySpawnPoints.Add(new Vector3(room.TopLeftAreaCorner.x + 3, 1, room.TopLeftAreaCorner.y - 3));
        if (roomOrientation == RoomOrientation.Vertical)
        {
            for (int i = 2; i < rowCount - 2; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    int spawnChoice = UnityEngine.Random.Range(0, cameraRoomItems.Count + (cameraRoomItems.Count / 10));
                    if (spawnChoice >= cameraRoomItems.Count)
                    {
                        enemySpawnPoints.Add(new Vector3(gridRoom.grid[i, j].Center.x, 1, gridRoom.grid[i, j].Center.y));
                        // enemies.Add(Instantiate(enemy, new Vector3(gridRoom.grid[i, j].Center.x, 1, gridRoom.grid[i, j].Center.y), Quaternion.identity, roomParent.transform));
                    }
                    else
                    {


                        GameObject item = Instantiate(cameraRoomItems[spawnChoice], new Vector3(gridRoom.grid[i, j].Center.x, 0, gridRoom.grid[i, j].Center.y), new Quaternion(Quaternion.identity.x, UnityEngine.Random.Range(0, 360), Quaternion.identity.z, Quaternion.identity.w), roomParent.transform);
                        item.transform.localScale = new Vector3(1, 1, 1);
                        item.transform.localScale = new Vector3(5, 5, 5);
                        item.tag = "Ground";
                        item.layer = LayerMask.NameToLayer("Ground");

                        if (!item.TryGetComponent<MeshCollider>(out MeshCollider itemCollider))
                        {
                            itemCollider = item.AddComponent<MeshCollider>();
                        }



                    }
                }
            }
        }

        Vector3 middleBackWall = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, wallScale - 2, room.TopRightAreaCorner.y - 3);
        Vector3 roomCenter = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, 0, (room.BottomLeftAreaCorner.y + room.TopRightAreaCorner.y) / 2);
        GameObject cam = createCamera(middleBackWall, roomCenter, room, roomParent);
        Transform rotator = cam.transform.Find("rotator");
        if (rotator == null)
        {
            Debug.LogError("Rotator child not found in cameraInstance.");
        }
        CameraRotation camRotate = rotator.GetComponent<CameraRotation>();
        if (camRotate == null)
        {
            Debug.LogError("CameraRotation component not found on rotator.");
        }
        camRotate.rotationSpeed = 0;
        camRotate.maxAngle = 0;
        Transform spotLightTransform = rotator.Find("Spot Light");
        if (spotLightTransform != null)
        {
            Light spotLight = spotLightTransform.GetComponent<Light>();
            if (spotLight != null)
            {
                float roomSize = Mathf.Max(room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x, room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y);
                spotLight.range = roomSize;
                float angle = 180;
                spotLight.spotAngle = angle;
                spotLight.intensity = 2;
                SpotlightDetection spotlightDetection = spotLightTransform.GetComponent<SpotlightDetection>();
                if (spotlightDetection != null)
                {
                    spotlightDetection.spotAngle = angle;
                    spotlightDetection.detectionRange = roomSize;
                    spotlightDetection.spotlight = spotLight.transform;
                    spotlightDetection.detectionMask = LayerMask.GetMask("Player");
                    List<Vector3> enemySpawnPoints = new List<Vector3>();
                    // int numEnemies = UnityEngine.Random.Range(1, 4);
                    // for (int i = 0; i < numEnemies; i++)
                    // {
                    //     Vector3 enemySpawnPoint = new Vector3(UnityEngine.Random.Range(room.BottomLeftAreaCorner.x, room.TopRightAreaCorner.x), 5, UnityEngine.Random.Range(room.BottomLeftAreaCorner.y, room.TopRightAreaCorner.y));
                    //     enemySpawnPoints.Add(enemySpawnPoint);
                    // }
                    spotlightDetection.enemySpawnPoints = enemySpawnPoints;
                    spotlightDetection.specialCam = true;
                    spotlightDetection.roomEnemies = null;
                }
                else
                {
                    Debug.LogError("SpotlightDetection component not found on Spot Light.");
                }
            }


            else
            {
                Debug.LogError("Light component not found on Spot Light.");
            }
        }
        else
        {
            Debug.LogError("Spot Light child not found in cameraInstance.");
        }
    }

    private void buildCameraRoom(Node room, GameObject roomParent, RoomOrientation roomOrientation, GameObject player)
    {
        RoomGrid gridRoom = new RoomGrid((RoomNode)room);
        int rowCount = gridRoom.grid.GetLength(0);
        int colCount = gridRoom.grid.GetLength(1);
        Vector3 roomCenter = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, 0, (room.BottomLeftAreaCorner.y + room.TopRightAreaCorner.y) / 2);



        // Building the Cams for the Cam Room

        // For vertical room but almost 100% sure this will be verticla
        if (roomOrientation == RoomOrientation.Vertical)
        {
            for (int i = 1; i < rowCount - 1; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    int spawnChoice = UnityEngine.Random.Range(0, 3);
                    if (spawnChoice != 1)
                    {
                        int itemChoice = UnityEngine.Random.Range(0, cameraRoomItems.Count);
                        Vector3 itemPosition = new Vector3(gridRoom.grid[i, j].Center.x, 0, gridRoom.grid[i, j].Center.y);
                        Quaternion randomRoatation = Quaternion.Euler(Quaternion.identity.x, UnityEngine.Random.Range(0, 360), Quaternion.identity.z);
                        GameObject item = Instantiate(cameraRoomItems[itemChoice], itemPosition, randomRoatation, roomParent.transform);
                        item.transform.localScale = new Vector3(1, 1, 1);
                        item.transform.localScale = new Vector3(5, 5, 5);
                        if (!item.TryGetComponent<MeshCollider>(out MeshCollider itemCollider))
                        {
                            itemCollider = item.AddComponent<MeshCollider>();

                        }
                        item.tag = "Ground";
                        item.layer = LayerMask.NameToLayer("Ground");
                    }



                }
            }
            // Now we need to build the cameras On the back wall of the room
            float zWall = room.TopRightAreaCorner.y - 3;
            List<Vector3> cameraPositions = new List<Vector3>();
            Vector3 middleCam = new Vector3(roomCenter.x, wallScale - 3, zWall);
            Vector3 leftCam = new Vector3((roomCenter.x + room.TopLeftAreaCorner.x) / 2, wallScale - 3, zWall);
            Vector3 rightCam = new Vector3((roomCenter.x + room.TopRightAreaCorner.x) / 2, wallScale - 3, zWall);
            // cameraPositions.Add(middleCam);
            cameraPositions.Add(leftCam);
            cameraPositions.Add(rightCam);
            GameObject empty = new GameObject($"Camera Room {room.BottomLeftAreaCorner}");
            empty.AddComponent<RoomCamGroup>();
            List<GameObject> roomCameras = new List<GameObject>();
            foreach (var cameraPosition in cameraPositions)
            {
                GameObject cam = createCamera(cameraPosition, roomCenter, room, empty);
                if (cam != null)
                {
                    roomCameras.Add(cam);
                }
            }
            empty.GetComponent<RoomCamGroup>().roomCameras = roomCameras;


        }

    }

    private GameObject createCamera(Vector3 cameraPosition, Vector3 roomCenter, Node room, GameObject roomParent)
    {
        Quaternion cameraRotation = Quaternion.LookRotation(cameraPosition - roomCenter);
        GameObject cameraInstance = Instantiate(mainCam, cameraPosition, cameraRotation, roomParent.transform);
        cameraInstance.transform.localScale = new Vector3(1, 1, 1);
        // Attach DetectPlayer component to the main player

        Transform rotator = cameraInstance.transform.Find("rotator");
        if (rotator == null)
        {
            Debug.LogError("Rotator child not found in cameraInstance.");
            return null;
        }
        CameraRotation camRotate = rotator.GetComponent<CameraRotation>();
        if (camRotate == null)
        {
            Debug.LogError("CameraRotation component not found on rotator.");
            return null;
        }
        camRotate.rotationSpeed = UnityEngine.Random.Range(3, 20);
        camRotate.maxAngle = UnityEngine.Random.Range(20, 50);
        Transform spotLightTransform = rotator.Find("Spot Light");
        if (spotLightTransform != null)
        {
            Light spotLight = spotLightTransform.GetComponent<Light>();
            if (spotLight != null)
            {
                float roomSize = Mathf.Max(room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x, room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y);
                spotLight.range = roomSize * 0.75f;
                float angle = UnityEngine.Random.Range(30, 40);
                spotLight.spotAngle = angle;
                SpotlightDetection spotlightDetection = spotLightTransform.GetComponent<SpotlightDetection>();
                if (spotlightDetection != null)
                {
                    spotlightDetection.spotAngle = angle;
                    spotlightDetection.detectionRange = roomSize * 0.75f;
                    spotlightDetection.spotlight = spotLight.transform;
                    spotlightDetection.detectionMask = LayerMask.GetMask("Player");
                    List<Vector3> enemySpawnPoints = new List<Vector3>();
                    int numEnemies = UnityEngine.Random.Range(3, 9);
                    for (int i = 0; i < numEnemies; i++)
                    {
                        Vector3 enemySpawnPoint = new Vector3(UnityEngine.Random.Range(room.BottomLeftAreaCorner.x + 5, room.TopRightAreaCorner.x - 5), 2, UnityEngine.Random.Range(room.BottomLeftAreaCorner.y + 5, room.TopRightAreaCorner.y - 5));
                        enemySpawnPoints.Add(enemySpawnPoint);
                    }
                    spotlightDetection.enemySpawnPoints = enemySpawnPoints;
                }
                else
                {
                    Debug.LogError("SpotlightDetection component not found on Spot Light.");
                }
            }


            else
            {
                Debug.LogError("Light component not found on Spot Light.");
            }
        }
        else
        {
            Debug.LogError("Spot Light child not found in cameraInstance.");
        }
        return cameraInstance;
    }

    private void buildLaserRoom(Node room, GameObject roomParent, RoomOrientation roomOrientation)
    {
        RoomGrid gridRoom = new RoomGrid((RoomNode)room);
        int rowCount = gridRoom.grid.GetLength(0);
        int colCount = gridRoom.grid.GetLength(1);
        Vector3 upperRightCorner = new Vector3(room.TopRightAreaCorner.x - 1, wallScale - 2, room.TopRightAreaCorner.y - 1);
        Vector3 roomCenter = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, 0, (room.BottomLeftAreaCorner.y + room.TopRightAreaCorner.y) / 2);
        createCamera(upperRightCorner, roomCenter, room, roomParent);



        if (roomOrientation == RoomOrientation.Vertical)
        {
            // Instantiate Common Laser System 
            for (int i = 1; i < rowCount - 1; i++)
            {
                if (i % 2 == 0)
                {
                    int rowChoice = UnityEngine.Random.Range(0, 2);
                    if (rowChoice == 0)
                    {
                        Vector3 middlePosition = new Vector3((room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2, 0, (room.BottomLeftAreaCorner.y + room.TopRightAreaCorner.y) / 2);
                        Vector3 pillarPos = new Vector3(middlePosition.x, 0, gridRoom.grid[i, 0].TopRightAreaCorner.y);
                        GameObject middlePillar = Instantiate(roomPillar1, pillarPos, Quaternion.identity, roomParent.transform);
                        middlePillar.transform.localScale = new Vector3(15, wallScale, 15);
                        // Single Column in the Middle
                    }
                    else
                    {



                        Vector3 firstThirdPosition = new Vector3(room.BottomLeftAreaCorner.x + (room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 3, 0, room.BottomLeftAreaCorner.y + (room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y) / 3);
                        Vector3 secondThirdPosition = new Vector3(room.BottomLeftAreaCorner.x + 2 * (room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 3, 0, room.BottomLeftAreaCorner.y + 2 * (room.TopRightAreaCorner.y - room.BottomLeftAreaCorner.y) / 3);

                        Vector3 positionOne = new Vector3(firstThirdPosition.x, 0, gridRoom.grid[i, 0].TopRightAreaCorner.y);
                        Vector3 positionTwo = new Vector3(secondThirdPosition.x, 0, gridRoom.grid[i, 0].TopRightAreaCorner.y);
                        GameObject pillarOne = Instantiate(roomPillar1, positionOne, Quaternion.identity, roomParent.transform);
                        pillarOne.transform.localScale = new Vector3(15, wallScale, 15);

                        GameObject pillarTwo = Instantiate(roomPillar1, positionTwo, Quaternion.identity, roomParent.transform);
                        pillarTwo.transform.localScale = new Vector3(15, wallScale, 15);

                    }
                }
                else
                {


                    int laserCount = UnityEngine.Random.Range(1, 4);
                    for (int j = 0; j < laserCount; j++)
                    {
                        float yPosOne = UnityEngine.Random.Range(0f, 5f);
                        float yPosTwo = UnityEngine.Random.Range(0f, 5f);

                        Vector3 firstTurrentPosition = new Vector3(room.TopLeftAreaCorner.x, yPosOne, gridRoom.grid[i, 0].TopLeftAreaCorner.y);
                        Vector3 secondTurretPositon = new Vector3(room.TopRightAreaCorner.x, yPosTwo, gridRoom.grid[i, colCount - 1].TopRightAreaCorner.y);

                        GameObject laser = Instantiate(laserSystem, firstTurrentPosition, Quaternion.identity, roomParent.transform);

                        GameObject turret1 = laser.transform.GetChild(0).gameObject;
                        GameObject turret2 = laser.transform.GetChild(1).gameObject;

                        turret1.transform.position = firstTurrentPosition;
                        turret2.transform.position = secondTurretPositon;


                        // VERTICAL MOVEMENT ON FULL LASER ROWS
                        if (j > 0)
                        {
                            int turretChoice = UnityEngine.Random.Range(0, 2);
                            if (turretChoice == 0)
                            {
                                turret1.AddComponent<MoveTurretVertical>();
                                var component = turret1.GetComponent<MoveTurretVertical>();
                                component.displacement = UnityEngine.Random.Range(3, 7);
                                component.speed = UnityEngine.Random.Range(3, 10);
                            }
                            else if (turretChoice == 1)
                            {
                                turret2.AddComponent<MoveTurretVertical>();
                                var component = turret2.GetComponent<MoveTurretVertical>();
                                component.displacement = UnityEngine.Random.Range(4, 9);
                                component.speed = UnityEngine.Random.Range(3, 10);


                            }
                            else
                            {
                                turret1.AddComponent<MoveTurretVertical>();
                                var component = turret1.GetComponent<MoveTurretVertical>();
                                component.displacement = UnityEngine.Random.Range(3, 7);
                                component.speed = UnityEngine.Random.Range(3, 15);

                                turret2.AddComponent<MoveTurretVertical>();
                                var component2 = turret2.GetComponent<MoveTurretVertical>();
                                component2.displacement = UnityEngine.Random.Range(4, 9);
                                component2.speed = UnityEngine.Random.Range(5, 10);


                            }
                            // turret1.AddComponent<MoveTurretHorizontal>();
                            // var horizontalComponent = turret1.GetComponent<MoveTurretHorizontal>();
                            // horizontalComponent.displacement = UnityEngine.Random.Range(-10, -3);
                            // horizontalComponent.speed = UnityEngine.Random.Range(3, 15);





                        }
                    }
                }

            }




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
        // Vector3 upperRightCorner = new Vector3(room.TopRightAreaCorner.x - 1, wallScale - 2, room.TopRightAreaCorner.y - 1);
        // Quaternion cameraRotation = Quaternion.LookRotation(upperRightCorner - new Vector3(room.BottomLeftAreaCorner.x, wallScale - 2, room.BottomLeftAreaCorner.y)) * Quaternion.Euler(-28, 0, 0);
        // GameObject cameraInstance = Instantiate(mainCam, upperRightCorner, cameraRotation, roomParent.transform);
        // cameraInstance.transform.localScale = new Vector3(1, 1, 1);
        // // Attach DetectPlayer component to the main player
        // CameraDetectPlayer detectPlayer = cameraInstance.GetComponentInChildren<CameraDetectPlayer>();
        // Transform playerChild = player.transform.Find("Player");
        // if (playerChild != null)
        // {
        //     detectPlayer.player = playerChild.gameObject;
        // }
        // else
        // {
        //     detectPlayer.player = player;
        //     Debug.LogError("Child with name 'Player' not found in player GameObject.");
        // }

        // Building Phase Wall
        Vector3 wallPosition;
        Quaternion wellRotation = Quaternion.identity;
        Vector3 wellScale;

        if (roomOrientation == RoomOrientation.Vertical)
        {
            Debug.Log("Vertical Room FOr Stasrt ");
            int rowLocation = rowCount - 1;

            // Calculate the middle x position
            float middleX = (room.BottomLeftAreaCorner.x + room.TopRightAreaCorner.x) / 2;

            // Calculate the z position at the upper right corner
            float upperRightZ = room.TopRightAreaCorner.y - 12f;

            // Place a single box at the calculated position
            wallPosition = new Vector3(middleX, 0, upperRightZ);
            wellRotation = Quaternion.LookRotation(Vector3.forward);
            wellScale = new Vector3(room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x, 2 * wallScale, 2);

            GameObject wellInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wellInstance.transform.position = wallPosition;
            Destroy(wellInstance.GetComponent<Collider>());
            wellInstance.transform.rotation = wellRotation;
            wellInstance.transform.localScale = wellScale;
            wellInstance.transform.parent = roomParent.transform;
            wellInstance.GetComponent<MeshRenderer>().material = phaseMaterial;
            // for (int col = 0; col < colCount; col++)
            // {
            //     wallPosition = new Vector3(gridRoom.grid[rowLocation, col].Center.x, 0, gridRoom.grid[rowLocation, col].Center.y);
            //     wellRotation = Quaternion.LookRotation(Vector3.forward);
            //     wellScale = new Vector3(10, 2 * wallScale, 2);

            //     GameObject wellInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //     wellInstance.transform.position = wallPosition;
            //     Destroy(wellInstance.GetComponent<Collider>());
            //     wellInstance.transform.rotation = wellRotation;
            //     wellInstance.transform.localScale = wellScale;
            //     wellInstance.transform.parent = roomParent.transform;
            //     wellInstance.GetComponent<MeshRenderer>().material = phaseMaterial;

            // }

        }
        else
        {
            int colLocation = colCount - 2;
            for (int row = 0; row < rowCount; row++)
            {
                wallPosition = new Vector3(gridRoom.grid[row, colLocation].Center.x, 0, gridRoom.grid[row, colLocation].Center.y);
                wellRotation = Quaternion.LookRotation(Vector3.right);
                wellScale = new Vector3(1, wallScale, 1);

                GameObject wellInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wellInstance.transform.position = wallPosition;
                wellInstance.transform.rotation = wellRotation;
                wellInstance.transform.localScale = wellScale;
                wellInstance.transform.parent = roomParent.transform;
            }
        }

        // Build DNE Room

        // Instantiate the DoNotEnter GameObject
        // Vector3 doNotEnterPosition;
        // Quaternion doNotEnterRotation = Quaternion.identity;
        // Vector3 doNotEnterScale;

        // if (roomOrientation == RoomOrientation.Horizontal)
        // {
        //     doNotEnterPosition = new Vector3(gridRoom.grid[rowCount-1, colCount / 4].Center.x, 0, gridRoom.grid[rowCount-1, colCount / 4].Center.y);
        //     doNotEnterRotation = Quaternion.LookRotation(Vector3.right);
        //     doNotEnterScale = new Vector3((room.TopRightAreaCorner.x - room.BottomLeftAreaCorner.x) / 2, wallScale, 1);
        // }
        // else
        // {
        //     Debug.Log("Vertical Room");
        //     doNotEnterPosition = new Vector3(room.TopLeftAreaCorner.x + 16, -3, room.TopLeftAreaCorner.y - 32 );
        //     doNotEnterRotation = Quaternion.LookRotation(Vector3.back);
        //     doNotEnterScale = new Vector3(2.5f, 2.5f, 2.5f);
        // }

        // GameObject doNotEnterInstance = Instantiate(doNotEnter, doNotEnterPosition, doNotEnterRotation, roomParent.transform);
        // // Normalize the GameObject before scaling
        // doNotEnterInstance.transform.localScale = Vector3.one;
        // doNotEnterInstance.transform.localScale = doNotEnterScale;


        // Building Offices
        if (roomOrientation == RoomOrientation.Vertical)
        {
            xStart = rowCount / 2 - 1;
            xEnd = rowCount - 1;
            yStart = colCount - 1;
            yEnd = colCount / 2;
        }
        else
        {

            xStart = colCount / 2 - 1;
            xEnd = colCount - 1;
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
            for (int i = colLocation; i < colCount - 2; i += 2)
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

            for (int j = rowLocation; j < rowCount - 2; j += 2)
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
        Quaternion rotation = wallPrefab == wallHorizontal ? Quaternion.identity : Quaternion.Euler(0, 90, 0);

        GameObject wall = Instantiate(wallPrefab, wallPosition, rotation, wallParent.transform);
        wall.transform.localScale = new Vector3(wall.transform.localScale.x, wall.transform.localScale.y * wallScale, wall.transform.localScale.z);
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

        // Define vertices
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        float width = topRightCorner.x - bottomLeftCorner.x;
        float length = topRightCorner.y - bottomLeftCorner.y;

        // Calculate center position
        float centerX = bottomLeftCorner.x + width / 2f;
        float centerZ = bottomLeftCorner.y + length / 2f;

        // Create the plane
        GameObject dungeonFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);

        // Set position
        dungeonFloor.transform.position = new Vector3(centerX, 0f, centerZ);

        // Scale the plane
        dungeonFloor.transform.localScale = new Vector3(width / 10f, 4f, length / 10f);

        // Assign the material
        if (material != null)
        {
            dungeonFloor.GetComponent<Renderer>().material = material;
        }
        else
        {
            Debug.LogError("Material is not assigned. Please assign a material.");
        }

        // Add a BoxCollider (optional)
        dungeonFloor.AddComponent<BoxCollider>();

        // Remove the MeshCollider if not needed
        DestroyImmediate(dungeonFloor.GetComponent<MeshCollider>());

        // Set tag and layer
        dungeonFloor.tag = "Ground";
        dungeonFloor.layer = LayerMask.NameToLayer("Ground");
        // Correct vertex order to ensure proper normals
        //     Vector3[] vertices = new Vector3[]
        //     {
        // bottomLeftV, // 0
        // bottomRightV, // 1
        // topLeftV, // 2
        // topRightV // 3
        //     };

        //     // Define UVs
        //     Vector2[] uvs = new Vector2[vertices.Length];
        //     for (int i = 0; i < uvs.Length; i++)
        //     {
        //         uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        //     }

        //     // Correct triangle winding order
        //     int[] triangles = new int[]
        //     {
        // 0, 2, 1,
        // 2, 3, 1
        //     };

        //     // Create the mesh
        //     Mesh mesh = new Mesh();
        //     mesh.vertices = vertices;
        //     mesh.uv = uvs;
        //     mesh.triangles = triangles;

        //     // Recalculate normals and other properties
        //     mesh.RecalculateNormals();
        //     mesh.RecalculateBounds();
        //     mesh.RecalculateTangents();

        //     // Optionally, manually set normals
        //     // Vector3[] normals = new Vector3[vertices.Length];
        //     // for (int i = 0; i < normals.Length; i++)
        //     // {
        //     //     normals[i] = Vector3.up;
        //     // }
        //     // mesh.normals = normals;

        //     // Create the GameObject
        //     GameObject dungeonFloor = new GameObject("Floor " + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
        //     dungeonFloor.transform.position = Vector3.zero;
        //     dungeonFloor.transform.localScale = Vector3.one;

        //     // Assign the mesh and material
        //     dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        //     dungeonFloor.GetComponent<MeshRenderer>().material = material;

        //     // Add collider
        //     dungeonFloor.AddComponent<BoxCollider>();

        //     // Set tag and layer
        //     dungeonFloor.tag = "Ground";
        //     dungeonFloor.layer = LayerMask.NameToLayer("Ground");


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
        // GameObject lightGameObject = new GameObject("RoomLight");
        // Light lightComp = lightGameObject.AddComponent<Light>();
        // lightComp.color = Color.white;
        // lightComp.intensity = 1.0f;
        // lightGameObject.transform.position = (bottomLeftV + topRightV) / 2 + Vector3.up * 2;

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