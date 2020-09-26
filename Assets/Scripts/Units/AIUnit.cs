using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnit : Unit
{
    public override void StartMove()
    {
        base.StartMove();

        TargetUnit = UnitManager.Instance.GetNearestUnit(this);

        Path = UnitManager.Instance.GetPathToUnit(this , TargetUnit);

        Types.HighlightTiles(Path , UnitType == UnitType.red ? Color.red : Color.blue );

        
    }
}
