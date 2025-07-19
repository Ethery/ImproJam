using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityTools.Systems.Inputs;

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NavMeshAgent))]
public class Human : MonoBehaviour
{
    [SerializeField] private InputManager.Input m_moveInput;
    public bool showPath;
    public bool showAhead;

    private NavMeshAgent agent;
    private InputManager.InputEvent m_canceledEvent;
    private InputManager.InputEvent m_performedEvent;

	Usable m_target;

	// Start is called before the first frame update
	private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GetComponent<Selectable>().OnSelect += OnSelect;
        GetComponent<Selectable>().OnDeselect += OnDeselect;
        m_performedEvent = new InputManager.InputEvent(OnMove_Performed, InputActionPhase.Performed);
        m_canceledEvent = new InputManager.InputEvent(OnMove_Canceled, InputActionPhase.Canceled);
    }
	private void Update()
	{
        if (m_target != null)
        {
            agent.SetDestination(m_target.transform.position);
            if (agent.isStopped)
            {
                m_target.Use();
                m_target = null;
            }
        }
	}

	private void OnSelect()
    {
        RegisterMove(true);
    }

    private void OnDeselect()
    {
        RegisterMove(false);
    }

    private void RegisterMove(bool register)
    {
        InputManager.RegisterInput(m_moveInput, m_performedEvent, register);
        InputManager.RegisterInput(m_moveInput, m_canceledEvent, register);
    }

    private void OnMove_Canceled(InputAction input)
    {
    }

    private void OnMove_Performed(InputAction input)
	{
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(input.ReadValue<Vector2>());
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
        foreach(Collider2D collider in colliders)
        {
            HandleClicOnCollider(collider);
        }
        agent.destination = Camera.main.ScreenToWorldPoint(input.ReadValue<Vector2>());
    }

	private void HandleClicOnCollider(Collider2D collider)
	{
        if(collider.TryGetComponent(out Usable usable))
        {
            m_target = usable;
        }
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
    {
        DrawGizmos(agent, showPath, showAhead);
    }

    public static void DrawGizmos(NavMeshAgent agent, bool showPath, bool showAhead)
    {
        if (Application.isPlaying && agent != null)
        {
            if (showPath && agent.hasPath)
            {
                var corners = agent.path.corners;
                if (corners.Length < 2)
                {
                    return;
                }

                int i = 0;
                for (; i < corners.Length - 1; i++)
                {
                    Debug.DrawLine(corners[i], corners[i + 1], Color.blue);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(agent.path.corners[i + 1], 0.03f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
                }

                Debug.DrawLine(corners[0], corners[1], Color.blue);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(agent.path.corners[1], 0.03f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(agent.path.corners[0], agent.path.corners[1]);
            }

            if (showAhead)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(agent.transform.position, agent.transform.up * 0.5f);
            }
        }
    }
#endif
}
