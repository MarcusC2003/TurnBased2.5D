using UnityEngine;
//accesses everything for scenes
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //serialized field is used to access through inspector
    //without other scrips being able to access them
    [SerializeField] private int speed;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private int stepsInGrass;
    [SerializeField] private int minStepsToEncounter;
    [SerializeField] private int maxStepsToEncounter;
    


    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;
    private bool movingInGrass;
    private float stepTimer;
    private int stepsToEncounter;
    private PartyManager partyManager;
    private Vector3 scale;

    //reference to animator
    private const string IS_WALK_PARAM = "IsWalk";
    private const string BATTLE_SCENE = "BattleScene";
    private const float TIME_PER_STEP = 0.5f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        CalculateStepsToNextEncounter();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable(){
        playerControls.Disable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        //if we have position saved --> move player
        if(partyManager.GetPosition() != Vector3.zero){
            transform.position = partyManager.GetPosition();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;

        //z since its 3D but uses y because the controls are vector2
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        movement = new Vector3(x, 0, z).normalized;

        anim.SetBool(IS_WALK_PARAM, movement != Vector3.zero);

        if (x != 0 && x < 0)
        {
            // playerSprite.flipX = true;
            //using scale instead of flip because it doesnt work with custom shaders
            playerSprite.transform.localScale = new Vector3(-scale.x,scale.y,scale.z);
        }
        if (x != 0 && x > 0)
        {
            // playerSprite.flipX = false;
            playerSprite.transform.localScale = new Vector3(scale.x,scale.y,scale.z);

        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);

        //overlap sphere --> more consistent than triggers
        //overlapShpere(position,radius,layer)
        Collider[] ColliderErrorState2D = Physics.OverlapSphere(transform.position, 1, grassLayer);
        //check if character is in grass and  if character is moving in grass
        movingInGrass = ColliderErrorState2D.Length != 0 && movement != Vector3.zero;

        //code block : counts steps in grass, simpler than using animation to count steps
        if (movingInGrass == true)
        {
            stepTimer += Time.fixedDeltaTime;
            if (stepTimer > TIME_PER_STEP)
            {
                stepsInGrass++;
                stepTimer = 0;

                if (stepsInGrass >= stepsToEncounter)
                {
                    partyManager.SetPosition(transform.position);
                    SceneManager.LoadScene(BATTLE_SCENE);
                }
            }
        }
    }

    private void CalculateStepsToNextEncounter()
    {
        stepsToEncounter = Random.Range(minStepsToEncounter, maxStepsToEncounter);
    }

    public void SetOverworldVisuals(Animator animator, SpriteRenderer spriteRenderer,Vector3 playerScale){
        anim = animator;
        playerSprite = spriteRenderer;
        scale = playerScale;
    }
}
