using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RepositorioUser : MonoBehaviour
{
    // Singleton sencillo para acceso desde la UI
    public static RepositorioUser Instance { get; private set; }


    private readonly List<User> _users = new List<User>();
    private int _nextId = 1; // contador de IDs únicos


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public User AddUser(string name, int age)
    {
        var user = new User(_nextId++, name, age);
        _users.Add(user);
        return user;
    }


    public bool RemoveUser(int id)
    {
        var u = _users.FirstOrDefault(x => x.Id == id);
        if (u != null)
        {
            _users.Remove(u);
            return true;
        }
        return false;
    }


    public User GetById(int id) => _users.FirstOrDefault(x => x.Id == id);


    public List<User> GetAll() => new List<User>(_users); // copia defensiva


    public List<User> GetOldest()
    {
        if (_users.Count == 0) return new List<User>();
        int maxAge = _users.Max(u => u.Age);
        return _users.Where(u => u.Age == maxAge).ToList();
    }
}