using UnityEngine;

public class ActionLocks : MonoBehaviour
{
    int moveLock = 0;
    int attackLock = 0;

    public bool CanMove => moveLock <= 0;
    public bool CanAttack => attackLock <= 0;

    public void LockMove() => moveLock++;
    public void UnlockMove() => moveLock = Mathf.Max(0, moveLock - 1);

    public void LockAttack() => attackLock++;
    public void UnlockAttack() => attackLock = Mathf.Max(0, attackLock - 1);

    public void ClearAll()
    {
        moveLock = 0;
        attackLock = 0;
    }
}
