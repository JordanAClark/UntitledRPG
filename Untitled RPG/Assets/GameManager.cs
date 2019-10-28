using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public List<CharacterSheet> m_playerParty;
    public GameObject m_mapPawnPrefab;
    GameObject m_mapPawn;
    public Vector3 m_mapPosition;
    public MapArea r_mapArea;

    void LoadWorldMap()
    {
        SceneManager.LoadScene("WorldMap", LoadSceneMode.Single);

    }

    public void NewGame()
    {
        LoadWorldMap();
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<Button>().Select();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "TitleScreen")
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                FindObjectOfType<Button>().Select();
        }
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Level Loaded");
        Debug.Log(scene.name);
        Debug.Log(mode);
        if (scene.name == "WorldMap")
        {
            m_mapPawn = Instantiate(m_mapPawnPrefab, m_mapPosition, Quaternion.identity);
            m_mapPawn.GetComponent<MapPawn>().m_gameManager = this;
        }

        if (scene.name == "Battle")
        {
            int encounterNum = Random.Range(0, r_mapArea.m_encounters.Count);
            Encounter currentEncounter = r_mapArea.m_encounters[encounterNum];
            BattleSystem currentBattleSystem = FindObjectOfType<BattleSystem>();
            foreach (CharacterSheet sheet in m_playerParty)
            {
                BattlePawn temp = Instantiate(sheet.m_battlePawnPrefab).GetComponent<BattlePawn>();
                temp.r_characterSheet = sheet;
                currentBattleSystem.m_charactersInBattle.Add(temp);
                temp.SetUp();
            }

            for (int i = 0; i < currentEncounter.m_enemies.Count; i++)
            {
                BattlePawn temp = Instantiate(currentEncounter.m_enemies[i].m_BattlePawn).GetComponent<BattlePawn>();
                temp.r_enemySheet = currentEncounter.m_enemies[i];
                temp.m_x = currentEncounter.m_enemyLocations[i].x;
                temp.m_y = currentEncounter.m_enemyLocations[i].y;
                currentBattleSystem.m_charactersInBattle.Add(temp);
                temp.SetUp();
            }

            currentBattleSystem.StartBattle();
        }
    }

    public void RandomBattle(MapArea mapArea)
    {
        m_mapPosition = m_mapPawn.transform.position;
        r_mapArea = mapArea;
        SceneManager.LoadScene("Battle", LoadSceneMode.Single);
    }

    public void BattleEnd(bool win)
    {
        if (win)
        {
            LoadWorldMap();
        }
        else
        {

        }
    }

}
