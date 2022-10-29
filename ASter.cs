using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ASter : MonoBehaviour
{
    [SerializeField] private GUIStyle gUIStyle;
    [Header("WorldSetting")]
    public Vector2Int worldSize;
    public Vector2Int origin;
    [Header("PointSetting")]
    public Vector2Int startPoint;
    public Vector2Int endPoint;
    [Header("Step")]
    [Range(1, 1000)] public int step;
    [Range(1, 1000)] public int step2;
    [Header("Gizmo")]
    [SerializeField] private float gizmoSize;
    private float[,] grid;//0=normal -1=wall -2=close 
    private Grid nextGrid;
    private List<Grid> openGridList = new List<Grid>();
    private List<Grid> resultGridList = new List<Grid>();
    private List<Vector2Int> pathGridList = new List<Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
    public bool GO;
    [SerializeField] private float rayHeight;
    public bool finded;
    private void OnDrawGizmos()
    {
        if (GO)
        {
            Gizmos.DrawSphere(V2IToV3(startPoint), gizmoSize);
            Gizmos.DrawSphere(V2IToV3(endPoint), gizmoSize);
            Initialize();
            for (int i = 0; i < step; i++)
            {
                Search(i);
                if (finded) { break; }
            }
            PathFind();
            for (int i = 0; i < pathGridList.Count; i++)
            {
                Gizmos.DrawWireSphere(V2IToV3(pathGridList[i]), gizmoSize);
            }
            for (int i = 0; i < resultGridList.Count; i++)
            {
                var dbg_str = grid[resultGridList[i].position.x, resultGridList[i].position.y].ToString();
                Handles.Label(V2IToV3(resultGridList[i].position) + Vector3.up, $"<color=#000000>{dbg_str}</color>", gUIStyle);
            }

        }
    }
 
    private void Initialize()
    {
        grid = new float[worldSize.x, worldSize.y];
        nextGrid = new Grid();
        openGridList = new List<Grid>();
        resultGridList = new List<Grid>();
        pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
        pathGridList = new List<Vector2Int>();
        for (int i = 0; i < worldSize.x; i++)
        {
            for (int ii = 0; ii < worldSize.y; ii++)
            {
                var thisGrid = new Vector3(origin.x + i, 0, origin.y + ii);
                var thisWorldGrid = V3ToV2I(thisGrid);
                if (CheckWall(thisGrid)) { SetGrid(thisWorldGrid, -1); }
                else { SetGrid(thisWorldGrid, 0); }
            }
        }
    }
    private void Search(int i)
    {
        finded = nextGrid.position == endPoint;
        var previousGrid = nextGrid.position;
        if (i == 0) { nextGrid.position = startPoint; Check8Direction(nextGrid); }
        else { Check8Direction(nextGrid); }
        nextGrid = CheckNextGrid(openGridList);
        if (!resultGridList.Exists(x => x.position == nextGrid.position))
        {
            resultGridList.Add(nextGrid);
        }
        else
        {
            nextGrid.parent = nextGrid.position;
        }
        pathGridDictionary.Add(nextGrid.position, nextGrid.parent);
    }
    private void Check8Direction(Grid thisGrid)
    {
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.up });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.up + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down + Vector2Int.right });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.down + Vector2Int.left });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.left });
        AddOpenGridList(new Grid { parent = thisGrid.position, position = thisGrid.position + Vector2Int.left + Vector2Int.up });
    }
    private void AddOpenGridList(Grid thisGrid)
    {
        if (AllowGrid(thisGrid)) { openGridList.Add(thisGrid); }
    }
    private Grid CheckNextGrid(List<Grid> grids)
    {
        var result = new Grid();
        var gridList = new List<Grid>();
        var costList = new List<float>();
        foreach (var thisGrid in grids)
        {
            if (!resultGridList.Exists(x => x.position == thisGrid.position))
            {
                var cost = CalcCost(thisGrid);
                thisGrid.cost = cost;
                gridList.Add(thisGrid);
                costList.Add(cost);
            }
        }
        var lowestCost = Mathf.Min(costList.ToArray());
        result = gridList.Find(x => x.cost == lowestCost);
        return result;
    }
    private void PathFind()
    {
        if (!finded) { return; }
        var nextPathGrid = new Vector2Int();
        for (int i = 0; i < step2; i++)
        {
            if (i == 0) { nextPathGrid = endPoint; }
            else
            {
                if (pathGridDictionary.ContainsKey(nextPathGrid))
                {
                    nextPathGrid = pathGridDictionary[nextPathGrid];
                }
            }
            pathGridList.Add(nextPathGrid);
        }
    }
    private void SetGrid(Vector2Int thisGrid, int set)
    {
        grid[thisGrid.x, thisGrid.y] = set;
    }
    private bool CheckWall(Vector3 thisWorldGrid)
    {
        var rayStart = thisWorldGrid + Vector3.up * rayHeight;
        var rayEnd = thisWorldGrid + Vector3.down;
        return Physics.Linecast(rayStart, rayEnd);
    }
    private bool IsWall(Vector2Int thisGrid)
    {
        return grid[thisGrid.x, thisGrid.y] == -1;
    }
    private bool IsClose(Vector2Int thisGrid)
    {
        return grid[thisGrid.x, thisGrid.y] != -1 && grid[thisGrid.x, thisGrid.y] != 0;
    }
    private bool AllowGrid(Grid thisGrid)
    {
        return !IsWall(thisGrid.position) && !IsClose(thisGrid.position);
    }
    private float CalcCost(Grid thisGrid)
    {
        var pos = thisGrid.position;
        if (AllowGrid(thisGrid)) { grid[pos.x, pos.y] = CostF(pos); }
        return grid[pos.x, pos.y];
    }
    private float CostF(Vector2Int input)
    {
        float fromStartDistance = Vector2Int.Distance(input, startPoint);
        float toEndDistance = Vector2Int.Distance(input, endPoint);
        return fromStartDistance + toEndDistance;
    }
    private int Cost(Vector2Int input)
    {
        var _xS = input.x - startPoint.x; var _yS = input.y - startPoint.y;
        int fromStartDistance = _xS * _xS + _yS * _yS;
        var _xE = input.x - endPoint.x; var _yE = input.y - endPoint.y;
        int toEndDistance = _xE * _xE + _yE * _yE;
        return fromStartDistance + toEndDistance;
    }
    private Vector3 V2IToV3(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
    private Vector2Int V3ToV2I(Vector3 input) { return new Vector2Int((int)input.x, (int)input.z); }
}
[System.Serializable]
public class Grid
{
    public float cost = 0;
    public Vector2Int position;
    public Vector2Int parent;
}