using UnityEngine;

public class Banana : MonoBehaviour, IProjectile
{
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private LayerMask _whatIsPlayer;
    [SerializeField] private LayerMask _whatIsPowerup;
    [SerializeField] private LayerMask _whatIsWindow;
    [SerializeField] private GameObject _explosionSpriteMask;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject[] _brokenWindowSprites;
    [SerializeField] private float _explosionRadiusDamageMultiplier = 2;
    [SerializeField] private float _destroyWhenDistanceOffscreen = -20f;
    [SerializeField] private float _rotationRate = 1f;
    [SerializeField] private AudioClip _explosionSFX;
    private float _explosionRadius;
    private Transform _explosionTransform;
    private Rigidbody2D _rb;
    private bool _createExplosionMask;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _explosionRadius = _explosionSpriteMask.transform.localScale.x / 2;
    }

    void Update()
    {
        CheckForGroundHit();

        // if the banana goes too far offscreen, destroy it
        if (transform.position.y < _destroyWhenDistanceOffscreen)
            CreateExplosionAndDestroy();

        if (!_createExplosionMask && _rb.linearVelocityY < 0)
        {
            // if we are moving down, change the zoom
            CameraManager.Instance.SetProjectileZenith(transform.position);
            CameraManager.Instance.UpdateCameraForProjectile();
        }

        transform.Rotate(0, 0, -_rotationRate * Time.deltaTime);
    }

    private void CheckForGroundHit()
    {
        _createExplosionMask = true;
        int playerHitId, otherPlayerId;

        // check if we hit a powerup first
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsPowerup);

        if (hit)
        {
            // we hit a powerup
            PlayerManager.Instance.AddRandomPlayerPowerup();
            Destroy(hit.collider.gameObject);
        }

        // check if we hit a player
        hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsPlayer);
        Collider2D[] hits;

        if (hit)
        {
            playerHitId = hit.transform.GetComponent<PlayerController>().PlayerId;
            otherPlayerId = (playerHitId + 1) % 2;
            CreateExplosionAndDestroy();
            CameraManager.Instance.RemovePlayer(playerHitId);
            GameManager.Instance.UpdateScore(otherPlayerId);
            PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");
            // we directly hit a player!!
            //Destroy(hit.transform.gameObject);
            PlayerManager.Instance.Players[playerHitId].PlayerController.DestroyPlayer();

            // Game over?
            GameManager.Instance.UpdateGameState(GameState.RoundComplete);
        }
        else
        {
            hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsGround);

            if (hit)
            {
                // we hit the ground, did the explosion hit a player?
                hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _whatIsPlayer);
                if (hits.Length > 0)
                {
                    playerHitId = hits[0].transform.GetComponent<PlayerController>().PlayerId;
                    otherPlayerId = (playerHitId + 1) % 2;
                    PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");
                    // the explosion hit a player!
                    CreateExplosionAndDestroy();
                    CameraManager.Instance.RemovePlayer(playerHitId);
                    GameManager.Instance.UpdateScore(otherPlayerId);
                    //Destroy(hits[0].gameObject);
                    PlayerManager.Instance.Players[playerHitId].PlayerController.DestroyPlayer();

                    // Game over?
                    GameManager.Instance.UpdateGameState(GameState.RoundComplete);
                }
                else
                {
                    // check to see if there are any explosion masks already at the hit point
                    foreach (var h in Physics2D.OverlapPointAll(transform.position))
                    {
                        // if there is, bail
                        if (h.CompareTag("ExplosionMask")) _createExplosionMask = false;
                    }

                    if (_createExplosionMask)
                    {
                        CreateExplosionAndDestroy();

                        // Next Players turn
                        GameManager.Instance.UpdateGameState(GameState.NextTurn, 1f);
                    }
                }
            }
        }
    }

    private void CreateExplosionAndDestroy()
    {
        // create the explosion crater with a mask
        Instantiate(_explosionSpriteMask, transform.position, Quaternion.identity, _explosionTransform);
        // find all of the windows in the blast radius (with multiplier)
        foreach (var h in Physics2D.OverlapCircleAll(transform.position, _explosionRadius * _explosionRadiusDamageMultiplier, _whatIsWindow))
        {
            GameObject randomSprite = _brokenWindowSprites[Random.Range(0, _brokenWindowSprites.Length)];
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            Instantiate(randomSprite, h.transform.position, randomRotation, h.transform);
        }
        CameraManager.Instance.RemoveProjectile();
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayAudioClip(_explosionSFX, 0.95f, 1.05f);

        // set the x position of the landing area for the AI to use
        PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.LastProjectileLandingPositionX = transform.position.x;

        Destroy(gameObject);
    }

    public void SetProjectileExplosionMaskParent(Transform explosionMaskParent)
    {
        _explosionTransform = explosionMaskParent;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
