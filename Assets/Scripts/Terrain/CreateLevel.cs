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
        public Vector2 size;
        public Vector2 pos;
    }

    List<Room> rooms;

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

        for (int i = 0; i < roomCount; i++){
            createRoom();
        }



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
            room.pos = new Vector2();
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
    private void createConnector()
    {

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
}
