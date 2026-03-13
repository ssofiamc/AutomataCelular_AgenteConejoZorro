using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //Creacion de variables
    [Header("Movimiento")]
    public float moveSpeed = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 20f;
    public float minZoom = 2f;
    public float maxZoom = 30f;


    private Camera cam;
    private Vector2 moveInput = Vector2.zero;
    private float zoomInput = 0f;

    void Start() //Capturar la camara
    {
        cam = Camera.main; //Camara principal de la escena, si hay solo una camara
        InputManager.Instance.OnCameraMove += val => moveInput = val;//Llama al InputManager y se puede acceder a todo lo publico que tiene, aqui se va a hacer la suscripcion, entonces se capturar el valor que tenga
        InputManager.Instance.OnCameraZoom += val => zoomInput = val;//Llama al InputManager y se puede acceder a todo lo publico que tiene, aqui se va a hacer la suscripcion, entonces se capturar el valor que tenga

    }

    void Update()
    {
        HandleMovement();
        HandleZoom(); //Time.deltaTime se encarga de normalizar
    }

    void HandleMovement()
    {
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f); //Realmente el 2d en unity es 3d, entonces es para mover el objeto en las 3 direcciones
        cam.transform.position += delta * moveSpeed * Time.deltaTime;//Para cambiar o mover un objeto, 5f es la rapidez
    }

    void HandleZoom()
    {
        cam.orthographicSize -= zoomInput * zoomSpeed * Time.deltaTime; //Para hacer zoom a cierta velocidad
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); // orthographic para aplnar mas la imagen, clamp limita y pone un rango
    }
}
