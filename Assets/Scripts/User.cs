using System;
using UnityEngine;


[Serializable]
public class User : MonoBehaviour
{
    public int Id; // ID único generado por la app
    public string Name; // Nombre del usuario
    public int Age; // Edad


    public User(int id, string name, int age)
    {
        Id = id;
        Name = name;
        Age = age;
    }
}