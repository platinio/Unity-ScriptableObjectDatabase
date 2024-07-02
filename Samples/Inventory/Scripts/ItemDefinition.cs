using Platinio.ScriptableObjectDatabase;
using UnityEngine;

namespace Platinio.InventorySample
{
    public class ItemDefinition : ScriptableItem
    {
        [SerializeField, TextArea] private string description;
        [SerializeField] private ItemGameRepresentation gameRepresentation;
        [SerializeField] private bool destroyAfterUse = false;
        [SerializeField] private int stackableAmount;

        public int StackableAmount => stackableAmount;
        public string Description => description;
        public bool DestroyAfterUse => destroyAfterUse;

        public ItemGameRepresentation CreateItemRepresentationInstance()
        {
            if (gameRepresentation == null) return null;
            return Instantiate(gameRepresentation);
        }

        public void Use(GameObject go)
        {
            Debug.Log($"Player use item {go.name}");
        }
    }
}