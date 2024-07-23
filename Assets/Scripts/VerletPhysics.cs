using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.ParticleSystem;

public class VerletPhysics : MonoBehaviour
{
    public List<Link> linkList = new List<Link>();
    public List<Particle> particleList = new List<Particle>();

    public void CreateParticles()
    {
        UpdateID(particleList);
        particleList.Add(new Particle(transform.position, particleList.Count + 1));
    }
    public void UpdateID<T>(List<T> list) where T : IID
    {
        for(int i = 0; i<list.Count; i++)
        {
            T element = list[i];
            element.UpdateID(i + 1);
            if(element is Link)
            {
                Link link = element as Link;
                link.UpdateData(GetParticle(link.ParticleA.GetID), GetParticle(link.ParticleB.GetID));
            }
        }
    }
    public void CreateLink(Particle pA, Particle pB)
    {
        if (CheckDuplicate(pA, pB))
            return;
        UpdateID(linkList);
        linkList.Add(new Link(pA, pB, linkList.Count + 1));
    }
    private bool CheckDuplicate(Particle pA, Particle pB)
    {
        Link findDuplicate = linkList.FirstOrDefault(p =>
                                                          (p.ParticleA.GetID == pA.GetID && p.ParticleB.GetID == pB.GetID) ||
                                                          (p.ParticleB.GetID == pA.GetID && p.ParticleA.GetID == pB.GetID));
        if(findDuplicate == null) return false;
        else return true;
    }
    private Particle GetParticle(int id)
    {
        Particle findParticle = particleList.FirstOrDefault(p => p.GetID == id);
        return findParticle;
    }
    private Link GetLink(int id)
    {
        Link findLink = linkList.FirstOrDefault(p => p.GetID == id);
        return findLink;
    }
    public void RecalculateCenterLinks(int id)
    {
        var updateLinks = linkList.Where(l => l.ParticleA.GetID == id || l.ParticleB.GetID == id)
                              .ToArray();
        foreach (Link l in updateLinks)
        {
            l.RecalculateCenterPosition();
        }
    }
    public void RecalculateCenterGroupLinks(List<int>IDs)
    {
        foreach (int l in IDs)
        {
            GetLink(l).RecalculateCenterPosition();
        }
    }
    public void RecalculateCenterAllLinks()
    {
        foreach (Link l in linkList)
        {
            l.RecalculateCenterPosition();
        }
    }
    public void DestructionLinkDueParticle(int id)
    {
        var removeLinks = linkList.Where(l => l.ParticleA.GetID == id || l.ParticleB.GetID == id)
                              .ToArray();
        foreach (Link l in removeLinks)
        {
            linkList.Remove(l);
        }
    }
}
