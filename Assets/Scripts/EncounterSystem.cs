using UnityEngine;
using System.Collections;

public class EncounterSystem : MonoBehaviour
{   
    [SerializeField] private Encounter[] enemiesInScene;
    [SerializeField] private int maxNumEnemies;

    private EnemyManager enemyManager;
    private void Start()
    {
        enemyManager = GameObject.FindAnyObjectByType<EnemyManager>();
        enemyManager.GenerateEnemiesByEncounter(enemiesInScene,maxNumEnemies);
    }

    private void Update()
    {
        
    }
}

[System.Serializable]
public class Encounter{
    public EnemyInfo Enemy;
    public int LevelMin;
    public int LevelMax;
}