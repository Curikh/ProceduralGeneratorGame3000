using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inventory
{
	[CreateAssetMenu(menuName = "Scriptable object/Item")]
	public class Item : ScriptableObject {

		[Header("Only gameplay")]
		public TileBase tile;
		public ItemType type;
		public ActionType actionType;
		public Vector2Int range = new Vector2Int(5, 4);
		public bool canBeDropped = true;

		[Header("Only UI")]
		public bool stackable = true;

		[Header("Both")]
		public Sprite image;

		[Header("Weapon Settings")]
		public float attackRange = 1.5f;
		public float attackCooldown = 0.5f;
		public int damage = 20;
		 public float knockbackForce = 5f;

	}

	public enum ItemType {
		Poition,
		Weapon,
		Coin,
		Key
	}
	public enum ActionType {
		Use,
		Attack
	}
}
