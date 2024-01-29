using UnityEngine;
using UnityEngine.UI;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    public class ItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text itemName;
        [SerializeField] private Text description;
        [SerializeField] private Text cost;

        public void UpdateContent(ShopItem item)
        {
            icon.sprite = item.Icon;
            itemName.text = item.Name;
            description.text = item.Description;
            cost.text = item.Cost.ToString();
        }
    }

}

