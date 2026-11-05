using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    private Vector3 velocity;

    public void SetTarget(Transform value)
    {
        target = value;
        velocity = Vector3.zero;
        if (target != null) transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = smoothTime <= 0f ? desired :
            Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, desired.z);
    }
}
