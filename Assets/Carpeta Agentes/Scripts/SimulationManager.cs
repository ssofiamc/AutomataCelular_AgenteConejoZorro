using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    // Variables
    public float secondsPerIteration = 1.0f;
    private float time = 0;

    public List<Bunny> bunnies = new List<Bunny>();
    public List<Predator> predators = new List<Predator>();

    void Start() //Lista de conejos
    {
        Bunny[] foundBunnies = FindObjectsByType<Bunny>(FindObjectsSortMode.InstanceID);
        bunnies = new List<Bunny>(foundBunnies);
        Predator[] foundPredators = FindObjectsByType<Predator>(FindObjectsSortMode.InstanceID);
        predators = new List<Predator>(foundPredators);
    }

    void Update()
    {
        time += Time.deltaTime;

        if (time > secondsPerIteration)
        {
            time = 0;
            Simulate();
        }
    }

    void Simulate()
    {
        foreach (Bunny b in bunnies) if (b != null && b.isAlive) b.Simulate(secondsPerIteration);

        foreach (Predator p in predators) if (p != null && p.isAlive) p.Simulate(secondsPerIteration);
    }
}
