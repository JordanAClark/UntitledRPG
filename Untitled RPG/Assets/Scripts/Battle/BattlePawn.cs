using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlePawn : MonoBehaviour
{
    public Sprite m_spriteR;
    public Sprite m_spriteL;

    [Header("Stats")]
    public int m_level;
    public string m_name;
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
    public bool m_isDefending;

    //how many turnes away this character's turn is
    [Space(20)]
    public int m_timeUntilTurn;

    public bool m_myTurn = false;
    public int m_x, m_y;
    public int m_oldX, m_oldY;
    public Vector3 attackPos;
    bool m_attackHasHit = false;
    public Vector3 oldPos;
    public Vector3 newPos;

    int m_targetX, m_targetY;
    public bool m_isDead = false;
    public bool m_deadTurn = false;
    public bool m_playingSkillAnimation = false;
    public bool m_playingItemAnimation = false;

    public float moveTimer = 0.0f;
    public float attackTimer = 0.0f;
    public BattleSystem m_battleSystem;

    public ParticleSystem r_hitParticle;
    public DamageNumbers r_damageNumbers;
    public GameObject r_turnMarker;
    public LevelUpResult m_levelUpResult;

    public CharacterSheet r_characterSheet;
    public EnemySheet r_enemySheet;

    //Enemy Veriables
    public List<BattlePawn> r_playerPawns;
    public BattlePawn r_targetPlayer;
    public float m_enemyActionTimer = 0;
    public float m_enemyActionDelay = 1;

    public float m_expGain = 0;

    public void SetUp()
    {
        m_isDefending = false;
        oldPos = gameObject.transform.position;
        newPos = gameObject.transform.position;
        m_battleSystem = FindObjectOfType<BattleSystem>();
        r_damageNumbers = GetComponentInChildren<DamageNumbers>();
        if (gameObject.tag == "Player")
        {
            CharacterSheet stats = r_characterSheet;
            m_spriteR = stats.m_spriteR;
            m_spriteL = stats.m_spriteL;
            GetComponent<SpriteRenderer>().sprite = m_spriteL;

            m_level = stats.m_level;
            m_name = stats.name;

            m_maxHP = stats.m_maxHP;
            m_HP = stats.m_currentHP;

            m_maxSP = stats.m_maxSP;
            m_SP = stats.m_currentSP;

            m_maxAP = stats.m_maxAP;
            m_AP = stats.m_currentAP;

            m_attack = stats.m_strength;
            m_defence = stats.m_fortitude;
            m_magic = stats.m_wisdom;
            m_magicDefence = stats.m_resistance;
            m_speed = stats.m_agility;

            m_x = stats.m_x;
            m_y = stats.m_y;
        }

        if (gameObject.tag == "Enemy")
        {
            EnemySheet stats = r_enemySheet;
            m_spriteR = stats.m_spriteR;
            m_spriteL = stats.m_spriteL;
            GetComponent<SpriteRenderer>().sprite = m_spriteR;

            m_level = stats.m_level;

            m_maxHP = stats.m_maxHP;
            m_HP = stats.m_HP;

            m_maxSP = stats.m_maxSP;
            m_SP = stats.m_SP;

            m_maxAP = stats.m_maxAP;
            m_AP = stats.m_AP;

            m_attack = stats.m_attack;
            m_defence = stats.m_defence;
            m_magic = stats.m_magic;
            m_magicDefence = stats.m_magicDefence;
            m_speed = stats.m_speed;

            foreach (BattlePawn pawn in m_battleSystem.m_charactersInBattle)
            {
                if (pawn.gameObject.tag == "Player")
                {
                    r_playerPawns.Add(pawn);
                }
            }
            r_targetPlayer = r_playerPawns[0];
        }


    }

    public void StartBattle()
    {
        m_battleSystem.m_battleSpaces[m_x, m_y].m_occupied = true;
        m_battleSystem.m_battleSpaces[m_x, m_y].m_pawn = this;

        gameObject.transform.position = new Vector3(m_battleSystem.m_battleSpaces[m_x, m_y].m_cube.transform.position.x,
                                    0.5f,
                                    m_battleSystem.m_battleSpaces[m_x, m_y].m_cube.transform.position.z);
        if (gameObject.tag == "Enemy")
        {
            foreach (BattlePawn pawn in m_battleSystem.m_charactersInBattle)
            {
                if (pawn.gameObject.tag == "Player")
                {
                    r_playerPawns.Add(pawn);
                }
            }
            r_targetPlayer = r_playerPawns[0];
        }
    }


    public LevelUpResult LevelUp()
    {
        r_characterSheet.m_experience += (int)m_expGain;
        m_levelUpResult = r_characterSheet.LevelUP();
        if (m_levelUpResult != null)
            while (m_levelUpResult.m_anotherLevel == true)
            {
                m_levelUpResult = r_characterSheet.LevelUP(m_levelUpResult);
            }
        return m_levelUpResult;
    }

    public List<BattleSpace> GetAdjacentSpaces()
    {
        List<BattleSpace> adjacentSpaces = new List<BattleSpace>();
        //right space
        if (m_x < 7)
        {
            adjacentSpaces.Add(m_battleSystem.m_battleSpaces[m_x + 1, m_y]);
        }
        //up space
        if (m_y < 3)
        {
            adjacentSpaces.Add(m_battleSystem.m_battleSpaces[m_x, m_y + 1]);
        }
        //left space
        if (m_x > 0)
        {
            adjacentSpaces.Add(m_battleSystem.m_battleSpaces[m_x - 1, m_y]);
        }

        //down space
        if (m_y > 0)
        {
            adjacentSpaces.Add(m_battleSystem.m_battleSpaces[m_x, m_y - 1]);
        }

        return adjacentSpaces;
    }

    public void TakeTurn()
    {
        Debug.Log(gameObject.name + " starts turn");
        if (m_isDead)
        {
            m_battleSystem.EndTurn();
            if (!m_deadTurn)
            {
                m_battleSystem.m_dead++;
                m_deadTurn = true;
            }
            return;
        }

        if (gameObject.tag == "Player")
        {
            m_battleSystem.m_battleState = BattleState.BATTLESTATE_PLAYER_TURN;
            m_battleSystem.r_characterActionButtons.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            m_battleSystem.r_attackButton.Select();
        }
        else
        {
            m_battleSystem.m_battleState = BattleState.BATTLESTATE_ENEMY_TURN;
            m_battleSystem.r_characterActionButtons.SetActive(false);
        }

        m_isDefending = false;
        m_AP += 2;
        if (m_AP > m_maxAP)
            m_AP = m_maxAP;
        m_myTurn = true;

    }

    public void MoveTo(int targX, int targY, Vector3 pos)
    {
        m_battleSystem.m_battleSpaces[m_x, m_y].m_occupied = false;
        m_battleSystem.m_battleSpaces[m_x, m_y].m_pawn = null;

        m_oldX = m_x;
        m_oldY = m_y;
        m_x = targX;
        m_y = targY;

        m_battleSystem.m_battleSpaces[m_x, m_y].m_occupied = true;
        m_battleSystem.m_battleSpaces[m_x, m_y].m_pawn = this;

        oldPos = transform.position;
        newPos = pos;

        moveTimer = 1.0f;
        if (targX > m_oldX)
            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteR;
        else if (targX < m_oldX)
            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteL;
    }

    public void Attack(int targX, int targY, Vector3 targPos)
    {
        BattlePawn a_target = m_battleSystem.m_battleSpaces[targX, targY].m_pawn;
        attackPos = targPos + new Vector3(0, 0, -0.1f);
        oldPos = new Vector3(m_battleSystem.m_battleSpaces[m_x, m_y].m_cube.transform.position.x,
                                    0.5f,
                                    m_battleSystem.m_battleSpaces[m_x, m_y].m_cube.transform.position.z);
        m_attackHasHit = false;
        attackTimer = 1.0f;
        m_targetX = targX;
        m_targetY = targY;

        if (targX > m_x)
            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteR;
        else if (targX < m_x)
            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteL;
    }

    public void TakeDamage(BattlePawn attacker, DamageType damageType, float DamageMod)
    {
        bool isCrit = false;
        int damageTotal = 0;
        //calculate crit
        if (Random.Range(0, 31 - attacker.m_speed + m_speed) == 0)
            isCrit = true;

        if (isCrit)
        {
            // a criticle hit will not use defence
            damageTotal = (int)((attacker.m_attack * Random.Range(1.0f, 1.5f)) * (DamageMod));

        }
        else
        {
            damageTotal = (int)((attacker.m_attack * Random.Range(1.0f, 1.5f) - (m_defence / 2 * Random.Range(1.0f, 0.5f)))*(DamageMod));
        }

        if (m_isDefending)
        {
            damageTotal = (int)(damageTotal * DamageMod / 2);
        }

        m_HP -= damageTotal;

        r_hitParticle.emission.SetBurst(0, new ParticleSystem.Burst(0, damageTotal * 2));
        r_hitParticle.Play();
        r_damageNumbers.SpawnNumber(damageTotal, Color.red);
    }

    public void EndTurn()
    {
        m_myTurn = false;
        m_battleSystem.EndTurn();

        Debug.Log(gameObject.name + " Ends Turn");
    }

    void Death()
    {
        if (m_isDead)
            return;
        m_battleSystem.Death(this);
        m_isDead = true;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        r_turnMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isDead && m_myTurn)
        {
            m_battleSystem.EndTurn();
            return;
        }

        if (m_playingSkillAnimation || m_playingItemAnimation)
            return;

        //move animation
        if (moveTimer > 0)
        {
            moveTimer -= Time.deltaTime * 2;
            gameObject.transform.SetPositionAndRotation(Vector3.Lerp(newPos, oldPos, moveTimer), gameObject.transform.rotation);
        }
        else if (moveTimer < 0)
        {
            moveTimer = 0;
            gameObject.transform.SetPositionAndRotation(Vector3.Lerp(newPos, oldPos, moveTimer), gameObject.transform.rotation);
        }

        //attackAnimation
        if (attackTimer > 0 && m_attackHasHit == false)
        {
            attackTimer -= Time.deltaTime * 4;
            gameObject.transform.SetPositionAndRotation(Vector3.Lerp(attackPos, oldPos, attackTimer), gameObject.transform.rotation);
        }
        else if (attackTimer < 0 && m_attackHasHit == false)
        {
            m_attackHasHit = true;
            //deal Damage
            m_battleSystem.m_battleSpaces[m_targetX, m_targetY].m_pawn.TakeDamage(this, DamageType.DAMAGETYPE_PHYSICAL, 1.0f);
            if (gameObject.tag == "Player")
                m_battleSystem.AttackEnd(false);
        }
        else if (attackTimer > 1 && m_attackHasHit == true)
        {
            attackTimer = 1;
            gameObject.transform.SetPositionAndRotation(Vector3.Lerp(attackPos, oldPos, attackTimer), gameObject.transform.rotation);
        }
        else if (attackTimer < 1 && m_attackHasHit == true)
        {
            attackTimer += Time.deltaTime * 3;
            gameObject.transform.SetPositionAndRotation(Vector3.Lerp(attackPos, oldPos, attackTimer), gameObject.transform.rotation);
        }



        if (m_HP <= 0)
        {
            Death();
            return;
        }

        if (!m_myTurn)
        {
            return;
        }

        if (m_AP == 0 && m_myTurn)
        {
            EndTurn();
            return;
        }

        if (gameObject.tag == "Enemy" && m_myTurn)
        {
            m_enemyActionTimer += Time.deltaTime;
            if (m_enemyActionTimer < m_enemyActionDelay)
                return;
            //check if player is in range
            m_enemyActionTimer = 0;
            //attack an adjecent enemy
            {
                //right space
                if (m_x < 7)
                {
                    if (m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_occupied)
                    {

                        if (m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_pawn.gameObject.tag == "Player")
                        {
                            Attack(m_x + 1, m_y, m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_pawn.gameObject.transform.position);
                            m_AP--;
                            return;
                        }
                    }
                }
                //up space
                if (m_y < 3)
                {
                    if (m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_occupied)
                    {
                        if (m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_pawn.gameObject.tag == "Player")
                        {
                            Attack(m_x, m_y + 1, m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_pawn.gameObject.transform.position);
                            m_AP--;
                            return;
                        }
                    }
                }
                //left space
                if (m_x > 0)
                {

                    if (m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_occupied)
                    {
                        if (m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_pawn.gameObject.tag == "Player")
                        {
                            Attack(m_x - 1, m_y, m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_pawn.gameObject.transform.position);
                            m_AP--;
                            return;
                        }
                    }

                }

                //down space
                if (m_y > 0)
                {
                    if (m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_occupied)
                    {
                        if (m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_pawn.gameObject.tag == "Player")
                        {
                            Attack(m_x, m_y - 1, m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_pawn.gameObject.transform.position);
                            m_AP--;
                            return;
                        }
                    }

                }
            }
            //if no adjecent enemy, move towards target
            List<BattleSpace> targetSpaces = r_targetPlayer.GetAdjacentSpaces();
            List<float> distences = new List<float>();

            int i = 0;
            //right space
            if (r_targetPlayer.m_x > 0)
            {
                distences.Add(PointFunctions.DistanceTo(new Vector2(m_x, m_y), new Vector2(targetSpaces[i].x, targetSpaces[i].y)));
                i++;
            }
            //up space
            if (r_targetPlayer.m_y < 3)
            {
                distences.Add(PointFunctions.DistanceTo(new Vector2(m_x, m_y), new Vector2(targetSpaces[i].x, targetSpaces[i].y)));
                i++;
            }
            //left space
            if (r_targetPlayer.m_x < 7)
            {
                distences.Add(PointFunctions.DistanceTo(new Vector2(m_x, m_y), new Vector2(targetSpaces[i].x, targetSpaces[i].y)));
                i++;
            }

            //down space
            if (r_targetPlayer.m_y > 0)
            {
                distences.Add(PointFunctions.DistanceTo(new Vector2(m_x, m_y), new Vector2(targetSpaces[i].x, targetSpaces[i].y)));
                i++;
            }
            bool found = false;

            float lowestDist = 0;
            int lowestJ = 0;

            while (!found && distences.Count >= 0)
            {
                lowestJ = 0;
                lowestDist = 0;
                for (int j = 0; j < distences.Count; j++)
                {
                    if (distences[j] < lowestDist)
                    {
                        lowestJ = j;
                        lowestDist = distences[j];
                    }
                }

                if (targetSpaces[lowestJ].m_occupied)
                {
                    targetSpaces.RemoveAt(lowestJ);
                    distences.RemoveAt(lowestJ);
                }
                else
                {
                    found = true;
                }
            }

            if (found)
            {
                BattleSpace targetSpace = targetSpaces[lowestJ];
                //if the target space is to the right move right
                if (targetSpace.x > m_x)
                {
                    //only if right is unocupied
                    if (m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_occupied == false)
                    {
                        MoveTo(m_x + 1, m_y, new Vector3(m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_cube.transform.position.x,
                                    gameObject.transform.position.y,
                                    m_battleSystem.m_battleSpaces[m_x + 1, m_y].m_cube.transform.position.z));
                        m_AP--;
                        return;
                    }
                }

                //if it is left move left
                if (targetSpace.x < m_x)
                {
                    //and if left is unocupied 
                    if (m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_occupied == false)
                    {
                        MoveTo(m_x - 1, m_y, new Vector3(m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_cube.transform.position.x,
                                    gameObject.transform.position.y,
                                    m_battleSystem.m_battleSpaces[m_x - 1, m_y].m_cube.transform.position.z));
                        m_AP--;
                        return;
                    }
                }
                // if it is up
                if (targetSpace.y > m_y)
                {

                    if (m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_occupied == false)
                    {
                        MoveTo(m_x, m_y + 1, new Vector3(m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_cube.transform.position.x,
                                    gameObject.transform.position.y,
                                    m_battleSystem.m_battleSpaces[m_x, m_y + 1].m_cube.transform.position.z));
                        m_AP--;
                        return;
                    }
                }

                // if it is down
                if (targetSpace.y < m_y)
                {

                    if (m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_occupied == false)
                    {
                        MoveTo(m_x, m_y - 1, new Vector3(m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_cube.transform.position.x,
                                    gameObject.transform.position.y,
                                    m_battleSystem.m_battleSpaces[m_x, m_y - 1].m_cube.transform.position.z));
                        m_AP--;
                        return;
                    }
                }
            }



            EndTurn();
        }
    }

    public bool isInRange(int targetX, int targetY, int range)
    {
        float dist = PointFunctions.DistanceTo(new Vector2(m_x, m_y), new Vector2(targetX, targetY));

        if (dist <= range)
        {
            return true;
        }

        return false;
    }
}

public enum DamageType
{
    DAMAGETYPE_PHYSICAL,
    DAMAGETYPE_MAGIC
}

public static class PointFunctions
{
    public static float DistanceTo(Vector2 point1, Vector2 point2)
    {
        var a = (float)(point2.x - point1.x);
        var b = (float)(point2.y - point1.y);

        return Mathf.Sqrt(a * a + b * b);
    }
}

