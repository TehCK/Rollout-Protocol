using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FootStepSoundControl : MonoBehaviour
{
    [Header("Audio Configuration")]
    public AudioSource SFXSource;

    [Header("Startup Sound")]
    public AudioClip startupSound;
    public float startupVolume;
    public string startupStateName;
    private int startupStateHash;
    private bool hasPlayedStartup = false;

    [Header("Walking Sound")]
    public AudioClip[] footstepSounds;
    public string walkingStateName;
    private int walkingStateHash;
    public float footstepVolume;
    public float stepInterval;

    [Header("Jump Sound")]
    private bool isLandingSound = false;

    [Header("Rolling Sound")]
    public AudioClip goToRollSounds;
    public float goToRollVolume;
    public AudioClip RollingSound;
    public float RollingVolume;
    public AudioClip exitRollSounds;
    public float exitRollVolume;
    public string goToRollStateName;
    private int goToRollStateHash;
    public string rollStateName;
    private int rollStateHash;
    public string exitRollStateName;
    private int exitRollStateHash;
    private bool hasPlayedGoToRollSound = false;
    private bool hasPlayedRollingSound = false;
    private bool isPlayingRollingSound = false;

    private PlayerMovement playerMovement;
    private Animator animator;
    private float lastStepTime;
    private bool wasWalking;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        walkingStateHash = Animator.StringToHash(walkingStateName);
        startupStateHash = Animator.StringToHash(startupStateName);
        goToRollStateHash = Animator.StringToHash(goToRollStateName);
        rollStateHash = Animator.StringToHash(rollStateName);
        exitRollStateHash = Animator.StringToHash(exitRollStateName);

        if (SFXSource == null)
        {
            SFXSource = gameObject.AddComponent<AudioSource>();
            SFXSource.playOnAwake = false;
            SFXSource.spatialBlend = 1f;
        }
    }

    private void Update()
    {
        if (playerMovement.rolling)
        {
            CheckPlayGoToRollSound();
            CheckPlayRollingSound();
        }
        else
        {
            CheckStartupAnimation();
            CheckPlayFootStepSound();
            CheckPlayJumpSound();
            CheckExitRollSound();
        }
    }

    private void CheckStartupAnimation()
    {
        bool isInStartupAnim = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == startupStateHash;

        // Play startup sound only once when entering the startup animation
        if (isInStartupAnim && !hasPlayedStartup)
        {
            PlayStartupSound();
            hasPlayedStartup = true;
        }
        // Reset the flag when not in startup animation
        else if (!isInStartupAnim)
        {
            hasPlayedStartup = false;
        }
    }

    private void CheckPlayFootStepSound()
    {
        // Check if the walking animation state
        bool isWalking = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == walkingStateHash;

        // If walking and enough time has passed since last step
        if (isWalking && Time.time >= lastStepTime + stepInterval)
        {
            PlayFootstepSound();
            lastStepTime = Time.time;
        }

        // Reset timer if just started walking
        if (isWalking && !wasWalking)
        {
            lastStepTime = Time.time;
        }

        wasWalking = isWalking;
    }
    private void CheckPlayJumpSound()
    {
        if (!playerMovement.readyToJump)
        {
            PlayFootstepSound();
            isLandingSound = true;
        }

        if (playerMovement.grounded && isLandingSound == true)
        {
            isLandingSound = false;
            PlayFootstepSound();
        }
    }

    private void CheckPlayGoToRollSound()
    {
        bool isGoToRoll = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == goToRollStateHash;

        if (isGoToRoll && !hasPlayedGoToRollSound)
        {
            PlayGoToRollSound();
            hasPlayedGoToRollSound = true;
        }
    }

    private void CheckPlayRollingSound()
    {
        bool isRollingState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == rollStateHash;

        if (isRollingState && !hasPlayedRollingSound)
        {
            PlayRollingSound();
            hasPlayedRollingSound = true;
        }
        else if (!isRollingState)
        {
            if (hasPlayedRollingSound)
            {
                SFXSource.loop = false;
                SFXSource.Stop();
            }
            hasPlayedRollingSound = false;
            isPlayingRollingSound = false;
        }

        StopRollingWhenAir();
    }

    private void CheckExitRollSound()
    {
        bool isEixtRollState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == exitRollStateHash;

        if (isEixtRollState && hasPlayedGoToRollSound)
        {
            PlayExitRollSound();
            hasPlayedGoToRollSound = false;
        }
    }

    private void PlayStartupSound()
    {
        if (startupSound == null) return;

        // Play startup sound
        SFXSource.clip = startupSound;
        SFXSource.volume = startupVolume;
        SFXSource.pitch = 1f; // No random pitch for startup sound
        SFXSource.Play();
    }


    private void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || !playerMovement.grounded)
            return;

        AudioClip randomStep = footstepSounds[Random.Range(0, footstepSounds.Length)];

        SFXSource.clip = randomStep;
        SFXSource.volume = footstepVolume;
        SFXSource.pitch = Random.Range(0.95f, 1.05f);
        SFXSource.Play();
    }

    private void PlayGoToRollSound()
    {
        SFXSource.clip = goToRollSounds;
        SFXSource.volume = goToRollVolume;
        SFXSource.Play();
    }

    private void PlayRollingSound()
    {
        SFXSource.clip = RollingSound;
        SFXSource.volume = RollingVolume;
        SFXSource.loop = true;
        SFXSource.Play();
        isPlayingRollingSound = true;
    }

    private void PlayExitRollSound()
    {
        SFXSource.clip = exitRollSounds;
        SFXSource.volume = exitRollVolume;
        SFXSource.loop = false;
        SFXSource.Play();
    }

    private void StopRollingWhenAir()
    {
        if (!playerMovement.grounded && isPlayingRollingSound)
        {
            SFXSource.Stop();
            hasPlayedRollingSound = false;
        }
    }
}
