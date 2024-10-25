using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class ShopTrigger : MonoBehaviour
{

    [SerializeField]
    private GameObject shopUI;
    public bool shopIsOpen { get; private set; }
    public bool canTriggerShop = false;

    // get character and camera to disable when shop is shown;
    private PlayerController playerController;
    private CinemachineFreeLook freeLookCamera;



    // Start is called before the first frame update
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && canTriggerShop)
        {
           OpenShop();
        }
    }

     void Start()
    {
        shopUI.SetActive(false);
        playerController = FindObjectOfType<PlayerController>();
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();

    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        // Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        shopIsOpen = true;

        if (playerController != null)
        {
            playerController.enabled = false; // Disable player movement
        }

        if (freeLookCamera != null)
        {
            freeLookCamera.enabled = false; // Disable the CinemachineFreeLook component
        }
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        shopIsOpen = false;

        if (playerController != null)
        {
            playerController.enabled = true; // Re-enable player movement
        }

        if (freeLookCamera != null)
        {
            freeLookCamera.enabled = true; // Re-enable the CinemachineFreeLook component
        }
    }
}