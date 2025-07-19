using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityTools.Systems.Inputs;

[RequireComponent(typeof(SelectionRectangleDrawer))]
public class GamePlayer : MonoBehaviour
{
    [SerializeField] private InputManager.Input m_clickInput;
    [SerializeField] private List<Selectable> m_selectedObjects = new List<Selectable>();

    [SerializeField] private Vector2 mouseWorldPos;

    [SerializeField] private Vector2 firstClickWorldPos;

    [SerializeField] private Vector2 botLeftPos;
    [SerializeField] private Vector2 topRightPos;

    [SerializeField] private Vector2 boxSize;
    [SerializeField] private Vector2 boxCenter;
    private bool isSelecting;

    private Vector2 m_firstClickScreenPos = Vector2.zero;

    private Vector2 m_secondScreenPos = Vector2.zero;

    private void Start()
    {
        InputManager.RegisterInput(m_clickInput
            , new InputManager.InputEvent(OnClick_Performed, InputActionPhase.Performed)
            , true);
        InputManager.RegisterInput(m_clickInput
            , new InputManager.InputEvent(OnClick_Canceled, InputActionPhase.Canceled)
            , true);
    }

    private void Update()
    {
        if (isSelecting)
        {
            GetComponent<SelectionRectangleDrawer>().pos1 = m_firstClickScreenPos;
            GetComponent<SelectionRectangleDrawer>().pos2 = m_secondScreenPos;

            if (Camera.main == null) return;

            firstClickWorldPos = Camera.main.ScreenToWorldPoint(m_firstClickScreenPos);

            mouseWorldPos = Camera.main.ScreenToWorldPoint(m_secondScreenPos);

            //bottom left corner of the box defined by firstClickWorldPos and mouseWorldPos
            botLeftPos = new Vector2(Math.Min(firstClickWorldPos.x, mouseWorldPos.x),
                Math.Min(firstClickWorldPos.y, mouseWorldPos.y));

            //top right corner of the box defined by firstClickWorldPos and mouseWorldPos
            topRightPos = new Vector2(Math.Max(firstClickWorldPos.x, mouseWorldPos.x),
                Math.Max(firstClickWorldPos.y, mouseWorldPos.y));

            // Middle point between the two to get real box center.
            boxSize = topRightPos - botLeftPos;
            boxCenter = botLeftPos + boxSize / 2;

            // ReSharper disable once Unity.PreferNonAllocApi because non alloc is deprecated
            Collider2D[] colliderHits =
                Physics2D.OverlapBoxAll(boxCenter, boxSize, 0);

            List<Selectable> newSelection = new List<Selectable>();

            for (int i = 0; i < colliderHits.Length; i++)
            {
                if (colliderHits[i].gameObject.TryGetComponent(out Selectable selectable))
                {
                    selectable.Select();
                    newSelection.Add(selectable);
                }
            }

            UpdateSelection(newSelection);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mouseWorldPos, 0.2f);
        Gizmos.DrawWireSphere(firstClickWorldPos, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(botLeftPos, 0.25f);
        Gizmos.DrawWireSphere(topRightPos, 0.25f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(boxCenter, 0.3f);
        Gizmos.DrawWireSphere(boxCenter + boxSize, 0.3f);
    }

    private void OnClick_Canceled(InputAction input)
    {
        isSelecting = false;
        m_secondScreenPos = Vector2.zero;
        m_firstClickScreenPos = Vector2.zero;
        GetComponent<SelectionRectangleDrawer>().pos1 = Vector2.zero;
        GetComponent<SelectionRectangleDrawer>().pos2 = Vector2.zero;
    }

    private void OnClick_Performed(InputAction input)
    {
        if (!isSelecting)
        {
            m_firstClickScreenPos = input.ReadValue<Vector2>();
            m_secondScreenPos = input.ReadValue<Vector2>();
        }
        else
        {
            m_secondScreenPos = input.ReadValue<Vector2>();
        }

        isSelecting = true;
    }

    private void UpdateSelection(List<Selectable> newSelection)
    {
        for (int i = m_selectedObjects.Count - 1; i >= 0; i--)
        {
            Selectable selectable = m_selectedObjects[i];
            if (!newSelection.Contains(selectable))
            {
                selectable.Unselect();
                m_selectedObjects.RemoveAt(i);
            }
            else
            {
                newSelection.Remove(selectable);
            }
        }

        foreach (Selectable selectable in newSelection)
        {
            m_selectedObjects.Add(selectable);
            selectable.Select();
        }
    }
}
