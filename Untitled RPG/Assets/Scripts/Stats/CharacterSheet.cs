using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    //Character Text
    //The character's name
    public string m_name;
    //Describes the character's current role in the story
    public string m_title;
    public Sprite m_spriteR;
    public Sprite m_spriteL;
    public GameObject m_battlePawnPrefab;
    [Space(20)]

    //Stats - base stats are the stats at level 1

    //progression stats
    //current level
    public int m_level = 1;
    //current experience
    public int m_experience = 0;

    public int m_expForNextLevel = 5;
    public int m_baseExpForNextLevel = 5;
    public float m_expRequiredUpPower = 0.5f;
    //Tech Points - spent to gain new skills. gained at level up
    public int m_TP = 0;
    [Space(10)]
    [Header("Stats")]
    //Health - how much damage a character can take before being knocked out
    public int m_maxHP;
    public int m_currentHP;
    public Growth m_growthHP;
    public int m_baseHP;
    [Space(10)]
    //Skill Points - used to cast spells and special skills 
    public int m_maxSP;
    public int m_currentSP;
    public Growth m_growthSP;
    public int m_baseSP;
    [Space(10)]
    //Action Points - used to take actions
    public int m_maxAP;
    public int m_currentAP;
    public int m_baseAP = 3;
    //AP grows with story progress not on level up
    [Space(10)]
    //strength - affects attack power
    public int m_strength;
    public Growth m_growthStrength;
    public int m_baseStrength;
    [Space(10)]
    //fortitude - affects defence against attacks
    public int m_fortitude;
    public Growth m_growthFortitude;
    public int m_baseFortitude;
    [Space(10)]
    //wisdom - affects magic attack and healing
    public int m_wisdom;
    public Growth m_growthWisdom;
    public int m_baseWisdom;
    [Space(10)]
    //resistance - affects defence against magic attacks
    public int m_resistance;
    public Growth m_growthResistance;
    public int m_baseResistance;
    [Space(10)]
    //agility - affects turn order and evasion
    public int m_agility;
    public Growth m_growthAgility;
    public int m_baseAgility;
    //positioning in battle
    public int m_x, m_y;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Skill> m_skills;
    public List<Item> m_items;

    public LevelUpResult LevelUP(LevelUpResult carryLevel = null)
    {
        LevelUpResult alevel = new LevelUpResult();

        if (m_experience >= m_expForNextLevel)
        {
            alevel.m_startLevel = m_level;
            if (carryLevel != null)
                alevel.m_startLevel = carryLevel.m_startLevel;
            m_level++;
            alevel.m_finalLevel = m_level;
            m_experience -= m_expForNextLevel;
            m_expForNextLevel += (int)Mathf.Pow(m_expForNextLevel, m_expRequiredUpPower);
            if (m_experience >= m_expForNextLevel)
                alevel.m_anotherLevel = true;

            //growth section
            float growth;
            //HP growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthHP)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_maxHP += 5;
                            alevel.m_HP = 5;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_maxHP += 5;
                            alevel.m_HP = 5;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_maxHP += 5;
                            alevel.m_HP = 5;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_maxHP += 5;
                            alevel.m_HP = 5;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_maxHP += 5;
                            alevel.m_HP = 5;
                        }
                        break;
                    default:
                        alevel.m_HP = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_HP += carryLevel.m_HP;
                m_currentHP = m_maxHP;
            }
            //SP growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthSP)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_maxSP += 3;
                            alevel.m_SP = 3;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_maxSP += 3;
                            alevel.m_SP = 3;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_maxSP += 3;
                            alevel.m_SP = 3;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_maxSP += 3;
                            alevel.m_SP = 3;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_maxSP += 3;
                            alevel.m_SP = 3;
                        }
                        break;
                    default:
                        alevel.m_SP = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_SP += carryLevel.m_SP;
                m_currentSP = m_maxSP;
            }
            //Strength growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthStrength)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_strength += 1;
                            alevel.m_strength = 1;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_strength += 1;
                            alevel.m_strength = 1;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_strength += 1;
                            alevel.m_strength = 1;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_strength += 1;
                            alevel.m_strength = 1;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_strength += 1;
                            alevel.m_strength = 1;
                        }
                        break;
                    default:
                        alevel.m_strength = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_strength += carryLevel.m_strength;
            }
            //Fortitude growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthFortitude)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_fortitude += 1;
                            alevel.m_fortitude = 1;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_fortitude += 1;
                            alevel.m_fortitude = 1;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_fortitude += 1;
                            alevel.m_fortitude = 1;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_fortitude += 1;
                            alevel.m_fortitude = 1;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_fortitude += 1;
                            alevel.m_fortitude = 1;
                        }
                        break;
                    default:
                        alevel.m_fortitude = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_fortitude += carryLevel.m_fortitude;
            }
            //Wisdom growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthWisdom)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_wisdom += 1;
                            alevel.m_wisdom = 1;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_wisdom += 1;
                            alevel.m_wisdom = 1;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_wisdom += 1;
                            alevel.m_wisdom = 1;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_wisdom += 1;
                            alevel.m_wisdom = 1;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_wisdom += 1;
                            alevel.m_wisdom = 1;
                        }
                        break;
                    default:
                        alevel.m_wisdom = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_wisdom += carryLevel.m_wisdom;
            }
            //Resistance growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthResistance)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_resistance += 1;
                            alevel.m_resistance = 1;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_resistance += 1;
                            alevel.m_resistance = 1;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_resistance += 1;
                            alevel.m_resistance = 1;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_resistance += 1;
                            alevel.m_resistance = 1;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_resistance += 1;
                            alevel.m_resistance = 1;
                        }
                        break;
                    default:
                        alevel.m_resistance = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_resistance += carryLevel.m_resistance;
            }
            //Agility growth
            {
                growth = Random.Range(0.0f, 1.0f);
                switch (m_growthAgility)
                {
                    case Growth.GROWTH_A:
                        if (growth <= 0.8f)
                        {
                            m_agility += 1;
                            alevel.m_agility = 1;
                        }
                        break;
                    case Growth.GROWTH_B:
                        if (growth <= 0.65f)
                        {
                            m_agility += 1;
                            alevel.m_agility = 1;
                        }
                        break;
                    case Growth.GROWTH_C:
                        if (growth <= 0.5f)
                        {
                            m_agility += 1;
                            alevel.m_agility = 1;
                        }
                        break;
                    case Growth.GROWTH_D:
                        if (growth <= 0.35f)
                        {
                            m_agility += 1;
                            alevel.m_agility = 1;
                        }
                        break;
                    case Growth.GROWTH_E:
                        if (growth <= 0.2f)
                        {
                            m_agility += 1;
                            alevel.m_agility = 1;
                        }
                        break;
                    default:
                        alevel.m_agility = 0;
                        break;
                }
                if (carryLevel != null)
                    alevel.m_agility += carryLevel.m_agility;
            }
        }
        else
            alevel = null;

        return alevel;
    }
}


//dictates how offten stats increase
public enum Growth
{
    GROWTH_A,
    //80%
    GROWTH_B,
    //65%
    GROWTH_C,
    //50%
    GROWTH_D,
    //35%
    GROWTH_E
    //20%
}

public class LevelUpResult
{
    //a class to record growth of stats on level up;
    public bool m_anotherLevel;
    public int m_startLevel;
    public int m_finalLevel;
    public int m_HP;
    public int m_SP;
    public int m_strength;
    public int m_fortitude;
    public int m_wisdom;
    public int m_resistance;
    public int m_agility;
}
