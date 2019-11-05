using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Ailment : MonoBehaviour
{
    //the pawn that has the ailment, and the one that made it
    public BattlePawn r_InflictedPawn, r_Creator;
    //when the effect of the ailment takes place
    public ActiveTime m_activeTime;
    //how many times the activate should be called before it ends by it self
    public int m_turnsUntilEnd;
    //how many times the ailment has taken effect
    public int m_timesActivated;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //gets called by battle pawn at the proper time based on active time
    abstract public void Activate();
    //called by the skill that made it when it is first made. should set up the visual indicator and initialize values
    abstract public void Inflict(BattlePawn inflicted, BattlePawn creator);
    //remove the ailment and it's visual
    abstract public void End();
}

public enum ActiveTime
{
    ACTIVETIME_NONE,
    ACTIVETIME_TURNSTART,
    ACTIVETIME_TURNEND,
    ACTIVETIME_ACT
}
