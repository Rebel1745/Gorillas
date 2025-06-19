using System;
using UnityEngine;

public class Banana : MonoBehaviour, IProjectile
{
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private LayerMask _whatIsPlayer;
    [SerializeField] private LayerMask _whatIsWindow;
    [SerializeField] private GameObject _explosionSpriteMask;
    [SerializeField] private GameObject[] _brokenWindowSprites;
    [SerializeField] private float _explosionRadiusDamageMultiplier = 2;
    [SerializeField] private float _destroyWhenDistanceOffscreen = -20f;
    private float _explosionRadius;
    private Transform _explosionTransform;
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _explosionRadius = _explosionSpriteMask.transform.localScale.x / 2;
    }

    void Update()
    {
        // if we are moving down, change the zoom
        if (_rb.linearVelocityY < 0)
        {
            CameraManager.Instance.SetProjectileZenith(transform.position);
            CameraManager.Instance.UpdateCameraForProjectile();
        }

        CheckForGroundHit();

        // if the banana goes too far offscreen, destroy it
        if (transform.position.y < _destroyWhenDistanceOffscreen) CreateExplosionAndDestroy();
    }

    private void CheckForGroundHit()
    {
        bool createExplosionMask = true;

        // check if we hit a player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsPlayer);
        Collider2D[] hits;

        if (hit)
        {
            CameraManager.Instance.RemovePlayer(hit.transform.position);
            int playerHitId = hit.transform.GetComponent<PlayerController>().PlayerId;
            int otherPlayerId = (playerHitId + 1) % 2;
            PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");
            // we directly hit a player!!
            Destroy(hit.transform.gameObject);
            CreateExplosionAndDestroy();

            // Game over?
            GameManager.Instance.UpdateGameState(GameState.GameOver);
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
                    CameraManager.Instance.RemovePlayer(hits[0].transform.position);
                    int playerHitId = hits[0].transform.GetComponent<PlayerController>().PlayerId;
                    int otherPlayerId = (playerHitId + 1) % 2;
                    PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");
                    // the explosion hit a player!
                    CameraManager.Instance.RemovePlayer(hits[0].transform.position);
                    Destroy(hits[0].gameObject);
                    CreateExplosionAndDestroy();

                    // Game over?
                    GameManager.Instance.UpdateGameState(GameState.GameOver);
                }
                else
                {
                    // check to see if there are any explosion masks already at the hit point
                    foreach (var h in Physics2D.OverlapPointAll(transform.position))
                    {
                        // if there is, bail
                        if (h.CompareTag("ExplosionMask")) createExplosionMask = false;
                    }

                    if (createExplosionMask)
                    {
                        CreateExplosionAndDestroy();

                        // Next Players turn
                        GameManager.Instance.UpdateGameState(GameState.NextTurn);
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
            GameObject randomSprite = _brokenWindowSprites[UnityEngine.Random.Range(0, _brokenWindowSprites.Length)];
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
            Instantiate(randomSprite, h.transform.position, randomRotation, h.transform);
        }
        CameraManager.Instance.RemoveProjectile();
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
