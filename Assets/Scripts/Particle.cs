using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Particle : IID
{
    [SerializeField] private int ID;
    public int GetID => ID;
    [SerializeField] private Vector3 position;
    public Vector3 Position { get { return position; }
                              set { position = value; }}
    public Particle(Vector3 pos, int id)
    {
        position = pos;
        ID = id;
    }
    public void UpdateID(int id)
    {
        ID = id;
    }
}
