using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Link : IID
{
    [SerializeField] private int ID;
    public int GetID => ID;
    [SerializeField] private Vector3 centerPosition;
    public Vector3 CenterPosition => centerPosition;
    [SerializeField] private Particle particleA;
    public Particle ParticleA => particleA;
    [SerializeField] private Particle particleB;
    public Particle ParticleB => particleB;

    public Link(Vector3 pointA, int aID, Vector3 pointB, int bID, int id)
    {
        particleA = new Particle(pointA, aID);
        particleB = new Particle(pointB, bID);
        centerPosition = CalculateCenterPosition(pointA, pointB);
        ID = id;
    }

    public Link(Particle _particleA, Particle _particleB, int id)
    {
        particleA = _particleA;
        particleB = _particleB;
        centerPosition = CalculateCenterPosition(particleA.Position, particleB.Position);
        ID = id;
    }
    public void UpdatePositionLink(Vector3 pos)
    {
        Vector3 delta = pos - centerPosition;
        centerPosition = pos;
        particleA.Position = particleA.Position + delta;
        particleB.Position = particleB.Position + delta;
    }
    private Vector3 CalculateCenterPosition(Vector3 particleAPos, Vector3 particleBPos)
    {
        float x = (particleBPos.x + particleAPos.x) / 2;
        float y = (particleBPos.y + particleAPos.y) / 2;
        float z = (particleBPos.z + particleAPos.z) / 2;

        return new Vector3(x, y, z);
    }
    public void RecalculateCenterPosition()
    {
        centerPosition = CalculateCenterPosition(particleA.Position, particleB.Position);
    }
    public void UpdateID(int id)
    {
        ID = id;
    }
    public void UpdateData(Particle partA, Particle partB)
    {
        particleA = partA;
        particleB = partB;
        CalculateCenterPosition(partA.Position, partB.Position);
    }
}
