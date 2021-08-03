using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TriangleNet.Geometry;

public class DungeonController : MonoBehaviour
{
    public GameObject startRoom;
    public GameObject endRoom;
    public GameObject[,] levelLayout = null;
    private Graph RoomGraph;
    public GameObject corridorTilesParent;

    [Space]
    public MST MST;
    public Graph MSTGraph;

    [Space]
    [Header("Prefabs")]
    public GameObject RoomPrefab;
    public GameObject CorridorPrefab;
    public GameObject EmptySpacePrefab;
    public GameObject WallPrefab;
    public GameObject floorPrefab;
    public GameObject PlayerPrefab;
    public GameObject skeletonPrefab;
    public GameObject vampirePrefab;
    public GameObject exitPrefab;

    [Header("Item Prefabs")]
    public GameObject potionPrefab;
    public GameObject ArrowPrefab;
    public GameObject CoinPrefab;
    public GameObject chestPrefab;
    public GameObject keyPrefab;

    [Header("Generation properties")]
    public int roomAmount = 15;
    public int minMissionLength;
    public int requiredRoomAmount;
    public float requiredRoomRatio;
    public int requiredRoomSize;
    public int enemyAmount;
    public int trapAmount;
    public float maxAdjustmentTime;
    [SerializeField]
    private float radius;
    public AstarGrid aStarGrid;
    [Header("Gizmos modes")]
    public bool showDelauney;
    public bool showNodeDelauney;
    public bool showMST;
    [Space]


    [SerializeField]
    private bool finishedPositioning = false;
    public List<GameObject> roomList;
    public List<GameObject> validRoomList;
    private List<RandomRoom> MainRoute = new List<RandomRoom>();
    private Grid levelGrid;
    private Vector2 bottomLeft;
    private float adjustmentTime = 0f;
    private bool needKey = false;
    private bool needChest = true;



    struct digger
    {
       public Vector2 position;
       public Vector2 direction;
       public Vector2 targetPos;
       public Transform corridorParent;
    }

    private List<digger> diggers = new List<digger>();
    private int iterationAmount = 1000;

    private void Awake()
    {
        Init();

    }

    private void Init()
    {
        aStarGrid = GetComponent<AstarGrid>();


        Time.timeScale = 5f;
        MST = new MST();
        Polygon polygon = new Polygon();
        RoomGraph = new Graph();
        MSTGraph = new Graph();
        roomList = new List<GameObject>();
        for (int i = 0; i < roomAmount; i++)
        {
            roomList.Add(Instantiate(RoomPrefab, RandomInsideCircle(), Quaternion.identity, transform));
        }
        int ID_iterator = 0;
        foreach (var room in roomList)
        {
            room.GetComponent<RandomRoom>().ID = ID_iterator;
            room.GetComponent<BoxCollider2D>().enabled = true;
            ID_iterator++;
        }
    }

    private void Update()
    {
        if (!finishedPositioning)
        {
            adjustmentTime += Time.unscaledDeltaTime;
            bool done = true;
            int iterator = 0;
            foreach (var room in roomList)
            {
               if(room.GetComponent<Rigidbody2D>().IsSleeping())
                {
                    iterator++;
                }
               else
                {
                    if (adjustmentTime >= maxAdjustmentTime)
                    {
                        roomList.Remove(room);
                        Destroy(room, 1f);
                    }
                }
            }
            if (iterator == roomList.Count)
            {
                Time.timeScale = 1f;
                finishedPositioning = true;
                foreach (var room in roomList)
                {
                    room.GetComponent<BoxCollider2D>().enabled = false;
                    room.GetComponent<Rigidbody2D>().simulated = false;

                }
                foreach (var room in roomList)
                {
                    room.GetComponent<RandomRoom>().SnapToGrid();
                }
                FindValidRooms();
            }

        }



    }
    Vector2 RandomInsideCircle()
    {
        Vector2 position = new Vector2();
        #region METHOD 1
        float r;
        r = radius * Mathf.Sqrt(Random.value);
        float theta = 2 * Random.value * Mathf.PI;
        position.x = 0 + r * Mathf.Cos(theta);
        position.y = 0 + r * Mathf.Sin(theta);
        #endregion
        position.Set(Mathf.Round(position.x), Mathf.Round(position.y));
        return position;
    }

