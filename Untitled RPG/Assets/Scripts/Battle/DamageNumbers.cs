using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumbers : MonoBehaviour
{
    List<GameObject> Numbers;
    public GameObject textPrefab;

    public float Speed;
    public float Distance;
    // Start is called before the first frame update
    void Start()
    {
        Numbers = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = Numbers.Count - 1; i >= 0; i--)
        {
            Numbers[i].transform.SetPositionAndRotation(Numbers[i].transform.position + new Vector3(0, Speed * Time.deltaTime, 0), Numbers[i].transform.rotation);
            Numbers[i].GetComponent<Text>().color = new Color(
                Numbers[i].GetComponent<Text>().color.r,
                Numbers[i].GetComponent<Text>().color.g,
                Numbers[i].GetComponent<Text>().color.b,
                Mathf.Lerp(1, 0, Numbers[i].transform.position.y / (Distance + 1) ));

            if (Numbers[i].transform.position.y > Distance)
            {
                GameObject temp = Numbers[i];
                Numbers.Remove(Numbers[i]);
                Destroy(temp);

            }
        }
    }

    public void SpawnNumber(int amount, Color color)
    {
        Numbers.Add(Instantiate(textPrefab, gameObject.transform));
        Numbers[Numbers.Count - 1].GetComponent<Text>().text = amount.ToString();
        Numbers[Numbers.Count - 1].GetComponent<Text>().color = color;
    }
}
