using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Item : MonoBehaviour
{
    public string m_name;
    public string m_description;
    public BattlePawn m_user;
    public BattlePawn m_target;
    public GameObject m_particle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    abstract public bool UseItem(BattlePawn user, int targetX, int targetY);

    abstract public bool ValidTarget(BattlePawn user, int targetX, int targetY);
}
