using System;
using System.Collections.Generic;
using System.Linq;
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class Attributes : MonoBehaviour
    {
        [SerializeField] private List<AttributeValue> attributeValues;

        public AttributeValue GetAttribute(AttributeDefinition attributeDefinition) => attributeValues
            .Where(x => x.AttributeDefinition == attributeDefinition).FirstOrDefault();
        
        public void ModifyAttribute(AttributeDefinition attributeDefinition, float v)
        {
            foreach (var attribute in attributeValues)
            {
                if (attribute.AttributeDefinition == attributeDefinition)
                {
                    attribute.Modify(v);
                    return;
                }
            }
        }
    }

    [Serializable]
    public class AttributeValue
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(AttributeDefinitionDatabase))]
        private AttributeDefinition attribute;
        [SerializeField] private float currentValue;
        [SerializeField] private float maxValue;

        public AttributeDefinition AttributeDefinition => attribute;
        public Action OnValueChanged { get; set; }
        public float CurrentValue => currentValue;
        public float MaxValue => maxValue;

        public void Modify(float modifier)
        {
            currentValue += modifier;
            currentValue = Mathf.Clamp(currentValue, 0, maxValue);
            OnValueChanged?.Invoke();
        }
    }
}