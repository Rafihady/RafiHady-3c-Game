using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource FootstepSFX;
    [SerializeField] AudioSource LandingSFX;
    [SerializeField] AudioSource PunchSFX;
    [SerializeField] AudioSource GlideSFX;

    private void playFootstepSFX()
    {
        FootstepSFX.volume = Random.Range(0.8f, 1f);
        FootstepSFX.pitch = Random.Range(0.5f, 2.5f);
        FootstepSFX.Play();
    }

    private void PlayLandingSFX()
    {
        LandingSFX.Play();
    }

    private void PlayPunchSFX()
    {
        PunchSFX.volume = Random.Range(0.8f, 1f);
        PunchSFX.pitch = Random.Range(-.8f, 1f);
        PunchSFX.Play();
    }
    
    public void PlayGlidingSFX ()
    {
        GlideSFX.Play();
    }

    public void StopGlidingSFX ()
    {
        GlideSFX.Stop();
    }
}
