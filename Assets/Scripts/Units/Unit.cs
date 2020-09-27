using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GridTile CurrentTile;
    public int Movement = 1;

    public List<GridTile> Path;

    public Unit TargetUnit;

    public UnitType UnitType = UnitType.red;

    public int Health=10;

    public List<Ability> Abilities;


    #region CallBacks
    public delegate void ActionCallback();

    public ActionCallback Callback;
    #endregion // CallBacks



    private void Start()
    {
        Abilities.Add(new Ability());
    }
    public virtual void StartNextAction()
    {
        // Check for usable abilities
        foreach (Ability ability in Abilities)
        {
            if (CanUseAbility(ability))
            {
                UseAbility(ability, StartNextAction);

                return;
                
            }
        }

        Move();
    }

    public virtual void Move()
    {

        TargetUnit = UnitManager.Instance.GetNearestUnit(this);

        Path = UnitManager.Instance.GetPathToUnit(this, TargetUnit);

        if (Path.Count >0)
        {
            MoveToTile(Path[0] , StartNextAction);
        }
        //Types.HighlightTiles(Path, UnitType == UnitType.red ? Color.red : Color.blue);
    }

    public bool CanUseAbility(Ability ability)
    {
        // Get the path to the nearest enemy,

        TargetUnit = UnitManager.Instance.GetNearestUnit(this);

        Path = UnitManager.Instance.GetPathToUnit(this, TargetUnit);

        if (Path.Count <= ability.Range)
        {
            return true;
        }

        return false;
    }

    public void UseAbility(Ability ability, ActionCallback acb = null)
    {
        // Play animation
        ability.UseAbility(TargetUnit, acb);
    }
    public void MoveToTile(GridTile tile, ActionCallback acb = null)
    {
        transform.DOMove(tile.GetWalkPosition(), 0.5f).SetEase(Ease.OutCubic).OnComplete
            (() =>
            {
                CurrentTile = tile;
                if (acb != null)
                {
                    acb();
                }
            }
            );
    }


    public void EndTurn()
    {

    }

    public GridTile GetCurrentTile()
    {
        return CurrentTile;
    }


}
