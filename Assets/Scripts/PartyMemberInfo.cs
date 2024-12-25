using UnityEngine;

[CreateAssetMenu(menuName = "New Party Member")]
public class PartyMemberInfo : ScriptableObject
{
    public string MemberName;
    public int StartingLevel;
    public int BaseHealth;
    public int BaseStr;
    public int BaseInitiative;
    public GameObject MemberBattleVisualPrefab; //what wil be displayed in battle
    public GameObject MemberOverworldVisualPrefab;//what wil be displayed in the overworld scene
}
