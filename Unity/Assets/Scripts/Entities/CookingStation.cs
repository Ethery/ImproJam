using UnityEngine;

public class CookingStation : Usable
{
	public enum EState
	{
		harvesting,
		available
	}

	private EState m_state;

	private float m_time;

	[SerializeField]
	private float m_harvestingTime = 10f;

	[SerializeField]
	private Animator m_animator;

	private void Start()
	{
		m_state = EState.harvesting;
	}

	private void Update()
	{
		bool isAvailable = m_state == EState.available;

		m_animator.SetBool("IsAvailable", isAvailable);

		if (m_state == EState.harvesting)
		{
			m_time = m_time + Time.deltaTime;
			if (m_time > m_harvestingTime)
			{
				m_state = EState.available;
				m_time = 0f;
			}
		}
	}

	public override void Use(Human human)
	{
		if (m_state == EState.available)
		{
			human.SetStamina(human.MaxStamina);
			m_state = EState.harvesting;
		}
	}

	public override void StopUse(Human human)
	{

	}

}
