using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoned : Ailment
{
    public GameObject m_visualPrefab, m_visual;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Activate()
    {
        r_InflictedPawn.TakeDamage(r_Creator, DamageType.DAMAGETYPE_MAGIC, 0.2f);
    }

    public override void Inflict(BattlePawn inflicted, BattlePawn creator)
    {
        r_InflictedPawn = inflicted;
        r_Creator = creator;
        m_visual = Instantiate(m_visualPrefab, r_InflictedPawn.gameObject.transform, false);
        m_visual.transform.SetPositionAndRotation(r_InflictedPawn.transform.position, r_InflictedPawn.transform.rotation);
    }

    public override void End()
    {
        Destroy(m_visual);
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        Destroy(m_visual);
    }
}
