using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public GameObject Player;
    public Transform PlayerTransform;
    void Awake()
    {
        Player = null;
        StartCoroutine("FindPlayer");
    }

    IEnumerator FindPlayer()
    {
        while (Player == null)
        {
            Player = GameObject.FindWithTag("Player");

            yield return null;
        }

        PlayerTransform = Player.transform;
        StartCoroutine("FollowPlayer");
    }
    
    IEnumerator FollowPlayer()
    {
        while (true)
        {
            transform.position = PlayerTransform.position;

            yield return null;
        }
    }
}
