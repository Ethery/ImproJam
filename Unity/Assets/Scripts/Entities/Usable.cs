using UnityEngine;

public abstract class Usable : MonoBehaviour
{
    public abstract void Use(Human user);
    public abstract void StopUse(Human user);
}
