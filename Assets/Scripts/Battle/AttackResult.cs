using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackResult 
{
    public enum Status
    {
        Hit, Misdelivered, Sunk, Error
    }
       
    public Status status = Status.Error;
    public Vector2 clearAreaStart, clearAreaEnd;
}
