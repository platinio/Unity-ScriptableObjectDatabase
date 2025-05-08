using System;
using System.Collections.Generic;
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class AttributeModifierItem : ItemDefinition
    {
        [SerializeField] private List<AttributeModifier> attributeModifiers;
        
        public override void Use(GameObject go)
        {
            var attributes = go.GetComponent<Attributes>();

            foreach (var attributeModifier in attributeModifiers)
            {
                attributes.GetAttribute(attributeModifier.AttributeDefinition).Modify(attributeModifier.Value);
            }
        }
    }

    [Serializable]
    public class AttributeModifier
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(AttributeDefinitionDatabase))]
        private AttributeDefinition attribute;
        [SerializeField] private float value;

        public AttributeDefinition AttributeDefinition => attribute;
        public float Value => value;
    }
}