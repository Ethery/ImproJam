using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class MovingEntityAnimator : MonoBehaviour
{
	private NavMeshAgent m_agent;
	private Animator m_animator;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		m_agent = GetComponent<NavMeshAgent>();
		m_animator = GetComponent<Animator>();
	}

    // Update is called once per frame
    void Update()
    {
		bool isMoving = m_agent.velocity.sqrMagnitude > 0;
		m_animator.SetBool("IsFlying", isMoving);
    }
}
