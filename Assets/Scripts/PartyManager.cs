using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMemberInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;//must add [System.Serializable] to class
    [SerializeField] private PartyMemberInfo defaultPartyMember;

    private Vector3 playerPosition;
    private static GameObject instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            //first instance
            instance = this.gameObject;
            AddMemberToPartyByName(defaultPartyMember.MemberName);
        }
        //this makes sure that enemy Manager does not get destroyed when transfering scenes
        DontDestroyOnLoad(gameObject);

    }

    public void AddMemberToPartyByName(string memberName)
    {
        for (int i = 0; i < allMembers.Length; i++)
        {
            if (allMembers[i].MemberName == memberName)
            {
                PartyMember newPartyMember = new PartyMember();
                newPartyMember.MemberName = allMembers[i].MemberName;
                newPartyMember.Level = allMembers[i].StartingLevel;
                newPartyMember.CurrHealth = allMembers[i].BaseHealth;
                newPartyMember.MaxHealth = newPartyMember.CurrHealth;
                newPartyMember.Strength = allMembers[i].BaseStr;
                newPartyMember.Initiative = allMembers[i].BaseInitiative;
                newPartyMember.MemberBattleVisualPrefab = allMembers[i].MemberBattleVisualPrefab;
                newPartyMember.MemberOverworldVisualPrefab = allMembers[i].MemberOverworldVisualPrefab;

                currentParty.Add(newPartyMember);
            }
        }

    }

    public List<PartyMember> GetAliveParty()
    {
        List<PartyMember> aliveParty = new List<PartyMember>();
        aliveParty = currentParty;
        for (int i = 0; i < currentParty.Count; i++)
        {
            if(aliveParty[i].CurrHealth <= 0){
                aliveParty.RemoveAt(i);
            }
        }
        return aliveParty;
    }
    public List<PartyMember> GetCurrentParty(){
        return currentParty;
    }

    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].CurrHealth = health;
    }

    public void SetPosition(Vector3 position)
    {
        playerPosition = position;
    }

    public Vector3 GetPosition()
    {
        return playerPosition;
    }
}
[System.Serializable]
public class PartyMember
{
    public string MemberName;
    public int Level;
    public int CurrHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public int CurrExp;
    public int MaxExp;
    public GameObject MemberBattleVisualPrefab;
    public GameObject MemberOverworldVisualPrefab;
}
