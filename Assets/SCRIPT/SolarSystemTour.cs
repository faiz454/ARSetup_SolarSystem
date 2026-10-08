using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemTour : MonoBehaviour
{
    [Header("Planets in Order (Mercury to Neptune)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();

    [Header("Moon References")]
    [SerializeField] private Transform moontarget;
    [SerializeField] private Animator moonAnimator;

    [Header("Satellite References")]
    [SerializeField] private Transform satelliteTarget;
    [SerializeField] private Animator satelliteAnimator;

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f;

    // Distances framed so Earth + orbiting bodies fit fully on screen
    [SerializeField] private float moonDistanceFromCamera = 0.8f;
    [SerializeField] private float satelliteDistanceFromCamera = 0.7f;

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
    private bool isSatelliteAudioPlaying = false;

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

        // Auto-assign parent animators if missing
        if (moonAnimator == null && moontarget != null && moontarget.parent != null)
        {
            moonAnimator = moontarget.parent.GetComponent<Animator>();
        }

        if (satelliteAnimator == null && satelliteTarget != null && satelliteTarget.parent != null)
        {
            satelliteAnimator = satelliteTarget.parent.GetComponent<Animator>();
        }

        // Ensure Moon is disabled initially
        if (moontarget != null)
        {
            moontarget.gameObject.SetActive(false);
        }
        if (moonAnimator != null)
        {
            moonAnimator.enabled = false;
        }

        // Ensure Satellite is disabled initially
        if (satelliteTarget != null)
        {
            satelliteTarget.gameObject.SetActive(false);
        }
        if (satelliteAnimator != null)
        {
            satelliteAnimator.enabled = false;
        }

        StartCoroutine(StartTourRoutine());
    }

    void Update()
    {
        // 1. Planets Axial Spin & Sun Revolutions
        for (int i = 0; i < planetTargets.Count; i++)
        {
            if (planetTargets[i] == null) continue;

            planetTargets[i].Rotate(0f, axialSpinSpeed * Time.deltaTime, 0f, Space.Self);

            // Revolve around Sun after the full tour completes
            if (isTourComplete && sunCenter != null)
            {
                float revSpeed = (i < revolutionSpeeds.Length) ? revolutionSpeeds[i] : 5f;
                planetTargets[i].RotateAround(sunCenter.position, Vector3.up, revSpeed * Time.deltaTime);
            }
        }

        // 2. Moon Axial Spin (ONLY when active AND audio is finished)
        if (moontarget != null && moontarget.gameObject.activeSelf && !isMoonAudioPlaying)
        {
            moontarget.Rotate(0f, axialSpinSpeed * Time.deltaTime, 0f, Space.Self);
        }

        // 3. Satellite Axial Spin (ONLY when active AND audio is finished)
        if (satelliteTarget != null && satelliteTarget.gameObject.activeSelf && !isSatelliteAudioPlaying)
        {
            satelliteTarget.Rotate(0f, axialSpinSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }

    private IEnumerator StartTourRoutine()
    {
        // 1. Initial overview pause
        yield return new WaitForSeconds(4.0f);

        // 2. Tour Mercury through Neptune
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

        // 4. Moon Focus Sequence
        if (moontarget != null)
        {
            moontarget.gameObject.SetActive(true);

            // Mark audio playing so manual axial spin stops
            isMoonAudioPlaying = true;

            // Pause animator orbit if present
            if (moonAnimator != null)
            {
                moonAnimator.enabled = false;
            }

            // Move the Solar System so Moon is centered in front of AR Camera
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

            // Resume Moon Orbit Animation & Spin AFTER audio finishes
            isMoonAudioPlaying = false;
            if (moonAnimator != null)
            {
                moonAnimator.enabled = true;
                moonAnimator.speed = 1f;
            }

            yield return new WaitForSeconds(3.0f);
        }

        // 5. Artificial Satellite Focus Sequence
        if (satelliteTarget != null)
        {
            satelliteTarget.gameObject.SetActive(true);

            isSatelliteAudioPlaying = true;

            if (satelliteAnimator != null)
            {
                satelliteAnimator.enabled = false;
            }

            // Move Solar System so Satellite is centered in front of AR Camera
            yield return StartCoroutine(MovePlanetToCamera(satelliteTarget, satelliteDistanceFromCamera));

            // Play Satellite Audio clip
            if (audioManager != null)
            {
                audioManager.PlaySatelliteAudio(0);
                yield return null;
                yield return null;
                yield return new WaitWhile(() => audioManager.IsPlaying());
            }

            yield return new WaitForSeconds(pauseAfterAudio);

            // Resume Satellite Orbit Animation AFTER audio finishes
            isSatelliteAudioPlaying = false;
            if (satelliteAnimator != null)
            {
                satelliteAnimator.enabled = true;
                satelliteAnimator.speed = 1f;
            }

            yield return new WaitForSeconds(3.0f);
        }

        // 6. Return back to full system overview
        yield return StartCoroutine(ReturnToInitialOverview());
        yield return new WaitForSeconds(overviewWaitTime);

        // 7. Complete tour: Enable planetary revolutions around Sun
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