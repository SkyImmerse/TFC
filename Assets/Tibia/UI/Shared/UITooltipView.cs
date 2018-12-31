using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.UI.Shared
{
    public class UITooltipView : MonoBehaviour
    {

        public bool IsActive
        {
            get
            {
                return gameObject.activeSelf;
            }
        }

        public TMPro.TMP_Text tooltipText;

        void Awake()
        {
            instance = this;
            HideTooltip();
        }
        public Vector3 last;
        public void Update()
        {
            var current = Input.mousePosition;
            var delta = current - last;
            last = current;
            var position = transform.position;
            position += new Vector3(delta.x,delta.y);
            if (position.y <= 0)
            {
                position.y = transform.position.y;
            }
            if (position.y >= Screen.height)
            {
                position.y = transform.position.y;
            }

            if (position.x <= 0)
            {
                position.x = transform.position.x;
            }
            if (position.x >= Screen.width)
            {
                position.x = transform.position.x;
            }
            transform.position = position;
        }
        public void ShowTooltip(string text, Vector3 pos)
        {
            if (tooltipText.text != text)
                tooltipText.text = text;

            transform.position = pos;

            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        // Standard Singleton Access 
        private static UITooltipView instance;
        public static UITooltipView Instance
        {
            get
            {
                if (instance == null)
                    instance = GameObject.FindObjectOfType<UITooltipView>();
                return instance;
            }
        }
    }
}
