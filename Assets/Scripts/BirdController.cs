using UnityEngine;

public class BirdController : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // On collision with the player, trigger death for the player
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharacterController2D player = other.gameObject.GetComponent<CharacterController2D>();
            if (player != null)
            {
                player.FallDeath(); // Call the FallDeath function in CharacterController2D
            }
        }
    }

    // Update can be removed if no manual input is needed
    void Update()
    {
        // No need to manually control the animation anymore
    }
}
