using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthManager : MonoBehaviour
{

    public Image healthBar;
    public float healthAmount = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(healthAmount <= 0) {
            Application.LoadLevel(Application.loadedLevel);
        }

        if(Input.GetKeyDown(KeyCode.T)) {
            takeDamage(20);
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            heal(20);
        }
    }

    public void takeDamage(float damage) {
        healthAmount -= damage;
        healthBar.fillAmount = healthAmount / 100f;
    }

    public void heal(float healingAmount) {
        healthAmount += healingAmount;
        healingAmount = Mathf.Clamp(healingAmount, 0 , 100);

        healthBar.fillAmount = healthAmount / 100f;
    }
}
