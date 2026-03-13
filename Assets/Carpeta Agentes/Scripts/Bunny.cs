using UnityEngine;

public class Bunny : MonoBehaviour
{
    // Variables de la configuracion del conejo
    [Header("Bunny Settings")]
    public float energy = 10f;
    public float age = 0f;
    public float maxAge = 20f;
    public float speed = 1.0f;
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
        if (isAlive) return;

        this.h = h; // Especificar la claase de donde proviene

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

    void Move() // Se encarga de desplazar 
        {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed = h
            );


        energy -= speed + h;
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
                energy = +food.nutrition;
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
}
