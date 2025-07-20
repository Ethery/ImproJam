using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTools.Game;
using NavMeshPlus.Components;

[Serializable]
public class GameDatas : BaseGameDatas
{
	public NavMeshSurface NavSurface;
	public Tilemap DiggableTilemap;
}
