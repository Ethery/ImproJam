using System;
using UnityEngine;
using UnityEngine.UI;


public class Selectable : MonoBehaviour
{
    public delegate void SelectionChanged_delegate();

    [SerializeField] private SpriteRenderer IsSelectedImage;
    private bool m_isSelected;

    public SelectionChanged_delegate OnDeselect;
    public SelectionChanged_delegate OnSelect;

    private void Update()
    {
        if (IsSelectedImage != null)
            IsSelectedImage.gameObject.SetActive(m_isSelected);
    }

    public void Select()
    {
        if (!m_isSelected)
        {
            m_isSelected = true;
            if (OnSelect != null)
            {
                OnSelect();
            }
        }
    }

    public void Unselect()
    {
        if (m_isSelected)
        {
            m_isSelected = false;
            if (OnDeselect != null)
            {
                OnDeselect();
            }
        }
    }
}
