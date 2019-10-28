using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPawn : MonoBehaviour
{
    public float m_walkSpeed;
    public Vector3 m_velocity;
    public Rigidbody m_rigidbody;
    public GameManager m_gameManager;
    public MapArea m_mapArea;
    // Start is called before the first frame update
    void Start()
    {
        m_velocity = Vector3.zero;
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_velocity.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        m_velocity = m_velocity.normalized* m_walkSpeed;
        m_rigidbody.velocity = m_velocity;

        if(Input.GetKeyDown(KeyCode.B))
        {
            m_gameManager.RandomBattle(m_mapArea);
        }
    }
}
