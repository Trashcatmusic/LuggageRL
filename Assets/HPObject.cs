using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPObject : MonoBehaviour
{
    public int maxHP = 10;
    protected int hp = 0;
    public bool isDead = false;
    public HPBar hpbar;
    public void ApplyDamage(int damage)
    {
        FloatingTextManager.Instance.addText(damage.ToString(), transform.position + new Vector3(0, 1, 0), Color.red);
        hp -= damage;

        hpbar.updateHPBar(hp, maxHP);
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Heal(int damage)
    {

        hp += damage;
        hp = Mathf.Min(hp, maxHP);
        hpbar.updateHPBar(hp, maxHP);
    }
    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            DieInteral();
        }
    }

    protected virtual void DieInteral()
    {

    }
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        hp = maxHP;
        if (hpbar == null)
        {
            hpbar = GetComponentInChildren<HPBar>();
        }
        
        hpbar.updateHPBar(hp, maxHP);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
