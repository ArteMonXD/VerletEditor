using UnityEditor;
using UnityEngine;

public class ParticleGUIButtons : GUIButton<Particle>
{
    private bool isSelect;
    override public bool IsSelect => isSelect;
    private bool isPreselect;
    public override bool IsPreselect => isPreselect;
    private Vector3 position;
    override public Vector3 Position => position;
    private Vector2 size;
    override public Vector2 Size => size;
    private Particle particle;
    override public Particle Element => particle;
    private int ID;
    override public int GetID => ID;

    override public void Select()
    {
        isSelect = !isSelect;
    }
    public override void Preselect()
    {
        isPreselect = !isPreselect;
    }
    public ParticleGUIButtons(Particle _particle)
    {
        isSelect = false;
        position = _particle.Position;
        particle = _particle;
        ID = _particle.GetID;
        UpdateButtonSize();
    }
    public ParticleGUIButtons()
    {
        isSelect = false;
        position = Vector3.zero;
        particle = null;
        ID = 0;
        size = Vector2.zero;
    }
    override public bool SetData(Particle _particle)
    {
        if (!(_particle is Particle))
            return false;
        
        Particle newParticle = _particle as Particle;
        isSelect = false;
        position = newParticle.Position;
        particle = newParticle;
        ID = newParticle.GetID;
        UpdateButtonSize();
        return true;
    }
    override public void HandleUpdatePosition(Vector3 pos)
    {
        position = pos;
        particle.Position = position;
    }
    override public void HandleUpdatePositionDelta(Vector3 delta)
    {
        position = position + delta;
        particle.Position = position;
    }
    override public void UpdatePosition()
    {
        position = particle.Position;
    }
    override public void UpdateButtonSize()
    {
        float simpleSize = HandleUtility.GetHandleSize(position) / 20f;
        size = new Vector2(simpleSize, simpleSize);
    }
    override public void UpdateID()
    {
        ID = particle.GetID;
    }
}