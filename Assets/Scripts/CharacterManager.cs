using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject joinPopup;
    [SerializeField] private TextMeshProUGUI joinPopupText;

    private bool infrontOfPartyMember;
    private GameObject joinableMember;
    private PlayerControls playerControls;
    private List<GameObject> overworldCharacters = new List<GameObject>();


    private const string NPC_JOINABLE_TAG = "NPCJoinable";
    private const string PARTY_JOINED_MESSAGE = " has joined the party.";

    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    void Start()
    {
        //subscribing to function
        //+= : delegate function to event
        //_=> : parameters passed
        playerControls.Player.Interact.performed += _ => Interact();
        SpawnOverworldMembers();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Interact()
    {
        if (infrontOfPartyMember == true && joinableMember != null)
        {
            //add member
            MemberJoined(joinableMember.GetComponent<JoinableCharacterScript>().MemberToJoin);
            infrontOfPartyMember = false;
            //prevent spamming interact
            joinableMember = null;
        }
    }
    private void MemberJoined(PartyMemberInfo partyMember)
    {
        //add party member
        GameObject.FindFirstObjectByType<PartyManager>().AddMemberToPartyByName(partyMember.MemberName);
        //disable joinable member
        joinableMember.GetComponent<JoinableCharacterScript>().CheckIfJoined();
        //join pop up
        joinPopup.SetActive(true);
        joinPopupText.text = partyMember.MemberName + PARTY_JOINED_MESSAGE;
        //add overworld member
        SpawnOverworldMembers();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void SpawnOverworldMembers()
    {
        for (int i = 0; i < overworldCharacters.Count; i++)
        {
            Destroy(overworldCharacters[i]);
        }
        overworldCharacters.Clear();
        List<PartyMember> currentParty = GameObject.FindFirstObjectByType<PartyManager>().GetCurrentParty();

        for (int i = 0; i < currentParty.Count; i++)
        {

            if (i == 0)
            { //first member
                GameObject player = gameObject; // get player
                GameObject playerVisual = Instantiate(currentParty[i].MemberOverworldVisualPrefab, player.transform.position, Quaternion.identity);
                
                playerVisual.transform.SetParent(player.transform);
                player.GetComponent<PlayerController>().SetOverworldVisuals(playerVisual.GetComponent<Animator>(),playerVisual.GetComponent<SpriteRenderer>(), playerVisual.transform.localScale);
                playerVisual.GetComponent<MemberFollowAI>().enabled = false;
                overworldCharacters.Add(playerVisual);
            }
            else
            { // other follower
                Vector3 positionToSpawn = transform.position;
                positionToSpawn.x -=i; 
                GameObject tempFollower = Instantiate(currentParty[i].MemberOverworldVisualPrefab, positionToSpawn, Quaternion.identity);
                
                //ai settings
                tempFollower.GetComponent<MemberFollowAI>().SetFollowDistance(i);
                overworldCharacters.Add(tempFollower);
            }   
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            //enable prompt
            infrontOfPartyMember = true;
            joinableMember = other.gameObject;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            infrontOfPartyMember = true;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(false);
            joinableMember = null;
        }
    }
}
