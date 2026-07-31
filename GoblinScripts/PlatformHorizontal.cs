using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatformHorizontal : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 3f;

    private Rigidbody2D rb;
    private float startX;
    private Vector2 lastPosition;
    private Vector2 _velocity;

    public Vector2 Velocity => _velocity;  

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startX = transform.position.x;
        lastPosition = rb.position;
    }

    private void FixedUpdate()
    {
        float newX = startX + Mathf.PingPong(Time.time * speed, distance) - distance / 2f;
        Vector2 targetPos = new Vector2(newX, rb.position.y);
        rb.MovePosition(targetPos);

        _velocity = (rb.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = rb.position;
    }
}