using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ASter : MonoBehaviour
{
    [SerializeField] private GUIStyle gUIStyle;
    public Vector2Int worldSize;
    [Range(-100, 1000)] public int worldSizeRange;
    [Header("PointSetting")]
    public Transform startPoint;
    public Transform endPoint;
    private Vector2Int startGrid;
    private Vector2Int endGrid;
    [Header("Step")]
    public int step;
    public int step2;
    [Header("Gizmo")]
    [SerializeField] private float gizmoSize;
    private Grid nextGrid;
    private Vector2Int nextPath;
    private List<Grid> openGridList = new List<Grid>();
    private List<Vector2Int> resultGridList = new List<Vector2Int>();
    private List<Vector2Int> pathGridList = new List<Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
    private Dictionary<Vector2Int, float> costDictionary = new Dictionary<Vector2Int, float>();
    public bool finded;
    public bool complete;
    [SerializeField] private bool viewPath;

    public float[,] localGridCost;
    private void Start()
    {
        Initialize();
    }
    private void Update()
    {
        if (!complete)
        {
            Search();
            PathFind();
        }
    }
    private void OnDrawGizmos()
    {
        //for (int i = 0; i < worldSize.x; i++)
        //{
        //    for (int ii = 0; ii < worldSize.y; ii++)
        //    {
        //        var gridPos = new Vector2Int((-worldSize.x / 2) + i, (-worldSize.y / 2) + ii);
        //        var dbg_str = costDictionary[gridPos].ToString("F1");
        //        Handles.Label(V2IToV3(gridPos) + Vector3.up, $"<color=#000000>{dbg_str}</color>", gUIStyle);
        //       // Gizmos.DrawSphere(V2IToV3(gridPos), gizmoSize);
        //    }
        //} 
        if (viewPath)
        {
            Gizmos.DrawSphere(startPoint.position, gizmoSize);
            Gizmos.DrawSphere(endPoint.position, gizmoSize);
            for (int i = 0; i < pathGridList.Count; i++)
            {
                Gizmos.DrawSphere(V2IToV3(pathGridList[i]), gizmoSize);
            }
            for (int i = 0; i < resultGridList.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(V2IToV3(resultGridList[i]), gizmoSize);
            }
            for (int i = 0; i < openGridList.Count; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(V2IToV3(openGridList[i].position), gizmoSize / 2);
            }
        }

    }

    private void Initialize()
    {
        step = 0;
        step2 = 0;
        finded = false;
        complete = false;
        nextGrid = new Grid();
        nextPath = new Vector2Int();
        openGridList = new List<Grid>();
        resultGridList = new List<Vector2Int>();
        pathGridList = new List<Vector2Int>();
        pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
        startGrid = V3ToV2I(startPoint.position);
        endGrid = V3ToV2I(endPoint.position);
        nextGrid.position = startGrid;
        costDictionary = new Dictionary<Vector2Int, float>();

        localGridCost = new float[worldSize.x, worldSize.y];
        for (int i = 0; i < worldSize.x; i++)
        {
            for (int ii = 0; ii < worldSize.y; ii++)
            {
                var gridPos = new Vector2Int((-worldSize.x / 2) + i, (-worldSize.y / 2) + ii);
                var cost = CheckWall(V2IToV3(gridPos)) ? -1f : 0f;
                if (cost == -1) { costDictionary.Add(gridPos, cost); }
                //  localGridCost[i, ii] = CostF(gridPos);
            }
        }
    }
    private void Search()
    {
        if (finded) { return; }
        finded = nextGrid.position == endGrid;
        Check8Direction(nextGrid.position);
        if (CheckNextGrid(openGridList, out nextGrid))
        {
            if (!resultGridList.Exists(x => x == nextGrid.position))
            {
                resultGridList.Add(nextGrid.position);
            }
            else { nextGrid.parent = nextGrid.position; }
            if (!pathGridDictionary.ContainsKey(nextGrid.position)) { pathGridDictionary.Add(nextGrid.position, nextGrid.parent); }
            openGridList.Remove(nextGrid);
            step++;
        }
    }
    private void Check8Direction(Vector2Int position)
    {
        AddOpenGridList(position, position + Vector2Int.up);
        AddOpenGridList(position, position + Vector2Int.up + Vector2Int.right);
        AddOpenGridList(position, position + Vector2Int.right);
        AddOpenGridList(position, position + Vector2Int.down + Vector2Int.right);
        AddOpenGridList(position, position + Vector2Int.down);
        AddOpenGridList(position, position + Vector2Int.down + Vector2Int.left);
        AddOpenGridList(position, position + Vector2Int.left);
        AddOpenGridList(position, position + Vector2Int.left + Vector2Int.up);
    }
    private void AddOpenGridList(Vector2Int parent, Vector2Int position)
    {
        if (AllowGrid(position)) { var grid = new Grid { parent = parent, position = position, cost = CostF(position) }; openGridList.Add(grid); }
    }
    private bool CheckNextGrid(List<Grid> grids, out Grid result)
    {
        var gridList = new List<Grid>(grids.Count);
        var costList = new List<float>(grids.Count);
        for (int i = 0; i < grids.Count; i++)
        {
            var thisGrid = grids[i];
            if (resultGridList.Exists(x => x == thisGrid.position)) { continue; }
            gridList.Add(thisGrid);
            costList.Add(thisGrid.cost);
        }
        var lowestCost = Mathf.Min(costList.ToArray());
        result = gridList.Find(x => x.cost == lowestCost);
        return result != null;
    }
    private void PathFind()
    {
        if (!finded) { return; }
        if (step2 == 0) { nextPath = endGrid; }
        else
        {
            if (pathGridDictionary.ContainsKey(nextPath))
            {
                nextPath = pathGridDictionary[nextPath];
            }
        }
        pathGridList.Add(nextPath);
        step2++;
        complete = nextPath == startGrid;
    }
    private bool CheckWall(Vector3 thisWorldGrid)
    {
        return Physics.CheckSphere(thisWorldGrid, 0.5f);
    }
    private bool AllowGrid(Vector2Int thisGridPosition)
    {
        return !costDictionary.ContainsKey(thisGridPosition);
    }
    private float CostF(Vector2Int input)
    {
        float fromStartDistance = Vector2Int.Distance(input, startGrid);
        float toEndDistance = Vector2Int.Distance(input, endGrid);
        return fromStartDistance + toEndDistance;
    }
    private float Cost(Vector2Int input)
    {
        var _xS = input.x - startGrid.x; var _yS = input.y - startGrid.y;
        float fromStartDistance = (_xS * _xS) + (_yS * _yS);
        var _xE = input.x - endGrid.x; var _yE = input.y - endGrid.y;
        float toEndDistance = (_xE * _xE) + (_yE * _yE);
        return fromStartDistance + toEndDistance;
    }
    private float Cost2(Vector2Int input)
    {
        var _xS = input - startGrid;
        var _xE = input - endGrid;
        return (_xS + _xE).magnitude;
    }
    private float Cost3(Vector2Int input)
    {
        var _xS = (input - startGrid).magnitude;
        var _xE = (input - endGrid).magnitude;
        return (_xS * _xE);
    }
    private Vector3 V2IToV3(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
    private Vector2Int V3ToV2I(Vector3 input) { return new Vector2Int((int)input.x, (int)input.z); }
}
[System.Serializable]
public class Grid
{
    public float cost;
    public Vector2Int position;
    public Vector2Int parent;
}