using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    private Grid m_GM;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        m_GM = (Grid)target;

        if (GUILayout.Button("Generate grid"))
        {
            m_GM.GenerateGrid();
        }

        if (GUILayout.Button("Clear grid"))
        {
            m_GM.ClearGrid();
        }


    }
}


#endif
public class Grid : Singleton<Grid>
{
    public GridTile TilePrefab;

    public int Xtiles, Ztiles;
    public Dictionary<TileKey, GridTile> GridTiles = new Dictionary<TileKey, GridTile>();


    #region Pathfinding
    private List<GridTile> m_ClosedList = new List<GridTile>();
    private List<GridTile> m_OpenList = new List<GridTile>();
    private List<GridTile> m_Path = new List<GridTile>();

    private List<GridTile> m_PathableUpgrades = new List<GridTile>();
    #endregion
    protected override void Awake()
    {
        base.Awake();
        transform.position = Vector3.zero;
        if (transform.childCount == 0)
        {
            GenerateGrid();
        }
        else
        {
            LoadGridFromWorld();
        }
    }
        

    public void GenerateGrid()
    {

#if UNITY_EDITOR
        for (int xindex = 0; xindex < Xtiles; xindex++)
        {
            for (int zindex = 0; zindex < Ztiles; zindex++)
            {
                TileKey _newKey = new TileKey(xindex , zindex);
                GridTile _newTile = (GridTile)PrefabUtility.InstantiatePrefab(TilePrefab, transform);
                _newTile.Init(_newKey);
                GridTiles.Add(_newKey, _newTile);
            }
        }

        foreach (KeyValuePair<TileKey, GridTile> item in GridTiles)
        {
            item.Value.SetSurroundingTiles(this);
        }

#endif
    }

    public void ClearGrid()
    {
        for (int _transformChild = transform.childCount - 1; _transformChild > -1; _transformChild--)
        {
            DestroyImmediate(transform.GetChild(_transformChild).gameObject);
        }

        GridTiles.Clear();
    }

    public void LoadGridFromWorld()
    {
        foreach (var item in GridTiles)
        {
            item.Value.ClearSurroundingTiles(this);

        }
        GridTiles.Clear();

        GridTile[] _tileArray = FindObjectsOfType<GridTile>();

        // inactive gridtiles should not be included in this fetch
        for (int i = _tileArray.Length - 1; i >= 0; i--)
        {
            GridTile _tile = _tileArray[i];

            TileKey _key = GetKeyFromVector(_tile.transform.position);
            if (GridTiles.ContainsKey(_key))
            {

                DestroyImmediate(_tile.gameObject);
                Debug.Log(_key + " destroyed");
                continue;
            }

            Vector3 _position = Vector3.zero;
            _position.x = Mathf.RoundToInt(_key.X);
            _position.z = Mathf.RoundToInt(_key.Z);
            _tile.transform.position = _position;
            _tile.transform.parent = transform;
            _tile.name = _key.X + " " + _key.Z;
            _tile.Init(_key);

            GridTiles.Add(_key, _tile);
        }


        foreach (KeyValuePair<TileKey, GridTile> item in GridTiles)
        {
            item.Value.SetSurroundingTiles(this);
        }
        Debug.Log(GridTiles.Count);
    }

    public List<GridTile> GetPath(GridTile startTile, GridTile goalTile)
    {
        m_OpenList.Clear();
        m_ClosedList.Clear();
        m_Path.Clear();

        GridTile _CurrentTile;
        GridTile _PathTile;
        // Reset values of grid tiles
        foreach (KeyValuePair<TileKey, GridTile> item in GridTiles)
        {
            _CurrentTile = item.Value;
            _CurrentTile.f = Mathf.Infinity;
            _CurrentTile.h = Mathf.Infinity;
            _CurrentTile.g = Mathf.Infinity;
        }

        startTile.g = 0;
        startTile.h = 0;
        startTile.f = 0;
        m_OpenList.Add(startTile);

        if (startTile == goalTile)
        {
            m_OpenList.Add(goalTile);
            return m_OpenList;
        }
        while (m_OpenList.Count != 0)
        {
            // Sort the list playerList.Sort((p1,p2)=>p1.score.CompareTo(p2.score));
            m_OpenList.Sort((tileOne, tileTwo) => tileOne.f.CompareTo(tileTwo.f));

            // Get the lowest f
            _CurrentTile = m_OpenList[0];
            m_OpenList.RemoveAt(0);
            m_ClosedList.Add(_CurrentTile);
            // Iterate all surrounding tiles
            GridTile _surroundingTile;
            float _newH, _newG, _newF;
            foreach (KeyValuePair<TileKey, GridTile> item in _CurrentTile.SurroundingTiles)
            {
                _surroundingTile = item.Value;

                if (m_ClosedList.Contains(_surroundingTile) || !TileIsClear(_surroundingTile))
                {
                    continue;
                }

                if (_surroundingTile == goalTile)
                {
                    // Assign parent tile
                    _surroundingTile.ParentTile = _CurrentTile;
                    _PathTile = _surroundingTile;
                    while (_PathTile != startTile)
                    {
                        m_Path.Add(_PathTile);
                        _PathTile = _PathTile.ParentTile;
                    }

                    m_Path.Add(_PathTile);
                    // Goal found, create path
                    m_Path.Remove(startTile);
                    return m_Path;
                }
                _newH = GetHeuristic(_surroundingTile.Key, goalTile.Key);
                _newG = _CurrentTile.g + 1;
                _newF = _newG + _newH;
                // check this is a better path
                if (_newF < _surroundingTile.f)
                {
                    _surroundingTile.ParentTile = _CurrentTile;
                    _surroundingTile.h = _newH;
                    _surroundingTile.g = _newG;
                    _surroundingTile.f = _newF;

                    m_OpenList.Add(_surroundingTile);

                }
            }
        }

        return null;
    }

