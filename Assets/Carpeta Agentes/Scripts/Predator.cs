using UnityEngine;

public class Predator : MonoBehaviour
{
    // Variables de la configuracion del depredador
    [Header("Predator Settings")]
    public float energy = 10f;
    public float age = 0f;
    public float maxAge = 20f;
    public float speed = 1f;
    public float visionRange = 5f;


    // Variables de los estados del conejo
    [Header("Predator States")]
    public bool isAlive = true;
    public PredatorState currentState = PredatorState.Exploring;

    private Vector3 destination;
    private float h;


    void Start()
    {
        destination = transform.position; // El conejo decide a donde quiere ir
    }

    public void Simulate(float h)
    {
        if (!isAlive) return;

        this.h = h; // Especificar la clase de donde proviene

        switch (currentState) // Le da una lista de condiciones, es como una lista de if
        {
            case PredatorState.Exploring:
                Explore();
                break;
            case PredatorState.SearchingFood:
                SearchFood();
                break;
            case PredatorState.Eating:
                Eat();
                break;
        }

        Move();
        Age();
        CheckState();

    }

    void Move() // Se encarga de desplazar 
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * h
            );


        energy -= speed * h;
    }

    void Age() // Añade edad a medida de que avanza el tiempo
    {
        age += h;
    }

    void CheckState() // Confirma el estado en el que se encuentra
    {
        if (energy <= 0 || age >= maxAge)
        {
            isAlive = false;
            Destroy(gameObject);
        }
    }

    void SearchFood() // Estado de buscar la comida
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny == null)
        {
            currentState = PredatorState.Exploring;
            return;
        }

        destination = nearestBunny.transform.position;

        if (Vector3.Distance(transform.position, nearestBunny.transform.position) < 0.2f)
        {
            currentState = PredatorState.Eating;
        }
    }

    void Eat() // Estado de comer
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Bunnies")); // Verifica si choca con algun elemento
        if (foodHit != null)
        {
            Bunny food = foodHit.GetComponent<Bunny>(); // Trae la nutricion qu tiene esa comida
            if (food != null)
            {
                energy += food.age;
                Destroy(food.gameObject); // Especifica que se estruye solo ese gameObject
            }
        }
        currentState = PredatorState.Exploring; // Luego de comer vuelve al estado de explorar
    }

    void Explore() // Estado de explorar
    {
        Bunny nearestBunny = FindNearestBunny();
        if (nearestBunny != null)
        {
            currentState = PredatorState.SearchingFood;
            destination = nearestBunny.transform.position;
            return;
        }

        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            SelectNewDestination();
        }
    }


    void SelectNewDestination() // Elige un nuevo destino segun su rango de vision
    {
        Vector3 direction = new Vector3(
            Random.Range(-visionRange, visionRange),
            Random.Range(-visionRange, visionRange),
            0
            );

        Vector3 target = transform.position + direction;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, visionRange, LayerMask.GetMask("Obstacle"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)direction.normalized * offset;
        }
        else
        {
            destination = target;
        }
    }

    Bunny FindNearestBunny() // Evalua cual de las comidas que hay a su alrededor es la mas cercana
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Bunnies")); // Como golpe varios hay que ver cuantos hay al rededor

        Bunny nearestFood = null;
        float distance = Mathf.Infinity; // Ve la distacia infinita viendo uno por uno cual esta mas cerca, va buscando desde lo que esta mas lejos hasta lo que esta mas cerca

        foreach (Collider2D hit in hits)
        {
            Bunny food = hit.GetComponent<Bunny>();
            if (food != null)
            {
                float dist = Vector2.Distance(transform.position, food.transform.position);
                if (dist < distance)
                {
                    distance = dist;
                    nearestFood = food;
                }
            }
        }
        return nearestFood;
    }

    private void OnDrawGizmosSelected() // Esto es lo que se va adibujar para conocer lo que hace el conejo
    {
        Gizmos.color = Color.green; // Este representa el rango de vision
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red; // Este representa el punto al que va
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow; // Este representa la linea del camino que toma
        Gizmos.DrawLine(transform.position, destination);
    }
}
