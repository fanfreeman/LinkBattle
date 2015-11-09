using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraEffects : MonoBehaviour {

    public static CameraEffects instance = null;
    public GameObject damageDisplayPrefab; // 伤害数字

    // camera的transform
    Transform camTransform;

    // 持续时间
    float duration;

    // 颤抖强度
    float magnitude;

    // 减弱速度
    float dampening;

    Vector3 originalPos;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        if (camTransform == null)
        {
            camTransform = GetComponent<Camera>().transform;
            originalPos = camTransform.localPosition;
        }
    }

    void Update()
    {
        if (duration > 0 && magnitude > 0)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * magnitude;

            magnitude -= Time.deltaTime * dampening;
            duration -= Time.deltaTime;
        }
        else // 颤抖结束
        {
            //if (duration <= 0) Debug.Log("camera shake: duration ran out");
            //if (magnitude <= 0) Debug.Log("camera shake: magnitude ran out");
            camTransform.localPosition = originalPos;
            enabled = false;
        }
    }

    public void Shake(float magnitudeVal = 0.2f, float durationVal = 0.3f, float dampeningVal = 0.5f)
    {
        //Debug.Log("shaking");
        duration = durationVal;
        magnitude = magnitudeVal;
        dampening = dampeningVal;

        enabled = true;
    }

    public void CreateDamagePopup(float damage, Vector3 position)
    {
        GameObject damageGameObject = (GameObject)Instantiate(damageDisplayPrefab, position, Quaternion.identity);
        damageGameObject.GetComponentInChildren<Text>().text = Mathf.Round(damage).ToString();
    }
}
