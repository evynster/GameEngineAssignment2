using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCommandInvoker : MonoBehaviour
{

    static Queue<LevelCommand> commandList;

    static List<LevelCommand> commandHistory;

    static int counter;

    private void Awake()
    {
        commandList = new Queue<LevelCommand>();
        commandHistory = new List<LevelCommand>();
    }

    public static void AddCommand(LevelCommand command)
    {
        while (commandHistory.Count > counter)
        {
            commandHistory.RemoveAt(counter);
        }
        commandList.Enqueue(command);
    }
    public static void clearAll()
    {
        while (commandList.Count > 0)
            commandList.Dequeue();
        while (commandHistory.Count > 0)
            commandHistory.RemoveAt(0);
        counter = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (commandList.Count > 0)
        {
            LevelCommand c = commandList.Dequeue();
            c.Execute();

            commandHistory.Add(c);
            counter++;
        }
    }
    public void undo()
    {
        if (counter > 0)
        {
            counter--;
            commandHistory[counter].Undo();
        }

    }
    public void redo()
    {
        if (counter < commandHistory.Count)
        {
            commandHistory[counter].Execute();
            counter++;
        }
    }
}
