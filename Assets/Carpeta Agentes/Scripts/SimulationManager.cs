using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    // Variables
    public float secondsPerIteration = 1.0f;
    private float time = 0;

    void Start()
    {
        
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

    }
}
