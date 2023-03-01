using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ItemType { ore,herb}
public class GridManager : Singleton<GridManager>
{
    float tileSize = 2f;
    public int Rows = 2;
    public int Columns = 3;
    public GameObject EmptyGridPrefab;
    public GameObject itemPrefab;
    public Vector3 startPosition;
    public Transform bk;
    public Transform items;
    public Dictionary<Vector2Int, GameObject> GridArray = new Dictionary<Vector2Int, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();

        AddGrid(0, 0, ItemType.ore);
        AddGrid(1, 0, ItemType.herb);
        AddGrid(2, 1, ItemType.ore);
    }

    int rotatedTime = 0;
    public void Rotate(int time)
    {
        rotatedTime += time;
        rotatedTime %= 4;
        transform.eulerAngles = new Vector3(0, 0, 90 * rotatedTime);
    }
    public void Move(int x, int y)
    {
        var moveVector = new Vector2Int(x, y);
        var tempVector = (transform.rotation * (Vector2)moveVector);
        if (rotatedTime==1 || rotatedTime ==3)
        {
            tempVector = -tempVector;
        }
        moveVector = new Vector2Int((int)tempVector.x, (int)tempVector.y);
        x = moveVector.x;
        y = moveVector.y;
        Debug.Log("move " + moveVector);

        var myList = new List<Vector2Int>();
        foreach(var key in GridArray.Keys)
        {
            myList.Add(key);
        }
        myList.Sort(delegate (Vector2Int a, Vector2Int b) {
            if (x == 1)
            {
                return b.x.CompareTo(a.x);
            } else if (x == -1)
            {
                return a.x.CompareTo(b.x);
            }else if(y == 1)
            {

                return b.y.CompareTo(a.y);
            }
            else
            {

                return a.y.CompareTo(b.y);
            }
        });

        foreach(var key in myList)
        {
            var newKey = key;
            int test=0;
            while (true)
            {
                test++;
                if (test > 10)
                {
                    break;
                }
                newKey += moveVector;
                if (!CanMoveTo(newKey))
                {
                    newKey -= moveVector;
                    break;
                }
            }
            if (newKey != key)
            {
                MoveItemToPos(key, newKey, GetItem(key));
            }
        }
    }

    bool CanMoveTo(Vector2Int pos)
    {
        return !HasItem(pos) && pos.x >= 0 && pos.x < Columns && pos.y >= 0 && pos.y < Rows;
    }

    bool HasItem(Vector2Int pos)
    {
        return GridArray.ContainsKey(pos);
    }

    GameObject GetItem(Vector2Int pos)
    {
        return GridArray[pos];
    }

    Vector3 IndexToPosition(Vector2Int ind)
    {
        return IndexToPosition(ind.x, ind.y);
    }

    Vector3 IndexToPosition(int i,int j)
    {
        return new Vector3(tileSize * i,  tileSize * j);
    }

    public void MoveItemToPos(Vector2Int start, Vector2Int end, GameObject obj)
    {

        GridArray[end] = obj;

        GridArray.Remove(start);

        obj.transform.localPosition = IndexToPosition(end);
    }
    public void GenerateGrid()
    {

        // build our grid map for our level based on the map object values
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                GameObject obj = Instantiate(EmptyGridPrefab);
                obj.name = $"grid-x{i}-y{j}";
                obj.transform.SetParent(bk);
                obj.transform.localPosition = IndexToPosition(i, j);
            }
        }
    }


    public void AddGrid(int i, int j, ItemType type)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("items/"+type.ToString()));
        obj.name = $"grid-x{i}-y{j}";
        obj.transform.SetParent(items);

        obj.transform.localPosition = IndexToPosition(i, j);
        // add to grid once instantiated
        GridArray[new Vector2Int(i,j)] = obj;
    }
}
