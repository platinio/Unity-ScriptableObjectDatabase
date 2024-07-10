using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.InventorySample
{
    /// <summary>
    /// In game representation of an item
    /// </summary>
    public class ItemGameRepresentation : MonoBehaviour 
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(ItemDefinitionDatabase))]
        protected ItemDefinition itemDefinition;
        public ItemDefinition ItemDefinition => itemDefinition;
    }
}