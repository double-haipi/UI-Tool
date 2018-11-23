using UnityEngine;
using System.Collections;

public class DragInterupt : MonoBehaviour {
    public Transform trans;

    void OnDrag(Vector2 delta)
    {
        Vector3 originalPosition = trans.localPosition;
        Vector3 newPosition = new Vector3(originalPosition.x + delta.x, originalPosition.y + delta.y, originalPosition.z);
        trans.localPosition = newPosition;
        if (delta.x > 100 || delta.y > 100)
        {
            Debug.LogWarning("delta:" + delta.ToString());            
        }
        Debug.Log("delta:" + delta.ToString());            

    }
}
