using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemTour : MonoBehaviour
{
    [Header("Planets in Order (Mercury to Neptune)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();// List of planets in the order they should be presented

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f; // Wait 1 sec after voice finishes
    [SerializeField] private float transitionSpeed = 2f;// Speed at which the solar system moves to the next planet
    [SerializeField] private float distanceFromCamera = 0.6f;// Distance in front of the camera where the planet should appear

    [Header("Audio Reference")]
    public AudioManger audioManager;// Reference to the AudioManger script for playing planet voiceovers

    private Transform arCameraTransform;// Reference to the AR camera's transform

    private void Start()
    {
        if (Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;// Get the main camera's transform
        }

        // Auto-find AudioManager if slot is left empty
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManger>();// Find the first instance of AudioManger in the scene
        }

        StartCoroutine(StartTourRoutine());
    }

    private IEnumerator StartTourRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            // 1. Move the planet in front of the camera
            yield return StartCoroutine(MovePlanetToCamera(targetPlanet));

            // 2. Play this planet's voiceover
            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);// Play the audio for the current planet

                yield return null; // Wait 1 frame so audio registers

                // 3. Wait until the planet audio finishes playing
                yield return new WaitWhile(() => audioManager.IsPlaying());
            }

            // 4. Brief delay before flying to the next planet
            yield return new WaitForSeconds(pauseAfterAudio);
        }
    }

    private IEnumerator MovePlanetToCamera(Transform targetPlanet)// Coroutine to smoothly move the solar system so that the target planet is in front of the camera
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            Vector3 planetOffset = targetPlanet.position - transform.position;
            Vector3 desiredSystemPosition = targetCameraPosition - planetOffset;

            transform.position = Vector3.Lerp(transform.position, desiredSystemPosition, elapsed / duration);
            yield return null;
        }
    }
}