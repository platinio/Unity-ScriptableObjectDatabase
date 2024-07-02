using Platinio.InventorySample;
using Platinio.ScriptableObjectDatabase;
using UnityEngine;
using UnityEngine.Serialization;

public class Test : MonoBehaviour
{
    [FormerlySerializedAs("item")] [SerializeField] [ScriptableItemDatabaseSelector(typeof(ItemDefinitionDatabase))]
    private ItemDefinition itemDefinition;
}
