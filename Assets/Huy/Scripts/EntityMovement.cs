using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    public float Speed = 1f;
    public Vector2 Direction = Vector2.left;

    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    private void OnBecameVisible()
    {
        enabled = true;
    }

    private void OnBecameInvisible()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        _rigidbody.WakeUp();
    }

    private void OnDisable()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.Sleep();
    }

    private void FixedUpdate()
    {
        _velocity.x = Direction.x * Speed;
        _velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);

        if (_rigidbody.Raycast(Direction)) 
            Direction = -Direction;

        if (_rigidbody.Raycast(Vector2.down))
            _velocity.y = Mathf.Max(_velocity.y, 0f);
    }
}
