using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager>
{
    public List<Unit> AllUnits;
    public List<Unit> BlueUnits;
    public List<Unit> RedUnits;

    public delegate void WaitCB();
    private WaitCB waitCB;
    bool bwaiting = false;
    float waitTime;
    float currentTime = 0;

    protected override void Awake()
    {
        base.Awake();

        Unit[] unitarr = Resources.FindObjectsOfTypeAll<Unit>();
        for (int i = 0; i < unitarr.Length; i++)
        {
            AllUnits.Add(unitarr[i]);
        }
    }

    private void Start()
    {
        foreach (Unit unit in AllUnits)
        {
            unit.StartNextAction();
        }
    }
    private void Update()
    {
        if (bwaiting)
        {
            currentTime += Time.deltaTime;
            if (currentTime > waitTime)
            {
                if (waitCB!=null)
                {
                    bwaiting = false;
                    waitCB();
                    
                }
            }
        }
    }
    void StartNextTurn()
    {
        AllUnits[Random.Range(0, AllUnits.Count)].Move();
        Wait(0.5f , StartNextTurn);
    }

    void Wait(float time, WaitCB CB)
    {
        bwaiting = true;
        waitTime = time;
        waitCB = CB;
        currentTime = 0;
    }


    private struct DistancePacket
    {
        public float Distance;
        public Unit Unit;
    }

    public List<GridTile> GetPathToUnit(Unit start,Unit end)
    {
        return Grid.Instance.GetPath(start.GetCurrentTile() , end.GetCurrentTile());
    }

    public List<Unit> GetUnitsInRange(Unit start, int range)
    {
        List<Unit> _units = new List<Unit>();

       


        return _units;
    }
    public Unit GetNearestUnit(Unit start)
    {
        // todo: check path to target
        DistancePacket current;
        current.Distance = Mathf.Infinity;
        current.Unit = null;

        foreach (Unit unit in AllUnits)
        {
            if (unit == start)
            {
                continue;
            }
            float dist = Mathf.Abs( Vector3.Distance(start.GetCurrentTile().GetWalkPosition() , unit.GetCurrentTile().GetWalkPosition()));
            if (dist < current.Distance)
            {
                current.Distance = dist;
                current.Unit = unit;
            }
        }

        return current.Unit;
    }
}
