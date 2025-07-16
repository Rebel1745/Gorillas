using UnityEngine;

public interface IProjectile
{
    public void SetProjectileExplosionMaskParent(Transform explosionMaskParent);
    public void SetExplosionSizeMultiplier(float multiplier);
    public void SetLastProjectileInBurst();
    public void SetProjectileNumber(int number);
}
