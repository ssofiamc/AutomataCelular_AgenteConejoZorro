using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 50; //Ancho
    public int height = 30; //Alto
    public float updateTime = 0.1f; //Cada cuanto se actualiza
    public float cellSpawnChance = 0.95f; //Probabilidad de que aparezcan celulas
    public GameObject cellPrefab; // Usa el prefab creado
    

    private bool[,] grid; //Array en ambas dimensiones 
    private bool[,] nextGrid;
    private GameObject[,] cellObjects; //Se guarda cada celula como un gameobject para poder actualizar su estado visualmente
    private float timer;
    private bool isPaused = true;

    void Start() //Iniciar el proceso
    {
        grid = new bool[width, height];  //Inicializar el array de celulas, cada celda puede estar viva (true) o muerta (false)
        nextGrid = new bool[width, height]; //Array para calcular el siguiente estado de la celula sin modificar el estado actual       
        cellObjects = new GameObject[width, height];  //Array para guardar los gameobjects de cada celula, para poder actualizar su estado visualmente

        InputManager.Instance.OnPause += TogglePause;
        InputManager.Instance.OnRestart += RestartSimulation;
        InputManager.Instance.OnClear += ClearSimulation;
        InputManager.Instance.OnToggleCell += ToggleCellInput;
        InputManager.Instance.OnStep += StepOnce;

        GenerateGrid();
        RandomizeGrid();
    }

    void Update()
    {
        if (isPaused) return; // Si esta pausado el juego, vuelve

        timer += Time.deltaTime; //Time.deltaTime son los segunds que pasan entr frame y frame
        if (timer >= updateTime)
        {
            Step(); // Pasa  la siguiene 'generacion'
            UpdateVisuals(); //Actualiza la parte visual
            timer = 0f; //Reinicia el times para crear un bucle infinito
        }
    }

    void StepOnce()
    {
        Step();
        UpdateVisuals();
        timer = 0;
    }

    void TogglePause()
    {
        isPaused = !isPaused; //Si esta pausado que muestre la simulacion pausada, sino que reanude la simulacion
        Debug.Log(isPaused ? "Simulación pausada" : "Simulación reanudada"); // el ? es como un if corto
    }

    void ToggleCellInput()
    {
        // Si hay mouse disponible (PC), usar clic real
        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            HandleMouseClick();
            return;
        }

        // Si no hay mouse, usar el centro de la cámara
        Vector3 camPos = Camera.main.transform.position;
        int x = Mathf.RoundToInt(camPos.x);
        int y = Mathf.RoundToInt(camPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }


    void ClearSimulation()
    {
        Debug.Log("Limpiando simulación...");
        ClearGrid(); //Itera el grid y lo limpia
        timer = 0f;
    }

    void RestartSimulation()
    {
        Debug.Log("Reiniciando simulación...");
        RandomizeGrid(); //Llama la funcion de randomizar
        timer = 0f;
    }

    void GenerateGrid() //Se llama al iniio en el start y genera la grilla 
    {
        for (int x = 0; x < width; x++) //Comienza a llena la grilla por columnas
        {
            for (int y = 0; y < height; y++) //Quaternion como gimbal lock
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity); //Crea una celula en la posicion (x,y,0) con rotacion por defecto
                cell.transform.parent = transform; //El padre de la celula va a ser el gamemanager y todo sale desde aqui
                cellObjects[x, y] = cell; //
            }
        }
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = false;
            }
        }
        UpdateVisuals();
    }

    void RandomizeGrid() //Se ejecuta al inicio y cuando se reinicia la simulacion
    {
        for (int x = 0; x < width; x++) //Iterar en x
        {
            for (int y = 0; y < height; y++) //Iterar en y
            {
                grid[x, y] = Random.value > cellSpawnChance; //En la grilla en la posicion x, y va a ser un valor al azar de 0,95 para arriba
            }
        }
        UpdateVisuals();
    }

    void Step() //En cada paso pasa a una nuva generacion
    {
        for (int x = 0; x < width; x++) //Recorre toda la grilla 
        {
            for (int y = 0; y < height; y++)
            {
                int aliveNeighbors = CountAliveNeighbors(x, y); // Cuenta cuantos vecinos vivos tiene cada celulas
                bool alive = grid[x, y]; //Toma la posicion de la celula actual en la iteracion y dice si esta viva o muerta

                if (alive && (aliveNeighbors < 2 || aliveNeighbors > 3)) // Verifica si tiene menos de 2 vecinos o mas de 3, en la primera muere por soledad, en la segunda muere por sobrepoblacion
                    nextGrid[x, y] = false; // Muere
                else if (!alive && aliveNeighbors == 3) // Verifica si tiene exactamente 3 vecinos y con esto hace que nazca una celula
                    nextGrid[x, y] = true;  // Nace
                else // Si no se cumple ninguna de las 3 reglas anteriores, las celulas se mantienen
                    nextGrid[x, y] = alive; // Se mantiene
            }
        }

        // Swap grids
        var temp = grid; //Guarda la grilla actual en una variable temporal
        grid = nextGrid; //La grilla sera la nueva grilla
        nextGrid = temp;// La nueva grilla es la temporal
    }

    int CountAliveNeighbors(int x, int y) //Pasa la posicion
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++) //Recorre de izquierda a derecha, entonces recorre de abajo a hacia arriba verificando si hay mas celulas vivas o muertas, uno atras y uno aelante
        {
            for (int dy = -1; dy <= 1; dy++) //Uno abajo y uno arriba
            {
                if (dx == 0 && dy == 0) continue; //Si esta en la misma casilla que la celula en la que se trabaja, pues lo salta y continua analizando el entorno
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height) //Limita y verifica que haya ms mapa alrededor de la celula en la que estamos trabajando
                {
                    if (grid[nx, ny]) count++;
                }
            }
        }

        return count;
    }

    void HandleMouseClick()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x < 0 || x >= width || y < 0 || y >= height)
            return;

        grid[x, y] = !grid[x, y];
        UpdateVisuals();
    }



    void UpdateVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var rend = cellObjects[x, y].GetComponent<SpriteRenderer>(); // Dl objeto de la posicion actual obtener el sprite render
                rend.color = grid[x, y] ? Color.black : Color.white; // Si la elula esta viva le da el color negro y si esta muerta le da el color blanco
            }
        }
    }
}
