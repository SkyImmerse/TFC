using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Tibia.UI.Shared
{
    [RequireComponent(typeof(UITooltipTrigger))]
    public class UIProgress : MonoBehaviour
    {
        public TMP_Text TextValue;
        public RectTransform ProgressValue;

        public void SetProgress(double percent, string textValue = "", string tooltip = "")
        {
            var max = (double)ProgressValue.parent.GetComponent<RectTransform>().sizeDelta.x;
            ProgressValue.sizeDelta = new Vector2((float)(percent * max), ProgressValue.sizeDelta.y);

            if(textValue != "")
            {
                TextValue.text = textValue;
            }
            if(tooltip!="")
            {
                GetComponent<UITooltipTrigger>().text = tooltip;
            }
        }
    }
}
