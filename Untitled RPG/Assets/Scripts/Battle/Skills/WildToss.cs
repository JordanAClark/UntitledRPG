using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildToss : Skill
{
    //how many spaces back the target will be thrown
    const int THROWRANGE = 3;
    //how high the sprite will be visualy while moving
    const float THROWHIGHT = 0.5f;
    //how long it takes to move through a space
    const float THROW_TIME_PER_SPACE = 0.5f;
    //how long it takes to throw target up
    const float UP_TIME = 0.2f;
    //wether the animation has started
    bool m_animationStarted;
    //timer for throw animation
    [SerializeField]
    float m_throwTimer;
    float m_airTimeTimer;
    Direction m_throwDirection;
    //the square the target will land on
    BattleSpace m_landingSquare;
    [SerializeField]
    int m_landingDist;
    //the character that was hit by the target that was thrown
    [SerializeField]
    BattlePawn m_hitPawn;
    //whether the thrown pawn collided with another
    [SerializeField]
    bool m_hasCollided;
    //timer for moving in to attack
    float attackTimer;
    //flag for user touching and throwing target
    bool m_attackHasHit;
    //where the user is standing
    Vector3 oldPos;
    Vector3 targetStart;

    void Start()
    {
        m_animationStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_animationStarted)
            return;
        //moving towards enemy and picking them up
        if (attackTimer > 0 && m_attackHasHit == false)
        {
            attackTimer -= Time.deltaTime * 4;
            //movement towards enemy
            m_user.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_target.transform.position, oldPos, attackTimer), gameObject.transform.rotation);
        }
        else if (attackTimer < 0 && m_attackHasHit == false)
        {
            m_attackHasHit = true;
            
        } // the enemy moving through the air towards empty space
        else if (m_attackHasHit && m_throwTimer > 0 && !m_hasCollided)
        {
            m_throwTimer -= Time.deltaTime * 2;
            m_target.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_landingSquare.m_cube.transform.position + new Vector3(0, 0.5f, 0), targetStart, m_throwTimer), gameObject.transform.rotation);

            if(attackTimer < 1)
            {
                attackTimer += Time.deltaTime * 4;
                m_user.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_target.transform.position, oldPos, attackTimer), gameObject.transform.rotation);
            }
            else
            {
                attackTimer = 1;
                m_user.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_target.transform.position, oldPos, attackTimer), gameObject.transform.rotation);
            }
        }
        else if (m_attackHasHit && m_throwTimer > 0 && m_hasCollided)
        {
            m_throwTimer -= Time.deltaTime * 2;
            m_target.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_hitPawn.transform.position, targetStart, m_throwTimer), gameObject.transform.rotation);
            if (attackTimer < 1)
            {
                
                attackTimer += Time.deltaTime * 4;
                m_user.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_target.transform.position, oldPos, attackTimer), gameObject.transform.rotation);
            }
            else
            {
                attackTimer = 1;
                m_user.gameObject.transform.SetPositionAndRotation(Vector3.Lerp(m_target.transform.position, oldPos, attackTimer), gameObject.transform.rotation);
            }
        }
        else if(m_throwTimer < 0)
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
        m_user.m_SP -= m_spCost;
        m_user.m_AP -= m_apCost;
        attackTimer = 1.0f;
        if (m_user.m_x < targetX)
            m_throwDirection = Direction.DIRECTION_RIGHT;
        else if (m_user.m_x > targetX)
            m_throwDirection = Direction.DIRECTION_LEFT;
        else if (m_user.m_y < targetY)
            m_throwDirection = Direction.DIRECTION_UP;
        else if (m_user.m_y > targetY)
            m_throwDirection = Direction.DIRECTION_DOWN;

        oldPos = user.transform.position;
        calcLandingSquare();
        m_throwTimer = 1;
        m_airTimeTimer = UP_TIME + (THROW_TIME_PER_SPACE * m_range) + UP_TIME / 2;
        targetStart = m_target.gameObject.transform.position + new Vector3(0, 1, 0);
        m_animationStarted = true;
        //EndSkill();
        return true;
    }

    void EndSkill()
    {

        m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_occupied = false;
        m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_pawn = null;
        m_target.m_x = m_landingSquare.x;
        m_target.m_y = m_landingSquare.y;

        m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_occupied = true;
        m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_pawn = m_target;
        m_target.transform.position = new Vector3(m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_cube.transform.position.x,
                                    m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_cube.transform.position.y + 0.5f,
                                    m_target.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y].m_cube.transform.position.z + 0.1f);
        m_user.transform.SetPositionAndRotation(new Vector3(m_user.m_battleSystem.m_battleSpaces[m_user.m_x, m_user.m_y].m_cube.transform.position.x,
                                    m_user.m_battleSystem.m_battleSpaces[m_user.m_x, m_user.m_y].m_cube.transform.position.y + 0.5f,
                                    m_user.m_battleSystem.m_battleSpaces[m_user.m_x, m_user.m_y].m_cube.transform.position.z + 0.1f), Quaternion.identity);
        //throw target Damage
        if (m_hasCollided)
        {
            m_target.TakeDamage(m_user, DamageType.DAMAGETYPE_PHYSICAL, 1.75f);
            m_hitPawn.TakeDamage(m_user, DamageType.DAMAGETYPE_PHYSICAL, 0.5f);
        }
        else
        {
            m_target.TakeDamage(m_user, DamageType.DAMAGETYPE_PHYSICAL, 1.25f);
        }

        m_user.m_battleSystem.m_playingSkillAnimation = false;
        m_user.m_playingSkillAnimation = false;
        
        m_user.m_battleSystem.SkillEnd(false);
    }

    public override bool ValidTarget(BattlePawn m_user, int targetX, int targetY)
    {
        if (m_user.isInRange(targetX, targetY, m_range) && m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_occupied)
        {
            if (m_user.m_battleSystem.m_battleSpaces[targetX, targetY].m_pawn.gameObject.tag == "Enemy")
            {
                return true;
            }
        }

        return false;
    }

    void calcLandingSquare()
    {
        int throwMax;
        int range;
        int i = 0;
        switch (m_throwDirection)
        {
            case Direction.DIRECTION_NONE:
                break;
            case Direction.DIRECTION_UP:
                //calculate the throw  range so it cant go out of bounds
                throwMax = 3 - m_target.m_y;
                if (throwMax < THROWRANGE)
                    range = throwMax;
                else
                    range = THROWRANGE;

                if(m_target.m_y == 3)
                {
                    m_hitPawn = null;
                    m_hasCollided = false;
                    m_landingDist = 1;
                    m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y];
                    break;
                }

                //loop through all squares in the throw path
                for (i = 1; i <= range; i++)
                {
                    //if there is an enemy in one, set it so thay will be hit and stop one tile short
                    if (m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y + i].m_occupied == true)
                    {
                        m_hitPawn = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y + i].m_pawn;
                        m_hasCollided = true;
                        m_landingDist = i - 1;
                        m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y + i - 1];
                        return;
                    }
                }
                //if an enemy won't be hit, stop at the end of the range
                m_hitPawn = null;
                m_hasCollided = false;
                m_landingDist = range;
                m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y + i - 1];
                break;

            case Direction.DIRECTION_DOWN:
                //calculate the throw  range so it cant go out of bounds
                throwMax = 0 + m_target.m_y;
                if (throwMax < THROWRANGE)
                    range = throwMax;
                else
                    range = THROWRANGE;

                if (m_target.m_y == 0)
                {
                    m_hitPawn = null;
                    m_hasCollided = false;
                    m_landingDist = 1;
                    m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y];
                    break;
                }

                //loop through all squares in the throw path
                for (i = 1; i <= range; i++)
                {
                    //if there is an enemy in one, set it so thay will be hit and stop one time short
                    if (m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y - i].m_occupied == true)
                    {
                        m_hitPawn = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y - i].m_pawn;
                        m_hasCollided = true;
                        m_landingDist = i - 1;
                        m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y - i + 1];
                        return;
                    }
                }
                //if an enemy won't be hit, stop at the end of the range
                m_hitPawn = null;
                m_hasCollided = false;
                m_landingDist = range;
                m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y - i + 1];
                break;

            case Direction.DIRECTION_RIGHT:
                //calculate the throw  range so it cant go out of bounds
                throwMax = 7 - m_target.m_x;
                if (throwMax < THROWRANGE)
                    range = throwMax;
                else
                    range = THROWRANGE;

                m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteR;
                if (m_target.m_x == 7)
                {
                    m_hitPawn = null;
                    m_hasCollided = false;
                    m_landingDist = 1;
                    m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y];
                    break;
                }

                //loop through all squares in the throw path
                for (i = 1; i <= range; i++)
                {
                    //if there is an enemy in one, set it so thay will be hit and stop one time short
                    if (m_user.m_battleSystem.m_battleSpaces[m_target.m_x + 1, m_target.m_y].m_occupied == true)
                    {
                        m_hitPawn = m_user.m_battleSystem.m_battleSpaces[m_target.m_x + i, m_target.m_y].m_pawn;
                        m_hasCollided = true;
                        m_landingDist = i - 1;
                        m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x + i - 1, m_target.m_y];
                        return;
                    }
                }
                //if an enemy won't be hit, stop at the end of the range
                m_hitPawn = null;
                m_hasCollided = false;
                m_landingDist = range;
                m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x + i - 1, m_target.m_y];
                break;
            case Direction.DIRECTION_LEFT:
                //calculate the throw  range so it cant go out of bounds
                throwMax = 0 + m_target.m_x;
                if (throwMax < THROWRANGE)
                    range = throwMax;
                else
                    range = THROWRANGE;

                m_user.GetComponent<SpriteRenderer>().sprite = m_user.m_spriteL;

                if (m_target.m_x == 0)
                {
                    m_hitPawn = null;
                    m_hasCollided = false;
                    m_landingDist = 1;
                    m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x, m_target.m_y];
                    break;
                }

                //loop through all squares in the throw path
                for (i = 1; i <= range; i++)
                {
                    //if there is an enemy in one, set it so thay will be hit and stop one time short
                    if (m_user.m_battleSystem.m_battleSpaces[m_target.m_x - i, m_target.m_y].m_occupied == true)
                    {
                        m_hitPawn = m_user.m_battleSystem.m_battleSpaces[m_target.m_x - i, m_target.m_y].m_pawn;
                        m_hasCollided = true;
                        m_landingDist = i - 1;
                        m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x - i + 1, m_target.m_y];
                        return;
                    }
                }
                //if an enemy won't be hit, stop at the end of the range
                m_hitPawn = null;
                m_hasCollided = false;
                m_landingDist = range;
                m_landingSquare = m_user.m_battleSystem.m_battleSpaces[m_target.m_x - i + 1, m_target.m_y];
                break;
            default:
                break;
        }
    }
}

