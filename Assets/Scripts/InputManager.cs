using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;} //singleton: instancia de una clase que existe una sol vez de manera global para poer usarlo en cualquier clase que queramos

    private PlayerController controls;

    //Eventos de camara
    public event Action<float> OnCameraZoom; //El zoom devuelve datos positivos o negativos, como tener un zoom de 1.1
    public event Action<Vector2> OnCameraMove; //El movimiento se mueve en un vector x, y segun su posicion

    //Eventos del gameplay
    public event Action OnPause;
    public event Action OnRestart;
    public event Action OnClear;
    public event Action OnToggleCell;
    public event Action OnStep;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); //Para destruir el objeto del juego
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); //Que el objeto no se destruya, como dejarlo permanente

        controls = new PlayerController(); //Se crea una clase

        //Relacionar eventos de Gameplay y Camara de PlayerContoller

        //Emite un mensaje / Manda el paquete
        //Camara
        controls.Camera.Move.performed += ctx => OnCameraMove?.Invoke(ctx.ReadValue<Vector2>());//ctx es contexto, que retorna algo, en este caso los datos en el vector el movimiento de la camara
        controls.Camera.Move.canceled += ctx => OnCameraMove?.Invoke(Vector2.zero);//para cuando no se haga ningun movimiento o cambio
        controls.Camera.Zoom.performed += ctx => OnCameraZoom?.Invoke(ctx.ReadValue<float>());//ctx da el contexto y tambien envia los valores del vector
        controls.Camera.Zoom.canceled += ctx => OnCameraZoom?.Invoke(0);//para cuando no se haga ningun movimiento o cambio

        //Gameplay
        //En estas se esta solo emitiendo
        controls.Gameplay.Pause.performed += _ => OnPause?.Invoke();// Callback para cuando vaya a usarse
        controls.Gameplay.Restart.performed += _ => OnRestart?.Invoke();// Cada que se presiona una de las teclas se emite el evento que corrresponda
        controls.Gameplay.Clear.performed += _ => OnClear?.Invoke();// Performed es programacion orientada a eventos
        controls.Gameplay.ToggleCell.performed += _ => OnToggleCell?.Invoke();//Funcion flecha (=>) es un retorno implicito
        controls.Gameplay.Step.performed += _ => OnStep?.Invoke();//Funcion flecha (=>) es un retorno implicito
    }

    private void OnEnable() //Cuando se activa la clase InputManager se activan los controles
    {
        controls.Camera.Enable();
        controls.Gameplay.Enable();
    }

    private void OnDisable() //Cuando se desactiva la clase InputManager se desactivan los controles
    {
        controls.Camera.Disable();
        controls.Gameplay.Disable();
    }
}
