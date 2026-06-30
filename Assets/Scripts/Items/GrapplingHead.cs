using UnityEngine;

public class GrapplingHead : MonoBehaviour
{
    public GrapplingHook parentHook;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (parentHook != null)
        {
            parentHook.HandleCollision(other);
        }
    }
}