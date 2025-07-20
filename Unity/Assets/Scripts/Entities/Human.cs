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
            if (agent.remainingDistance < 1 && agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                m_target.Use();
                m_target = null;
            }
        }
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_IsWalkingAnimParam, agent.velocity.sqrMagnitude > 0.5f);
        }
        foreach(SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.flipX = agent.velocity.sqrMagnitude > 0.5f && agent.velocity.x < 0;
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

        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                HandleClicOnCollider(collider);
            }
        }
        else
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
			DiggableTilemap.SetTile(tilePos, DiggedTile);
            DiggableTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
			NavMesh.BuildNavMesh();
		}
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
