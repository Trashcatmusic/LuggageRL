using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ItemType { ore,herb, arrow, poison}
public class GridManager : Singleton<GridManager>
{

    static public float animTime = 0.4f;
    public float tileSize = 2f;
    public int Rows = 2;
    public int Columns = 3;
    public GameObject EmptyGridPrefab;
    public GameObject itemPrefab;
    public Vector3 startPosition;
    public Transform bk;
    public Transform items;
    public Dictionary<Vector2Int, GameObject> GridArray = new Dictionary<Vector2Int, GameObject>();
    public List<GridEmptyCell> emptyGridList = new List<GridEmptyCell>();

    public bool CanDraw(out string failedReason, int drawCount)
    {
        failedReason = "";
        List<Vector2Int> availableEmpty = new List<Vector2Int>();
        foreach (var key in emptyGridList)
        {
            if (!GridArray.ContainsKey(key.index))
            {
                availableEmpty.Add(key.index);
            }
        }

        if (availableEmpty.Count < drawCount)
        {
            failedReason = "Bag is Full!";
            return false;
        }
        return true;
    }
    
    public IEnumerator DrawItem(int drawCount)
    {
        List<Vector2Int> availableEmpty = new List<Vector2Int>();
        foreach(var key in emptyGridList)
        {
            if (!GridArray.ContainsKey(key.index))
            {
                availableEmpty.Add(key.index);
            }
        }
        for(int i = 0;i< drawCount; i++)
        {
            var picked = availableEmpty[Random.Range(0, availableEmpty.Count)];
            AddGrid(picked.x, picked.y, (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length));
            availableEmpty.Remove(picked);
        }

        yield return StartCoroutine(MoveAfter(0, -1));
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();

        //AddGrid(0, 0, ItemType.ore);
        //AddGrid(1, 0, ItemType.herb);
        //AddGrid(1, 1, ItemType.ore);
        //AddGrid(2, 0, ItemType.herb);


        StartCoroutine(MoveAfter(0, -1));
    }
    public int rotatedTime = 0;
    public void Rotate(int time, bool applyGravity)
    {
        rotatedTime += time;
        rotatedTime %= 4;
        //transform.eulerAngles = new Vector3(0, 0, 90 * rotatedTime);
        transform.DORotate(new Vector3(0, 0, 90 * rotatedTime), animTime);
        if (applyGravity)
        {
            StartCoroutine(MoveAfter(0, -1));
        }
    }

    public IEnumerator MoveAfter(int x,int y)
    {
        yield return new WaitForSeconds(animTime);
        MoveInternal(x,y,false);
    }
    List<Transform> sortEmptyCells()
    {
        var myList = new List<Transform>();
        foreach (var key in emptyGridList)
        {
            myList.Add(key.transform);
        }
        myList.Sort(delegate (Transform a, Transform b) {
            if (Mathf.Approximately(a.position.x , b.position.x))
            {
                return a.position.y.CompareTo(b.position.y);
            }
            else 
            {
                return b.position.x.CompareTo(a.position.x);
            }
        });
        return myList;
    }

    public List<Vector2Int> getFrontCellsIndexFromBottomToTop()
    {
        var myList = sortEmptyCells();

        List<Vector2Int> res = new List<Vector2Int>();
        var x = myList[0].position.x;
        for(int i= 0; i<10; i++){
            if (Mathf.Approximately(myList[i].position.x, x))
            {
                res.Add(myList[i].GetComponent<GridEmptyCell>().index);
            }
        }
        return res;
    }

    public List<Transform> getFrontCellsFromBottomToTop()
    {
        var myList = sortEmptyCells();

        List<Transform> res = new List<Transform>();
        var x = myList[0].position.x;
        for (int i = 0; i < 10; i++)
        {
            if (Mathf.Approximately( myList[i].position.x, x))
            {
                res.Add(myList[i]);
            }
        }
        return res;
    }
    List<GameObject> previewCells = new List<GameObject>();
    public GameObject gridPreviewCell;
    public void showAttackPreviewOfEnemy(Enemy enemy)
    {
        clearAttackPreview();
        var cells = getFrontCellsFromBottomToTop();
        if (enemy.attackFromBottom)
        {
            var cell = cells[enemy.attackInd];
            var go= Instantiate(gridPreviewCell,cell.position,cell.rotation);
            previewCells.Add(go);
        }
    }

    public GridItem itemEnemyAttack(Enemy enemy)
    {

        var cells = getFrontCellsIndexFromBottomToTop();
        if (enemy.attackFromBottom)
        {
            var cell = cells[enemy.attackInd];
            var item = GridArray.ContainsKey(cell) ? GridArray[cell].GetComponent<GridItem>() : null;
            return item;
        }
        return null;
    }

    public void clearAttackPreview()
    {
        foreach(var cell in previewCells)
        {
            Destroy(cell);
        }
        previewCells.Clear();
    }

    void getFrontCellsBottomToUp()
    {

    }

