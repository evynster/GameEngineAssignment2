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
        if (hall.start.x == hall.end.x)
        {
            if (hall.start.y < hall.end.y)
                for (int n = (int)hall.start.y; n < (int)hall.end.y; n++)
                {
                    GameObject newFloor = GameObject.Instantiate(floor);
                    newFloor.transform.parent = levelStart.transform;
                    newFloor.transform.position = new Vector3(hall.start.x * 4, 0, n * 4) + levelStart.transform.position;
                    newFloor.transform.localScale = new Vector3(200, 200, 200);
                    newFloor.layer = LayerMask.NameToLayer("Ground");
                    newFloor.transform.SetParent(levelStart.transform);
                    hall.hallComponents.Add(newFloor);
                }
            else
                for (int n = (int)hall.start.y; n >= (int)hall.end.y; n--)
                {
                    GameObject newFloor = GameObject.Instantiate(floor);
                    newFloor.transform.parent = levelStart.transform;
                    newFloor.transform.position = new Vector3(hall.start.x * 4, 0, n * 4) + levelStart.transform.position;
                    newFloor.transform.localScale = new Vector3(200, 200, 200);
                    newFloor.layer = LayerMask.NameToLayer("Ground");
                    newFloor.transform.SetParent(levelStart.transform);
                    hall.hallComponents.Add(newFloor);
                }
        }
        else
        {
            if (hall.start.x < hall.end.x)
                for (int n = (int)hall.start.x; n < (int)hall.end.x; n++)
                {
                    GameObject newFloor = GameObject.Instantiate(floor);
                    newFloor.transform.parent = levelStart.transform;
                    newFloor.transform.position = new Vector3(n * 4, 0, hall.start.y * 4) + levelStart.transform.position;
                    newFloor.transform.localScale = new Vector3(200, 200, 200);
                    newFloor.layer = LayerMask.NameToLayer("Ground");
                    newFloor.transform.SetParent(levelStart.transform);
                    hall.hallComponents.Add(newFloor);
                }
            else
                for (int n = (int)hall.start.x; n >= (int)hall.end.x; n--)
                {
                    GameObject newFloor = GameObject.Instantiate(floor);
                    newFloor.transform.parent = levelStart.transform;
                    newFloor.transform.position = new Vector3(n * 4, 0, hall.start.y * 4) + levelStart.transform.position;
                    newFloor.transform.localScale = new Vector3(200, 200, 200);
                    newFloor.layer = LayerMask.NameToLayer("Ground");
                    newFloor.transform.SetParent(levelStart.transform);
                    hall.hallComponents.Add(newFloor);
                }
        }
    }
    public void Undo()
    {
        for (int i = 0; i < hall.hallComponents.Count; i++)
        {
            GameObject.Destroy(hall.hallComponents[i]);
        }
    }
}