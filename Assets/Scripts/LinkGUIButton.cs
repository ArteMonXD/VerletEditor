using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.UIElements;
using System.Drawing;
using UnityEditor;

public class LinkGUIButton : GUIButton<Link>
{
    private bool isSelect;
    override public bool IsSelect => isSelect;
    private bool isPreselect;
    override public bool IsPreselect => isPreselect;
    private int ID;
    override public int GetID => ID;
    private Vector3 linkCenter;
    override public Vector3 Position => linkCenter;
    private Link link;
    override public Link Element => link;
    private Vector2 size;
    override public Vector2 Size => size;

    override public void Select()
    {
        isSelect = !isSelect;
    }
    override public void Preselect()
    {
        isPreselect = !isPreselect;
    }
    public LinkGUIButton(Link _link)
    {
        isSelect = false;
        link = _link;
        linkCenter = link.CenterPosition;
        ID = link.GetID;
        UpdateButtonSize();
    }
    public LinkGUIButton()
    {
        isSelect = false;
        link = null;
        linkCenter = Vector3.zero;
        ID = 0;
        size = Vector2.zero;
    }
    public override bool SetData(Link element)
    {
        if (!(element is Link))
            return false;

        Link _link = element as Link;
        isSelect = false;
        link = _link;
        linkCenter = link.CenterPosition;
        ID = link.GetID;
        UpdateButtonSize();
        return true;
    }
    override public void HandleUpdatePosition(Vector3 pos)
    {
        linkCenter = pos;
        link.UpdatePositionLink(linkCenter);
    }
    override public void HandleUpdatePositionDelta(Vector3 delta)
    {
        linkCenter = linkCenter + delta;
        link.UpdatePositionLink(linkCenter);
    }
    override public void UpdateButtonSize()
    {
        float visualSize = HandleUtility.GetHandleSize(linkCenter)*4f;
        float interactiveSize = HandleUtility.GetHandleSize(linkCenter)*10f;
        size = new Vector2(visualSize, interactiveSize);
    }
    public override void UpdatePosition()
    {
        linkCenter = link.CenterPosition;
    }
    override public void UpdateID()
    {
        ID = link.GetID;
    }
}
