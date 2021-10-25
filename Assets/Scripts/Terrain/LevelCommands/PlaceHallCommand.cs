using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceHallCommand : LevelCommand
{
    Structures.Hall hall;
    GameObject floor;
    GameObject levelStart;
    public PlaceHallCommand(Structures.Hall h, GameObject f, GameObject l)
    {
        hall = h;
        floor = f;
        levelStart = l;
    }
    public void Execute()
    {
        if (hall.start.x == hall.end.x)//up and down
        {
            if (hall.start.y < hall.end.y)//hall going downwards
                for (int n = (int)hall.start.y; n < (int)hall.end.y; n++)
                {
                    GameObject newFloor = FloorFactory.createFloor(0);
                    newFloor.transform.position = new Vector3(hall.start.x * 4, 0, n * 4) + levelStart.transform.position;
                    hall.hallComponents.Add(newFloor);
                }
            else//hall going upwards
                for (int n = (int)hall.start.y-1; n >= (int)hall.end.y; n--)
                {
                    GameObject newFloor = FloorFactory.createFloor(0);
                    newFloor.transform.position = new Vector3(hall.start.x * 4, 0, n * 4) + levelStart.transform.position;
                    hall.hallComponents.Add(newFloor);
                }
        }
        else
        {
            if (hall.start.x < hall.end.x)//left and right
                for (int n = (int)hall.start.x; n < (int)hall.end.x; n++)//hall going right
                {
                    GameObject newFloor = FloorFactory.createFloor(0);
                    newFloor.transform.position = new Vector3(n * 4, 0, hall.start.y * 4) + levelStart.transform.position;
                    hall.hallComponents.Add(newFloor);
                }
            else//hall going left
                for (int n = (int)hall.start.x-1; n >= (int)hall.end.x; n--)
                {
                    GameObject newFloor = FloorFactory.createFloor(0);
                    newFloor.transform.position = new Vector3(n * 4, 0, hall.start.y * 4) + levelStart.transform.position;
                    hall.hallComponents.Add(newFloor);
                }
        }
    }
    public void Undo()
    {
        for (int i = 0; i < hall.hallComponents.Count; i++)
        {
            hall.hallComponents[i].SetActive(false);
        }
        hall.hallComponents.Clear();
    }
}