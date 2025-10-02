using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrigger : MonoBehaviour
{
    public GameObject EnterText;
    //public GameObject EnterButton;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EnterText.SetActive(true);
            //EnterButton.SetActive(true);
            //CarEnterExit.outsideCar = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EnterText.SetActive(false);
            //EnterButton.SetActive(false);
            //CarEnterExit.outsideCar = false;
        }
    }
}