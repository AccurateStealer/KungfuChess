using UnityEngine;

public class PlayerState : MonoBehaviour
{
    int moveLock = 0;
    int attackLock = 0;
    int isInulnerable = 0;

    public bool CanMove => moveLock <= 0;
    public bool CanAttack => attackLock <= 0;
    public bool IsInulnerable => isInulnerable <= 0;
    

    public void LockMove() => moveLock++;
    public void UnlockMove() => moveLock = Mathf.Max(0, moveLock - 1);

    public void LockAttack() => attackLock++;
    public void UnlockAttack() => attackLock = Mathf.Max(0, attackLock - 1);

    public void GainInvulnerability() => isInulnerable++;
    public void LoseInvulnerability() => isInulnerable = Mathf.Max(0, isInulnerable - 1);


    public void ClearAll()
    {
        moveLock = 0;
        attackLock = 0;
        isInulnerable = 0;
    }
}
