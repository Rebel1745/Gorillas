using System;
using UnityEngine;

public class Banana : MonoBehaviour, IProjectile
{
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private GameObject _explosionSpriteMask;
    [SerializeField] private Transform _explosionTransform;

    void Update()
    {
        CheckForGroundHit();
    }

    private void CheckForGroundHit()
    {
        bool createExplosionMask = true;

        if (Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsGround))
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

    private void CreateExplosionAndDestroy()
    {
        Instantiate(_explosionSpriteMask, transform.position, Quaternion.identity, _explosionTransform);
        Destroy(gameObject);
    }

    public void SetProjectileExplosionMaskParent(Transform explosionMaskParent)
    {
        _explosionTransform = explosionMaskParent;
    }
}
