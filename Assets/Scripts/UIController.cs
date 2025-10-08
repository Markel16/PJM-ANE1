using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField nameInput;     // Campo de texto para el nombre
    public TMP_InputField ageInput;      // Campo de texto para la edad
    public TMP_InputField searchIdInput; // Campo de texto para buscar por ID

    [Header("Botones")]
    public Button addButton;
    public Button listAllButton;
    public Button searchByIdButton;
    public Button showOldestButton;

    [Header("Lista (ScrollView)")]
    public Transform listContent;      // Contenedor donde se instancian las filas
    public GameObject userRowPrefab;   // Prefab de una fila (UserRow)

    [Header("Panel Eliminación (Modal)")]
    public GameObject deleteModal;     // Panel de confirmación
    public TMP_Text deleteTitleText;   // Texto del panel (“Eliminar usuario X”)
    public Slider deleteSlider;        // Barra de progreso 0→1
    public Button confirmDeleteButton;
    public Button cancelDeleteButton;

    [Header("Toast (Mensajes temporales)")]
    public CanvasGroup toastGroup;     // Controla transparencia del toast
    public TMP_Text toastText;         // Texto del mensaje
    public float toastDuration = 1.8f; // Duración visible del toast (segundos)

    // Variables internas
    private int _pendingDeleteUserId = -1; // ID del usuario que se quiere borrar
    private Coroutine _deleteRoutine;      // Para detener la corutina si se cancela
    private Coroutine _toastRoutine;       // Para detener el toast anterior si se muestra otro

    void Start()
    {
        
        addButton.onClick.AddListener(OnAddUser);
        listAllButton.onClick.AddListener(RefreshListAll);
        searchByIdButton.onClick.AddListener(OnSearchById);
        showOldestButton.onClick.AddListener(OnShowOldest);

        
        deleteModal.SetActive(false);
        toastGroup.alpha = 0f;

        
        if (nameInput != null && nameInput.characterLimit == 0)
            nameInput.characterLimit = 24;
    }

    // -------------------- AÑADIR USUARIO --------------------
    private void OnAddUser()
    {
        string name = (nameInput.text ?? string.Empty).Trim();
        string ageStr = (ageInput.text ?? string.Empty).Trim();

        // Validaciones
        if (string.IsNullOrEmpty(name))
        {
            ShowToast("El nombre no puede estar vacío.");
            return;
        }

        if (!int.TryParse(ageStr, out int age) || age < 0 || age > 120)
        {
            ShowToast("Edad inválida (0-120).");
            return;
        }

        
        var user = RepositorioUser.Instance.AddUser(name, age);

        // Mensaje y limpieza
        ShowToast($"Usuario #{user.Id} añadido.");
        nameInput.text = string.Empty;
        ageInput.text = string.Empty;

        RefreshListAll();
    }

    // -------------------- LISTAR TODOS --------------------
    private void RefreshListAll()
    {
        var list = RepositorioUser.Instance.GetAll();
        RenderList(list);
    }

    // -------------------- BUSCAR POR ID --------------------
    private void OnSearchById()
    {
        string idStr = (searchIdInput.text ?? string.Empty).Trim();

        if (!int.TryParse(idStr, out int id))
        {
            ShowToast("ID inválido.");
            return;
        }

        var user = RepositorioUser.Instance.GetById(id);

        if (user == null)
        {
            RenderList(new List<User>());
            ShowToast($"No existe el usuario #{id}.");
        }
        else
        {
            RenderList(new List<User> { user });
            ShowToast($"Mostrando usuario #{id}.");
        }
    }

    // -------------------- MOSTRAR MÁS MAYORES --------------------
    private void OnShowOldest()
    {
        var oldest = RepositorioUser.Instance.GetOldest();
        RenderList(oldest);

        if (oldest.Count == 0)
            ShowToast("No hay usuarios.");
        else
            ShowToast($"Mayor(es) con {oldest[0].Age} años.");
    }

    // -------------------- RENDERIZAR LISTA --------------------
    private void RenderList(List<User> users)
    {
        
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        
        foreach (var u in users)
        {
            var go = Instantiate(userRowPrefab, listContent);
            var row = go.GetComponent<UserRowUI>();
            row.Setup(this, u);
        }
    }

    // -------------------- ELIMINAR CON RETARDO --------------------
    public void RequestDeleteUser(int userId, UserRowUI row)
    {
        _pendingDeleteUserId = userId;
        deleteTitleText.text = $"Eliminar usuario #{userId}";
        deleteSlider.value = 0f;
        deleteModal.SetActive(true);

        
        confirmDeleteButton.onClick.RemoveAllListeners();
        cancelDeleteButton.onClick.RemoveAllListeners();

        confirmDeleteButton.onClick.AddListener(() => StartDeleteCountdown(userId));
        cancelDeleteButton.onClick.AddListener(CancelDelete);
    }

    private void StartDeleteCountdown(int userId)
    {
        if (_deleteRoutine != null)
            StopCoroutine(_deleteRoutine);

        _deleteRoutine = StartCoroutine(DeleteCountdownRoutine(userId, 3f));
    }

    private IEnumerator DeleteCountdownRoutine(int userId, float seconds)
    {
        deleteSlider.value = 0f;
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            deleteSlider.value = Mathf.Clamp01(t / seconds);
            yield return null;
        }

        bool ok = RepositorioUser.Instance.RemoveUser(userId);
        deleteModal.SetActive(false);
        _pendingDeleteUserId = -1;

        if (ok) ShowToast($"Usuario #{userId} eliminado.");
        else ShowToast($"Usuario #{userId} no existe.");

        RefreshListAll();
    }

    private void CancelDelete()
    {
        if (_deleteRoutine != null)
        {
            StopCoroutine(_deleteRoutine);
            _deleteRoutine = null;
        }

        deleteModal.SetActive(false);
        _pendingDeleteUserId = -1;
        ShowToast("Eliminación cancelada.");
    }

    // -------------------- TOASTS (mensajes temporales) --------------------
    public void ShowToast(string message)
    {
        if (_toastRoutine != null)
            StopCoroutine(_toastRoutine);

        _toastRoutine = StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string msg)
    {
        toastText.text = msg;

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(toastGroup, 0f, 1f, 0.15f));

        // Mantener visible
        yield return new WaitForSeconds(toastDuration);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(toastGroup, 1f, 0f, 0.25f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float a, float b, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(a, b, t / dur);
            yield return null;
        }
        cg.alpha = b;
    }
}
