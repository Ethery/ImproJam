using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(NavMeshAgent))]
public class Demon : MonoBehaviour
{
	[SerializeField]
	private float m_newCommandTime = 4f;

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private NavMeshAgent m_agent;

	private float m_time;

	private float Speed => m_agent.speed;

	private float DistanceMax => Speed * m_newCommandTime;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		m_agent = GetComponent<NavMeshAgent>();
	}

	// Update is called once per frame
	private void Update()
	{
		m_time = m_time + Time.deltaTime;
		if (m_time > m_newCommandTime)
		{
			NewCommand();
			m_time = 0f;
		}
	}

	void NewCommand()
	{
		Vector2 direction = new Vector2();
		direction.x = Random.Range(-1f, 1f);
		direction.y = Random.Range(-1f, 1f);
		direction = direction.normalized;
		float distance = Random.Range(0, DistanceMax);
		direction *= distance;
		m_agent.SetDestination((Vector2)transform.position + direction);
	}
}
