
using UnityEngine;
using System.Collections;
using System;

public class ClientAmbience : MonoBehaviour
{
    [Header("Ambience Objects")]
    [SerializeField] private GameObject selecta;
    [SerializeField] private GameObject jeepney;
    [SerializeField] private GameObject grinder;
    [SerializeField] private GameObject motorCycle;

    private void Start()
    {
        StartCoroutine(PlaySelecta());
        StartCoroutine(PlayJeepney());
        StartCoroutine(PlayGrinder());
        StartCoroutine(PlayMotorcycle());
    }

    IEnumerator PlaySelecta()
    {
        while (true)
        {
            selecta.SetActive(true);
            yield return new WaitForSeconds(9f);

            selecta.SetActive(false);
            yield return new WaitForSeconds(20f);
        }
    }

    IEnumerator PlayJeepney()
    {
        while (true)
        {
            jeepney.SetActive(true);
            yield return new WaitForSeconds(10f);

            jeepney.SetActive(false);
            yield return new WaitForSeconds(15f);
        }
    }

    IEnumerator PlayGrinder()
    {
        while (true)
        {
            grinder.SetActive(true);
            yield return new WaitForSeconds(10f);

            grinder.SetActive(false);
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator PlayMotorcycle()
    {
        while (true)
        {
            motorCycle.SetActive(true);
            yield return new WaitForSeconds(10f);

            motorCycle.SetActive(false);
            yield return new WaitForSeconds(18f);
        }
    }
}
