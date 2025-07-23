using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class AStarCollector : MonoBehaviour
{
	private char[][] matrix;
	private Tilemap m_tilemap => GetComponent<Tilemap>();

	public void Collect()
	{
		BoundsInt boundaries = m_tilemap.cellBounds;
		Vector3Int testedPos = new Vector3Int();
		for (int x = boundaries.xMin;x<boundaries.xMax;x++)
		{
			testedPos.x = x;
			for(int y = boundaries.yMin; y<boundaries.yMax;y++)
			{
				testedPos.y = y;
				if(Physics2D.Raycast(m_tilemap.CellToWorld(testedPos), Vector2.down))
				{
					//Todo : make AStar datas here.
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		
	}
}
