using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonBolt : Skill
{
    const float ARROW_SPEED = 5.0f;

    bool m_animationStarted;
    public Sprite r_CharacterSpriteLeft, r_CharacterSpriteRight;
    public GameObject r_BoltPrefab, m_Bolt;
    Direction m_fireDirection;
    public GameObject m_ailmentPrefab;

    

    void Start()
    {
        m_animationStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_animationStarted)
            return;
        //move arrow to the target
        m_Bolt.transform.SetPositionAndRotation(Vector3.MoveTowards(m_Bolt.transform.position, m_target.transform.position + new Vector3(0,0.5f,0), ARROW_SPEED * Time.deltaTime), m_Bolt.transform.rotation);
        m_Bolt.transform.LookAt(m_target.transform.position + new Vector3(0, 0.5f, 0));

        if(Vector3.Distance(m_Bolt.transform.position, m_target.transform.position + new Vector3(0, 0.5f, 0)) <= 0.0f)
        {
            m_animationStarted = false;
            EndSkill();
        }
    }

    public override bool UseSkill(BattlePawn user, int targetX, int targetY)
    {
        m_user = user;
        m_target = user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn;
        m_user.m_battleSystem.m_playingSkillAnimation = true;
        m_user.m_playingSkillAnimation = true;
        if (m_user.m_facingLeft)
        {
            m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteL;
        }
        else
        {
            m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteR;
        }

        m_animationStarted = false;
        m_user.m_SP -= m_spCost;
        m_user.m_AP -= m_apCost;
        if (m_user.m_x < targetX)
            m_fireDirection = Direction.DIRECTION_RIGHT;
        else if (m_user.m_x > targetX)
            m_fireDirection = Direction.DIRECTION_LEFT;

        //determin the direction and face the arrow and user that way
        switch (m_fireDirection)
        {
            case Direction.DIRECTION_NONE:
                break;
            case Direction.DIRECTION_UP:
                break;
            case Direction.DIRECTION_DOWN:
                break;
            case Direction.DIRECTION_RIGHT:
                m_user.GetComponent<SpriteRenderer>().sprite = r_CharacterSpriteRight;
                m_Bolt = Instantiate(r_BoltPrefab, m_user.gameObject.transform.position + new Vector3(0, 0, 0.7f), Quaternion.Euler(0, 90, 0));
                break;
            case Direction.DIRECTION_LEFT:
                m_user.GetComponent<SpriteRenderer>().sprite = r_CharacterSpriteLeft;
                m_Bolt = Instantiate(r_BoltPrefab, m_user.gameObject.transform.position + new Vector3(0, 0, 0.7f), Quaternion.identity);
                break;
            default:
                break;
        }
        m_animationStarted = true;

        return true;
    }

    void EndSkill()
    {
        m_target.TakeDamage(m_user, DamageType.DAMAGETYPE_SKILL, 1);
        //randomize poison effect
        if (Random.Range(0.0f, 1.0f) >= 0.5)
        {
            if (m_target.m_ailment == null)
            {
                //poison the target
                m_target.m_ailment = Instantiate(m_ailmentPrefab, m_target.gameObject.transform).GetComponent<Poisoned>();
                m_target.m_ailment.Inflict(m_target, m_user);
            }
        }
        switch (m_fireDirection)
        {
            case Direction.DIRECTION_NONE:
                break;
            case Direction.DIRECTION_UP:
                break;
            case Direction.DIRECTION_DOWN:
                break;
            case Direction.DIRECTION_RIGHT:
                m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteR;
                break;
            case Direction.DIRECTION_LEFT:
                m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteL;
                break;
            default:
                break;
        }
        Destroy(m_Bolt);

        m_user.m_battleSystem.m_playingSkillAnimation = false;
        m_user.m_playingSkillAnimation = false;

        m_user.m_battleSystem.SkillEnd(false);
    }

    public override bool ValidTarget(BattlePawn m_user, int targetX, int targetY)
    {
        if (m_user.isInRange(targetX, targetY, m_rangeMin,m_rangeMax) && m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_occupied)
        {
            if (m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn.gameObject.tag == "Enemy")
            {
                return true;
            }
        }

        return false;
    }
}

