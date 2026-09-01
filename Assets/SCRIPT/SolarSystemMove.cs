using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemTour : MonoBehaviour
{
    [Header("Planets in Order (Mercury, Venus, etc.)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();

    [Header("Tour Settings")]
    [SerializeField] private float waitTimePerPlanet = 2.5f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f; // How close to view the planet

    private Transform arCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;
        }

        // Auto-find planets from children if list is empty
        if (planetTargets.Count == 0)
        {
            foreach (Transform child in transform)
            {
                planetTargets.Add(child);
            }
        }

        StartCoroutine(StartTourRoutine());
    }

    private IEnumerator StartTourRoutine()
    {
        // Initial delay before starting the tour
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            // Move solar system so the target planet aligns in front of the AR camera
            yield return StartCoroutine(MovePlanetToCamera(targetPlanet));

            // Wait 2 to 3 seconds on the current planet
            yield return new WaitForSeconds(waitTimePerPlanet);
        }
    }

    private IEnumerator MovePlanetToCamera(Transform targetPlanet)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);

        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;

            // Calculate offset needed so target planet arrives at targetCameraPosition
            Vector3 planetOffset = targetPlanet.position - transform.position;
            Vector3 desiredSystemPosition = targetCameraPosition - planetOffset;

            transform.position = Vector3.Lerp(transform.position, desiredSystemPosition, elapsed / duration);
            yield return null;
        }
    }
}