using UnityEngine;
using UnityEngine.Tilemaps;

public class Diggable : Usable
{
	public override void Use()
	{
		Tilemap tilemap = GetComponentInParent<Tilemap>();
		Vector3Int tilePos = tilemap.WorldToCell(transform.position);
		tilemap.SetTile(tilePos, null); // Remove tile at 0,0,0
	}
}
