using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlaceRoomCommand : LevelCommand
{
    
    Structures.Room room;
    GameObject floor;
    GameObject levelStart;

    public PlaceRoomCommand(Structures.Room r,GameObject f,GameObject l)
    {
        room = r;
        floor = f;
        levelStart = l;
    }
    public void Execute()
    {
        int colour = Random.Range(0, 6);
        for (int i = 0; i<room.size.x; i++)
        {
            for (int n = 0; n <room.size.y; n++)
            {
                GameObject newFloor = FloorFactory.createFloor((FloorFactory.floorColour)colour);
                newFloor.transform.parent = levelStart.transform;
                newFloor.transform.position = new Vector3((room.pos.x + i) * 4, 0, (room.pos.y + n) * 4) + levelStart.transform.position;
                newFloor.transform.localScale = new Vector3(200, 200, 200);
                newFloor.layer = LayerMask.NameToLayer("Ground");
                newFloor.transform.SetParent(levelStart.transform);
                room.roomComponents.Add(newFloor);
            }
        }
    }

    public void Undo()
    {
        for(int i =0;i< room.roomComponents.Count; i++)
        {
            GameObject.Destroy(room.roomComponents[i]);
        }
    }
}
