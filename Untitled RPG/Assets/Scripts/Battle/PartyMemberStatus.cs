using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberStatus : MonoBehaviour
{
    public BattlePawn r_character;
    public Text m_name;
    public Text m_level;
    public Text m_hp;
    public Text m_sp;
    public Text m_ap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(r_character == null)
        {
            gameObject.SetActive(false);
            return;
        }

        m_name.text = r_character.name;
        m_level.text = "LV " + r_character.m_level;
        m_hp.text = r_character.m_HP + " / " + r_character.m_maxHP;
        m_sp.text = r_character.m_SP + " / " + r_character.m_maxSP;
        m_ap.text = r_character.m_AP + " / " + r_character.m_maxAP;
    }
}
