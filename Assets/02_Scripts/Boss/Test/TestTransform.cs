using UnityEngine;

public class TestTransform : MonoBehaviour
{
    public Transform target;

    private void Start()
    {
        transform.position = target.position + target.forward * 1f + target.right * 1f;
        transform.rotation = target.localRotation;
    }
}
