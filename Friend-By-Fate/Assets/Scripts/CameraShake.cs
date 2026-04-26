using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Transform cameraToShake;

    public float shakeDuration = 0f;
    public float shakeMagnitude = 0.1f;
    private Vector3 originalPosition;

    void Start()
    {
        if (cameraToShake == null)
        {
            if (Camera.main != null)
                cameraToShake = Camera.main.transform;
            else
                cameraToShake = transform;
        }

        originalPosition = cameraToShake.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0f)
        {
            cameraToShake.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;
            cameraToShake.localPosition = originalPosition;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}