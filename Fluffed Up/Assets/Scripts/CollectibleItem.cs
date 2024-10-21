using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemName; // Name or type of the item
    public int totalAmount; // the total amount, or quantity * value. Typically only used by currency.
    public int quantity; // quantity of the item: For example, 1 Health pack.
    public int value; // Value of item: For example, if health then gain 50 health.

    private void setItemData()
    {
        if (gameObject.name == "Heal_Up" || gameObject.name == "Heal_Up(Clone)")
        {
            itemName = "Health";
            quantity = 1;
            value = 50;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Bullet_Up" || gameObject.name == "Bullet_Up(Clone)")
        {
            itemName = "Bullet";
            quantity = 1; 
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Coin_Up" || gameObject.name == "Coin_Up(Clone)")
        {
            itemName = "Coin";
            quantity = 1; 
            value = 25;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Exclamation_Up" || gameObject.name == "Exclamation_Up(Clone)")
        {
            itemName = "Exclamation";
            quantity = 1; 
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Heart_Up" || gameObject.name == "Heart_Up(Clone)")
        {
            itemName = "Heart";
            quantity = 1;
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Power_Down" || gameObject.name == "Power_Down(Clone)")
        {
            itemName = "PowerDown";
            quantity = 1; 
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Power_Up" || gameObject.name == "Power_Up(Clone)")
        {
            itemName = "PowerUp";
            quantity = 1; 
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "PowerUp_Sphere" || gameObject.name == "PowerUp_Sphere(Clone)")
        {
            itemName = "Sphere";
            quantity = 1;
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Question_Up" || gameObject.name == "Question_Up(Clone)")
        {
            itemName = "Question";
            quantity = 1;
            value = 0;
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Rocket_Up" || gameObject.name == "Rocket_Up(Clone)")
        {
            itemName = "Rocket";
            quantity = 1;
            value = 0;  
            totalAmount = quantity * value;
        }
        else if (gameObject.name == "Star_Up" || gameObject.name == "Star_Up(Clone)")
        {
            itemName = "Star";
            quantity = 1;
            value = 0;    
            totalAmount = quantity * value;
        }
        else
        {
            Debug.Log("Unknown dropped item: " + gameObject.name);
        }
        // Add more conditions for other item types as needed
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                setItemData();
                // Collect the item
                player.CollectItem(this);
                Destroy(gameObject); // Destroy the item after collection
            }
        }
    }
}
