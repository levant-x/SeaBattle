using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerGameField;

public interface IAttacker 
{
    AttackResult AttackGameField(PlayerGameField gameField);
    bool isGameOver { get; }
}
