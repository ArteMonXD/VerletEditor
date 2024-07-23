using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using System.Linq;
using static UnityEditor.PlayerSettings;
using static UnityEngine.ParticleSystem;
using UnityEditor.Presets;
using UnityEngine.UI;
using UnityEngine.UIElements;

[EditorTool("Verlet Physics Editor", typeof(VerletPhysics))]
public class VerletPhysicsEditor : EditorTool
{
    private VerletPhysics m_Physics;
    private List<ParticleGUIButtons> particleButtons = new List<ParticleGUIButtons>();
    private List<LinkGUIButton> linkButtons = new List<LinkGUIButton>();
    private ISelected singleSelectElement;
    private List<ISelected> allSelectElements = new List<ISelected>();
    private List<ISelected> preselectElements = new List<ISelected>();
    private LinkGUIButton preselectLine;
    private Dictionary<string, bool> allCurrentInput = new Dictionary<string, bool>();
    private bool isParticle = true;
    private bool isLink = false;
    private int m_DefaultControl;
    private bool buttonLM;
    private bool allocateStart = false;
    private Vector2 firstMousePos;
    private int allocateHandleID;
    private void Init()
    {
        if (m_Physics == null)
            m_Physics = target as VerletPhysics;
        InitInput();
    }
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView))
            return;

        CheckInput();
        CreateInterfaceButton();
        if (isParticle)
        {
            CreateButtons(particleButtons, m_Physics.particleList, Handles.DotHandleCap);
            CreateNoInteractLine();
            CreateHandlePositions<ParticleGUIButtons, Particle>();
            //Allocation<ParticleGUIButtons, Particle>(particleButtons);
        }
        else if (isLink)
        {
            CheckLine();
            CreateButtons(linkButtons, m_Physics.linkList);
            CreateHandlePositions<LinkGUIButton, Link>();
            //Allocation<LinkGUIButton, Link>(linkButtons);
            BlockMouseInput();
        }
    }
    private void SingleSelectedControl(ISelected selected)
    {
        if (allSelectElements.Count > 0)
        {
            foreach (ISelected s in allSelectElements)
            {
                s.Select();
            }
            allSelectElements.Clear();
        }
        if (singleSelectElement != null)
        {
            if (singleSelectElement == selected)
            {
                selected.Select();
                singleSelectElement = null;
            }
            else
            {
                singleSelectElement.Select();
                selected.Select();
                singleSelectElement = selected;
            }
        }
        else
        {
            selected.Select();
            singleSelectElement = selected;
        }
    }
    private void MultiSelectedControl(ISelected selected)
    {
        if (singleSelectElement != null)
        {
            if (singleSelectElement == selected)
            {
                selected.Select();
                singleSelectElement = null;
            }
            else
            {
                allSelectElements.Add(singleSelectElement);
                singleSelectElement = null;
                allSelectElements.Add(selected);
                selected.Select();
            }
        }
        else
        {
            if (selected.IsSelect)
            {
                if (allSelectElements.Exists(p => p == selected))
                    allSelectElements.Remove(selected);
            }
            else
            {
                allSelectElements.Add(selected);
            }
            selected.Select();
        }
    }
    private void CreateInterfaceButton()
    {
        Handles.BeginGUI();
        GUILayout.BeginHorizontal();
        GUILayout.BeginArea(new Rect(55, 10, 170, 100));
        if (GUILayout.Button("Create Particle", GUILayout.Width(150)))
        {
            Debug.Log("Create Particle");
            m_Physics.CreateParticles();
            UpdateButtonsID<ParticleGUIButtons, Particle>(particleButtons);
        }
        if (GUILayout.Button("Working with Particles", GUILayout.Width(150)))
        {
            Debug.Log("Working with Particles");
            if (!isParticle)
            {
                isParticle = true;
                isLink = false;
                ClearSelect();
            }
            else if(isLink) isLink = false;
        }
        if (GUILayout.Button("Working with Links", GUILayout.Width(150)))
        {
            Debug.Log("Working with Links");
            if (!isLink)
            {
                isLink = true;
                isParticle = false;
                ClearSelect();
            }
            else if(isParticle) isParticle = false;
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(210, 10, 170, 100));
        if (singleSelectElement != null || allSelectElements.Count > 0)
        {
            if (GUILayout.Button("Delete", GUILayout.Width(150)))
            {
                Debug.Log("Delete");
                if(isParticle)
                    DeleteElements(particleButtons, m_Physics.particleList);
                else if (isLink)
                    DeleteElements(linkButtons, m_Physics.linkList);
            }
        }
        GUILayout.EndArea();
        GUILayout.EndHorizontal();
        Handles.EndGUI();
    }
    private void CreateButtons<B, E>(List<B> buttonList, List<E> elementList, Handles.CapFunction capFunction = null) 
        where B: GUIButton<E>, new()
        where E : IID
    {
        CheckingListDifferences(buttonList, elementList);
        foreach (B button in buttonList)
        {
            if (button.Element.GetID == 0)
                continue;
            button.UpdatePosition();
            button.UpdateButtonSize();
            Vector3 pos = button.Position;
            if (button.IsSelect) Handles.color = Handles.selectedColor;
            else if (button.IsPreselect) Handles.color = Handles.preselectionColor;
            if (button.Element is Particle)
            {
                if(!button.IsSelect && !button.IsPreselect) Handles.color = Color.cyan;
                if (Handles.Button(pos, Quaternion.identity, button.Size.x, button.Size.y, capFunction))
                {
                    Debug.Log($"The button was pressed particle {button.Element.GetID}!");

                    if (allCurrentInput["LeftShift"]) MultiSelectedControl(button);
                    else if (allCurrentInput["LeftControl"])
                    {
                        if (button is ParticleGUIButtons)
                        {
                            ParticleGUIButtons particleButton = button as ParticleGUIButtons;
                            CreateLink(particleButton);
                        }
                    }
                    else SingleSelectedControl(button);
                }
            }
            else if (button.Element is Link)
            {
                if(!button.IsSelect && !button.IsPreselect) Handles.color = Color.black;
                LinkGUIButton linkGUIButton = button as LinkGUIButton;
                Handles.DrawLine(linkGUIButton.Element.ParticleA.Position, linkGUIButton.Element.ParticleB.Position, linkGUIButton.Size.x);
            }
        }
    }
    private void CreateNoInteractLine()
    {
        CheckingListDifferences(linkButtons, m_Physics.linkList);
        foreach (LinkGUIButton button in linkButtons)
        {
            if (button.Element.GetID == 0)
                continue;
            button.UpdatePosition();
            button.UpdateButtonSize();
            Handles.color = Color.black;
            Handles.DrawLine(button.Element.ParticleA.Position, button.Element.ParticleB.Position, button.Size.x / 2f);
        }
    }
    private void CheckingListDifferences<B, E>(List<B> buttonList, List<E> elementList) 
        where B : GUIButton<E>, new()
        where E : IID
    {
        m_Physics.UpdateID(elementList);
        m_Physics.RecalculateCenterAllLinks();
        UpdateButtonsID<B, E>(buttonList);
        if (buttonList.Count != elementList.Count)
        {
            if (buttonList.Count == 0)
            {
                for (int i = 0; i < elementList.Count; i++)
                {
                    B button = new B();
                    if (button.SetData(elementList[i]))
                    {
                        buttonList.Add(button);
                    }
                }
            }
            else
            {
                int difference = elementList.Count - buttonList.Count;
                if (difference > 0)
                {
                    var differenceElements = elementList.OrderBy(p => p.GetID)
                                                         .Select(p => p)
                                                         .Except(buttonList.OrderBy(p => p.Element.GetID)
                                                                           .Select(p => p.Element))
                                                         .ToArray();

                    foreach (E p in differenceElements)
                    {
                        B button = new B();
                        if (button.SetData(p))
                        {
                            buttonList.Add(button);
                        }
                    }
                }
                else if (difference < 0f)
                {
                    var differenceButtons = buttonList.OrderBy(p => p.GetID)
                                                      .Select(p => p.GetID)
                                                      .Except(elementList.OrderBy(p => p.GetID)
                                                                         .Select(p => p.GetID))
                                                      .ToArray();
                    List<B> deleteButton = new List<B>(); 
                    foreach(int id in differenceButtons)
                    {
                        foreach(B b in buttonList)
                        {
                            if (b.GetID == id)
                                deleteButton.Add(b);
                            else
                                continue;
                        }
                    }
                    foreach(B dB in deleteButton)
                    {
                        buttonList.Remove(dB);
                    }
                    m_Physics.UpdateID(m_Physics.particleList);
                }
                UpdateButtonsID<B, E>(buttonList);
            }
        }
    }
    private void CreateHandlePositions<B, T>()
        where B : GUIButton<T>
        where T : IID
    {
        if (singleSelectElement != null || allSelectElements.Count > 0)
        {
            if (singleSelectElement != null)
            {
                Undo.RecordObject(m_Physics, "Move Position Point");
                B currentButton = singleSelectElement as B;
                Vector3 pos = currentButton.Position;
                EditorGUI.BeginChangeCheck();
                pos = Handles.PositionHandle(pos, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    currentButton.HandleUpdatePosition(pos);
                    if (currentButton.Element is Particle)
                        m_Physics.RecalculateCenterLinks(currentButton.Element.GetID);
                    else if (currentButton.Element is Link)
                        (currentButton.Element as Link).RecalculateCenterPosition();
                    HandleUtility.Repaint();
                }
            }
            else if (allSelectElements.Count > 0)
            {
                Undo.RecordObject(m_Physics, "Multi Move Position Points");
                B lastButtom = allSelectElements[allSelectElements.Count - 1] as B;
                Vector3 pos = lastButtom.Position;
                Vector3 lastPos = pos;
                EditorGUI.BeginChangeCheck();
                pos = Handles.PositionHandle(pos, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    lastButtom.HandleUpdatePosition(pos);
                    if (lastButtom.Element is Particle)
                        m_Physics.RecalculateCenterLinks(lastButtom.Element.GetID);
                    Vector3 delta = pos - lastPos;
                    for (int i = 0; i < allSelectElements.Count - 1; i++)
                    {
                        B button = allSelectElements[i] as B;
                        button.HandleUpdatePositionDelta(delta);
                        if (button.Element is Particle)
                            m_Physics.RecalculateCenterLinks(button.Element.GetID);
                    }
                    if(lastButtom.Element is Link)
                    {
                        List<int> recalculateLinks = new List<int>();
                        for (int i = 0; i<allSelectElements.Count; i++)
                        {
                            B button = allSelectElements[i] as B;
                            recalculateLinks.Add(button.GetID);
                        }
                        m_Physics.RecalculateCenterGroupLinks(recalculateLinks);
                    }
                    HandleUtility.Repaint();
                }
            }
        }
    }
    private void CheckInput()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift)
        {
            Debug.Log("Left Shift Press");
            allCurrentInput["LeftShift"] = true;
            e.Use();
        }
        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftShift)
        {
            Debug.Log("Left Shift Unpress");
            allCurrentInput["LeftShift"] = false;
            e.Use();
        }
        if(e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftControl)
        {
            Debug.Log("Left Control Press");
            allCurrentInput["LeftControl"] = true;
            e.Use();
        }
        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftControl)
        {
            Debug.Log("Left Control Unpress");
            allCurrentInput["LeftControl"] = false;
            e.Use();
        }
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log("Left Mouse Button Press");;
            allCurrentInput["LMButton"] = true;
            firstMousePos = e.mousePosition;
            if(!allocateStart) allocateStart = true;
            allocateHandleID = GUIUtility.hotControl;
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            Debug.Log("Left Mouse Button Unpress");
            allCurrentInput["LMButton"] = false;
            buttonLM = false;
        }
        else if(GUIUtility.hotControl != allocateHandleID)
        {
            Debug.Log("Left Mouse Button Unpress");
            allCurrentInput["LMButton"] = false;
            buttonLM = false;
        }
    }
    private void InitInput()
    {
        if (!allCurrentInput.ContainsKey("LeftShift")) allCurrentInput.Add("LeftShift", false);
        if (!allCurrentInput.ContainsKey("LeftControl")) allCurrentInput.Add("LeftControl", false);
        if (!allCurrentInput.ContainsKey("LMButton")) allCurrentInput.Add("LMButton", false);
    }
    private void DeleteElements<B, E>(List<B> buttons, List<E> elements)
        where B : GUIButton<E>
        where E : IID
    {
        if(singleSelectElement != null || allSelectElements.Count > 0)
        {
            if(singleSelectElement!= null)
            {
                B button = singleSelectElement as B;
                if (button.Element is Particle) DestructionLineButtonDueParticle(button.Element.GetID);
                elements.Remove(button.Element);
                buttons.Remove(button);
            }
            if(allSelectElements.Count > 0)
            {
                for(int i = 0; i<allSelectElements.Count; i++)
                {
                    B button = allSelectElements[i] as B;
                    if (button.Element is Particle) DestructionLineButtonDueParticle(button.Element.GetID);
                    elements.Remove(button.Element);
                    buttons.Remove(button);
                }
                allSelectElements.Clear();
            }
            HandleUtility.Repaint();
        }
    }
    private void CheckLine()
    {
        if (GUIUtility.hotControl != 0 || (allCurrentInput["LMButton"] && buttonLM))
            return;
        List<LinkGUIButton> preselectLines = new List<LinkGUIButton>();
        foreach (LinkGUIButton line in linkButtons)
        {
            float distance = HandleUtility.DistanceToLine(line.Element.ParticleA.Position, line.Element.ParticleB.Position);
            if (distance <= line.Size.y) preselectLines.Add(line);
            else
            {
                if (line.IsPreselect)
                {
                    line.Preselect();
                    if(preselectLine == line)
                        preselectLine = null;
                    HandleUtility.Repaint();
                }
            }
        }
        LinkGUIButton nearestLine = null;
        float currentDistance = 999f;
        for(int i = 0; i<preselectLines.Count; i++)
        {
            if(i == 0)
            {
                currentDistance = HandleUtility.DistanceToLine(preselectLines[i].Element.ParticleA.Position, preselectLines[i].Element.ParticleB.Position);
                nearestLine = preselectLines[i];
            }
            else
            {
                float distance = HandleUtility.DistanceToLine(preselectLines[i].Element.ParticleA.Position, preselectLines[i].Element.ParticleB.Position);
                if(currentDistance > distance)
                {
                    currentDistance = distance;
                    if(nearestLine != null && nearestLine.IsPreselect)
                    {
                        nearestLine.Preselect();
                        if (preselectLine == nearestLine)
                            preselectLine = null;
                    } 
                    nearestLine = preselectLines[i];
                }
                else
                {
                    if (preselectLines[i] != null && preselectLines[i].IsPreselect)
                    {
                        preselectLines[i].Preselect();
                        if(preselectLine == preselectLines[i])
                            preselectLine = null;
                    }
                }
            }
        }
        if (nearestLine != null && !nearestLine.IsPreselect)
        {
            nearestLine.Preselect();
            preselectLine = nearestLine;
            HandleUtility.Repaint();
        }
        preselectLines.Clear();
        if (allCurrentInput["LMButton"])
        {
            if (preselectLine == null)
            {
                if(!buttonLM)
                    buttonLM = true;
                return;
            }
            if (!buttonLM && GUIUtility.hotControl == 0)
            {
                Debug.Log("Select Line");
                if (allCurrentInput["LeftShift"]) MultiSelectedControl(preselectLine);
                else SingleSelectedControl(preselectLine);
                HandleUtility.Repaint();
                buttonLM = true;
            }
        }
    }
    private void CreateLink(ParticleGUIButtons particleButton)
    {
        if (singleSelectElement == null)
            SingleSelectedControl(particleButton);
        else
            m_Physics.CreateLink((singleSelectElement as ParticleGUIButtons).Element, particleButton.Element);
    }
    private void UpdateButtonsID<B, E>(List<B> buttonList)
        where B : GUIButton<E>
        where E : IID
    {
        foreach(B b in buttonList)
        {
            b.UpdateID();
        }
    }
    private void Allocation<B, E>(List<B> buttons) 
        where B : GUIButton<E>
        where E : IID
    {
        if (!allocateStart)
            return;

        var box = CalculateBox(Event.current.mousePosition);
        if (allCurrentInput["LMButton"])
        {
            Debug.Log("StartAllocate");
            foreach (B b in buttons)
            {
                if (CheckInBox(box, b.GetScreenPosition()))
                {
                    if (!preselectElements.Contains(b)) preselectElements.Add(b);
                    if (!b.IsPreselect) b.Preselect();
                }
                else
                {
                    if (preselectElements.Contains(b)) preselectElements.Remove(b);
                    if (b.IsPreselect) b.Preselect();
                }
            }
            HandleUtility.Repaint();
        }
        else
        {
            Debug.Log("EndAllocate");
            foreach (B b in buttons)
            {
                if (CheckInBox(box, b.GetScreenPosition()))
                {
                    if (!preselectElements.Contains(b)) preselectElements.Add(b);
                    if (!b.IsPreselect) b.Preselect();
                }
                else
                {
                    if (preselectElements.Contains(b)) preselectElements.Remove(b);
                    if (b.IsPreselect) b.Preselect();
                }
            }
            if(preselectElements.Count == 1)
            {
                singleSelectElement = preselectElements[0];
                if(!singleSelectElement.IsSelect) singleSelectElement.Select();
                if (singleSelectElement.IsPreselect) singleSelectElement.Preselect();
            }
            else if (preselectElements.Count > 1)
            {
                foreach(ISelected b in preselectElements)
                {
                    allSelectElements.Add(b);
                    if (!b.IsSelect) b.Select();
                    if (b.IsPreselect) b.Preselect();
                }
            }
            preselectElements.Clear();
            HandleUtility.Repaint();
            if (allocateStart) allocateStart = false;
        }
    }
    private float[] CalculateBox(Vector2 currentPos)
    {
        float[] result = new float[4];
        if(firstMousePos.x > currentPos.x)
        {
            result[0] = currentPos.x;
            result[2] = firstMousePos.x;
        }
        else
        {
            result[2] = currentPos.x;
            result[0] = firstMousePos.x;
        }
        if (firstMousePos.y > currentPos.y)
        {
            result[1] = currentPos.x;
            result[3] = firstMousePos.x;
        }
        else
        {
            result[3] = currentPos.x;
            result[1] = firstMousePos.x;
        }
        return result;
    }
    private bool CheckInBox(float[] box, Vector2 elementPos)
    {
        if (elementPos.x > box[0] &&
           elementPos.y > box[1] &&
           elementPos.x < box[2] &&
           elementPos.y < box[3]) return true;
        else return false;
    }
    private void ClearSelect()
    {
        if (singleSelectElement != null) 
        {
            if (singleSelectElement.IsSelect) singleSelectElement.Select();
            singleSelectElement = null;
        }
        if (allSelectElements.Count > 0)
        {
            foreach(ISelected s in allSelectElements)
            {
                if(s.IsSelect) s.Select();
            }
            allSelectElements.Clear();
        }
        if(preselectLine != null)
        {
            if (preselectLine.IsPreselect) preselectLine.Preselect();
            preselectLine = null;
        }
        if(preselectElements.Count > 0)
        {
            foreach (ISelected s in preselectElements)
            {
                if (s.IsPreselect) s.Preselect();
            }
            preselectElements.Clear();
        }
    }
    private void BlockMouseInput()
    {
        if (preselectLine != null || preselectElements.Count > 0)
        {
            m_DefaultControl = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_DefaultControl);
        }
    }
    public void DestructionLineButtonDueParticle(int id)
    {
        m_Physics.DestructionLinkDueParticle(id);
        var removeLinks = linkButtons.Where(l => l.Element.ParticleA.GetID == id || l.Element.ParticleB.GetID == id)
                              .ToArray();
        foreach (LinkGUIButton l in removeLinks)
        {
            linkButtons.Remove(l);
        }
    }
    private void OnEnable()
    {
        Init();
    }
    private void OnDisable()
    {
        particleButtons.Clear();
        linkButtons.Clear();
        singleSelectElement = null;
        allSelectElements.Clear();
    }
}