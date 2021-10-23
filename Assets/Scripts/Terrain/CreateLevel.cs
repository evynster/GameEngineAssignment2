using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CreateLevel : MonoBehaviour
{
    /*
     * This is our DLL import
     */
    [DllImport("EnginesQuizDLL")] 
    private static extern Vector2Int generateRoomSize(int minSize, int maxSize);

    [DllImport("EnginesQuizDLL")]
    private static extern Vector2Int generateRoomPos(int minPos, int maxPosX, int maxPosY);
    //THIS DOES NOT WORK
    //[DllImport("EnginesQuizDLL")]
    //private static extern string generateRandomDungeon(int sizeX, int sizeY, int minRoom, int maxRoom, int minSize, int maxSize);

    public static event System.Action generateAction;

    [HideInInspector]
    public SingletonGeneration generateAmount;

    [SerializeField]
    private GameObject floor = null;
    struct Room
    {
        public Room(bool con = false)
        {
            pos = new Vector2();
            size = new Vector2();
            connected = con;
        }
        public Vector2 size;
        public Vector2 pos;
        public bool connected;
    }

    struct Hall
    {
        public Vector2 start;
        public Vector2 end;
    }
    List<Room> rooms;
    List<Hall> halls;

    List<Room> unconnectedRooms;
    List<Room> connectedRooms;
    List<Room> completeRooms;

    [SerializeField]
    private GameObject levelstart = null;


    private int[,] levelBaseData;

    private void Awake()
    {
        generateAmount = SingletonGeneration.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private int minRooms = 30;
    private int maxRooms = 100;

    private int minRoomSize = 5;
    private int maxRoomSize = 12;//these variables let us change generation functionality easily

    private int maxHallLength = 5;

    /*
     * working on a linear generation
     */
    public void generateLevel()
    {
        generateAction?.Invoke();
        generateAmount.generations++;
        for(int i = levelstart.transform.childCount; i > 0; i--)
        {
            GameObject.Destroy(levelstart.transform.GetChild(i-1).gameObject);
        }

        levelBaseData = new int[100, 100];

        int roomCount = Random.Range(minRooms,maxRooms);//get a count for how many rooms to make

        //THIS DOES NOT WORK
        //Debug.Log(generateRandomDungeon(levelBaseData.GetLength(0), levelBaseData.GetLength(1),minRooms,maxRooms,minRoomSize,maxRoomSize));
        rooms = new List<Room>();
        unconnectedRooms = new List<Room>();
        connectedRooms = new List<Room>();
        completeRooms = new List<Room>();

        for (int i = 0; i < roomCount; i++){
            createRoom();
        }
        Room temp = rooms[0];
        temp.connected = true;

        halls = new List<Hall>();

        for (int i = 0; i < rooms.Count; i++)
            unconnectedRooms.Add(rooms[i]);

        connectedRooms.Add(unconnectedRooms[0]);
        unconnectedRooms.RemoveAt(0);

        int stuckChecker = 0;
        int previousCount = 0;
        while (unconnectedRooms.Count > 0)
        {
            if (connectedRooms.Count == previousCount)
                stuckChecker++;
            previousCount = connectedRooms.Count;
           
            for (int i = 0; i < connectedRooms.Count; i++)
                connectNearbyRooms(connectedRooms[i]);
            if (stuckChecker > 50)
            {
                unconnectedRooms.Clear();
                Debug.LogWarning("There is an unconnected room!");
            }
        }
        

        createConnectors();



        for (int i = 0; i < levelBaseData.GetLength(0); i++)
        {
            for (int n = 0; n < levelBaseData.GetLength(1); n++)
            {
                if (levelBaseData[i, n] == 1)
                {
                    GameObject newFloor =  GameObject.Instantiate(floor);
                    newFloor.transform.parent = levelstart.transform;
                    newFloor.transform.position = new Vector3(i*4,0,n*4) + levelstart.transform.position;
                    newFloor.transform.localScale = new Vector3(200, 200, 200);
                    newFloor.layer = LayerMask.NameToLayer("Ground");
                }
            }
        }


    }
    private void createRoom()
    {
        Room tempRoom = new Room();
        tempRoom.connected = false;
        /*
         * this uses the dll
         */
        tempRoom.size = generateRoomSize(minRoomSize, maxRoomSize);


        tempRoom = PlaceRoom(tempRoom);

        for(int i = 0; i < tempRoom.size.x; i++)
        {
            for (int n = 0; n < tempRoom.size.y; n++)
            {
                levelBaseData[(int)tempRoom.pos.x + i, (int)tempRoom.pos.y + n] = 1;
            }
        }
        if(tempRoom.pos.x!=-1)
            rooms.Add(tempRoom);
    }
    private int emergeCounter = 0;
    private Room PlaceRoom(Room room)
    {
        if (rooms.Count == 0)
        {
            room.pos = new Vector2(0, 0);//position is the bottom left corner of the room
        }
        else
        {
            
            room.pos = generateRoomPos(0, levelBaseData.GetLength(0) - (int)room.size.x, levelBaseData.GetLength(0) - (int)room.size.y);
        }
        if(emergeCounter>=50)
        {
            room.pos = new Vector2(-1,-1);
            room.size = new Vector2();
            emergeCounter = 0;
            return room;
        }    
        if (checkCollision(room))
        {
            emergeCounter++;
            return PlaceRoom(room);
        }
        emergeCounter = 0;
        return room;
    }
    private void createConnectors()
    {
        for(int i = 0; i < halls.Count; i++)
        {
            if (halls[i].start.x == halls[i].end.x)
            {
                if(halls[i].start.y<halls[i].end.y)
                    for(int n = (int)halls[i].start.y; n < (int)halls[i].end.y; n++)
                    {
                        levelBaseData[(int)halls[i].start.x, n] = 1;
                    }
                else
                    for (int n = (int)halls[i].start.y; n >= (int)halls[i].end.y; n--)
                    {
                        levelBaseData[(int)halls[i].start.x, n] = 1;
                    }
            }
            else
            {
                if (halls[i].start.x < halls[i].end.x)
                    for (int n = (int)halls[i].start.x; n < (int)halls[i].end.x; n++)
                    {
                        levelBaseData[n,(int)halls[i].start.y] = 1;
                    }
                else
                    for (int n = (int)halls[i].start.x; n >= (int)halls[i].end.x; n--)
                    {
                        levelBaseData[ n, (int)halls[i].start.y] = 1;
                    }
            }
        }
    }
    /*
     * check to see if the next room collides with any of the old rooms
     */
    private bool checkCollision(Room room)
    {
        for(int i = 0; i < rooms.Count; i++)
        {
            bool xCollision = false, yCollision = false;

            xCollision = room.pos.x - 1 + room.size.x + 1 >= rooms[i].pos.x-1 && rooms[i].pos.x - 1 + rooms[i].size.x + 1 >= room.pos.x-1;
            yCollision = room.pos.y - 1 + room.size.y + 1 >= rooms[i].pos.y-1 && rooms[i].pos.y - 1 + rooms[i].size.y + 1 >= room.pos.y-1;
            if (xCollision && yCollision)
            {
                return true;
            }
        }
        return false;
    }

    /*
     * this function will find the nearest unconnected room so we can connect to it
     * if no rooms are available we will return the original room
     */
    private void connectNearbyRooms(Room room)
    {
        bool above = false;
        bool below = false;
        bool left = false;
        bool right = false;
        //4 lists to sort through and find the closest on each side
        List<Room> aboveRooms = new List<Room>();//rooms below
        List<Room> belowRooms = new List<Room>();
        List<Room> leftRooms = new List<Room>();
        List<Room> rightRooms = new List<Room>();

        //we go through all the rooms
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].pos == room.pos)
                continue;
            //those that collide on the x axis are our y rooms which we split up between the ones above and the ones below
            if (room.pos.x + room.size.x-0.1 >= rooms[i].pos.x && rooms[i].pos.x + rooms[i].size.x >= room.pos.x+0.1)
            {
                if (room.pos.y > rooms[i].pos.y)
                    belowRooms.Add(rooms[i]);
                else
                    aboveRooms.Add(rooms[i]);
            }
                //same but swapping the axis top find the left and right rooms
            if (room.pos.y + room.size.y -0.1 >= rooms[i].pos.y && rooms[i].pos.y + rooms[i].size.y >= room.pos.y +0.1)
            {
                if (room.pos.x > rooms[i].pos.x)
                    rightRooms.Add(rooms[i]);
                else
                    leftRooms.Add(rooms[i]);
            } 
        }

        int aboveDistance = 10000;
        Room closestAbove = new Room();

        int belowDistance = 10000;
        Room closestBelow = new Room();

        int leftDistance = 10000;
        Room closestLeft = new Room();

        int rightDistance = 10000;
        Room closestRight = new Room();


        for (int i = 0; i < aboveRooms.Count; i++)
        {
            int tempDist = (int)(aboveRooms[i].pos.y - room.pos.y);
            if (tempDist < aboveDistance)
            {
                aboveDistance = tempDist;
                closestAbove = aboveRooms[i];
            }
        }

        for (int i = 0; i < belowRooms.Count; i++)
        {
            int tempDist = (int)(room.pos.y - belowRooms[i].pos.y);
            if (tempDist < belowDistance)
            {
                belowDistance = tempDist;
                closestBelow = belowRooms[i];
            }
        }

        for (int i = 0; i < leftRooms.Count; i++)
        {
            int tempDist = (int)(leftRooms[i].pos.x - room.pos.x);
            if (tempDist < leftDistance)
            {
                leftDistance = tempDist;
                closestLeft = leftRooms[i];
            }
        }

        for (int i = 0; i < rightRooms.Count; i++)
        {
            int tempDist = (int)(room.pos.x - rightRooms[i].pos.x);
            if (tempDist < rightDistance)
            {
                rightDistance = tempDist;
                closestRight = rightRooms[i];
            }
        }

        //if the room can exist and it's not already connected
        if (aboveRooms.Count > 0)
        {
            int hallLength = 00;// (int)(closestAbove.pos.y - (room.pos.y + room.size.y));
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestAbove.pos && hallLength <= maxHallLength)
                {
                    above = true;
                }
            }
            if (above)
            {

                Hall tempHall = new Hall();

                int minX, maxX;

                if (room.pos.x > closestAbove.pos.x)
                    minX = (int)room.pos.x;
                else
                    minX = (int)closestAbove.pos.x;

                if (room.pos.x + room.size.x < closestAbove.pos.x + closestAbove.size.x)
                    maxX = (int)(room.pos.x + room.size.x);
                else
                    maxX = (int)(closestAbove.pos.x + closestAbove.size.x);

                int xLoc = Random.Range(minX, maxX);
                tempHall.start = new Vector2(xLoc, room.pos.y + room.size.y);

                tempHall.end = new Vector2(xLoc, closestAbove.pos.y);
                
                closestAbove.connected = true;
                connectedRooms.Add(closestAbove);
                for(int i = 0; i < unconnectedRooms.Count; i++)
                {
                    if (unconnectedRooms[i].pos == closestAbove.pos)
                    {
                        unconnectedRooms.RemoveAt(i);
                    }
                }
               
                halls.Add(tempHall);
            }
        }  
        
        if (belowRooms.Count > 0)
        {
            int hallLength = 0;// (int)((closestBelow.pos.y + closestBelow.size.y) - room.pos.y);
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestBelow.pos && hallLength <= maxHallLength)
                {
                    below = true;
                }
            }
            if (below)
            {
                Hall tempHall = new Hall();

                int minX, maxX;

                if (room.pos.x > closestBelow.pos.x)
                    minX = (int)room.pos.x;
                else
                    minX = (int)closestBelow.pos.x;

                if (room.pos.x + room.size.x < closestBelow.pos.x + closestBelow.size.x)
                    maxX = (int)(room.pos.x + room.size.x);
                else
                    maxX = (int)(closestBelow.pos.x + closestBelow.size.x);

                int xLoc = Random.Range(minX, maxX);
                tempHall.start = new Vector2(xLoc, room.pos.y);

                tempHall.end = new Vector2(xLoc, closestBelow.pos.y + closestBelow.size.y);

                closestBelow.connected = true;

                connectedRooms.Add(closestBelow);
                for (int i = 0; i < unconnectedRooms.Count; i++)
                {
                    if (unconnectedRooms[i].pos == closestBelow.pos)
                    {
                        unconnectedRooms.RemoveAt(i);
                    }
                }
                halls.Add(tempHall);
            }
        }        
        
        if (leftRooms.Count > 0)
        {
            int hallLength = 0;// (int)(room.pos.x -( closestLeft.pos.x + closestLeft.size.x));
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestLeft.pos && hallLength <= maxHallLength)
                {
                    left = true;
                }
            }
            if (left)
            {
                Hall tempHall = new Hall();

                int minY, maxY;

                if (room.pos.y > closestLeft.pos.y)
                    minY = (int)room.pos.y;
                else
                    minY = (int)closestLeft.pos.y;

                if (room.pos.y + room.size.y < closestLeft.pos.y + closestLeft.size.y)
                    maxY = (int)(room.pos.y + room.size.y);
                else
                    maxY = (int)(closestLeft.pos.y + closestLeft.size.y);

                int yLoc = Random.Range(minY, maxY);
                tempHall.start = new Vector2(room.pos.x + room.size.x,yLoc);

                tempHall.end = new Vector2( closestLeft.pos.x, yLoc);

                closestLeft.connected = true;
                connectedRooms.Add(closestLeft);
                for (int i = 0; i < unconnectedRooms.Count; i++)
                {
                    if (unconnectedRooms[i].pos == closestLeft.pos)
                    {
                        unconnectedRooms.RemoveAt(i);
                    }
                }

                halls.Add(tempHall);
            }
        }
        
        if (rightRooms.Count > 0)
        {
            int hallLength = 0;// (int)(closestRight.pos.x - (room.pos.x + room.size.x) );
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestRight.pos && hallLength <= maxHallLength)
                {
                    right = true;
                }
            }
            if (right)
            {
                Hall tempHall = new Hall();

                int minY, maxY;

                if (room.pos.y > closestRight.pos.y)
                    minY = (int)room.pos.y;
                else
                    minY = (int)closestRight.pos.y;

                if (room.pos.y + room.size.y < closestRight.pos.y + closestRight.size.y)
                    maxY = (int)(room.pos.y + room.size.y);
                else
                    maxY = (int)(closestRight.pos.y + closestRight.size.y);

                int yLoc = Random.Range(minY, maxY);
                tempHall.start = new Vector2(room.pos.x, yLoc);

                tempHall.end = new Vector2(closestRight.pos.x + closestRight.size.x, yLoc);

                closestRight.connected = true;
                connectedRooms.Add(closestRight);
                for (int i = 0; i < unconnectedRooms.Count; i++)
                {
                    if (unconnectedRooms[i].pos == closestRight.pos)
                    {
                        unconnectedRooms.RemoveAt(i);
                    }
                }

                halls.Add(tempHall);
            }
        }
        
        if (!above && !below && !left && !right)
        {
            completeRooms.Add(room);
            for(int i = 0; i < connectedRooms.Count; i++)
            {
                if (room.pos == connectedRooms[i].pos)
                {
                    connectedRooms.RemoveAt(i);
                }
            }
        }
    }
}
