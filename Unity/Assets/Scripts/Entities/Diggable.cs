using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityTools.Game;

public class Diggable : Usable
{
	public static Dictionary<Vector3Int, Diggable> DiggableReferences = new Dictionary<Vector3Int, Diggable>();

	public static Diggable CreateDiggableAt(Tilemap tilemap, Vector3Int position)
	{
		if (!DiggableReferences.ContainsKey(position))
		{
			GameObject go = new GameObject();
			go.transform.position = tilemap.CellToWorld(position);
			go.transform.position += tilemap.cellSize / 2;
			Diggable diggable = go.AddComponent<Diggable>();
			DiggableReferences.Add(position, diggable);
		}
		return DiggableReferences[position];
	}

	public Tilemap DiggableTilemap => GameManager.Instance.GetGameDatas<GameDatas>().DiggableTilemap;
	public TileBase DiggedTile => GameManager.Instance.GetGameRules<GameRules>().DiggedTile;
	public NavMeshSurface NavMesh => GameManager.Instance.GetGameDatas<GameDatas>().NavSurface;

	List<Human> users = new List<Human>();

	[SerializeField] private float m_baseDurability = 10;
	[SerializeField] private float m_durability = 10;

	Vector3Int m_TilePos => DiggableTilemap.WorldToCell(transform.position);

	public override void Use(Human user)
	{
		if (!users.Contains(user))
		{
			users.Add(user);
			user.SetDigging(true);
		}
	}

	public override void StopUse(Human user)
	{
		user.SetDigging(false);
		users.Remove(user);
	}
	private void OnDestroy()
	{
		foreach (Human user in users)
		{
			user.SetDigging(false);
		}
	}

	private void Update()
	{
		if (users.Count > 0)
		{
			foreach(Human user in users)
			{
				m_durability -= Time.deltaTime;
			}
			Debug.Log($"{m_TilePos} dur : {m_durability}");
			if(m_durability<=0)
			{
				DiggableTilemap.SetTile(m_TilePos, DiggedTile);
				DiggableTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
				NavMesh.BuildNavMesh();
				Destroy(gameObject);
			}
		}
		else
		{
			m_durability = m_baseDurability;
		}

		if (DiggableTilemap.GetTile(m_TilePos) != null)
		{
			Destroy(gameObject);
		}
	}
}
