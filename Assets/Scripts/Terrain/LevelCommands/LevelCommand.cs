using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface LevelCommand 
{
    void Execute();

    void Undo();
}
