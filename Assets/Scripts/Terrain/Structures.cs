using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structures
{
    public class Room 
    {
        public Room(bool con = false)
        {
            pos = new Vector2();
            size = new Vector2();
            connected = con;
            roomComponents = new List<GameObject>();
        }
        public Vector2 size;
        public Vector2 pos;
        public bool connected;
        public List<GameObject> roomComponents;
    }
    public class Hall
    {
        public Hall()
        {
            start = new Vector2();
            end = new Vector2();
            hallComponents = new List<GameObject>();
        }
        public Vector2 start;
        public Vector2 end;
        public List<GameObject> hallComponents;
    }

}
