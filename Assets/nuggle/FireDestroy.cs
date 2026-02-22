using UnityEngine;

public class FireDestroy : MonoBehaviour
{
    public float shrinkSpeed = 0.5f;
    public float minScale = 0.4f;
    private bool isShrinking = false;

    void Update()
    {
        if (isShrinking)
        {
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

            if (transform.localScale.x <= minScale)
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void SetisShrinking(bool Within)
    {
        isShrinking = Within;
    }
}
