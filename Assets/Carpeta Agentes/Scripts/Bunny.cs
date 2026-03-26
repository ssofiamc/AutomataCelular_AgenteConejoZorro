using UnityEngine;

public class Bunny : MonoBehaviour
{
    // Variables de la configuracion del conejo
    [Header("Bunny Settings")]
    public float energy = 10f;
    public float age = 0f;
    public float maxAge = 20f;
    public float speed = 1f;
    public float visionRange = 5f;


    // Variables de los estados del conejo
    [Header("Bunny States")]
    public bool isAlive = true;
    public BunnyState currentState = BunnyState.Exploring;

    private Vector3 destination;
    private float h;

    void Start()
    {
        destination = transform.position; // El conejo decide a donde quiere ir
    }

    public void Simulate (float h)
    {
        if (!isAlive) return;

        this.h = h; // Especificar la clase de donde proviene

        EvaluateState();

        switch (currentState) // Le da una lista de condiciones, es como una lista de if
        {
            case BunnyState.Exploring:
                Explore();
                break;
            case BunnyState.SearchingFood:
                SearchFood();
                break;
            case BunnyState.Eating:
                Eat();
                break;
            case BunnyState.Fleeing:
                Flee();
                break;
            
        }
        Move();
        Age();
        CheckState();

    }

    void EvaluateState()
    {
        // Casos y prioriddes del conejo
        // 1. Si hay un depredador tengo que huir
        if (PredatorInRange())
        {
            currentState = BunnyState.Fleeing;
            return;
        }

        // 2. Si tengo poca energia, tengo que buscar comida
        if (energy < 500f)
        {
            Food nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                currentState = BunnyState.SearchingFood;
                destination = nearestFood.transform.position;
            }
        }

        // 3. Si tengo comida, tengo que comer
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Food")); // Verifica si choca con algun elemento
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>(); // Trae la nutricion qu tiene esa comida
            if (food != null)
            {
                currentState = BunnyState.Eating;
                return;
            }
        }

        // 4. Si estoy de chill, tengo que explorar
        if (currentState == BunnyState.Eating == false)
        {
            currentState = BunnyState.Exploring;
        }
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

    void Explore() // Estado de explorar
    {
        Food nearestFood = FindNearestFood();
        if (nearestFood != null)
            {
            currentState = BunnyState.SearchingFood;
            destination = nearestFood.transform.position;
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
    void SearchFood() // Estado de buscar la comida
    {
        Food nearestFood = FindNearestFood();
        if (nearestFood == null)
        {
            currentState = BunnyState.Exploring;
            return;
        }

        destination = nearestFood.transform.position;

        if (Vector3.Distance(transform.position, nearestFood.transform.position) < 0.2f)
        {
            currentState = BunnyState.Eating;
        }
    }

    void Eat() // Estado de comer
    {
        Collider2D foodHit = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Food")); // Verifica si choca con algun elemento
        if (foodHit != null)
        {
            Food food = foodHit.GetComponent<Food>(); // Trae la nutricion qu tiene esa comida
            if (food != null)
            {
                energy += food.nutrition;
                Destroy(food.gameObject); // Especifica que se estruye solo ese gameObject
            }
        }
        currentState = BunnyState.Exploring; // Luego de comer vuelve al estado de explorar
    }

    Food FindNearestFood() // Evalua cual de las comidas que hay a su alrededor es la mas cercana
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Food")); // Como golpe varios hay que ver cuantos hay al rededor

        Food nearestFood = null;
        float distance = Mathf.Infinity; // Ve la distacia infinita viendo uno por uno cual esta mas cerca, va buscando desde lo que esta mas lejos hasta lo que esta mas cerca

        foreach (Collider2D hit in hits)
        {
            Food food = hit.GetComponent<Food>();
            if (food != null)
            {
                float dist = Vector3.Distance(transform.position, food.transform.position);
                if (dist < distance)
                {
                    distance = dist;
                    nearestFood = food;
                }
            }
        }
        return nearestFood;
    }
    void Flee()
    {
        // Elegir dirección contraria al depredador
        Vector3 fleeDir = (transform.position - GetNearestPredatorPosition()).normalized;
        destination = transform.position + fleeDir * visionRange;

        // Después de huir vuelve a explorar
        currentState = BunnyState.Exploring;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, fleeDir, visionRange, LayerMask.GetMask("Obstacle"));

        if (hit.collider != null)
        {
            float offset = transform.localScale.magnitude * 0.5f;
            destination = hit.point - (Vector2)fleeDir * offset;
        }
        else
        {
            destination = transform.position + fleeDir * visionRange;
        }
    }
    bool PredatorInRange()
    {
        Collider2D predator = Physics2D.OverlapCircle(transform.position, visionRange, LayerMask.GetMask("Predator"));
        return predator != null;
    }

    Vector3 GetNearestPredatorPosition()
    {
        Collider2D[] predators = Physics2D.OverlapCircleAll(transform.position, visionRange, LayerMask.GetMask("Predator"));
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var p in predators)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                pos = p.transform.position;
            }
        }
        return pos;
    }

    private void OnDrawGizmosSelected() // Esto es lo que se va adibujar para conocer lo que hace el conejo
    {
        Gizmos.color = Color.green; // Este representa el rango de vision
        Gizmos.DrawWireSphere (transform.position, visionRange);

        Gizmos.color = Color.red; // Este representa el punto al que va
        Gizmos.DrawSphere(destination, 0.2f);

        Gizmos.color = Color.yellow; // Este representa la linea del camino que toma
        Gizmos.DrawLine(transform.position, destination);
    }
}
