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
    private Vector2 m_firstClickScreenPos = Vector2.zero;
    private HashSet<Selectable> m_selectedObjects = new HashSet<Selectable>();

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
    }

    private void OnClick_Canceled(InputAction input)
    {
        Debug.Log(nameof(OnClick_Canceled));
        m_firstClickScreenPos = Vector2.zero;
        GetComponent<SelectionRectangleDrawer>().pos1 = Vector2.zero;
        GetComponent<SelectionRectangleDrawer>().pos2 = Vector2.zero;
    }

    private void OnClick_Performed(InputAction input)
    {
        if (m_firstClickScreenPos == Vector2.zero)
        {
            m_firstClickScreenPos = input.ReadValue<Vector2>();
            GetComponent<SelectionRectangleDrawer>().pos1 = m_firstClickScreenPos;
        }
        else
        {
            GetComponent<SelectionRectangleDrawer>().pos2 = input.ReadValue<Vector2>();

            if (Camera.main == null) return;

            Vector2 firstClickWorldPos = Camera.main.ScreenToWorldPoint(m_firstClickScreenPos);

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(input.ReadValue<Vector2>());

            Vector2 boxPos = new Vector2(Math.Min(firstClickWorldPos.x, mouseWorldPos.x),
                Math.Min(firstClickWorldPos.y, mouseWorldPos.y));

            Vector2 boxSize = new Vector2(Math.Max(firstClickWorldPos.x, mouseWorldPos.x),
                Math.Max(firstClickWorldPos.y, mouseWorldPos.y));

            // ReSharper disable once Unity.PreferNonAllocApi because non alloc is deprecated
            Collider2D[] colliderHits =
                Physics2D.OverlapBoxAll(boxPos, boxSize, 0);

            foreach (Selectable selectable in m_selectedObjects)
            {
                if (selectable != null)
                    selectable.Unselect();
            }

            m_selectedObjects.Clear();
            for (int i = 0; i < colliderHits.Length; i++)
            {
                if (colliderHits[i].gameObject.TryGetComponent(out Selectable selectable))
                {
                    selectable.Select();
                    m_selectedObjects.Add(selectable);
                }
            }
        }
    }
}
