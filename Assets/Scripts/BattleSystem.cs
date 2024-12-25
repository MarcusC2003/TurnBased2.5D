using System.Collections;//required for IEnumerator
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run };
    [Header("Battle State")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    //import for TMP
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;


    private PartyManager partyManager;
    private EnemyManager enemyManager;
    //for turn order
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action: ";
    private const string WIN_MESSAGE = "Your party won the battle!";
    private const string LOSE_MESSAGE = "Your party has been defeated";
    private const string SUCCESFUL_RUN_MESSAGE = "Your party ran away";
    private const string UNSUCCESFUL_RUN_MESSAGE = "You party couldn't run away";
    private const int TURN_DURATION = 2;
    private const int RUN_CHANCE = 50;
    private const string OVERWORLD_SCENE = "OverworldScene";


    private void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        if (partyManager == null || enemyManager == null)
        {
            Debug.LogError("PartyManager or EnemyManager is not found!");
            return;
        }

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
    }

    //enumerator interface allows us to use yields.
    //BattleRoutine() is for after we finish enemy selection.
    private IEnumerator BattleRoutine()
    {
        //enemy selection menu disabled and change state
        enemySelectionMenu.SetActive(false);
        state = BattleState.Battle;
        bottomTextPopUp.SetActive(true);

        //loop through battlers and perform their action
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (state == BattleState.Battle && allBattlers[i].CurrHealth >0)
            {
                switch (allBattlers[i].BattleAction)
                {
                    case BattleEntities.Action.Attack:
                        //Start Coroutine allows us to call an Ienumerator class
                        yield return StartCoroutine(AttackRoutine(i));
                        break;
                    case BattleEntities.Action.Run:
                        yield return StartCoroutine(RunRoutine());
                        break;
                    default:
                        Debug.Log("Error - incorrect battle action");
                        break;
                }
            }
        }
        RemoveDeadBattlers();
        if (state == BattleState.Battle)
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();
        }
        yield return null;

    }
    private void CreatePartyEntities()
    {
        //get current party
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();

        //Count is for Lists, Length is for arrays
        for (int i = 0; i < currentParty.Count; i++)
        {
            //create battle entities for party members
            BattleEntities tempEntity = new BattleEntities();

            //assign values
            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth,
            currentParty[i].Initiative, currentParty[i].Strength, currentParty[i].Level, true);

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);

            //Spawn Visuals, Set Values, set entity
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab, partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].MaxHealth, currentParty[i].MaxHealth, currentParty[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;
        }
    }

    private IEnumerator AttackRoutine(int i)
    {
        //player turn
        if (allBattlers[i].IsPlayer == true)
        {
            BattleEntities currAttacker = allBattlers[i];
            //if player is targetting player or enemy out of array bounds
            if (allBattlers[currAttacker.Target].CurrHealth <= 0){
                currAttacker.SetTarget(GetRandomEnemy());
            }
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            //attack selected enemy ( using attack action )
            AttackAction(currAttacker, currTarget);

            //wait a few seconds --> then kill enemy
            yield return new WaitForSeconds(TURN_DURATION);
            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                enemyBattlers.Remove(currTarget);

                //check if player won battle
                if (enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
            }

        }

        //enemy turn
        //i < allBattlers.Count --> prevents softlock where player cant select move
        if (i < allBattlers.Count && allBattlers[i].IsPlayer == false)
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTarget(GetRandomPartyMember());
            BattleEntities currTarget = allBattlers[currAttacker.Target];

            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            //kill party member
            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                playerBattlers.Remove(currTarget);

                if (playerBattlers.Count <= 0)
                {
                    state = BattleState.Lost;
                    bottomText.text = LOSE_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    Debug.Log("Game Over");
                }
            }
            //attack selected party member
        }
    }
    private IEnumerator RunRoutine()
    {
        if (state == BattleState.Battle)
        {
            if (Random.Range(1, 101) >= RUN_CHANCE)
            {
                //run away
                bottomText.text = SUCCESFUL_RUN_MESSAGE;
                state = BattleState.Run;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                SceneManager.LoadScene(OVERWORLD_SCENE);

            }
            else
            {
                //unsuccesful run
                bottomText.text = UNSUCCESFUL_RUN_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION);
            }
        }
    }

    private void RemoveDeadBattlers(){
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if(allBattlers[i].CurrHealth <= 0){
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void CreateEnemyEntities()
    {
        //get current party
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        //Count is for Lists, Length is for arrays
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            //create battle entities for party members
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth,
            currentEnemies[i].Initiative, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);

            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;
        }
    }
    public void ShowBattleMenu()
    {
        //whos action
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        //enable battle menu
        battleMenu.SetActive(true);

    }

    public void ShowEnemySelectionMenu()
    {
        //disable the battle menu
        battleMenu.SetActive(false);
        //set our enemy selection buttons
        enemySelectionMenu.SetActive(true);
        //enable our selection menu
        SetEnemySelectionButtons();
    }

    private void SetEnemySelectionButtons()
    {
        //disable all buttons(if there is less than 3 enemies)
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }
        //enable buttons for each enemy
        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            //change button text (depending on enemy type)
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }

    }

    // select menu onClick()
    public void SelectEnemy(int currentEnemy)
    {
        //set current members target
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        //tell battle system this member attacks
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;
        //increment through members
        currentPlayer++;
        //if all players select action , start battle, else show menu for next player
        if (currentPlayer >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            //show for next player
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }
    }

    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        int damage = currAttacker.Strength; //get damage
        currAttacker.BattleVisuals.PlayAttackAnimation();//play attack animation
        currTarget.CurrHealth -= damage; //deal the damage
        currTarget.BattleVisuals.PlayHitAnimation(); //play hit animation
        currTarget.UpdateUI();//update UI
        bottomText.text = string.Format("{0} attacks {1} for {2} damage!", currAttacker.Name, currTarget.Name, damage);
        SaveHealth();
    }

    // may server the purpose of bug fixes
    // ex. if 2 enemies attack a party member but they die after the first attack
    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            //if we have member
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrHealth > 0)
            {
                partyMembers.Add(i);
            }
        }
        return partyMembers[Random.Range(0, partyMembers.Count)];//return random member
    }

    // may server the purpose of bug fixes
    // ex. if 2 party members attack an enemy but it dies after the first attack
    private int GetRandomEnemy()
    {
        List<int> enemies = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            //if we have member
            if (allBattlers[i].IsPlayer == false && allBattlers[i].CurrHealth > 0)
            {
                enemies.Add(i);
            }
        }
        return enemies[Random.Range(0, enemies.Count)];//return random member
    }

    private void SaveHealth()
    {
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrHealth);
        }
    }
    private void DetermineBattleOrder()
    {
        //sort by comparing battle initiatives
        // negative sign --> ascending sort
        allBattlers.Sort((bi1, bi2) => -bi1.Initiative.CompareTo(bi2.Initiative));
    }

    public void SelectRunAction()
    {
        state = BattleState.Selection;
        //set current members target
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];

        //tell battle system this member runs
        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;
        battleMenu.SetActive(false);
        //increment through members
        currentPlayer++;
        //if all players select action , start battle, else show menu for next player
        if (currentPlayer >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            //show for next player
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }
    }
}


[System.Serializable]
public class BattleEntities
{
    public enum Action { Attack, Run }
    //if BattleAction is not set, it will default to Action[0] which is attack
    public Action BattleAction;

    public string Name;
    public int CurrHealth;
    public int MaxHealth;
    public int Initiative;
    public int Strength;
    public int Level;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public int Target;

    public void SetEntityValues(string name, int currHealth, int maxHealth, int initiative, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrHealth = currHealth;
        MaxHealth = maxHealth;
        Initiative = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
    }
    public void SetTarget(int target)
    {
        Target = target;
    }
    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrHealth);
    }
}