using System;
using UnityEngine;

public class Banana : MonoBehaviour, IProjectile
{
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private LayerMask _whatIsPlayer;
    [SerializeField] private GameObject _explosionSpriteMask;
    private float _explosionRadius;
    private Transform _explosionTransform;

    private void Start()
    {
        _explosionRadius = _explosionSpriteMask.transform.localScale.x / 2;
    }

    void Update()
    {
        CheckForGroundHit();
    }

    private void CheckForGroundHit()
    {
        bool createExplosionMask = true;

        // check if we hit a player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsPlayer);
        Collider2D[] hits;

        if (hit)
        {
            // we directly hit a player!!
            Destroy(hit.transform.gameObject);
            CreateExplosionAndDestroy();
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
                    // the explosion hit a player!
                    Destroy(hits[0].gameObject);
                    CreateExplosionAndDestroy();
                }
                else
                {
                    // check to see if there are any explosion masks already at the hit point
                    foreach (var h in Physics2D.OverlapPointAll(transform.position))
                    {
                        // if there is, bail
                        if (h.CompareTag("ExplosionMask")) createExplosionMask = false;
                    }

                    if (createExplosionMask) CreateExplosionAndDestroy();
                }
            }
        }
    }

    private void CreateExplosionAndDestroy()
    {
        Instantiate(_explosionSpriteMask, transform.position, Quaternion.identity, _explosionTransform);
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
