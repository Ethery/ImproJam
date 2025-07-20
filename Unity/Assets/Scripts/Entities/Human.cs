using NavMeshPlus.Components;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityTools.Game;
using UnityTools.Systems.Inputs;

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NavMeshAgent))]
public class Human : MonoBehaviour
{
    public Tilemap DiggableTilemap => GameManager.Instance.GetGameDatas<GameDatas>().DiggableTilemap;
    public TileBase DiggedTile => GameManager.Instance.GetGameRules<GameRules>().DiggedTile;
    public NavMeshSurface NavMesh => GameManager.Instance.GetGameDatas<GameDatas>().NavSurface;

    [SerializeField] private InputManager.Input m_moveInput;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private string m_IsWalkingAnimParam;
    [SerializeField] private string m_IsDiggingAnimParam;

    public float Stamina = 50;
    public float MaxStamina = 50;
    public float AgentSpeed;
	
    public bool showPath;
    public bool showAhead;

    private NavMeshAgent agent;
    private InputManager.InputEvent m_canceledEvent;
    private InputManager.InputEvent m_performedEvent;

	Usable m_useTarget = null;
	Vector3 m_moveTarget = Vector3.negativeInfinity;
    Vector3 m_lastPosition = Vector3.negativeInfinity;

    Usable UseTarget
    {
        get => m_useTarget;
        set
        {
            m_useTarget = value;
            m_moveTarget = Vector3.negativeInfinity;
        }
    }

	Vector3 MoveTarget
	{
        get => m_moveTarget;
        set
        {
            m_useTarget = null;
            m_moveTarget = value;
        }
    }

    Vector3 PosTarget
    {
        get
        {
            if(m_useTarget == null)
            {
                return m_moveTarget;
            }
            else
            {
                return m_useTarget.transform.position;
            }
        }
    }

    bool m_isWalking => agent.velocity.sqrMagnitude > 0.5f && !agent.isStopped;
    bool m_IsDigging ;

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
        if (Stamina <= 0)
        {
            Stamina = 0;
            agent.speed = AgentSpeed / 2;
        }
        else
        {
            agent.speed = AgentSpeed / 2;
        }

		if ((PosTarget - Vector3.negativeInfinity).sqrMagnitude > Mathf.Epsilon)
        {
            agent.isStopped = false;
            agent.SetDestination(PosTarget);
			if (agent.remainingDistance < 1 && agent.velocity.sqrMagnitude <= 0.5f)
            {
                agent.isStopped = true;
                if (UseTarget != null )
                {
					UseTarget.Use(this);
					UseTarget = null;
                }
            }
        }

		if (m_Animator != null)
        {
            m_Animator.SetBool(m_IsDiggingAnimParam, m_IsDigging && !m_isWalking);
            m_Animator.SetBool(m_IsWalkingAnimParam, m_isWalking);
        }

        foreach(SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.flipX = m_isWalking && agent.velocity.x < 0;
		}

	}
	private void LateUpdate()
	{
        Vector3 dist = transform.position - m_lastPosition;
        SetStamina(Stamina - dist.magnitude);
	    m_lastPosition = transform.position;
	}

    public void SetStamina(float newValue)
    {
        Stamina = newValue;
    }

    public void SetDigging(bool isDigging)
    {
        m_IsDigging = isDigging;
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

        bool handled = false;
        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                if(HandleClicOnCollider(collider))
                {
                    handled = true;
				}
            }
        }

        if(!handled)
        {
            HandleClickOnEmptySpace(worldPos);
        }
        
        agent.destination = Camera.main.ScreenToWorldPoint(input.ReadValue<Vector2>());
    }

	private void HandleClickOnEmptySpace(Vector3 worldPos)
	{
		Vector3Int tilePos = DiggableTilemap.WorldToCell(worldPos);
        if(DiggableTilemap.GetTile(tilePos) == null)
        {
            UseTarget = Diggable.CreateDiggableAt(DiggableTilemap,tilePos);
		}
        else
        {
			MoveTarget = worldPos;
        }
	}

	private bool HandleClicOnCollider(Collider2D collider)
	{
        if(collider.TryGetComponent(out Usable usable))
        {
			UseTarget = usable;
            return true;
		}
        return false;
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
