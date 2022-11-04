using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
public class ASter : MonoBehaviour
{
    [SerializeField] private GUIStyle gUIStyle;
    [Range(1, 500)] public int worldSizeRange;
    [Header("PointSetting")]
    public Transform startPoint;
    public Transform endPoint;
    private Vector2Int startGrid;
    private Vector2Int endGrid;
    [Header("Step")]
    public int searchStep;
    public int pathFindStep;
    public int maxStep = 10000;
    [Header("Gizmo")]
    [SerializeField] private float gizmoSize;
    [SerializeField] private bool viewGizmo;
    [SerializeField] private bool viewLinePath;
    [SerializeField] private bool viewHasCode;
    private Grid nextGrid;
    private Vector2Int nextPath;
    private List<Grid> openGridList = new List<Grid>();
    private List<Vector2Int> resultGridList = new List<Vector2Int>();
    private List<Vector2Int> pathGridList = new List<Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> pathGridDictionary = new Dictionary<Vector2Int, Vector2Int>();
    private Dictionary<Vector2Int, float> costDictionary = new Dictionary<Vector2Int, float>();
    [SerializeField] private bool finded;
    [SerializeField] private bool complete;
    private bool[,] localGrid;
    private bool initialized;
    private Dictionary<int, Vector2Int> localGridDictionary = new Dictionary<int, Vector2Int>();
    private SimpleControls inputActions;
    private LineRenderer lineRenderer;
    private void Awake()
    {
        inputActions = new SimpleControls();
        inputActions.Enable();
        inputActions.Default.Fire.performed += ctx =>
        {
            StartASter();
        };
    }
    public void StartASter()
    {
        Initialize();
        var context = SynchronizationContext.Current;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (initialized)
            {
                for (int i = 0; i < maxStep; i++)
                {
                    if (complete) { break; }
                    else
                    {
                        if (!finded) { Search(); }
                        else { PathFind(); }
                    }
                }
            }
            context.Post(__ =>
            {
                OnComplete();
                RenderPath();
            }, null);
        });
    }
    private void Update()
    {
        var leftStick = inputActions.Default.LeftStick.ReadValue<Vector2>();
        startPoint.position += new Vector3(leftStick.x, 0, leftStick.y) * 0.1f;
        FPSCounter();
    }
    private void OnDrawGizmos()
    {
        if (viewHasCode)
        {
            for (int i = 0; i < worldSizeRange; i++)
            {
                for (int ii = 0; ii < worldSizeRange; ii++)
                {
                    var worldSizeOrigin = -worldSizeRange / 2;
                    var gridPos = new Vector2Int(worldSizeOrigin + i, worldSizeOrigin + ii);
                    var dbg_str = GetHashCode(gridPos).ToString();
                    Handles.Label(V2IToV3(gridPos) + Vector3.up, dbg_str, gUIStyle);
                }
            }
        }
        if (viewGizmo)
        {
            var w = worldSizeRange / 2;
            var h = worldSizeRange / 2;
            var p0 = startPoint.position + new Vector3(w, 0, h);
            var p1 = startPoint.position + new Vector3(w, 0, -h);
            var p2 = startPoint.position + new Vector3(-w, 0, -h);
            var p3 = startPoint.position + new Vector3(-w, 0, h);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
            Gizmos.DrawSphere(startPoint.position, gizmoSize);
            Gizmos.DrawSphere(endPoint.position, gizmoSize);
            for (int i = 0; i < pathGridList.Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(V2IToV3(pathGridList[i]), gizmoSize * 1.25f);
                if (i != 0) { Gizmos.DrawLine(V2IToV3(pathGridList[i]), V2IToV3(pathGridList[i - 1])); }
            }
            for (int i = 0; i < resultGridList.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(V2IToV3(resultGridList[i]), gizmoSize);
            }
        }
    }
    private void Initialize()
    {
        searchStep = 0;
        pathFindStep = 0;
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
        localGrid = new bool[worldSizeRange, worldSizeRange];
        localGridDictionary = new Dictionary<int, Vector2Int>();
        for (int i = 0; i < worldSizeRange; i++)
        {
            for (int ii = 0; ii < worldSizeRange; ii++)
            {
                var worldSizeOrigin = (-worldSizeRange / 2);
                var gridPos = new Vector2Int(worldSizeOrigin + i, worldSizeOrigin + ii);
                var cost = CheckWall(V2IToV3(gridPos)) ? -1f : 0f;
                if (cost == -1) { costDictionary.Add(gridPos, cost); }
                var localPosition = new Vector2Int(i, ii);
                var hashCode = GetHashCode(gridPos);
                localGridDictionary.Add(hashCode, localPosition);
                localGrid[i, ii] = false;
            }
        }
        initialized = true;
    }
    private void Search()
    {
        if (complete) { return; }
        if (finded) { return; }
        finded = nextGrid.position == endGrid;
        Check8Direction(nextGrid.position);
        if (CheckNextGrid(openGridList, out nextGrid))
        {
            var hashCode = GetHashCode(nextGrid.position);
            var localPosition = localGridDictionary[hashCode];
            if (localGrid[localPosition.x, localPosition.y] == false)
            {
                resultGridList.Add(nextGrid.position);
                localGrid[localPosition.x, localPosition.y] = true;
            }
            else { nextGrid.parent = nextGrid.position; localGrid[localPosition.x, localPosition.y] = true; }
            if (!pathGridDictionary.ContainsKey(nextGrid.position)) { pathGridDictionary.Add(nextGrid.position, nextGrid.parent); }
            openGridList.Remove(nextGrid);
            searchStep++;
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
            var hashCode = GetHashCode(thisGrid.position);
            var localPosition = localGridDictionary[hashCode];
            if (localGrid[localPosition.x, localPosition.y] == true) { continue; }
            gridList.Add(thisGrid);
            costList.Add(thisGrid.cost);
        }
        var lowestCost = Mathf.Min(costList.ToArray());
        result = gridList.Find(x => x.cost == lowestCost);
        return result != null;
    }
    private void PathFind()
    {
        if (pathFindStep == 0) { nextPath = endGrid; }
        else
        {
            if (pathGridDictionary.ContainsKey(nextPath))
            {
                nextPath = pathGridDictionary[nextPath];
            }
        }
        pathGridList.Add(nextPath);
        pathFindStep++;
        complete = nextPath == startGrid;
    }
    private void OnComplete()
    {
        openGridList.Clear();
        resultGridList.Clear();
        costDictionary.Clear();
        pathGridDictionary.Clear();
        localGrid = new bool[0, 0];
        localGridDictionary.Clear();
        pathGridList.Reverse();
        initialized = false;
    }
    private void RenderPath()
    {
        if (TryGetComponent(out lineRenderer))
        {
            var pathGridArray = new Vector3[pathGridList.Count];
            for (int i = 0; i < pathGridList.Count; i++)
            {
                pathGridArray[i] = V2IToV3(pathGridList[i]);

            }
            lineRenderer.positionCount = pathGridArray.Length;
            lineRenderer.SetPositions(pathGridArray);
        }
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

    public int GetHashCode(Vector2Int input)
    {
        return input.x + input.y * worldSizeRange;
    }
    private Vector3 V2IToV3(Vector2Int input) { return new Vector3(input.x, 0, input.y); }
    private Vector2Int V3ToV2I(Vector3 input) { return new Vector2Int((int)input.x, (int)input.z); }
    int fps_frameCount;
    float fps_prevTime;
    string fps_CounterStr;
    private void FPSCounter()
    {
        ++fps_frameCount;
        float time = Time.realtimeSinceStartup - fps_prevTime;
        if (time >= 0.5f)
        {
            var fpsStr = (fps_frameCount / time).ToString("F2");
            fps_CounterStr = string.Format("{0}fps", fpsStr);
            fps_frameCount = 0;
            fps_prevTime = Time.realtimeSinceStartup;
        }
    }
    private void OnGUI() { GUI.Label(new Rect(0, 0, 500, 500), fps_CounterStr, gUIStyle); }
}
[System.Serializable]
public class Grid
{
    public float cost;
    public Vector2Int position;
    public Vector2Int parent;
}