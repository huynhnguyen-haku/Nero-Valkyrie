using System.Collections;
using UnityEngine;


public class ZoneLimitation : MonoBehaviour
{
    private ParticleSystem[] lines;
    private BoxCollider zoneCollider;

    #region Unity Methods

    private void Start()
    {
        // Hide mesh, cache collider and particle systems
        GetComponent<MeshRenderer>().enabled = false;
        zoneCollider = GetComponent<BoxCollider>();
        lines = GetComponentsInChildren<ParticleSystem>();
        ActivateWall(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Activate wall effect briefly when something enters the zone
        StartCoroutine(WallActivationCo());
        Debug.Log("My sensors are going crazy, I think it's dangerous!");
    }

    #endregion

    #region Wall Logic

    // Enable or disable the wall's visual and collider state
    private void ActivateWall(bool activate)
    {
        foreach (var line in lines)
        {
            if (activate)
                line.Play();
            else
                line.Stop();
        }
        zoneCollider.isTrigger = !activate;
    }

    // Coroutine to activate wall for a short duration
    private IEnumerator WallActivationCo()
    {
        ActivateWall(true);
        yield return new WaitForSeconds(1);
        ActivateWall(false);
    }

    #endregion
}
