using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPotion : Item
{
    public int HealAmount;
    public float animTime = 1.0f;
    public bool play = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(play)
        {
            animTime -= Time.deltaTime;
            if(animTime < 0)
            {
                play = false;
                End();
            }
        }
    }

    void End()
    {
        m_target.r_damageNumbers.SpawnNumber(HealAmount, Color.green);
        m_target.m_HP += HealAmount;
        if (m_target.m_HP > m_target.m_maxHP)
        {
            m_target.m_HP = m_target.m_maxHP;
        }
        m_user.m_battleSystem.m_playingItemAnimation = false;
        m_user.m_playingItemAnimation = false;
        m_user.r_characterSheet.m_items.Remove(this);
        m_user.m_battleSystem.ItemEnd(false);
        Destroy(gameObject);
    }
    public override bool UseItem(BattlePawn user, int targetX, int targetY)
    {
        m_user = user;
        m_user.m_battleSystem.m_playingItemAnimation = true;
        m_user.m_playingItemAnimation = true;
        m_target = m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn;
        Instantiate(m_particle, m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn.transform.position + new Vector3(0, 1.5f, 0), m_particle.transform.rotation);
        play = true;
        return true;
    }

    public override bool ValidTarget(BattlePawn user, int targetX, int targetY)
    {
        m_user = user;
        if (m_user.isInRange(targetX, targetY, 1) && m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_occupied)
        {
            if (m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn.gameObject.tag == "Player")
            {
                return true;
            }
        }
        return false;
    }
}
