using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleSystem : MonoBehaviour
{

    //map selection
    public GameObject r_selectionLight;
    public int m_selectionX, m_selectionY;

    public Button r_attackButton;
    public Command m_selectedCommand;
    public List<BattlePawn> m_charactersInBattle;
    public List<BattlePawn> m_partyMembers;
    public List<PartyMemberStatus> m_StatusBoxes;
    public GameObject r_playerSkills;
    public GameObject r_playerItems;
    public List<GameObject> r_skillButtons;
    public List<GameObject> r_itemButtons;
    public Skill r_selectedSkill;
    public Item r_selectedItem;
    public bool m_isAimingSkill = false;
    public bool m_isAimingItem = false;
    [Space(10)]
    [Header("Space Materials")]
    public Material m_materialNormal;
    public Material m_materialMove;
    public Material m_materialAttack;
    public Material m_materialHeal;
    [Space(10)]

    public int m_allies = 0;
    public int m_enemies = 0;
    public int m_dead = 0;

    public BattleState m_battleState;
    public BattlePawn m_currentTurnPawn;

    public GameObject r_characterActionButtons;

    public List<GameObject> r_spaces;

    public BattleSpace[,] m_battleSpaces;
    public List<BattleSpace> m_SelectableSpaces;

    public float m_inputTimer = 0;
    public float m_inputDelay = 0.2f;
    public float m_turnDelay = 2.0f;
    public float m_turnTimer = 0;
    public bool m_playingSkillAnimation = false;
    public bool m_playingItemAnimation = false;

    //Victory stuff
    [Space(10)]
    [Header("Victory")]
    public GameObject r_victoryScreen;
    public GameObject r_victoryBox1;
    public GameObject r_victoryBox2;
    public GameObject r_victoryBox3;
    public GameObject r_victoryBox4;
    VictoryState m_victoryState;

    //Function to start the fight after the Game Manager has set up the players and enemies
    public void StartBattle()
    {
        //sets the first flag for the victory screen 
        m_victoryState = VictoryState.VICTORYSTATE_START;

        m_SelectableSpaces = new List<BattleSpace>();
        m_battleSpaces = new BattleSpace[8, 4];
        int spaceNum = 0;
        //set up battle spaces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                m_battleSpaces[x, y] = new BattleSpace();
                m_battleSpaces[x, y].m_cube = r_spaces[spaceNum];
                m_battleSpaces[x, y].x = x;
                m_battleSpaces[x, y].y = y;
                spaceNum++;
            }
        }

        m_battleState = BattleState.BATTLESTATE_START;
        m_selectedCommand = Command.COMMAND_NONE;



        foreach (BattlePawn pawn in m_charactersInBattle)
        {
            m_battleSpaces[pawn.m_x, pawn.m_y].m_pawn = pawn;
            if (pawn.gameObject.tag == "Player")
            {
                m_partyMembers.Add(pawn);
                m_StatusBoxes[m_partyMembers.Count - 1].r_character = pawn;
            }

            pawn.StartBattle();
        }



        TurnOrder();
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();

        if (m_charactersInBattle[0].gameObject.tag == "Player")
        {
            m_battleState = BattleState.BATTLESTATE_PLAYER_TURN;
            r_characterActionButtons.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            r_attackButton.Select();
        }
        else if (m_charactersInBattle[0].gameObject.tag == "Enemy")
        {
            m_battleState = BattleState.BATTLESTATE_ENEMY_TURN;
            r_characterActionButtons.SetActive(false);
        }

        m_currentTurnPawn = m_charactersInBattle[0];
        m_charactersInBattle[0].TakeTurn();

    }

    public void Death(BattlePawn deadPawn)
    {

        foreach (BattlePawn pawn in m_charactersInBattle)
        {
            if (pawn.m_timeUntilTurn > deadPawn.m_timeUntilTurn)
                pawn.m_timeUntilTurn--;
        }
        m_charactersInBattle.Remove(deadPawn);

        m_battleSpaces[deadPawn.m_x, deadPawn.m_y].m_occupied = false;
        m_battleSpaces[deadPawn.m_x, deadPawn.m_y].m_pawn = null;
        if (deadPawn.tag == "Enemy")
        {
            m_enemies--;

            foreach (BattlePawn character in m_charactersInBattle)
            {
                if (character.tag == "Player")
                {
                    //Gain EXP based on enemy level
                    int levelDif = deadPawn.m_level - character.m_level;
                    int levelDifBoost = deadPawn.m_level + levelDif;
                    if (levelDifBoost < 0)
                        levelDifBoost = 0;
                    character.m_expGain += levelDifBoost + deadPawn.m_level;

                }
            }
        }
        else
            m_allies--;

        if (m_allies == 0)
        {
            m_battleState = BattleState.BATTLESTATE_LOSE;
            r_characterActionButtons.SetActive(false);
            return;
        }

        if (m_enemies == 0)
        {
            m_battleState = BattleState.BATTLESTATE_WIN;
            r_characterActionButtons.SetActive(false);
        }

    }

    public void EndTurn()
    {
        foreach (BattlePawn pawn in m_charactersInBattle)
        {
            if (pawn.m_timeUntilTurn == 0)
            {
                pawn.m_timeUntilTurn = m_charactersInBattle.Count;
            }

            pawn.m_timeUntilTurn--;
        }

        foreach (BattlePawn pawn in m_charactersInBattle)
        {
            if (pawn.m_timeUntilTurn == 0)
            {
                if (pawn.m_myTurn)
                    return;
                m_currentTurnPawn = pawn;
                m_turnTimer = m_turnDelay;
                m_battleState = BattleState.BATTLESTATE_INBETWEENTURN;
            }
        }

    }

    public void EndTurnButton()
    {
        m_currentTurnPawn.EndTurn();
    }

    public void DefendButton()
    {
        if (m_inputTimer < m_inputDelay)
            return;
        m_inputTimer = 0;
        m_currentTurnPawn.m_AP--;
        m_currentTurnPawn.m_isDefending = true;
        m_currentTurnPawn.EndTurn();
    }

    // Update is called once per frame
    void Update()
    {

        if (m_playingSkillAnimation || m_playingItemAnimation)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
            r_attackButton.Select();

        if (m_enemies == 0)
            Victory();

        switch (m_battleState)
        {
            case BattleState.BATTLESTATE_START:
                break;
            case BattleState.BATTLESTATE_PLAYER_TURN:

                if (Input.GetAxis("Horizontal") < 0.1 && Input.GetAxis("Horizontal") > -0.1 && Input.GetAxis("Vertical") < 0.1 && Input.GetAxis("Vertical") > -0.1 && Input.GetAxis("Submit") > 0.1 && Input.GetAxis("Cancel") > 0.1)
                    m_inputTimer = m_inputDelay;

                if (m_inputTimer < m_inputDelay)
                {
                    m_inputTimer += Time.deltaTime;
                    return;
                }


                switch (m_selectedCommand)
                {


                    case Command.COMMAND_NONE:
                        break;
                    case Command.COMMAND_ATTACK:
                        BattleCursorMovement();

                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            if (m_battleSpaces[m_selectionX, m_selectionY].m_selectable && m_battleSpaces[m_selectionX, m_selectionY].m_occupied)
                            {
                                foreach (BattleSpace space in m_SelectableSpaces)
                                {
                                    space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
                                    space.m_selectable = false;

                                }

                                foreach (BattleSpace space in m_battleSpaces)
                                {
                                    if (space.m_occupied)
                                    {
                                        space.m_pawn.r_turnMarker.SetActive(false);
                                        space.m_pawn.r_myTurnMarker.SetActive(false);
                                    }
                                }
                                m_SelectableSpaces.Clear();

                                //ATTACK CODE
                                m_currentTurnPawn.Attack(m_selectionX, m_selectionY,
                                    new Vector3(m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x,
                                    m_currentTurnPawn.transform.position.y,
                                    m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z));
                            }
                        }

                        if (Input.GetAxis("Cancel") > 0.1)
                        {
                            m_inputTimer = 0;
                            AttackEnd(true);
                        }
                        break;
                    case Command.COMMAND_SKILL:
                        if (Input.GetAxis("Cancel") > 0.1)
                        {
                            m_inputTimer = 0;
                            SkillEnd(true);
                            return;
                        }

                        if (!m_isAimingSkill)
                            return;
                        BattleCursorMovement();

                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            if (r_selectedSkill.ValidTarget(m_currentTurnPawn, m_selectionX, m_selectionY))
                            {
                                foreach (BattleSpace space in m_SelectableSpaces)
                                {
                                    space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
                                    space.m_selectable = false;
                                }

                                foreach (BattleSpace space in m_battleSpaces)
                                {
                                    if (space.m_occupied)
                                    {
                                        space.m_pawn.r_turnMarker.SetActive(false);
                                        space.m_pawn.r_myTurnMarker.SetActive(false);
                                    }
                                }
                                m_SelectableSpaces.Clear();
                                r_selectedSkill.UseSkill(m_currentTurnPawn, m_selectionX, m_selectionY);

                            }
                        }
                        break;
                    case Command.COMMAND_ITEM:
                        if (Input.GetAxis("Cancel") > 0.1)
                        {
                            m_inputTimer = 0;
                            ItemEnd(true);
                            return;
                        }

                        if (!m_isAimingItem)
                            return;
                        BattleCursorMovement();

                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            if (r_selectedItem.ValidTarget(m_currentTurnPawn, m_selectionX, m_selectionY))
                            {
                                foreach (BattleSpace space in m_SelectableSpaces)
                                {
                                    space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
                                    space.m_selectable = false;
                                }
                                m_SelectableSpaces.Clear();
                                foreach (BattleSpace space in m_battleSpaces)
                                {
                                    if (space.m_occupied)
                                    {
                                        space.m_pawn.r_turnMarker.SetActive(false);
                                        space.m_pawn.r_myTurnMarker.SetActive(false);
                                    }
                                }
                                r_selectedItem.UseItem(m_currentTurnPawn, m_selectionX, m_selectionY);

                            }
                        }
                        break;
                    case Command.COMMAND_MOVE:


                        BattleCursorMovement();

                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            if (m_battleSpaces[m_selectionX, m_selectionY].m_selectable)
                            {
                                foreach (BattleSpace space in m_SelectableSpaces)
                                {
                                    space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
                                    space.m_selectable = false;
                                }
                                m_SelectableSpaces.Clear();
                                foreach (BattleSpace space in m_battleSpaces)
                                {
                                    if (space.m_occupied)
                                    {
                                        space.m_pawn.r_turnMarker.SetActive(false);
                                        space.m_pawn.r_myTurnMarker.SetActive(false);
                                    }
                                }
                                m_currentTurnPawn.MoveTo(m_selectionX, m_selectionY,
                                    new Vector3(m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x,
                                    m_currentTurnPawn.transform.position.y,
                                    m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z));
                                MoveEnd(false);
                            }
                        }

                        if (Input.GetAxis("Cancel") > 0.1)
                        {
                            m_inputTimer = 0;
                            MoveEnd(true);
                        }

                        break;
                    case Command.COMMAND_DEFEND:
                        break;
                    default:
                        break;
                }

                break;
            case BattleState.BATTLESTATE_ENEMY_TURN:
                break;
            case BattleState.BATTLESTATE_WIN:
                if (m_inputTimer < m_inputDelay)
                {
                    m_inputTimer += Time.deltaTime;
                    return;
                }

                switch (m_victoryState)
                {
                    case VictoryState.VICTORYSTATE_START:
                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            LevelUp();
                            m_victoryState = VictoryState.VICTORYSTATE_LEVELUP;
                        }
                        break;
                    case VictoryState.VICTORYSTATE_LEVELUP:
                        if (Input.GetAxis("Submit") > 0.1)
                        {
                            m_inputTimer = 0;
                            EndBattle(true);
                        }
                        break;
                    case VictoryState.VICTORYSTATE_END:
                        break;
                    default:
                        break;
                }
                break;
            case BattleState.BATTLESTATE_LOSE:
                break;
            case BattleState.BATTLESTATE_INBETWEENTURN:
                m_turnTimer -= Time.deltaTime;
                if (m_turnTimer <= 0)
                {

                    foreach (BattlePawn pawn in m_charactersInBattle)
                    {
                        pawn.r_turnMarker.GetComponentInChildren<Text>().text = (pawn.m_timeUntilTurn).ToString();

                        if (pawn.m_timeUntilTurn == 0)
                        {
                            pawn.r_turnMarker.SetActive(false);
                            pawn.r_myTurnMarker.SetActive(true);
                        }
                        else
                        {
                            pawn.r_turnMarker.SetActive(true);
                            pawn.r_myTurnMarker.SetActive(false);
                        }

                        if (pawn.m_isDead)
                        {
                            pawn.r_turnMarker.SetActive(false);
                            pawn.r_myTurnMarker.SetActive(false);
                        }
                    }
                    m_currentTurnPawn.TakeTurn();
                }
                break;
            default:
                break;
        }
    }

    void EndBattle(bool win)
    {

        FindObjectOfType<GameManager>().BattleEnd(win);

    }

    public void Victory()
    {
        r_victoryScreen.SetActive(true);
        switch (m_partyMembers.Count)
        {
            case 1:
                r_victoryBox1.SetActive(true);
                break;
            case 2:
                r_victoryBox1.SetActive(true);
                r_victoryBox2.SetActive(true);
                break;
            case 3:
                r_victoryBox1.SetActive(true);
                r_victoryBox2.SetActive(true);
                r_victoryBox3.SetActive(true);
                break;
            case 4:
                r_victoryBox1.SetActive(true);
                r_victoryBox2.SetActive(true);
                r_victoryBox3.SetActive(true);
                r_victoryBox4.SetActive(true);
                break;
            default:
                break;
        }

        r_victoryBox1.transform.Find("Name").GetComponent<Text>().text = m_partyMembers[0].m_name;
        r_victoryBox1.transform.Find("Sprite").GetComponent<Image>().sprite = m_partyMembers[0].GetComponent<SpriteRenderer>().sprite;
        r_victoryBox1.transform.Find("EXP").GetComponent<Text>().text = "+" + m_partyMembers[0].m_expGain.ToString() + " EXP";

        if (m_partyMembers.Count > 1)
        {
            r_victoryBox2.transform.Find("Name").GetComponent<Text>().text = m_partyMembers[1].m_name;
            r_victoryBox2.transform.Find("Sprite").GetComponent<Image>().sprite = m_partyMembers[1].GetComponent<SpriteRenderer>().sprite;
            r_victoryBox2.transform.Find("EXP").GetComponent<Text>().text = "+" + m_partyMembers[1].m_expGain.ToString() + " EXP";
        }

        if (m_partyMembers.Count > 2)
        {
            r_victoryBox3.transform.Find("Name").GetComponent<Text>().text = m_partyMembers[2].m_name;
            r_victoryBox3.transform.Find("Sprite").GetComponent<Image>().sprite = m_partyMembers[2].GetComponent<SpriteRenderer>().sprite;
            r_victoryBox3.transform.Find("EXP").GetComponent<Text>().text = "+" + m_partyMembers[2].m_expGain.ToString() + " EXP";
        }

        if (m_partyMembers.Count > 3)
        {
            r_victoryBox4.transform.Find("Name").GetComponent<Text>().text = m_partyMembers[3].m_name;
            r_victoryBox4.transform.Find("Sprite").GetComponent<Image>().sprite = m_partyMembers[3].GetComponent<SpriteRenderer>().sprite;
            r_victoryBox4.transform.Find("EXP").GetComponent<Text>().text = "+" + m_partyMembers[3].m_expGain.ToString() + " EXP";
        }

        foreach (BattlePawn pawn in m_partyMembers)
        {
            pawn.r_characterSheet.m_currentHP = pawn.m_HP;
            pawn.r_characterSheet.m_currentSP = pawn.m_SP;
        }
    }

    void LevelUp()
    {
        for (int i = 0; i < m_partyMembers.Count; i++)
        {
            LevelUpResult result = m_partyMembers[i].LevelUp();

            r_victoryBox1.transform.Find("EXP").gameObject.SetActive(false);
            r_victoryBox2.transform.Find("EXP").gameObject.SetActive(false);
            r_victoryBox3.transform.Find("EXP").gameObject.SetActive(false);
            r_victoryBox4.transform.Find("EXP").gameObject.SetActive(false);


            if (result != null)
            {
                if (i == 0)
                {
                    r_victoryBox1.transform.Find("LevelUpText").gameObject.SetActive(true);
                    r_victoryBox1.transform.Find("Level").GetComponent<Text>().text = "Level " + result.m_startLevel + " - " + result.m_finalLevel;
                    r_victoryBox1.transform.Find("Level").gameObject.SetActive(true);

                    r_victoryBox1.transform.Find("Result").GetComponent<Text>().text = "";
                    if (result.m_HP != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Max HP + " + result.m_HP.ToString() + "\n";
                    if (result.m_SP != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Max SP + " + result.m_SP.ToString() + "\n";
                    if (result.m_strength != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Strength + " + result.m_strength.ToString() + "\n";
                    if (result.m_fortitude != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Fortitude + " + result.m_fortitude.ToString() + "\n";
                    if (result.m_wisdom != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Wisdom + " + result.m_wisdom.ToString() + "\n";
                    if (result.m_resistance != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Resistance + " + result.m_resistance.ToString() + "\n";
                    if (result.m_agility != 0)
                        r_victoryBox1.transform.Find("Result").GetComponent<Text>().text += "Agility + " + result.m_agility.ToString() + "\n";

                    r_victoryBox1.transform.Find("Result").gameObject.SetActive(true);
                }

                if (i == 1)
                {
                    r_victoryBox2.transform.Find("LevelUpText").gameObject.SetActive(true);
                    r_victoryBox2.transform.Find("Level").GetComponent<Text>().text = "Level " + result.m_startLevel + " - " + result.m_finalLevel;
                    r_victoryBox2.transform.Find("Level").gameObject.SetActive(true);

                    r_victoryBox2.transform.Find("Result").GetComponent<Text>().text = "";
                    if (result.m_HP != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Max HP + " + result.m_HP.ToString() + "\n";
                    if (result.m_SP != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Max SP + " + result.m_SP.ToString() + "\n";
                    if (result.m_strength != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Strength + " + result.m_strength.ToString() + "\n";
                    if (result.m_fortitude != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Fortitude + " + result.m_fortitude.ToString() + "\n";
                    if (result.m_wisdom != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Wisdom + " + result.m_wisdom.ToString() + "\n";
                    if (result.m_resistance != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Resistance + " + result.m_resistance.ToString() + "\n";
                    if (result.m_agility != 0)
                        r_victoryBox2.transform.Find("Result").GetComponent<Text>().text += "Agility + " + result.m_agility.ToString() + "\n";

                    r_victoryBox2.transform.Find("Result").gameObject.SetActive(true);
                }

                if (i == 2)
                {
                    r_victoryBox3.transform.Find("LevelUpText").gameObject.SetActive(true);
                    r_victoryBox3.transform.Find("Level").GetComponent<Text>().text = "Level " + result.m_startLevel + " - " + result.m_finalLevel;
                    r_victoryBox3.transform.Find("Level").gameObject.SetActive(true);

                    r_victoryBox3.transform.Find("Result").GetComponent<Text>().text = "";
                    if (result.m_HP != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Max HP + " + result.m_HP.ToString() + "\n";
                    if (result.m_SP != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Max SP + " + result.m_SP.ToString() + "\n";
                    if (result.m_strength != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Strength + " + result.m_strength.ToString() + "\n";
                    if (result.m_fortitude != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Fortitude + " + result.m_fortitude.ToString() + "\n";
                    if (result.m_wisdom != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Wisdom + " + result.m_wisdom.ToString() + "\n";
                    if (result.m_resistance != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Resistance + " + result.m_resistance.ToString() + "\n";
                    if (result.m_agility != 0)
                        r_victoryBox3.transform.Find("Result").GetComponent<Text>().text += "Agility + " + result.m_agility.ToString() + "\n";

                    r_victoryBox3.transform.Find("Result").gameObject.SetActive(true);
                }

                if (i == 3)
                {
                    r_victoryBox4.transform.Find("LevelUpText").gameObject.SetActive(true);
                    r_victoryBox4.transform.Find("Level").GetComponent<Text>().text = "Level " + result.m_startLevel + " - " + result.m_finalLevel;
                    r_victoryBox4.transform.Find("Level").gameObject.SetActive(true);

                    r_victoryBox4.transform.Find("Result").GetComponent<Text>().text = "";
                    if (result.m_HP != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Max HP + " + result.m_HP.ToString() + "\n";
                    if (result.m_SP != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Max SP + " + result.m_SP.ToString() + "\n";
                    if (result.m_strength != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Strength + " + result.m_strength.ToString() + "\n";
                    if (result.m_fortitude != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Fortitude + " + result.m_fortitude.ToString() + "\n";
                    if (result.m_wisdom != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Wisdom + " + result.m_wisdom.ToString() + "\n";
                    if (result.m_resistance != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Resistance + " + result.m_resistance.ToString() + "\n";
                    if (result.m_agility != 0)
                        r_victoryBox4.transform.Find("Result").GetComponent<Text>().text += "Agility + " + result.m_agility.ToString() + "\n";

                    r_victoryBox4.transform.Find("Result").gameObject.SetActive(true);
                }
            }
        }
    }

    //sort turn order by speed
    void TurnOrder()
    {

        m_charactersInBattle.Sort(SortBySpeed);

        for (int i = 0; i < m_charactersInBattle.Count; i++)
        {
            //give everyone their turn order
            m_charactersInBattle[i].m_timeUntilTurn = i;
            m_charactersInBattle[i].r_turnMarker.GetComponentInChildren<Text>().text = i.ToString();
            if (i == 0)
            {
                m_charactersInBattle[i].r_turnMarker.SetActive(false);
                m_charactersInBattle[i].r_myTurnMarker.SetActive(true);
            }
            else
            {
                m_charactersInBattle[i].r_turnMarker.SetActive(true);
                m_charactersInBattle[i].r_myTurnMarker.SetActive(false);
            }
            //count enemies and allies
            if (m_charactersInBattle[i].gameObject.tag == "Player")
            {
                m_allies++;
            }
            else if (m_charactersInBattle[i].gameObject.tag == "Enemy")
            {
                m_enemies++;
            }
        }
    }

    //sort function
    static int SortBySpeed(BattlePawn p1, BattlePawn p2)
    {
        return p2.m_speed.CompareTo(p1.m_speed);
    }

    public void SkillButtonPress()
    {
        if (m_inputTimer < m_inputDelay)
            return;

        m_inputTimer = 0;
        m_selectedCommand = Command.COMMAND_SKILL;
        r_characterActionButtons.SetActive(false);
        r_playerSkills.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        r_skillButtons[0].GetComponent<Button>().Select();
        int howManySkills = m_currentTurnPawn.r_characterSheet.m_skills.Count;
        for (int i = 0; i < r_skillButtons.Count; i++)
        {
            if (i < howManySkills)
            {
                r_skillButtons[i].SetActive(true);
                r_skillButtons[i].GetComponentInChildren<Text>().text = m_currentTurnPawn.r_characterSheet.m_skills[i].m_name;
            }
            else
            {
                r_skillButtons[i].SetActive(false);
            }
        }

    }

    public void ItemButtonPress()
    {
        if (m_inputTimer < m_inputDelay)
            return;

        m_inputTimer = 0;

        if (m_currentTurnPawn.r_characterSheet.m_items.Count <= 0)
            return;

        m_selectedCommand = Command.COMMAND_ITEM;
        r_characterActionButtons.SetActive(false);
        r_playerItems.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        r_itemButtons[0].GetComponent<Button>().Select();
        int howManyItems = m_currentTurnPawn.r_characterSheet.m_items.Count;
        for (int i = 0; i < r_itemButtons.Count; i++)
        {
            if (i < howManyItems)
            {
                r_itemButtons[i].SetActive(true);
                r_itemButtons[i].GetComponentInChildren<Text>().text = m_currentTurnPawn.r_characterSheet.m_items[i].m_name;
            }
            else
            {
                r_itemButtons[i].SetActive(false);
            }
        }

    }

    public void UseSkill(int num)
    {
        if (m_inputTimer < m_inputDelay)
            return;

        m_inputTimer = 0;
        if (m_currentTurnPawn.r_characterSheet.m_skills[num].CanUseSkill(m_currentTurnPawn))
        {
            r_selectedSkill = m_currentTurnPawn.r_characterSheet.m_skills[num];
            r_playerSkills.SetActive(false);
        }
        else
            return;

        foreach (BattleSpace space in m_battleSpaces)
        {
            if (m_currentTurnPawn.isInRange(space.x, space.y, r_selectedSkill.m_rangeMin, r_selectedSkill.m_rangeMax) && space != m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y])
            {
                space.m_cube.GetComponent<MeshRenderer>().material = m_materialAttack;
                space.m_selectable = true;
                m_SelectableSpaces.Add(space);
            }
        }
        m_selectionX = m_currentTurnPawn.m_x;
        m_selectionY = m_currentTurnPawn.m_y;
        r_selectionLight.SetActive(true);
        r_selectionLight.transform.SetPositionAndRotation(m_currentTurnPawn.transform.position, r_selectionLight.transform.rotation);
        m_isAimingSkill = true;
    }

    public void UseItem(int num)
    {
        if (m_inputTimer < m_inputDelay)
            return;

        m_inputTimer = 0;

        r_selectedItem = m_currentTurnPawn.r_characterSheet.m_items[num];
        r_playerItems.SetActive(false);


        foreach (BattleSpace space in m_battleSpaces)
        {
            if (m_currentTurnPawn.isInRange(space.x, space.y, 1))
            {
                space.m_cube.GetComponent<MeshRenderer>().material = m_materialHeal;
                space.m_selectable = true;
                m_SelectableSpaces.Add(space);
            }
        }
        m_selectionX = m_currentTurnPawn.m_x;
        m_selectionY = m_currentTurnPawn.m_y;
        r_selectionLight.SetActive(true);
        r_selectionLight.transform.SetPositionAndRotation(m_currentTurnPawn.transform.position, r_selectionLight.transform.rotation);
        m_isAimingItem = true;
    }

    public void AttackButtonPress()
    {
        if (m_inputTimer < m_inputDelay)
            return;
        m_inputTimer = 0;
        r_characterActionButtons.SetActive(false);
        m_selectionX = m_currentTurnPawn.m_x;
        m_selectionY = m_currentTurnPawn.m_y;
        //left space
        if (m_currentTurnPawn.m_x > 0)
        {

            m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y].m_cube.GetComponent<MeshRenderer>().material = m_materialAttack;
            m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y].m_selectable = true;
            m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y]);

        }
        //up space
        if (m_currentTurnPawn.m_y < 3)
        {

            m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1].m_cube.GetComponent<MeshRenderer>().material = m_materialAttack;
            m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1].m_selectable = true;
            m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1]);

        }
        //right space
        if (m_currentTurnPawn.m_x < 7)
        {

            m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y].m_cube.GetComponent<MeshRenderer>().material = m_materialAttack;
            m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y].m_selectable = true;
            m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y]);

        }
        //down space
        if (m_currentTurnPawn.m_y > 0)
        {
            m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1].m_cube.GetComponent<MeshRenderer>().material = m_materialAttack;
            m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1].m_selectable = true;
            m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1]);

        }

        r_selectionLight.SetActive(true);
        r_selectionLight.transform.SetPositionAndRotation(m_currentTurnPawn.transform.position, r_selectionLight.transform.rotation);


        m_selectedCommand = Command.COMMAND_ATTACK;
    }

    public void MoveButtonPress()
    {
        if (m_inputTimer < m_inputDelay)
            return;
        m_inputTimer = 0;
        r_characterActionButtons.SetActive(false);
        m_selectionX = m_currentTurnPawn.m_x;
        m_selectionY = m_currentTurnPawn.m_y;
        //left space
        if (m_currentTurnPawn.m_x > 0)
        {
            if (m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y].m_occupied == false)
            {
                m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y].m_cube.GetComponent<MeshRenderer>().material = m_materialMove;
                m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y].m_selectable = true;
                m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x - 1, m_currentTurnPawn.m_y]);
            }
        }
        //up space
        if (m_currentTurnPawn.m_y < 3)
        {
            if (m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1].m_occupied == false)
            {
                m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1].m_cube.GetComponent<MeshRenderer>().material = m_materialMove;
                m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1].m_selectable = true;
                m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y + 1]);
            }
        }
        //right space
        if (m_currentTurnPawn.m_x < 7)
        {
            if (m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y].m_occupied == false)
            {
                m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y].m_cube.GetComponent<MeshRenderer>().material = m_materialMove;
                m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y].m_selectable = true;
                m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x + 1, m_currentTurnPawn.m_y]);
            }
        }
        //down space
        if (m_currentTurnPawn.m_y > 0)
        {
            if (m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1].m_occupied == false)
            {
                m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1].m_cube.GetComponent<MeshRenderer>().material = m_materialMove;
                m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1].m_selectable = true;
                m_SelectableSpaces.Add(m_battleSpaces[m_currentTurnPawn.m_x, m_currentTurnPawn.m_y - 1]);
            }
        }

        r_selectionLight.SetActive(true);
        r_selectionLight.transform.SetPositionAndRotation(m_currentTurnPawn.transform.position, r_selectionLight.transform.rotation);


        m_selectedCommand = Command.COMMAND_MOVE;
    }

    public void MoveEnd(bool cancel)
    {
        //resolve character movement

        r_characterActionButtons.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

        r_attackButton.Select();
        foreach (BattleSpace space in m_SelectableSpaces)
        {
            space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
            space.m_selectable = false;
        }
        m_SelectableSpaces.Clear();

        r_selectionLight.SetActive(false);
        if (cancel == false)
            m_currentTurnPawn.m_AP--;

        m_selectedCommand = Command.COMMAND_NONE;
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_battleSpaces)
        {
            if (space.m_occupied)
            {
                if (space.m_pawn.m_timeUntilTurn != 0)
                    space.m_pawn.r_turnMarker.SetActive(true);
                else
                    space.m_pawn.r_myTurnMarker.SetActive(true);
            }
        }
    }

    public void AttackEnd(bool cancel)
    {
        //resolve attack
        m_inputTimer = 0;
        r_characterActionButtons.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_SelectableSpaces)
        {
            space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
            space.m_selectable = false;
        }
        m_SelectableSpaces.Clear();

        r_selectionLight.SetActive(false);
        if (cancel == false)
            m_currentTurnPawn.m_AP--;

        m_selectedCommand = Command.COMMAND_NONE;
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_battleSpaces)
        {
            if (space.m_occupied)
            {
                if (space.m_pawn.m_timeUntilTurn != 0)
                    space.m_pawn.r_turnMarker.SetActive(true);
                else
                    space.m_pawn.r_myTurnMarker.SetActive(true);
            }
        }
    }

    public void SkillEnd(bool cancel)
    {
        //resolve Skill

        r_characterActionButtons.SetActive(true);
        r_playerSkills.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_SelectableSpaces)
        {
            space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
            space.m_selectable = false;
        }
        m_SelectableSpaces.Clear();

        r_selectionLight.SetActive(false);

        m_selectedCommand = Command.COMMAND_NONE;
        m_isAimingSkill = false;
        r_selectedSkill = null;
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_battleSpaces)
        {
            if (space.m_occupied)
            {
                if (space.m_pawn.m_timeUntilTurn != 0)
                    space.m_pawn.r_turnMarker.SetActive(true);
                else
                    space.m_pawn.r_myTurnMarker.SetActive(true);
            }
        }
    }

    public void ItemEnd(bool cancel)
    {
        //resolve Item

        r_characterActionButtons.SetActive(true);
        r_playerItems.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();
        foreach (BattleSpace space in m_SelectableSpaces)
        {
            space.m_cube.GetComponent<MeshRenderer>().material = m_materialNormal;
            space.m_selectable = false;
        }
        m_SelectableSpaces.Clear();

        r_selectionLight.SetActive(false);
        if (cancel == false)
            m_currentTurnPawn.m_AP--;

        m_selectedCommand = Command.COMMAND_NONE;
        m_isAimingSkill = false;
        r_selectedSkill = null;

        EventSystem.current.SetSelectedGameObject(null);
        r_attackButton.Select();

        foreach (BattleSpace space in m_battleSpaces)
        {
            if (space.m_occupied)
            {
                if (space.m_pawn.m_timeUntilTurn != 0)
                    space.m_pawn.r_turnMarker.SetActive(true);
                else
                    space.m_pawn.r_myTurnMarker.SetActive(true);
            }
        }
    }

    void BattleCursorMovement()
    {
        //code that moves the cursor around the field with the player's input
        if (Input.GetAxis("Horizontal") > 0.1)
        {
            m_inputTimer = 0;
            if (m_selectionX < 7)
            {
                m_selectionX++;
                r_selectionLight.transform.SetPositionAndRotation(new Vector3
                    (m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x, 0, m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z),
                    r_selectionLight.transform.rotation);
            }
        }

        if (Input.GetAxis("Horizontal") < -0.1)
        {
            m_inputTimer = 0;
            if (m_selectionX > 0)
            {
                m_selectionX--;
                r_selectionLight.transform.SetPositionAndRotation(new Vector3
                    (m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x, 0, m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z),
                    r_selectionLight.transform.rotation);
            }
        }

        if (Input.GetAxis("Vertical") > 0.1)
        {
            m_inputTimer = 0;
            if (m_selectionY < 3)
            {
                m_selectionY++;
                r_selectionLight.transform.SetPositionAndRotation(new Vector3
                    (m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x, 0, m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z),
                    r_selectionLight.transform.rotation);
            }
        }

        if (Input.GetAxis("Vertical") < -0.1)
        {
            m_inputTimer = 0;
            if (m_selectionY > 0)
            {
                m_selectionY--;
                r_selectionLight.transform.SetPositionAndRotation(new Vector3
                    (m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.x, 0, m_battleSpaces[m_selectionX, m_selectionY].m_cube.transform.position.z),
                    r_selectionLight.transform.rotation);
            }
        }
    }
}

public class BattleSpace
{
    public GameObject m_cube;
    public BattlePawn m_pawn;
    public int x;
    public int y;
    public bool m_selectable = false;
    public bool m_occupied = false;
}

public enum Command
{
    COMMAND_NONE,
    COMMAND_ATTACK,
    COMMAND_SKILL,
    COMMAND_MOVE,
    COMMAND_DEFEND,
    COMMAND_ITEM
}

public enum BattleState
{
    BATTLESTATE_START,
    BATTLESTATE_PLAYER_TURN,
    BATTLESTATE_ENEMY_TURN,
    BATTLESTATE_WIN,
    BATTLESTATE_LOSE,
    BATTLESTATE_INBETWEENTURN
}

public enum VictoryState
{
    VICTORYSTATE_START,
    VICTORYSTATE_LEVELUP,
    VICTORYSTATE_END
}