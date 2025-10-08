using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;


public class UserRowUI : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text txtId;
    public TMP_Text txtName;
    public TMP_Text txtAge;
    public Button btnDelete;


    private int _userId;
    private UIController _ui; // referencia al controlador principal


    public void Setup(UIController ui, User user)
    {
        _ui = ui;
        _userId = user.Id;
        txtId.text = user.Id.ToString();
        txtName.text = user.Name;
        txtAge.text = user.Age.ToString();


        btnDelete.onClick.RemoveAllListeners();
        btnDelete.onClick.AddListener(() => _ui.RequestDeleteUser(_userId, this));
    }
    public void deleteUser() 
    {
        Destroy(gameObject);
    }
}