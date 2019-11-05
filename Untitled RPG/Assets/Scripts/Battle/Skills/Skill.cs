using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Skill : MonoBehaviour
{
    public int m_apCost;
    public int m_spCost;
    public string m_name;
    public string m_description;
    public BattlePawn m_user;
    public BattlePawn m_target;
    public int m_rangeMax;
    public int m_rangeMin = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    abstract public bool UseSkill(BattlePawn user, int targetX, int targetY);

    abstract public bool ValidTarget(BattlePawn user, int targetX, int targetY);

    public bool CanUseSkill(BattlePawn user)
    {
        //the check to see if the skill can be selected to use from the skill menu
        if (user.m_AP >= m_apCost && user.m_SP >= m_spCost)
            return true;
        else
            return false;
    }

}

public enum Direction
{
    DIRECTION_NONE,
    DIRECTION_UP,
    DIRECTION_DOWN,
    DIRECTION_RIGHT,
    DIRECTION_LEFT
}