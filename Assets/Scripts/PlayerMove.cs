using UnityEditor.Build;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    public float stopSpeed;
    public float jumpPower;
    public float randingDistance;
    public float knockbackPower;
    public int invincibleDuration;
    public float attackReactionJumpPower;
    public float dieJumpEffectPower;

    public GameManager gameManager;

    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator animator;
    BoxCollider2D boxCollider;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Jump
        if(Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);

            // Sound
            PlaySound("JUMP");
        }

        // Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        // Direction Sprite
        if(Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1f;
        }

        // Walk <-> Idle Animation Transition
        if(Mathf.Abs(rigid.linearVelocity.x) < stopSpeed)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        // Move Speed
        float horizontal = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * horizontal, ForceMode2D.Impulse);
    
        // Max Speed
        if(rigid.linearVelocity.x > maxSpeed) // Right Max Speed
        {
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        }
        else if(rigid.linearVelocity.x < maxSpeed * (-1f)) // Left Max Speed
        {
            rigid.linearVelocity = new Vector2(maxSpeed * (-1f), rigid.linearVelocity.y);
        }

        // Landing Platform
        if(rigid.linearVelocity.y < 0f)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector2.down, 1.0f, LayerMask.GetMask("Platform"));
            
            if (rayHit.collider != null)
            {
                if (rayHit.distance < randingDistance)
                {
                    //  Debug.Log(rayHit.collider.name);
                    animator.SetBool("isJumping", false);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack -> 플레이어가 낙하 중 + 몬스터보다 위에 있음
            if(rigid.linearVelocity.y < 0f && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);

                // Sound
                PlaySound("ATTACK");
            }
            // Damaged
            else
            {
                //  Debug.Log("플레이어가 맞았습니다!");
                OnDamaged(collision.transform.position);

                // Sound
                PlaySound("DAMAGED");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // Score
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
            {
                gameManager.stageScore += 50;
            }
            else if (isSilver)
            {
                gameManager.stageScore += 100;
            }
            else if (isGold)
            {
                gameManager.stageScore += 300;
            }

            // Deactive Item
            collision.gameObject.SetActive(false);

            // Sound
            PlaySound("ITEM");
        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();

            // Sound
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // Score
        gameManager.stageScore += 100;

        // Reaction Force
        rigid.AddForce(Vector2.up * attackReactionJumpPower, ForceMode2D.Impulse);

        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        // Decrease Health
        gameManager.DecreaseHealth();

        // Changed Layer
        gameObject.layer = 9;

        // View Alpha
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);

        // Reaction Force
        float dir = transform.position.x - targetPos.x > 0f ? 1f : -1f;
        rigid.AddForce(new Vector2(dir, 1f) * knockbackPower, ForceMode2D.Impulse);

        // Set Animator Parameter
        animator.SetTrigger("doDamaged");

        Invoke("OffDamaged", invincibleDuration);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    public void OnDeath()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        boxCollider.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * dieJumpEffectPower, ForceMode2D.Impulse);

        // Sound
        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.linearVelocity = Vector2.zero;
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }
        
        audioSource.Play();
    }
}