    void FindValidRooms()
    {
        foreach (var room in roomList)
        {
            room.SetActive(false);
            RandomRoom data = room.GetComponent<RandomRoom>();
            Vector2 vector = data.GetSize();

            if (vector.x * vector.y > requiredRoomSize && data.AdequateRoomRatio(requiredRoomRatio) && validRoomList.Count < requiredRoomAmount)
            {
                room.SetActive(true);
                validRoomList.Add(room);
                RoomGraph.CreateNode(data.ID.ToString(), room.transform.position);
                //data.wallParent.transform.DetachChildren();            
            }
        }
        startRoom = validRoomList[0];
        endRoom = validRoomList[validRoomList.Count - 1];

        RoomGraph.CreateMesh();
        CreateMSTGraph();
        FindMainRoute();
        if (MainRoute.Count < minMissionLength)
        {
            Debug.Log("RESETTING");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        ImprintLevel();
        //GenerateCorridors();
        PlaceExit();
        AssignMissionsToRooms();
        PlacePlayer();
    }

    private void ImprintLevel()
    {
        float maxX = 0;
        float minX = int.MaxValue;
        float maxY = 0;
        float minY = int.MaxValue;
        foreach (var room in validRoomList)
        {
            RandomRoom rndRoom = room.GetComponent<RandomRoom>();
            maxX = rndRoom.RightEdge() > maxX ? rndRoom.RightEdge() : maxX;
            minX = rndRoom.LeftEdge() < minX ? rndRoom.LeftEdge() : minX;
            maxY = rndRoom.TopEdge() > maxY ? rndRoom.TopEdge() : maxY;
            minY = rndRoom.BottomEdge() < minY ? rndRoom.BottomEdge() : minY;
        }
        int mapWidth = (int)(maxX + Mathf.Abs(minX));
        int mapHeight = (int)(maxY + Mathf.Abs(minY));
        //Debug.Log("size of array: " + mapWidth + " " + mapHeight);
        levelLayout = new GameObject[mapWidth, mapHeight];
        bottomLeft = new Vector2(minX, minY);
        aStarGrid.bottomLeft = bottomLeft;
        aStarGrid.initGrid(mapWidth, mapHeight);
        foreach (var room in validRoomList)
        {
            RandomRoom roomScript = room.GetComponent<RandomRoom>();
            foreach (var tile in roomScript.roomTiles)
            {
                levelLayout[(int)(tile.transform.position.x - bottomLeft.x),(int)(tile.transform.position.y - bottomLeft.y)] = tile;

                if (tile.CompareTag("Wall"))
                {
                    aStarGrid.grid[(int)(tile.transform.position.x - bottomLeft.x), (int)(tile.transform.position.y - bottomLeft.y)].isWall = true;
                }

            }
        }
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                if (levelLayout[j, i] == null)
                {
                    //levelLayout[j, i] = Instantiate(EmptySpacePrefab, (Vector3)(bottomLeft + new Vector2(j, i)), Quaternion.identity);
                }
            }
        }