    public void MoveInternal(int x, int y, bool isAttacking )
    {
        var moveVector = new Vector2Int(x, y);
        var tempVector = (transform.rotation * (Vector2)moveVector);
        if (rotatedTime == 1 || rotatedTime == 3)
        {
            tempVector = -tempVector;
        }
        moveVector = new Vector2Int((int)tempVector.x, (int)tempVector.y);
        x = moveVector.x;
        y = moveVector.y;
        Debug.Log("move " + moveVector);

        var myList = new List<Vector2Int>();
        foreach (var key in GridArray.Keys)
        {
            myList.Add(key);
        }
        myList.Sort(delegate (Vector2Int a, Vector2Int b) {
            if (x == 1)
            {
                return b.x.CompareTo(a.x);
            }
            else if (x == -1)
            {
                return a.x.CompareTo(b.x);
            }
            else if (y == 1)
            {

                return b.y.CompareTo(a.y);
            }
            else
            {

                return a.y.CompareTo(b.y);
            }
        });

        foreach (var key in myList)
        {
            var newKey = key;
            int test = 0;
            var obj = GetItem(key);
            var gridItem = obj.GetComponent<GridItem>();
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
                    if (IsHittingBoarder(newKey))
                    {
                        if (isAttacking)
                        {
                            gridItem.hitBorder(true, newKey - moveVector - key, items.transform.TransformPoint(IndexToPosition(newKey)) );
                        }
                    }
                    if (HasItem(newKey))
                    {

                        if (isAttacking)
                        {
                            GetItem(newKey).GetComponent<GridItem>().BeHit(gridItem);
                        }
                    }

                    newKey -= moveVector;
                    break;
                }
            }
            if (newKey != key)
            {
                MoveItemToPos(key, newKey, obj,animTime);

                obj.GetComponent<GridItem>().calculateHit();
            }
            else
            {
                obj.GetComponent<GridItem>().calculateHit();
            }
        }
    }

    public void Move(int x, int y)
    {
        MoveInternal(x, y,true);

        StartCoroutine(MoveAfter(0, -1));
    }

    bool IsHittingBoarder(Vector2Int pos)
    {
        return !(pos.x >= 0 && pos.x < Columns && pos.y >= 0 && pos.y < Rows);
    }
    bool CanMoveTo(Vector2Int pos)
    {
        return !HasItem(pos) && pos.x >= 0 && pos.x < Columns && pos.y >= 0 && pos.y < Rows;
    }

    public bool HasItem(Vector2Int pos)
    {
        return GridArray.ContainsKey(pos);
    }

    public GameObject GetItem(Vector2Int pos)
    {
        return GridArray[pos];
    }

    public Vector3 IndexToPosition(Vector2Int ind)
    {
        return IndexToPosition(ind.x, ind.y);
    }

    Vector3 IndexToPosition(int i,int j)
    {
        return new Vector3(tileSize * i,  tileSize * j);
    }
    public void updatePos(GridItem item)
    {
        item.StartCoroutine(item.move(IndexToPosition(item.index), animTime));
    }
    public void MoveItemToPos(Vector2Int start, Vector2Int end, GameObject obj, float animTime)
    {

        GridArray[end] = obj;

        GridArray.Remove(start);
        obj.GetComponent<GridItem>().index = end;
        obj.GetComponent<MonoBehaviour>().StartCoroutine(obj.GetComponent<GridItem>().move(IndexToPosition(end), animTime));

        // obj.transform.DOLocalMove(IndexToPosition(end), 0.3f);

        //obj.transform.localPosition = IndexToPosition(end);
    }

    public void swapItem(GridItem item1, GridItem item2, float animTime)
    {
        var ind1 = item1.index;
        var ind2 = item2.index;
        GridArray[ind1] = item2.gameObject;
        GridArray[ind2] = item1.gameObject;



        item1.index = ind2;

        item2.index = ind1;

        item1.GetComponent<MonoBehaviour>().StartCoroutine(item1.GetComponent<GridItem>().move(IndexToPosition(item1.index), animTime));
        item2.GetComponent<MonoBehaviour>().StartCoroutine(item2.GetComponent<GridItem>().move(IndexToPosition(item2.index), animTime));

    }

    public void FinishItemMoving(GameObject obj)
    {

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
                obj.GetComponent<GridEmptyCell>().index = new Vector2Int(i, j);
                emptyGridList.Add(obj.GetComponent<GridEmptyCell>());
            }
        }
    }


    public void AddGrid(int i, int j, ItemType type)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("items/"+type.ToString()));
        obj.GetComponent<GridItem>().index = new Vector2Int(i, j);
        obj.name = $"grid-x{i}-y{j}";
        obj.transform.SetParent(items);

        obj.transform.localPosition = IndexToPosition(i, j);
        // add to grid once instantiated
        GridArray[new Vector2Int(i,j)] = obj;
    }
    public void RemoveGrid(int i, int j)
    {
        GridArray.Remove(new Vector2Int(i, j));
    }
    public void RemoveGrid(Vector2Int ind)
    {
        GridArray.Remove(ind);
    }
}
