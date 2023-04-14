using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPlacer : MonoBehaviour
{
    [SerializeField] private Transform root;
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = root.position + new Vector3(0, -.08f, 0);
        playerInput.transform.localScale = new Vector3(.65f, .65f, .65f);
    }
}
