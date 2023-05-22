using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public const string Tag = "Player";

    public float Speed = 10;
    public CharacterController CharacterController;

    private void Awake()
    {
        Assert.IsNotNull(CharacterController);
    }

    private void FixedUpdate()
    {
        var direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            direction += Vector3.forward;
        if (Input.GetKey(KeyCode.A))
            direction += Vector3.left;
        if (Input.GetKey(KeyCode.S))
            direction += Vector3.back;
        if (Input.GetKey(KeyCode.D))
            direction += Vector3.right;

        CharacterController.SimpleMove(Speed * Time.deltaTime * direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EnemyStateMachine.Tag))
            Destroy(gameObject);
    }
}
