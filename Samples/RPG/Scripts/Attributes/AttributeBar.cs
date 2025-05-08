using ArcaneOnyx.ScriptableObjectDatabase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcaneOnyx.RPGSample
{
    public class AttributeBar : MonoBehaviour
    {
        [SerializeField, ScriptableItemDatabaseSelector(typeof(AttributeDefinitionDatabase))]
        private AttributeDefinition attribute;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image bar;
        [SerializeField] private Attributes target;

        private void Start()
        {
            bar.color = attribute.Color;
            
            target.GetAttribute(attribute).OnValueChanged += OnValueChanged;
            OnValueChanged();
        }

        private void OnValueChanged()
        {
            var attributeValue = target.GetAttribute(attribute);
            label.text = $"{attributeValue.CurrentValue} / {attributeValue.MaxValue}";
            bar.fillAmount = attributeValue.CurrentValue / attributeValue.MaxValue;
        }
    }
}