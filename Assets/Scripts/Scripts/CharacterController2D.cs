using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes

public class CharacterController2D : MonoBehaviour {

	// player controls
	[Range(0.0f, 10.0f)]
	public float moveSpeed = 3f;
	public float jumpForce = 600f;

	public int maxJumps = 2; // 1 = normal jump, 2 = double jump
	private int jumpCount = 0;

	// player health
	public int playerHealth = 1;

	public LayerMask whatIsGround;
	public Transform groundCheck;

	[HideInInspector]
	public bool playerCanMove = true;

	// SFXs
	public AudioClip coinSFX;
	public AudioClip deathSFX;
	public AudioClip fallSFX;
	public AudioClip jumpSFX;
	public AudioClip victorySFX;

	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;

	float _vx;
	float _vy;

	bool facingRight = true;
	bool isGrounded = false;
	bool isRunning = false;

	int _playerLayer;
	int _platformLayer;

	void Awake () {
		_transform = GetComponent<Transform> ();
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody == null)
			Debug.LogError("Rigidbody2D component missing from this gameobject");

		_animator = GetComponent<Animator>();
		if (_animator == null)
			Debug.LogError("Animator component missing from this gameobject");

		_audio = GetComponent<AudioSource> ();
		if (_audio == null) {
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			_audio = gameObject.AddComponent<AudioSource>();
		}

		_playerLayer = this.gameObject.layer;
		_platformLayer = LayerMask.NameToLayer("Platform");
	}

	void Update()
	{
		if (!playerCanMove || (Time.timeScale == 0f))
			return;

		_vx = Input.GetAxisRaw("Horizontal");
		isRunning = (_vx != 0);
		_animator.SetBool("Running", isRunning);

		isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);
		_animator.SetBool("Grounded", isGrounded);

		// Reset jump count when grounded
		if (isGrounded)
		{
			jumpCount = 0;
		}

		// Jump or double jump
		if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
		{
			Vector2 velocity = _rigidbody.velocity;
			velocity.y = 0; // Cancel upward/downward motion before jump
			_rigidbody.velocity = velocity;
			_rigidbody.AddForce(new Vector2(0, jumpForce));
			PlaySound(jumpSFX);
			jumpCount++;
		}

		// Cancel jump if player lets go early
		if (Input.GetButtonUp("Jump") && _rigidbody.velocity.y > 0f)
		{
			_rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f);
		}

		// Apply horizontal motion only
		_rigidbody.velocity = new Vector2(_vx * moveSpeed, _rigidbody.velocity.y);

		// Allow jumping through platforms
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_rigidbody.velocity.y > 0.0f));
	}


	void LateUpdate()
	{
		Vector3 localScale = _transform.localScale;

		if (_vx > 0)
			facingRight = true;
		else if (_vx < 0)
			facingRight = false;

		if (((facingRight) && (localScale.x < 0)) || ((!facingRight) && (localScale.x > 0))) {
			localScale.x *= -1;
		}

		_transform.localScale = localScale;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "MovingPlatform")
		{
			this.transform.parent = other.transform;
		}
	}

	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.tag == "MovingPlatform")
		{
			this.transform.parent = null;
		}
	}

	void FreezeMotion() {
		playerCanMove = false;
		_rigidbody.velocity = Vector2.zero;
		_rigidbody.isKinematic = true;
	}

	void UnFreezeMotion() {
		playerCanMove = true;
		_rigidbody.isKinematic = false;
	}

	void PlaySound(AudioClip clip)
	{
		_audio.PlayOneShot(clip);
	}

	public void ApplyDamage(int damage) {
		if (playerCanMove) {
			playerHealth -= damage;

			if (playerHealth <= 0) {
				PlaySound(deathSFX);
				StartCoroutine(KillPlayer());
			}
		}
	}

	public void FallDeath() {
		if (playerCanMove) {
			playerHealth = 0;
			PlaySound(fallSFX);
			StartCoroutine(KillPlayer());
		}
	}

	IEnumerator KillPlayer()
	{
		if (playerCanMove)
		{
			FreezeMotion();
			_animator.SetTrigger("Death");
			yield return new WaitForSeconds(2.0f);

			if (GameManager.gm)
				GameManager.gm.ResetGame();
			else
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	public void CollectCoin(int amount) {
		PlaySound(coinSFX);
		if (GameManager.gm)
			GameManager.gm.AddPoints(amount);
	}

	public void Victory() {
		PlaySound(victorySFX);
		FreezeMotion();
		_animator.SetTrigger("Victory");

		if (GameManager.gm)
			GameManager.gm.LevelCompete();
	}

	public void Respawn(Vector3 spawnloc) {
		UnFreezeMotion();
		playerHealth = 1;
		_transform.parent = null;
		_transform.position = spawnloc;
		_animator.SetTrigger("Respawn");
	}
}
