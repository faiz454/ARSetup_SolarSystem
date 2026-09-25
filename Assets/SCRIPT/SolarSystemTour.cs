using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemTour : MonoBehaviour
{
    [Header("Planets in Order (Mercury to Neptune)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();
    [SerializeField] private Transform moontarget;
    [SerializeField] private Animator moonAnimator;

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f;
    [SerializeField] private float moonDistanceFromCamera = 0.35f;

    [Header("Audio Reference")]
    public AudioManger audioManager;

    [Header("Center Reference (Drag 'Sun' here)")]
    [SerializeField] private Transform sunCenter;

    [Header("Speeds & Overview Settings")]
    [SerializeField] private float axialSpinSpeed = 25f;
    [SerializeField] private float overviewWaitTime = 3.0f;
    [SerializeField] private float overviewDistance = 2.5f;
    [SerializeField] private float[] revolutionSpeeds = new float[] { 14f, 11f, 9f, 7f, 5f, 4f, 3f, 2f };

    private Transform arCameraTransform;
    private Vector3 initialSpawnPosition;
    private bool isTourComplete = false;
    private bool isMoonAudioPlaying = false;


    private void Start()
    {
        if (Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;
        }

        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManger>();
        }
        initialSpawnPosition = transform.position;

        if (sunCenter == null)
        {
            Transform foundSun = transform.Find("Sun");
            sunCenter = (foundSun != null) ? foundSun : transform;
        }

        if (moonAnimator == null && moontarget != null)
        {
            if(moontarget.parent != null)
            {
                moonAnimator = moontarget.parent.GetComponent<Animator>();
            }
        }

        // Hide Moon initially & disable its animator component
        if (moontarget != null)
        {
            moontarget.gameObject.SetActive(false);
        }

        if (moonAnimator != null)
        {
            moonAnimator.enabled = false;
        }

        StartCoroutine(StartTourRoutine());
    }

    void Update()
    {
        for (int i = 0; i < planetTargets.Count; i++)
        {
            if (planetTargets[i] == null) continue;

            // Axial spin for planets
            planetTargets[i].Rotate(0f, axialSpinSpeed * Time.deltaTime, 0f, Space.Self);

            // Revolve around Sun after the entire tour finishes
            if (isTourComplete && sunCenter != null)
            {
                float revSpeed = (i < revolutionSpeeds.Length) ? revolutionSpeeds[i] : 5f;
                planetTargets[i].RotateAround(sunCenter.position, Vector3.up, revSpeed * Time.deltaTime);
            }
        }

        // Moon axial spin (ONLY when active AND audio is NOT playing)
        if (moontarget != null && moontarget.gameObject.activeSelf && !isMoonAudioPlaying)
        {
            moontarget.Rotate(0f, axialSpinSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    private IEnumerator StartTourRoutine()
    {
        // 1. Initial overview wait
        yield return new WaitForSeconds(4.0f);

        // 2. Tour planets Mercury through Neptune
        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];
            if (targetPlanet == null) continue;

            yield return StartCoroutine(MovePlanetToCamera(targetPlanet, distanceFromCamera));

            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);
                yield return null;
                yield return null;
                yield return new WaitWhile(() => audioManager.IsPlaying());
            }

            yield return new WaitForSeconds(pauseAfterAudio);
        }

        // 3. Return to solar system overview
        yield return StartCoroutine(ReturnToInitialOverview());
        yield return new WaitForSeconds(overviewWaitTime);

        // 4. Moon Focus, Audio, and Orbit Sequence
        if (moontarget != null)
        {
            // Unhide Moon
            moontarget.gameObject.SetActive(true);

            // Freeze ALL Moon movement during focus/audio
            isMoonAudioPlaying = true;
            if (moonAnimator != null)
            {
                moonAnimator.enabled = false;
            }

            // Zoom camera directly to Moon
            yield return StartCoroutine(MovePlanetToCamera(moontarget, moonDistanceFromCamera));

            // Play Moon Audio
            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(8);
                yield return null;
                yield return null;
                yield return new WaitWhile(() => audioManager.IsPlaying());
            }

            yield return new WaitForSeconds(pauseAfterAudio);

            // 5. Enable Animator & start orbit around Earth
            isMoonAudioPlaying = false;
            if (moonAnimator != null)
            {
                moonAnimator.enabled = true;
                moonAnimator.speed = 1f;
            }

            // Watch Moon revolve around Earth in focus
            yield return new WaitForSeconds(3.0f);

            // 6. Return back to full solar system overview
            yield return StartCoroutine(ReturnToInitialOverview());
            yield return new WaitForSeconds(overviewWaitTime);
        }

        // 7. Enable planetary revolutions around Sun
        isTourComplete = true;
    }

    private IEnumerator ReturnToInitialOverview()
    {
        float elapsed = 0f;
        float duration = 2.5f;
        Vector3 startPos = transform.position;

        Vector3 targetOverviewPos = arCameraTransform.position
                                  + (arCameraTransform.forward * overviewDistance)
                                  - (arCameraTransform.up * 0.2f);

        Vector3 sunLocalOffset = sunCenter != null ? (sunCenter.position - transform.position) : Vector3.zero;
        Vector3 finalSystemPos = targetOverviewPos - sunLocalOffset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, finalSystemPos, t);
            yield return null;
        }

        transform.position = finalSystemPos;
    }

    private IEnumerator MovePlanetToCamera(Transform targetPlanet, float focusDistance)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * focusDistance);

        Vector3 startSystemPos = transform.position;
        Vector3 planetInitialWorldPos = targetPlanet.position;
        Vector3 desiredSystemPosition = startSystemPos + (targetCameraPosition - planetInitialWorldPos);

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startSystemPos, desiredSystemPosition, t);
            yield return null;
        }

        transform.position = desiredSystemPosition;
    }
}