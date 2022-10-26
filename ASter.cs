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
    private Vector2Int nextGrid;
    private Vector2Int pathNextGrid;
    private List<Vector2Int> nextGridList = new List<Vector2Int>();
    private List<Vector2Int> resultGridList = new List<Vector2Int>();
    private List<Vector2Int> pathGridList = new List<Vector2Int>();
    public bool GO;
    [SerializeField] private float rayHeight;
    public bool finded;
    private void OnDrawGizmos()
    {
        if (GO)
        {
            Gizmos.DrawSphere(ConvertVector2Int(startPoint), gizmoSize);
            Gizmos.DrawSphere(ConvertVector2Int(endPoint), gizmoSize);
            Initialize();
            for (int i = 0; i < step; i++)
            {
                Search(i);
                if (finded) { break; }
            }
            for (int i = 0; i < step2; i++)
            {
                PathFind(i);
            }
            for (int i = 0; i < resultGridList.Count; i++)
            {
                var dbg_str = grid[resultGridList[i].x, resultGridList[i].y].ToString();
                Handles.Label(ConvertVector2Int(resultGridList[i]) + Vector3.up, $"<color=#000000>{dbg_str}</color>", gUIStyle);
                //Gizmos.DrawSphere(ConvertVector2Int(resultGridList[i]), gizmoSize);
            }
            for (int i = 0; i < pathGridList.Count; i++)
            {
                Gizmos.DrawSphere(ConvertVector2Int(pathGridList[i]), gizmoSize);
            }
            for (int i = 0; i < worldSize.x; i++)
            {
                for (int ii = 0; ii < worldSize.y; ii++)
                {
                    var thisGrid = new Vector3(origin.x + i, 0, origin.y + ii);
                    var dbg_str = grid[i, ii];
                    //Handles.Label(thisGrid + Vector3.up, $"<color=#000000>{dbg_str}</color>", gUIStyle);
                }
            }
        }
    }
    void PathFind(int i)
    {
        if (!finded) { return; }
        nextGridList = new List<Vector2Int>();
        if (i == 0) { Path_Check8Direction(endPoint); }
        else { Path_Check8Direction(pathNextGrid); }
        pathNextGrid = Path_CheckNextThisGrid(nextGridList);
        if (pathNextGrid != startPoint)
        {
            pathGridList.Add(pathNextGrid);
        }
    }
    private void Path_Check8Direction(Vector2Int thisGrid)
    {
        var up = thisGrid + Vector2Int.up;
        var upright = thisGrid + Vector2Int.up + Vector2Int.right;
        var right = thisGrid + Vector2Int.right;
        var rightdown = thisGrid + Vector2Int.down + Vector2Int.right;
        var down = thisGrid + Vector2Int.down;
        var downleft = thisGrid + Vector2Int.down + Vector2Int.left;
        var left = thisGrid + Vector2Int.left;
        var leftup = thisGrid + Vector2Int.left + Vector2Int.up;
        Path_AddNextGridList(up);
        Path_AddNextGridList(upright);
        Path_AddNextGridList(right);
        Path_AddNextGridList(rightdown);
        Path_AddNextGridList(down);
        Path_AddNextGridList(downleft);
        Path_AddNextGridList(left);
        Path_AddNextGridList(leftup);
    }
    private void Path_AddNextGridList(Vector2Int thisGrid)
    {
        if (resultGridList.Exists(x => x == thisGrid)) { nextGridList.Add(thisGrid); }
    }
    private Vector2Int Path_CheckNextThisGrid(List<Vector2Int> grids)
    {
        var result = new Vector2Int();
        var costs_tuple = new List<(float cost, Vector2Int grid)>();
        var costs = new List<float>();
        foreach (var thisGrid in grids)
        {
            var cost = grid[thisGrid.x, thisGrid.y]; costs.Add(cost);
            var cost_tuple = (cost, thisGrid); costs_tuple.Add(cost_tuple);
        }
        var lowestCost = Mathf.Min(costs.ToArray());
        result = costs_tuple.Find(x => x.cost == lowestCost).grid;
        return result;
    }
    private void Initialize()
    {
        grid = new float[worldSize.x, worldSize.y];
        nextGridList = new List<Vector2Int>();
        resultGridList = new List<Vector2Int>();
        pathGridList = new List<Vector2Int>();
        for (int i = 0; i < worldSize.x; i++)
        {
            for (int ii = 0; ii < worldSize.y; ii++)
            {
                var thisGrid = new Vector3(origin.x + i, 0, origin.y + ii);
                var thisWorldGrid = ConvertVector3(thisGrid);
                if (CheckWall(thisGrid)) { SetGrid(thisWorldGrid, -1); }
                else { SetGrid(thisWorldGrid, 0); }
            }
        }
    }
    private void Search(int i)
    {
        finded = nextGrid == endPoint;
        if (i == 0) { Check8Direction(startPoint); }
        else { Check8Direction(nextGrid); }
        nextGrid = CheckNextThisGrid(nextGridList);
        if (!resultGridList.Exists(x => x == nextGrid))
        {
            resultGridList.Add(nextGrid);
        }
    }
    private void Check8Direction(Vector2Int thisGrid)
    {
        var up = thisGrid + Vector2Int.up;
        var upright = thisGrid + Vector2Int.up + Vector2Int.right;
        var right = thisGrid + Vector2Int.right;
        var rightdown = thisGrid + Vector2Int.down + Vector2Int.right;
        var down = thisGrid + Vector2Int.down;
        var downleft = thisGrid + Vector2Int.down + Vector2Int.left;
        var left = thisGrid + Vector2Int.left;
        var leftup = thisGrid + Vector2Int.left + Vector2Int.up;
        AddNextGridList(up);
        AddNextGridList(upright);
        AddNextGridList(right);
        AddNextGridList(rightdown);
        AddNextGridList(down);
        AddNextGridList(downleft);
        AddNextGridList(left);
        AddNextGridList(leftup);
    }
    private void AddNextGridList(Vector2Int thisGrid)
    {
        if (AllowGrid(thisGrid)) { nextGridList.Add(thisGrid); }
    }
    private Vector2Int CheckNextThisGrid(List<Vector2Int> grids)
    {
        var result = new Vector2Int();
        var costs_tuple = new List<(float cost, Vector2Int grid)>();
        var costs = new List<float>();
        foreach (var thisGrid in grids)
        {
            if (!resultGridList.Exists(x => x == thisGrid))
            {
                var cost = CalcCost(thisGrid); costs.Add(cost);
                var cost_tuple = (cost, thisGrid); costs_tuple.Add(cost_tuple);
            }
        }
        var lowestCost = Mathf.Min(costs.ToArray());
        result = costs_tuple.Find(x => x.cost == lowestCost).grid;
        return result;
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
    private bool AllowGrid(Vector2Int thisGrid)
    {
        return !IsWall(thisGrid) && !IsClose(thisGrid);
    }
    private float CalcCost(Vector2Int thisGrid)
    {
        if (AllowGrid(thisGrid)) { grid[thisGrid.x, thisGrid.y] = CostF(thisGrid); }
        return grid[thisGrid.x, thisGrid.y];
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
    private Vector3 ConvertVector2Int(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
    private Vector2Int ConvertVector3(Vector3 input) { return new Vector2Int((int)input.x, (int)input.z); }
}