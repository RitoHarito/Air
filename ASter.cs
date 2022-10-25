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
    [Header("Start")]
    public Vector2Int startPoint;
    public Vector2Int endPoint;
    [Header("Step")]
    [Range(1, 100)] public int step;
    [Header("Gizmo")]
    [SerializeField] private float gizmoSize;
    private float[,] grid;//0=normal -1=wall -2=close 
    private Vector2Int nextGrid;
    private List<Vector2Int> NextGridList = new List<Vector2Int>();
    private List<Vector2Int> ResultGridList = new List<Vector2Int>();
    public bool GO;
    private void OnValidate()
    {
        GO = true;
    }
    private void OnDrawGizmos()
    {
        if (GO)
        {
            //GO = false;
            grid = new float[worldSize.x, worldSize.y];
            Gizmos.DrawSphere(ConvertVector2Int(startPoint), gizmoSize);
            Gizmos.DrawSphere(ConvertVector2Int(endPoint), gizmoSize);
            NextGridList = new List<Vector2Int>();
            ResultGridList = new List<Vector2Int>();
            for (int i = 0; i < worldSize.x; i++)
            {
                for (int ii = 0; ii < worldSize.y; ii++)
                {
                    grid[i, ii] = 0;
                }
            }
            for (int i = 0; i < step; i++)
            {
                if (i == 0)
                {
                    Check8Direction(startPoint);//Handles.Label(ConvertVector2Int(left) + Vector3.up, $"<color=#ff0000>{Cost(left)}</color>", gUIStyle);
                }
                else
                {
                    Check8Direction(nextGrid);
                }
                nextGrid = CheckNextThisGrid(NextGridList);
                if (!ResultGridList.Exists(x => x == nextGrid))
                {
                    ResultGridList.Add(nextGrid);
                }
            }
            //for (int i = 0; i < NextGridList.Count; i++)
            //{
            //    var dbg_str = NextGridList[i];
            //    Handles.Label(ConvertVector2Int(NextGridList[i]) + Vector3.up, $"<color=#ff0000>{dbg_str}</color>", gUIStyle);
            //}
            for (int i = 0; i < ResultGridList.Count; i++)
            {
                Gizmos.DrawSphere(ConvertVector2Int(ResultGridList[i]), gizmoSize);
            }
            for (int i = 0; i < worldSize.x; i++)
            {
                for (int ii = 0; ii < worldSize.y; ii++)
                {
                    var thisGrid = new Vector3(origin.x + i, 0, origin.y + ii);
                    //grid[i, ii] = 0;
                    var dbg_str = grid[i, ii];
                    Handles.Label(thisGrid, $"<color=#000000>{dbg_str}</color>", gUIStyle);
                }
            }
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
    private Vector2Int CheckNextThisGrid(List<Vector2Int> grids)
    {
        var result = new Vector2Int();
        var costs_tuple = new List<(float cost, Vector2Int grid)>();
        var costs = new List<float>();
        foreach (var thisGrid in grids)
        {
            if (!ResultGridList.Exists(x => x == thisGrid))
            {
                var cost = CalcCost(thisGrid); costs.Add(cost);
                var cost_tuple = (cost, thisGrid); costs_tuple.Add(cost_tuple);
            }
        }
        var lowestCost = Mathf.Min(costs.ToArray());
        result = costs_tuple.Find(x => x.cost == lowestCost).grid;
        return result;
    }
    private void AddNextGridList(Vector2Int thisGrid)
    {
        if (AllowGrid(thisGrid)) { NextGridList.Add(thisGrid); }
    }
    private float CalcCost(Vector2Int thisGrid)
    {
        if (AllowGrid(thisGrid)) { grid[thisGrid.x, thisGrid.y] = CostF(thisGrid); }
        return grid[thisGrid.x, thisGrid.y];
    }
    private void SetCost(Vector2Int thisGrid, int set)
    {
        grid[thisGrid.x, thisGrid.y] = set;
    }
    private bool IsWall(Vector2Int thisGrid)
    {
        return grid[thisGrid.x, thisGrid.y] == -1;
    }
    private bool IsClose(Vector2Int thisGrid)
    {
        return grid[thisGrid.x, thisGrid.y] != -1 && grid[thisGrid.x, thisGrid.y] != 0;
    }
    private int Cost(Vector2Int input)
    {
        var _xS = input.x - startPoint.x; var _yS = input.y - startPoint.y;
        int fromStartDistance = _xS * _xS + _yS * _yS;
        var _xE = input.x - endPoint.x; var _yE = input.y - endPoint.y;
        int toEndDistance = _xE * _xE + _yE * _yE;
        return fromStartDistance + toEndDistance;
    }
    private float CostF(Vector2Int input)
    {
        float fromStartDistance = Vector2Int.Distance(input, startPoint);
        float toEndDistance = Vector2Int.Distance(input, endPoint);
        return fromStartDistance + toEndDistance;
    }
    private bool AllowGrid(Vector2Int thisGrid)
    {
        return !IsWall(thisGrid) && !IsClose(thisGrid);
    }
    Vector3 ConvertVector2Int(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
}