using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        
    }
    GridEmptyCell swapOb;
    private void OnMouseDrag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); worldPosition.z = 0;
        transform.position = worldPosition;
        swapOb = null;
        float distance = 3;
        foreach(var item in GridManager.Instance.GridItemDict.Values)
        {
            item.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        foreach (var item in GridManager.Instance.emptyGridList)
        {
            item.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            var magnitude = (item.transform.position - transform.position).magnitude;
            if (magnitude < distance)
            {
                swapOb = item;
                    distance = magnitude;
            }
        }

        if (swapOb)
        {

            if (swapOb.index == GetComponent<GridItem>().index)
            {
                swapOb = null;
                return;
            }

            if (!GridManager.Instance.HasItem(swapOb.index))
            {
                swapOb.GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
            else
            {
                GridManager.Instance.GetItem(swapOb.index).GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
        }
    }
    private void OnMouseUp()
    {
        if (swapOb)
        {
            StartCoroutine(mouseUp());
        }
        else
        {
            GridManager.Instance.updatePos(GetComponent<GridItem>());

        }
    }

    IEnumerator mouseUp()
    {


        foreach (var item in GridManager.Instance.GridItemDict.Values)
        {
            item.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        foreach (var item in GridManager.Instance.emptyGridList)
        {
            item.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }

        if (swapOb.index == GetComponent<GridItem>().index)
        {
            yield break;
        }

        FloatingTextManager.Instance.addText("Move!", Vector3.zero,Color.white);

        var targetItem = GridManager.Instance.GetItem(swapOb.index);
        GridManager.Instance.MoveItemToIndex(GetComponent<GridItem>(), swapOb.index);

        GridManager.Instance.MoveItemToIndexEnumerator(GetComponent<GridItem>());
        if (targetItem!=null)
        {
            GridManager.Instance.MoveItemToIndexEnumerator(targetItem.GetComponent<GridItem>());
        }

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(GridManager.Instance.MoveAfter(0, -1));

        swapOb = null;

        StartCoroutine( BattleManager.Instance.Move());
    }
}
