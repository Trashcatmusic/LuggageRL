using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : Singleton<BattleManager>
{
    public Text LuggageAttackText;
    int selected;
    int moveMax = 4;
    int moveLeft;
    bool isBattleFinished = false;
    string[] attackString = new string[] {"Push","Upside Down","Throw And Back" };
    public GameObject[] enemies;
    public Transform[] enemyPositions;
    public Player player;
    int drawCount = 2;
    int startDrawCount = 4;
    public Transform ButtonCanvas;

    void hideButtonCanvas()
    {
        foreach (var button in ButtonCanvas.GetComponentsInChildren<Button>())
        {
            button.gameObject.SetActive(false);
        }
    }
    void showButtonCanvas()
    {
        if (isBattleFinished)
        {
            return;
        }
        foreach (var button in ButtonCanvas.GetComponentsInChildren<Button>(true))
        {
            button.gameObject.SetActive(true);
        }
    }
    public void SkipMove()
    {
        moveLeft = 0;

        StartCoroutine(EndOfTurn());
    }
    public void DrawItem(bool noCost = false)
    {
        StartCoroutine(DrawItemEnumerator(noCost));
    }
    public IEnumerator DrawItemEnumerator(bool noCost =false)
    {
        hideButtonCanvas();
        string failedReason;
        int count = noCost ? startDrawCount : drawCount;
        if (GridManager.Instance.CanDraw(out failedReason, count))
        {
            yield return StartCoroutine(GridManager.Instance.DrawItem(count));
            if (!noCost)
            {
                yield return useMove(1);
            }
        }
        else
        {

            FloatingTextManager.Instance.addText(failedReason, Vector3.zero, Color.red);
        }
        showButtonCanvas();
    }

    public IEnumerator FinishCurrentBattle()
    {
        if (!isBattleFinished)
        {
            isBattleFinished = true;
            FloatingTextManager.Instance.addText("Win Battle!", Vector3.zero, Color.red);
            yield return new WaitForSeconds(GridManager.animTime);
            //reward
            RemoveText();
            StartCoroutine(searchNextBattle());
        }


    }
    IEnumerator searchNextBattle()
    {
        hideButtonCanvas();
        yield return new WaitForSeconds(1);
        StartBattle();

    }

    void StartBattle()
    {
        //clear old enemies, this is bad, hacky solution
        foreach(var enemy in Transform.FindObjectsOfType<Enemy>(true))
        {
            //if (!enemy.gameObject.activeInHierarchy)
            {
                Destroy(enemy.gameObject);
            }
        }


        showButtonCanvas();
        isBattleFinished = false;
        AddEnemies();
        DrawItem(true);
        SelectAttack();
        EnemyManager.Instance.SelectEenmiesAttack();

    }
    void AddEnemies()
    {
        var pickedEnemy = enemies[Random.Range(0, enemies.Length)];
        var go = Instantiate(pickedEnemy);
        go.transform.position =  enemyPositions[0].position;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine( searchNextBattle());
    }

    void SelectAttack()
    {
        selected = Random.Range(0, 3);
        moveLeft = moveMax;
        UpdateText();
    }
    IEnumerator useMove(int amount)
    {
        moveLeft -= amount;
        UpdateText();
        if (moveLeft == 0)
        {
            yield return StartCoroutine(EndOfTurn());
            //yield return StartCoroutine(PlayerAttackMove());
        }

    }
    public IEnumerator Move()
    {
        hideButtonCanvas();
        yield return useMove(1);
        showButtonCanvas();
    }

    public void PlayerAttackManually()
    {
        if (moveLeft < 2)
        {
            return;
        }
        StartCoroutine(PlayerAttackMove());
    }

    public IEnumerator PlayerAttackMove()
    {
        hideButtonCanvas();
        switch (selected)
        {
            case 0:
                yield return StartCoroutine(Luggage.Instance.PushForwardAttack());
                break;
            case 1:
                yield return StartCoroutine(Luggage.Instance.UpsideDownAndDrop());
                break;
            case 2:
                yield return StartCoroutine(Luggage.Instance.ThrowOutAndHitBack());
                break;
        }
        yield return useMove(2);

        showButtonCanvas();
    }

    public IEnumerator EndOfTurn()
    {
        hideButtonCanvas();
        yield return StartCoroutine(EnemyManager.Instance.EnemiesAttack());
        SelectAttack();
        EnemyManager.Instance.SelectEenmiesAttack();
        showButtonCanvas();

    }

    void UpdateText()
    {
        if (isBattleFinished)
        {
            return;
        }
        LuggageAttackText.text = $"Next Attack: {attackString[selected]} (in {moveLeft} moves)";
    }

    void RemoveText()
    {
        LuggageAttackText.text = "Search for next Enemy";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
