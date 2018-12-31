using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Tibia.UI.Shared
{
    public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {

        public string text;

        public void Update()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartHover(new Vector3(eventData.position.x - GetComponent<RectTransform>().sizeDelta.x/2, eventData.position.y - GetComponent<RectTransform>().sizeDelta.y/2, 0f));
        }
        public void OnSelect(BaseEventData eventData)
        {
            StartHover(transform.position);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            StopHover();
        }
        public void OnDeselect(BaseEventData eventData)
        {
            StopHover();
        }

        void StartHover(Vector3 position)
        {
            if (text == "") return;
            UITooltipView.Instance.ShowTooltip(text, position);
        }
        void StopHover()
        {
            UITooltipView.Instance.HideTooltip();
        }

    }
}