    private float GetHeuristic(TileKey fromKey, TileKey toKey)
    {
        return Mathf.Abs(toKey.X - fromKey.X) + Mathf.Abs(toKey.Z - fromKey.Z);
    }
    private bool TileIsClear(GridTile tile)
    {
        return tile.Pathable();
    }


    public bool GetTile(TileKey key, out GridTile tile)
    {
        return GridTiles.TryGetValue(key, out tile);

    }

    public GridTile AddTileAtKey(TileKey key)
    {
        GridTile _newTile = Instantiate(TilePrefab, transform);
        _newTile.Init(key);
        GridTiles.Add(key, _newTile);
        _newTile.UpdateSurroundingTiles(this);
        return _newTile;
    }

    public void RemoveTileFromGrid(GridTile _tile)
    {
        _tile.ClearSurroundingTiles(this);
        GridTiles.Remove(_tile.Key);
        Destroy(_tile.gameObject);
    }
    public GridTile GetTile(TileKey key)
    {
        if (GridTiles.TryGetValue(key, out GridTile _returnTile))
        {
            return _returnTile;
        }

        return null;
    }

    public GridTile GetTile(int x, int z)
    {
        TileKey _key = new TileKey(x, z);
        if (GridTiles.TryGetValue(_key, out GridTile _returnTile))
        {
            return _returnTile;
        }

        return null;
    }

    public GridTile GetTile(Vector3 position)
    {
        GridTiles.TryGetValue(GetKeyFromVector(position), out GridTile _returnGridTile);
        return _returnGridTile;
    }

    public List<GridTile> GetTilesAdjacent(TileKey key)
    {
        List<GridTile> AdjacentTiles = new List<GridTile>();

        GridTile _tile;

        _tile = GetTile(key + TileKey.north);
        if (_tile != null)
        {
            AdjacentTiles.Add(_tile);
        }

        _tile = GetTile(key + TileKey.east);
        if (_tile != null)
        {
            AdjacentTiles.Add(_tile);
        }

        _tile = GetTile(key + TileKey.south);
        if (_tile != null)
        {
            AdjacentTiles.Add(_tile);
        }

        _tile = GetTile(key + TileKey.west);
        if (_tile != null)
        {
            AdjacentTiles.Add(_tile);
        }


        return AdjacentTiles;
    }
    public GridTile GetRandomPathableTile(GridTile startTile)
    {
        m_OpenList.Clear();
        m_ClosedList.Clear();
        m_OpenList.Add(startTile);

        GridTile _currentTile;
        while (m_OpenList.Count != 0)
        {
            _currentTile = m_OpenList[0];
            m_OpenList.RemoveAt(0);
            m_ClosedList.Add(_currentTile);

            foreach (var item in _currentTile.SurroundingTiles)
            {
                if (!m_ClosedList.Contains(item.Value))
                {
                    m_OpenList.Add(item.Value);
                }
            }
        }

        _currentTile = m_ClosedList[Random.Range(0, m_ClosedList.Count)];

        return _currentTile;

    }

    public GridTile GetRandomTile()
    {
        int _randomValue = Random.Range(0, GridTiles.Count);
        int _currentIteration = 0;
        foreach (var item in GridTiles)
        {
            _currentIteration += 1;
            if (_currentIteration == _randomValue)
            {
                return item.Value;
            }
        }
        return null;
    }
    public static TileKey GetKeyFromVector(Vector3 vec)
    {
        return new TileKey(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z));
    }

}
