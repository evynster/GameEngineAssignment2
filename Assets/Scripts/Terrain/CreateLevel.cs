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

    [DllImport("SaveLoadDLL")]
    private static extern void SaveData(int dataSize, int[] data);

    [DllImport("SaveLoadDLL")]
    private static extern System.IntPtr ReadData();

    [DllImport("SaveLoadDLL")]
    private static extern void DeleteArray(System.IntPtr array);

    //THIS DOES NOT WORK
    //[DllImport("EnginesQuizDLL")]
    //private static extern string generateRandomDungeon(int sizeX, int sizeY, int minRoom, int maxRoom, int minSize, int maxSize);

    public static event System.Action generateAction;

    [HideInInspector]
    public SingletonGeneration generateAmount;

    [SerializeField]
    private GameObject floor = null;

   
    static List<Structures.Room> rooms;
    static List<Structures.Hall> halls;

    List<Structures.Room> unconnectedRooms;
    List<Structures.Room> connectedRooms;
    List<Structures.Room> completeRooms;

    [SerializeField]
    private GameObject levelstart = null;

    public int maxSizeX = 100, maxSizeY = 100;

    private void Awake()
    {
        generateAmount = SingletonGeneration.Instance;
        originalMaxHallLength = maxHallLength;
        rooms = new List<Structures.Room>();
        unconnectedRooms = new List<Structures.Room>();
        connectedRooms = new List<Structures.Room>();
        completeRooms = new List<Structures.Room>();
        halls = new List<Structures.Hall>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [SerializeField]
    private int minRooms = 50;
    [SerializeField]
    private int maxRooms = 100;
    [SerializeField]
    private int minRoomSize = 5;
    [SerializeField]
    private int maxRoomSize = 12;//these variables let us change generation functionality easily

    [SerializeField]
    private int maxHallLength = 8;
    private int originalMaxHallLength;

    /*
     * working on a linear generation
     */
    public void generateLevel()
    {
        rooms = new List<Structures.Room>();
        unconnectedRooms = new List<Structures.Room>();
        connectedRooms = new List<Structures.Room>();
        completeRooms = new List<Structures.Room>();
        halls = new List<Structures.Hall>();

        LevelCommandInvoker.clearAll();
        generateAction?.Invoke();
        generateAmount.generations++;
        for(int i = levelstart.transform.childCount; i > 0; i--)
        {
            levelstart.transform.GetChild(i - 1).gameObject.SetActive(false);
        }


        int roomCount = Random.Range(minRooms,maxRooms);//get a count for how many rooms to make



        for (int i = 0; i < roomCount; i++){
            createRoom();
        }
        Structures.Room temp = rooms[0];
        temp.connected = true;

        halls = new List<Structures.Hall>();

        for (int i = 0; i < rooms.Count; i++)
            unconnectedRooms.Add(rooms[i]);

        connectedRooms.Add(unconnectedRooms[0]);
        unconnectedRooms.RemoveAt(0);


        int stuckChecker = 0;//TODO stuckchecker doesn't work
        int previousCount = 0;
        while (unconnectedRooms.Count > 0)
        {
            for (int i = 0; i < connectedRooms.Count; i++)
                connectNearbyRooms(connectedRooms[i]);

            if (connectedRooms.Count == previousCount)
            {
                
                maxHallLength++;
                stuckChecker++;
            }
            else
            {
                previousCount = connectedRooms.Count;
                stuckChecker = 0;
                maxHallLength = originalMaxHallLength;
            }
            

            if (stuckChecker > 5)
            {
                unconnectedRooms.Clear();
                Debug.LogWarning("There is an unconnected room!");
            }
        }
        

        


        for (int i = 0; i < rooms.Count; i++)
        {
            LevelCommandInvoker.AddCommand(new PlaceRoomCommand(rooms[i], floor, levelstart));
        }
        for (int i = 0; i < halls.Count; i++)
        {
            LevelCommandInvoker.AddCommand(new PlaceHallCommand(halls[i], floor, levelstart));
        }
    }
    /*
     * room creation
     * we use a dll to get a size of the room and then use the placeroom function to get a position for it
     * if the room wasn't able to get a position we don't add it to the list of rooms
     */
    private void createRoom()
    {
        Structures.Room tempRoom = new Structures.Room();
        tempRoom.connected = false;
        /*
         * this uses the dll
         */
        tempRoom.size = generateRoomSize(minRoomSize, maxRoomSize);


        tempRoom = PlaceRoom(tempRoom);

        if(tempRoom.pos.x!=-1)
            rooms.Add(tempRoom);
    }
    /*
     * the emergeCounter and placeRoom function are connected directly
     * we attempt to place a room in the grid of rooms using a dll generated coordinates
     * if the room can't place we retry using the function recursivively 
     * after 50 attempts we give up
     */
    private int emergeCounter = 0;
    private Structures.Room PlaceRoom(Structures.Room room)
    {
        if (rooms.Count == 0)
        {
            room.pos = new Vector2(0, 0);//position is the bottom left corner of the room
        }
        else
        {
            
            room.pos = generateRoomPos(0, maxSizeX - (int)room.size.x, maxSizeY - (int)room.size.y);
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
    /*
     * check to see if the next room collides with any of the old rooms
     */
    private bool checkCollision(Structures.Room room)
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
     * if no rooms are available we will add this room to the list of completed rooms
     * if the hallwaymax is too short to reach we will slowly increase the max until a room connects before reseting it
     */
    private void connectNearbyRooms(Structures.Room room)
    {
        bool above = false;//above tells us a room is available above us
        bool aboveDist = false;//aboveDist tells us if the hallway max will let us reach it

        bool below = false;
        bool belowDist = false;

        bool left = false;
        bool leftDist = false;

        bool right = false;
        bool rightDist = false;

        //4 lists to sort through and find the closest on each side
        List<Structures.Room> aboveRooms = new List<Structures.Room>();
        List<Structures.Room> belowRooms = new List<Structures.Room>();
        List<Structures.Room> leftRooms = new List<Structures.Room>();
        List<Structures.Room> rightRooms = new List<Structures.Room>();

        //we go through all the rooms
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].pos == room.pos)//if the room is our room we want to skip it
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
            else if (room.pos.y + room.size.y -0.1 >= rooms[i].pos.y && rooms[i].pos.y + rooms[i].size.y >= room.pos.y +0.1)
            {
                if (room.pos.x > rooms[i].pos.x)
                    rightRooms.Add(rooms[i]);
                else
                    leftRooms.Add(rooms[i]);
            } 
        }
        //these rooms will be the closest
        int aboveDistance = 10000;
        Structures.Room closestAbove = new Structures.Room();

        int belowDistance = 10000;
        Structures.Room closestBelow = new Structures.Room();

        int leftDistance = 10000;
        Structures.Room closestLeft = new Structures.Room();

        int rightDistance = 10000;
        Structures.Room closestRight = new Structures.Room();

        //go through all rooms in a direction and find the closest
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
            int hallLength = (int)(closestAbove.pos.y - (room.pos.y + room.size.y));//check how far it is
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestAbove.pos )
                {
                    above = true;
                    if (hallLength <= maxHallLength)
                        aboveDist = true;
                }
            }
            if (above&&aboveDist)
            {

                Structures.Hall tempHall = new Structures.Hall();

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
            int hallLength = (int)(room.pos.y-(closestBelow.pos.y + closestBelow.size.y));
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestBelow.pos )
                {
                    below = true;
                    if (hallLength <= maxHallLength)
                        belowDist = true;
                }
            }
            if (below&&belowDist)
            {
                Structures.Hall tempHall = new Structures.Hall();

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
            int hallLength = (int)(closestLeft.pos.x - (room.pos.x + room.size.x));
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestLeft.pos)
                {
                    left = true;
                    if (hallLength <= maxHallLength)
                        leftDist = true;
                }
            }
            if (left&&leftDist)
            {
                Structures.Hall tempHall = new Structures.Hall();

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
            int hallLength =(int)(room.pos.x - (closestRight.pos.x + closestRight.size.x));
            for (int i = 0; i < unconnectedRooms.Count; i++)
            {
                if (unconnectedRooms[i].pos == closestRight.pos)
                {
                    right = true;
                    if (hallLength <= maxHallLength)
                        rightDist = true;
                }
            }
            if (right&&rightDist)
            {
                Structures.Hall tempHall = new Structures.Hall();

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
    

    public void saveData()
    {
        List<int> test = new List<int>();
        Debug.Log(rooms.Count);
        for(int i = 0; i < rooms.Count; i++)
        {
            test.Add((int)rooms[i].pos.x);
            test.Add((int)rooms[i].pos.y);
            test.Add((int)rooms[i].size.x);
            test.Add((int)rooms[i].size.y);
            test.Add(-2);
        }
        for(int i = 0; i < halls.Count; i++)
        {
            test.Add((int)halls[i].start.x);
            test.Add((int)halls[i].start.y);
            test.Add((int)halls[i].end.x);
            test.Add((int)halls[i].end.y);
            test.Add(-3);
        }




        SaveData(test.Count,test.ToArray());
        
    }

    public void readData()
    {
        LevelCommandInvoker.clearAll();
        System.IntPtr test = ReadData();

        int arrayLength = Marshal.ReadInt32(test);

        System.IntPtr start = System.IntPtr.Add(test, 4);
        int[] result = new int[arrayLength];
        Marshal.Copy(start, result, 0, arrayLength);
        for (int i = levelstart.transform.childCount; i > 0; i--)
        {
            levelstart.transform.GetChild(i - 1).gameObject.SetActive(false); ;
        }

        if(rooms.Count>0)
            rooms.Clear();
        if(halls.Count>0)
            halls.Clear();

        for (int i = 1; i < result.Length; i+=5)
        {
            Vector2 temp1 = new Vector2(result[i], result[i + 1]);
            Vector2 temp2 = new Vector2(result[i+2], result[i + 3]);
            if (result[i + 4] == -2)
            {
                Structures.Room tempRoom = new Structures.Room();
                tempRoom.connected = true;
                tempRoom.pos = temp1;
                tempRoom.size = temp2;
                rooms.Add(tempRoom);
            }
            else
            {
                Structures.Hall tempHall = new Structures.Hall();
                tempHall.start = temp1;
                tempHall.end = temp2;
                halls.Add(tempHall);
            }
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            LevelCommandInvoker.AddCommand(new PlaceRoomCommand(rooms[i], floor, levelstart));
        }
        for (int i = 0; i < halls.Count; i++)
        {
            LevelCommandInvoker.AddCommand(new PlaceHallCommand(halls[i], floor, levelstart));
        }


        //there is unmanaged memory in the dll
        //DeleteArray(test);

    }
}
