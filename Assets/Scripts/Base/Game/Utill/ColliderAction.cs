using UnityEngine;

public class ColliderAction : MonoBehaviour
{
    [HideInInspector]
    public System.Action<Collider2D> TriggerEnterAction = null;

    [HideInInspector]
    public System.Action<Collider2D> TriggerExitAction = null;

    public System.Action AttackAction = null;

     public void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnterAction?.Invoke(collision);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        TriggerExitAction?.Invoke(collision);
    }


    public void AttackEvent()
    {
        AttackAction?.Invoke();
    }
}
