using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private int currHealth;
    private int maxHealth;
    private int level;

    private Animator anim;

    private const string LEVEL_ABR = "Lvl: ";

    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEAD_PARAM = "IsDead";

    //Cannot be start because the BattleSystem tries to play the animation before it is set
    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
    }
    public void SetStartingValues(int currHealth, int maxHealth, int level)
    {
        this.currHealth = currHealth;
        this.maxHealth = maxHealth;
        this.level = level;
        //Bad Practice
        //levelText.text = "Lvl: " + this.level.ToString();
        levelText.text = LEVEL_ABR + this.level.ToString();
        UpdateHealthBar();
    }
    public void ChangeHealth(int currHealth){
        this.currHealth = currHealth;
        if(currHealth <= 0){
            PlayDeadAnimation();
            //1f --> delay
            Destroy(gameObject,1f);
        }
        UpdateHealthBar();
    }

    public void UpdateHealthBar(){
        healthBar.maxValue = maxHealth;
        healthBar.value = currHealth;
    }

    public void PlayAttackAnimation(){
        anim.SetTrigger(IS_ATTACK_PARAM);
    }
    public void PlayHitAnimation(){
        anim.SetTrigger(IS_HIT_PARAM);
    }
    public void PlayDeadAnimation(){
        anim.SetTrigger(IS_DEAD_PARAM);
    }
}
