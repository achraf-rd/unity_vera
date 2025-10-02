using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarEnterExit : MonoBehaviour
{
    [Header("UI")]
    public GameObject EnterText;
    public GameObject ExitText;

    [Header("Cameras")]
    public GameObject AnimationCamera;
    public GameObject CarCamera;
    public GameObject playerCamera;

    [Header("Players & placeholders")]
    public GameObject playerMain;      // ton vrai joueur (avec controller, rigidbody, colliders)
    public GameObject EnterPlayer;     // avatar   activer quand on est dans la voiture (optionnel)
    public GameObject ExitPlayer;      // avatar utilis  sur la sortie (optionnel)
    public Transform carTransform;

    [Header("Controllers (assigner dans l'Inspector)")]
    public MonoBehaviour playerController; // script qui g re le mouvement du joueur
    public MonoBehaviour carController;    // script qui g re la voiture

    [Header("Rigidbodies & collisions (auto-find si non assign )")]
    public Rigidbody playerRigidbody;
    public Collider[] playerColliders;
    public CharacterController playerCharacterController; // si tu utilises CharacterController
    public Rigidbody carRigidbody; // pour stopper la voiture quand tu sors

    [Header("Exit placement")]
    public Vector3 exitOffset = new Vector3(-2f, 0f, 0f); // ajuster : -2 = c t  conducteur

    [Header("Animator (optionnel)")]
    public Animator playerAnimator;
    public Animator cameraAnimator;

    //  tat
    private bool insideCar = false;
    private bool nearCar = false;

    void Start()
    {
        EnterText.SetActive(false);
        ExitText.SetActive(false);

        // autocompl tion : r cup re rigidbody / colliders si pas assign s
        if (playerRigidbody == null && playerMain != null) playerRigidbody = playerMain.GetComponent<Rigidbody>();
        if ((playerColliders == null || playerColliders.Length == 0) && playerMain != null)
            playerColliders = playerMain.GetComponentsInChildren<Collider>(true);
        if (playerCharacterController == null && playerMain != null)
            playerCharacterController = playerMain.GetComponent<CharacterController>();

        // au d marrage : player activ , voiture d sactiv e
        if (playerController != null) playerController.enabled = true;
        if (carController != null) carController.enabled = false;
    }

    void Update()
    {
        // Input System minimal (Keyboard)
        if (Keyboard.current.fKey.wasPressedThisFrame && nearCar && !insideCar)
            EnterCar();

        if (Keyboard.current.vKey.wasPressedThisFrame && insideCar)
            ExitCar();
    }

    void EnterCar()
    {
        Debug.Log("EnterCar called");

        EnterText.SetActive(false);
        ExitText.SetActive(false);

        // 1) D sactiver contr les du joueur
        if (playerController != null) playerController.enabled = false;
        if (playerCharacterController != null) playerCharacterController.enabled = false;

        // 2) D sactiver collisions & physique du joueur pour  viter de pousser la voiture
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true; // on coupe la physique
        }
        if (playerColliders != null)
            foreach (var c in playerColliders) c.enabled = false;

        // 3) Optionnel : cacher le joueur r el et activer un avatar "inside car"
        if (playerMain != null) playerMain.SetActive(false);
        if (EnterPlayer != null)
        {
            EnterPlayer.SetActive(true);
            // faire suivre l'avatar avec la voiture
            EnterPlayer.transform.SetParent(carTransform, false);
        }

        // 4) Activer le controller de la voiture
        if (carController != null) carController.enabled = true;

        // 5) animations/cam ra
        if (playerAnimator != null) playerAnimator.SetTrigger("EnterCar");
        if (cameraAnimator != null) cameraAnimator.SetTrigger("CarEnter");

        AnimationCamera.SetActive(true);
        StartCoroutine(PauseForCamera());

        insideCar = true;
    }

    void ExitCar()
    {
        Debug.Log("ExitCar called");

        // 1) Calculer position de sortie relative   la voiture
        Vector3 exitPos = carTransform.position
                          + carTransform.right * exitOffset.x
                          + carTransform.forward * exitOffset.z
                          + Vector3.up * exitOffset.y;
        Quaternion exitRot = Quaternion.LookRotation(carTransform.forward);

        // 2) D sactiver le controller de la voiture et stopper sa v locit 
        if (carController != null) carController.enabled = false;
        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        // 3) D sactiver avatar "in car"
        if (EnterPlayer != null)
        {
            EnterPlayer.SetActive(false);
            EnterPlayer.transform.SetParent(null);
        }

        // 4) R activer le player physique, le placer   la position de sortie
        if (playerMain != null)
        {
            playerMain.SetActive(true);
            playerMain.transform.SetParent(null);
            playerMain.transform.position = exitPos;
            playerMain.transform.rotation = exitRot;
        }

        // 5) Restaurer physique/colliders et contr leur du player
        if (playerRigidbody != null) playerRigidbody.isKinematic = false;
        if (playerColliders != null)
            foreach (var c in playerColliders) c.enabled = true;
        if (playerCharacterController != null) playerCharacterController.enabled = true;
        if (playerController != null) playerController.enabled = true;

        // 6) animations / cam ra
        if (playerAnimator != null) playerAnimator.SetTrigger("ExitCar");
        if (cameraAnimator != null) cameraAnimator.SetTrigger("CarExit");

        AnimationCamera.SetActive(true);
        StartCoroutine(ExitCoroutine());

        insideCar = false;
    }

    IEnumerator PauseForCamera()
    {
        yield return new WaitForSeconds(1.2f);
        AnimationCamera.SetActive(false);
        if (CarCamera != null) CarCamera.SetActive(true);
        if (ExitText != null) ExitText.SetActive(true);
    }

    IEnumerator ExitCoroutine()
    {
        yield return new WaitForSeconds(1.2f);
        AnimationCamera.SetActive(false);
        if (CarCamera != null) CarCamera.SetActive(false);
        if (ExitPlayer != null) ExitPlayer.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !insideCar)
        {
            nearCar = true;
            EnterText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !insideCar)
        {
            nearCar = false;
            EnterText.SetActive(false);
        }
    }
}
