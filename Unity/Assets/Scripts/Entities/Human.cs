using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class Human : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Selectable>().OnSelect += OnSelect;
    }

    public void OnSelect()
    {
    }
}
