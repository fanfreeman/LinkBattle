using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnitStatusController : MonoBehaviour {

    public GameObject healthBar;
    public GameObject countDown;
    public GameObject attackPower;

    RawImage imageHealthBar;
    Text textCountDown;
    Text textAttackPower;

    void Awake()
    {
        imageHealthBar = healthBar.GetComponent<RawImage>();

        textCountDown = countDown.GetComponent<Text>();

        textAttackPower = attackPower.GetComponent<Text>();
    }

    public void SetHealthScale(float scaleFactor)
    {
        imageHealthBar.enabled = true;
        Vector3 scale = imageHealthBar.transform.localScale;
        scale.x = scaleFactor;
        imageHealthBar.transform.localScale = scale;
        //healthBarImage.color = Color.Lerp(healthBarMinColor, healthBarMaxColor, scaleFactor);
    }

    public void HideHealth()
    {
        imageHealthBar.enabled = false;
    }

    public void SetCountDown(int turnsLeft)
    {
        textCountDown.text = turnsLeft.ToString();
    }

    public void SetAttackPower(float attackPower)
    {
        textAttackPower.text = Mathf.Round(attackPower).ToString();
    }
}
