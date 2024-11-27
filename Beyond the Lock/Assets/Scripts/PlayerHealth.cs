using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public Image healthBar;
    public float health = 100f;

    // Start is called before the first frame update

    public void TakeDamage(float damage)
    {
        health -= damage; // Reduce health

        healthBar.fillAmount = health / 100f;

        if (health <= 0)
        {
            Die(); 
        }
    }

    private void Die()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("DeathScreen");
    }
}
