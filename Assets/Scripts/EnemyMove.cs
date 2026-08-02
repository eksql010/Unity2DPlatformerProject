using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator animator;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;

    public int nextMove;
    public int thinkInterval;
    public float platformCheckRangeRatio;
    public float dieJumpEffectPower;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        Invoke("Think", thinkInterval);
    }

    void FixedUpdate()
    {
        // Move
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);

        // Platform Check
        Vector2 frontVector = new Vector2(rigid.position.x + nextMove * platformCheckRangeRatio, rigid.position.y);
        Debug.DrawRay(frontVector, Vector3.down, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVector, Vector2.down, 1.0f, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            //  Debug.Log("³¶¶°·¯Áö!");
            nextMove *= -1;
            spriteRenderer.flipX = nextMove == 1;
            CancelInvoke();
            Invoke("Think", thinkInterval);
        }
    }

    void Think()
    {
        // Set Next Active
        nextMove = Random.Range(-1, 2);

        // Sprite Animation
        animator.SetInteger("WalkSpeed", nextMove);

        // Flip Sprite
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        // Recursive
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
        //  Debug.Log("Next Think Interval : " + nextThinkTime);
    }

    public void OnDamaged()
    {
        nextMove = 0;
        animator.SetInteger("WalkSpeed", nextMove);
        CancelInvoke();

        // Sprite Alpha
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        boxCollider.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * dieJumpEffectPower, ForceMode2D.Impulse);

        // Destroy
        Invoke("Deactive", 5);
    }

    void Deactive()
    {
        gameObject.SetActive(false);
    }
}