        //DigCorridorsBetweenRooms(bottomLeft);

    }

    private void DigCorridorsBetweenRooms(Vector2 gridOffset)
    {
        foreach (var node in MSTGraph.AllNodes)
        {
            foreach (var arc in node.Arcs)
            {
                //Vector3 p0 = arc.parent.position;
                //Vector3 p1 = arc.child.position;
                //Gizmos.DrawLine(p0, p1);
                
                DeployDigger(arc.parent.position, arc.child.position, gridOffset);
            }
        }
        UpdateDiggers(gridOffset);
    }



    private void DeployDigger(Vector2 startPos, Vector2 endPos, Vector2 gridOffset)
    {
        GameObject obj = new GameObject("corridorParent");
        obj.transform.parent = corridorTilesParent.transform;
        digger newDigger = new digger();
        newDigger.position = startPos - gridOffset;
        newDigger.targetPos = endPos - gridOffset;
        newDigger.corridorParent = obj.transform;
        diggers.Add(newDigger);
    }

    private void UpdateDiggers(Vector2 gridOffset)
    {
        Debug.Log("amount if diggers " + diggers.Count);
        int iterations = 0;
        do
        {
            for (int i = 0; i < diggers.Count; i++)
            {
                //place flooring for the corridor
                Vector2Int diggerPos = new Vector2Int((int)diggers[i].position.x, (int)diggers[i].position.y);
                if (levelLayout[diggerPos.x, diggerPos.y] == null)
                {
                    levelLayout[diggerPos.x, diggerPos.y] = Instantiate(floorPrefab, new Vector2(diggers[i].position.x, diggers[i].position.y) + gridOffset, Quaternion.identity, diggers[i].corridorParent);
                }
                else
                {
                    Destroy(levelLayout[diggerPos.x, diggerPos.y]);
                    levelLayout[diggerPos.x, diggerPos.y] = null;
                    levelLayout[diggerPos.x, diggerPos.y] = Instantiate(floorPrefab, new Vector2(diggers[i].position.x, diggers[i].position.y) + gridOffset, Quaternion.identity, diggers[i].corridorParent);
                }

                //decide diggers' next step direction
                if (Mathf.Abs(diggers[i].position.x - diggers[i].targetPos.x) < float.Epsilon)
                {
                    digger thisdigger = diggers[i];
                    thisdigger.direction = Vector2.zero;
                    thisdigger.direction.y = thisdigger.targetPos.y > thisdigger.position.y ? 1 : -1;
                    diggers[i] = thisdigger;
                }
                else
                {
                    digger thisdigger = diggers[i];
                    thisdigger.direction = Vector2.zero;
                    thisdigger.direction.x = thisdigger.targetPos.x > thisdigger.position.x ? 1 : -1;
                    diggers[i] = thisdigger;
                }
            }
            //move digger a step
            for (int i = 0; i < diggers.Count; i++)
            {
                digger thisDigger = diggers[i];
                thisDigger.position += thisDigger.direction;
                diggers[i] = thisDigger;
            }

            int check = 0;
            //Check if digger is at target position
            foreach (var digger in diggers)
            {
                if (digger.position == digger.targetPos)
                {
                    continue;
                }
                check++;
            }


            //check if all diggers are done, if so break the loop
            if (check == 0)
            {
                break;
            }
                //foreach (digger digger in diggers)
                //{
                //    //place flooring for the corridor
                //    Vector2Int diggerPos = new Vector2Int((int)digger.position.x, (int)digger.position.y);
                //    if (levelLayout[diggerPos.x, diggerPos.y] == null)
                //    {
                //        levelLayout[diggerPos.x, diggerPos.y] = Instantiate(floorPrefab, new Vector2(digger.position.x,digger.position.y), Quaternion.identity);
                //    }
                //    else
                //    {
                //        levelLayout[diggerPos.x, diggerPos.y].SetActive(false);
                //        levelLayout[diggerPos.x, diggerPos.y] = Instantiate(floorPrefab, new Vector2(digger.position.x, digger.position.y), Quaternion.identity);
                //    }

                //    //move digger towards target room
                //    if (Mathf.Abs(digger.position.x - digger.targetPos.x) < float.Epsilon)
                //    {
                //        digger.direction.y = digger.targetPos.y > digger.position.y ? 1 : -1;
                //    }

                //}
                iterations++;
        } while (iterations < iterationAmount);


        //put walls around uncovered floors
        for (int x = 0; x < levelLayout.GetLength(0); x++)
        {
            for (int y = 0; y < levelLayout.GetLength(1); y++)
            {
                if (levelLayout[x, y] == null)
                    continue;
                if (levelLayout[x,y].CompareTag("Floor"))
                {
                    if (levelLayout[x, y + 1] == null)
                    {
                        levelLayout[x, y + 1] = Instantiate(WallPrefab, new Vector2(x,y + 1) + gridOffset, Quaternion.identity, levelLayout[x,y].transform.parent);
                    }

                    if (levelLayout[x, y - 1] == null)
                    {
                        levelLayout[x, y - 1] = Instantiate(WallPrefab, new Vector2(x, y - 1) + gridOffset, Quaternion.identity, levelLayout[x, y].transform.parent);
                    }

                    if (levelLayout[x + 1, y] == null)
                    {
                        levelLayout[x + 1, y] = Instantiate(WallPrefab, new Vector2(x + 1, y) + gridOffset, Quaternion.identity, levelLayout[x, y].transform.parent);
                    }

                    if (levelLayout[x - 1, y] == null)
                    {
                        levelLayout[x - 1, y] = Instantiate(WallPrefab, new Vector2(x - 1, y) + gridOffset, Quaternion.identity, levelLayout[x, y].transform.parent);
                    }
                }
            }
        }
        PlaceExit();
        PlacePlayer();
    }

    public void CreateMSTGraph()
    {
        List<Node> test = new List<Node>(RoomGraph.AllNodes);
        //MST.InputDataPoints(RoomGraph.AllNodes, RoomGraph.Root);
        MST.InputDataPoints(test, RoomGraph.Root);
        MSTGraph = MST.GenerateGraph();
        foreach (Node node in MSTGraph.AllNodes)
        {
            //foreach (Arc arc in node.Arcs)
            //{
            //    if (arc.Weigth == node.value)
            //    {
            //        Debug.Log("work please");
            //        save = arc.child;
            //    }
            //}
            node.Arcs.Clear();
            //node.AddArc(save, 69);
        }
        foreach (Node node in MSTGraph.AllNodes)
        {
            foreach (var item in MSTGraph.AllNodes)
            {
                if (Mathf.RoundToInt(Vector2.Distance(item.position, node.position)) == item.value && node != item)
                {
                    node.AddArc(item, Mathf.RoundToInt(Vector2.Distance(item.position, node.position)));
                   // Debug.Log("Connected " + item.Name + " and" + node.Name);
                }
            }
        }
    }

    void GenerateCorridors()
    {
        foreach (var node in MSTGraph.AllNodes)
        {
            foreach (var arc in node.Arcs)
            {
                GameObject startRoom = null;
                GameObject endRoom = null;
                foreach (var room in validRoomList)
                {
                    if (room.GetComponent<RandomRoom>().ID.ToString() == arc.parent.Name)
                    {
                        startRoom = room;
                    }
                    if (room.GetComponent<RandomRoom>().ID.ToString() == arc.child.Name)
                    {
                        endRoom = room;
                    }
                }
                if (startRoom != null && endRoom != null)
                {
                    EvaluateRoomPositioning(startRoom, endRoom);
                }
            }
        }
    }

    void EvaluateRoomPositioning(GameObject startRoom, GameObject endRoom)
    {
        RandomRoom startRoomScript = startRoom.GetComponent<RandomRoom>();
        RandomRoom endRoomScript = endRoom.GetComponent<RandomRoom>();
        List<float> overlap = new List<float>();
        bool foundAlignment = false;

        if (endRoomScript.RoomOverlapHorizontally(startRoomScript))
        {
            //Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned horizontally");
            foundAlignment = true;
            if (endRoom.transform.position.x > startRoom.transform.position.x)
            {
                Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned horizontally and first room is to the right of the second room");
                // end room is to the right of start room
                overlap = CalculateOverlap(endRoomScript, startRoomScript, false);

                GameObject go = Instantiate(CorridorPrefab, new Vector3((endRoomScript.LeftEdge() + startRoomScript.RightEdge()) /2, overlap[Random.Range(0, overlap.Count)], 0), Quaternion.identity);
                go.GetComponent<Corridor>().setCorridorSize((int)Mathf.Abs(endRoomScript.LeftEdge() - startRoomScript.RightEdge()) + 1, 1);
                Debug.Log("THOSE ROOMS ARE CONNECTED WITH A CORRIDOR THAT IS THIS TILES LONG: " + Mathf.Abs(endRoomScript.LeftEdge() - startRoomScript.RightEdge()));


            }
            else
            {
                // end room is to the left of start room
                Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned horizontally and first room is to the left of the second room");
                overlap = CalculateOverlap(endRoomScript, startRoomScript, false);

                GameObject go = Instantiate(CorridorPrefab, new Vector3((endRoomScript.RightEdge() + startRoomScript.LeftEdge()) / 2,overlap[Random.Range(0, overlap.Count)], 0), Quaternion.identity);
                go.GetComponent<Corridor>().setCorridorSize((int)Mathf.Abs(endRoomScript.RightEdge() - startRoomScript.LeftEdge())+ 1,1);
                Debug.Log("THOSE ROOMS ARE CONNECTED WITH A CORRIDOR THAT IS THIS TILES LONG: " + Mathf.Abs(endRoomScript.RightEdge() - startRoomScript.LeftEdge()));
            }

        }
        if (endRoomScript.RoomOverlapVertically(startRoomScript))
        {
            //Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned vertically");
            foundAlignment = true;
            if (endRoom.transform.position.y > startRoom.transform.position.y)
            {
                // end room is above of start room
                Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned vertically and first room is above second room");
                overlap = CalculateOverlap(endRoomScript, startRoomScript, true);

                GameObject go = Instantiate(CorridorPrefab, new Vector3(overlap[Random.Range(0, overlap.Count)],(endRoomScript.BottomEdge() + startRoomScript.TopEdge())/2, 0), Quaternion.identity);
                go.GetComponent<Corridor>().setCorridorSize(1, (int)Mathf.Abs(endRoomScript.BottomEdge() - startRoomScript.TopEdge()) + 1);
                Debug.Log("THOSE ROOMS ARE CONNECTED WITH A CORRIDOR THAT IS THIS TILES LONG: " + Mathf.Abs(endRoomScript.TopEdge() - startRoomScript.BottomEdge()));

            }
            else
            {
                Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are aligned vertically and first room is below second room");
                // end room is below start room
                overlap = CalculateOverlap(endRoomScript, startRoomScript, true);

                GameObject go = Instantiate(CorridorPrefab, new Vector3(overlap[Random.Range(0,overlap.Count)],(endRoomScript.TopEdge() + startRoomScript.BottomEdge()) /2, 0), Quaternion.identity);
                go.GetComponent<Corridor>().setCorridorSize(1, (int)Mathf.Abs(endRoomScript.TopEdge() - startRoomScript.BottomEdge()) + 1);
                Debug.Log("THOSE ROOMS ARE CONNECTED WITH A CORRIDOR THAT IS THIS TILES LONG: " + Mathf.Abs(endRoomScript.TopEdge() - startRoomScript.BottomEdge()));
            }

        }
        if (!foundAlignment)
        {
            Debug.Log("Room " + endRoomScript.ID.ToString() + " and Room " + startRoomScript.ID.ToString() + " are not directly alligned");
        }
    }
    List<float> CalculateOverlap(RandomRoom room1, RandomRoom room2, bool verticalOverlap)
    {
        List<float> room1List = new List<float>();
        List<float> room2List = new List<float>();
        List<float> overlapNumbers = new List<float>();
        if (verticalOverlap)
        {
            room1List.Insert(0, room1.LeftEdge() + 1);
            room2List.Insert(0, room2.LeftEdge() + 1);
            for (int i = 1; i < room1.width-2; i++)
            {
                room1List.Insert(i, room1List[i - 1] + 1);
            }
            for (int i = 1; i < room2.width - 2; i++)
            {
                room2List.Insert(i, room2List[i - 1] + 1);
            }
            foreach (var number in room1List)
            {
                if (room2List.Contains(number))
                {
                    overlapNumbers.Add(number);
                }
            }
            Debug.Log("Room " + room1.ID.ToString() + " and " + room2.ID.ToString() + " have " + overlapNumbers.Count + " tiles overlapping");
        }
        else
        {
            room1List.Insert(0, room1.BottomEdge() + 1);
            room2List.Insert(0, room2.BottomEdge() + 1);
            for (int i = 1; i < room1.height - 2; i++)
            {
                room1List.Insert(i, room1List[i - 1] + 1);
            }
            for (int i = 1; i < room2.height - 2; i++)
            {
                room2List.Insert(i, room2List[i - 1] + 1);
            }
            foreach (var number in room1List)
            {
                if (room2List.Contains(number))
                {
                    overlapNumbers.Add(number);
                }
            }
            Debug.Log("Room " + room1.ID.ToString() + " and " + room2.ID.ToString() + " have " + overlapNumbers.Count + " tiles overlapping"); 
        }
        return overlapNumbers;
    }

    private void PlaceExit()
    {
        Instantiate(exitPrefab, endRoom.transform.position, Quaternion.identity);
    }

    private void PlacePlayer()
    {
        Instantiate(PlayerPrefab, startRoom.transform.position, Quaternion.identity);
        GameManager.instance.doingSetup = false;
        GameManager.instance.LoadingScreen.SetActive(false);
    }

    public Vector2 gridToWorldPos(int x, int y)
    {
        Vector2 worldPos = new Vector2(x + bottomLeft.x, y + bottomLeft.y);

        return worldPos;
    }

    public Vector2 worldToGridPos(int x, int y)
    {
        Vector2 gridPos = new Vector2(x - bottomLeft.x, y - bottomLeft.y);
        return gridPos;
    }

    public GridNode worldToNodePos(Vector2 pos)
    {
        return aStarGrid.grid[(int)(pos.x - bottomLeft.x), (int)(pos.y - bottomLeft.y)];
    }

    public RandomRoom getRoomById(string roomID)
    {
        foreach (var room in validRoomList)
        {
            RandomRoom r = room.GetComponent<RandomRoom>();
            if (r.ID.ToString() == roomID)
            {
                return r;
            }
        }
        return null;
    }

    void AssignMissionsToRooms()
    {
        foreach (var room in MainRoute)
        {
            GenerateMission(room);
        }

        foreach (var room in validRoomList)
        {
            if (MainRoute.Contains(room.GetComponent<RandomRoom>()))
            {
                continue;
            }
           GenerateMission(room.GetComponent<RandomRoom>());
        }
    }

    void FindMainRoute()
    {

        //MSTGraph;
        //List<RoomSearch> OpenList = new List<RoomSearch>();
        //List<RoomSearch> ClosedList = new List<RoomSearch>();

        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        //RoomSearch start = allRooms[0];
        //RoomSearch end = allRooms[allRooms.Count-1];
        Node start = null;
        Node end = null;
        foreach (var node in MSTGraph.AllNodes)
        {
            if (startRoom.GetComponent<RandomRoom>().ID.ToString() == node.Name)
            {
                start = node;
            }
            if (endRoom.GetComponent<RandomRoom>().ID.ToString() == node.Name)
            {
                end = node;
            }
        }

        OpenList.Add(start);

        while (OpenList.Count > 0)
        {
            Node current = OpenList[0];

            Debug.Log(current.Name + " current node");
            if (current == end)
            {
                Node c = end;
                while (c != start)
                {
                    MainRoute.Add(getRoomById(c.Name));
                    c = c.parent;
                }
                break;
            }
                OpenList.Remove(current);
                ClosedList.Add(current);

            foreach (var neighbour in GetNeighbourRooms(current))
            {
                if (ClosedList.Contains(neighbour))
                {
                    continue;
                }

                neighbour.parent = current;

                    if (!OpenList.Contains(neighbour))
                    {
                        OpenList.Add(neighbour);
                    }
            }
        }
    }

    List<Node> GetNeighbourRooms(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();
                foreach (var arc in currentNode.Arcs)
                {
                    neighbours.Add(arc.child);
                }
        return neighbours;
    }

    private void GenerateMission(RandomRoom room)
    {
        int missionType = Random.Range(0, 11);
        if (room.gameObject == startRoom)
        {
            missionType = 8;
        }

        if (needChest)
        {
            missionType = 9;
        }
        if (needKey)
        {
            missionType = 20;
        }

        if (room.gameObject == endRoom)
        {
            //always spawn boss in the last room
            missionType = 11;
        }
        switch (missionType)
        {
            //Exterminate Mission
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                int enemyAmount = Random.Range(1, 4);
                for (int i = 0; i < enemyAmount; i++)
                {
                    Instantiate(skeletonPrefab, room.RandomPointInRoom() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                }
                break;
            //Healing Spot
            case 8:
                Instantiate(potionPrefab, room.RandomPointInRoom(), Quaternion.identity);
                break;
            //Loot chest
            case 9:
            case 10:
                //spawn chest with guards
                Debug.Log("chest spawned");
                Instantiate(chestPrefab, room.RandomPointInRoom(), Quaternion.identity);
                enemyAmount = Random.Range(1, 7);
                for (int i = 0; i < enemyAmount; i++)
                {
                    Instantiate(skeletonPrefab, room.RandomPointInRoom() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                }
                needKey = true;
                needChest = false;
                break;
            //Boss room
            case 11:
                Instantiate(vampirePrefab, room.RandomPointInRoom() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                break;
            default:
                //spawn key with guards
                Instantiate(keyPrefab, room.RandomPointInRoom(), Quaternion.identity);
                enemyAmount = Random.Range(1, 5);
                for (int i = 0; i < enemyAmount; i++)
                {
                    Instantiate(skeletonPrefab, room.RandomPointInRoom() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                }
                needKey = false;
                break;
        }
    }


    public void OnDrawGizmos()
    {
        //if (RoomGraph.mesh == null)
        //{
        //    return;
        //}

        if (showDelauney)
        {
            Gizmos.color = Color.red;
            foreach (Edge edge in RoomGraph.mesh.Edges)
            {
                Vertex v0 = RoomGraph.mesh.vertices[edge.P0];
                Vertex v1 = RoomGraph.mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, (float)v0.y);
                Vector3 p1 = new Vector3((float)v1.x, (float)v1.y);
                //Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
                //Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
                Gizmos.DrawLine(p0, p1);
            }

        }

        if (showNodeDelauney)
        {
            Gizmos.color = Color.red;
            foreach (var node in RoomGraph.AllNodes)
            {
                foreach (var arc in node.Arcs)
                {
                    Vector3 p0 = arc.parent.position;
                    Vector3 p1 = arc.child.position;
                    Gizmos.DrawLine(p0, p1);
                }
            }
        }
        if (showMST)
        {
            Gizmos.color = Color.green;
            foreach (var node in MSTGraph.AllNodes)
            {
                foreach (var arc in node.Arcs)
                {
                    Vector3 p0 = arc.parent.position;
                    Vector3 p1 = arc.child.position;
                    Gizmos.DrawLine(p0, p1);
                }
            }
        }

        foreach (var node in MSTGraph.AllNodes)
        {
            Gizmos.DrawWireSphere(node.position, 1f);
        }

    }
}
