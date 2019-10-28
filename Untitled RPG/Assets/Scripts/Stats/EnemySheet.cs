using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySheet : MonoBehaviour
{
    public GameObject m_BattlePawn;
    public Sprite m_spriteR;
    public Sprite m_spriteL;
    //used to calculate player exp gain
    [Header("Stats")]
    public int m_level;
    [Space(10)]
    public int m_maxHP;
    public int m_HP;
    [Space(10)]
    public int m_maxSP;
    public int m_SP;
    [Space(10)]
    public int m_maxAP;
    public int m_AP;
    [Space(10)]
    public int m_attack;
    public int m_defence;
    public int m_magic;
    public int m_magicDefence;
    public int m_speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
