using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public GridTile CurrentTile;
    public int Movement = 1;
    public int AttackRange = 1;

    public List<GridTile> Path;

    public Unit TargetUnit;

    public UnitType UnitType = UnitType.red;

    #region CallBacks
    public delegate void MoveToTileCB();


    #endregion // CallBacks

    public virtual void StartMove() { }

    public virtual void StartAttack() { }

    public void MoveToTile(GridTile Tile, MoveToTileCB MoveCB = null)
    {
        transform.DOMove(Tile.GetWalkPosition(), 0.5f).SetEase(Ease.OutCubic).OnComplete
            (() =>
            {
                CurrentTile = Tile;
                if (MoveCB != null)
                {
                    MoveCB();
                }
            }
            );
    }


    public GridTile GetCurrentTile() 
    {
        return CurrentTile;
    }
}
