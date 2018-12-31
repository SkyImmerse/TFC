using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIVisibleToggle : MonoBehaviour
    {
        public bool Visible
        {
            get => GetComponent<CanvasGroup>().interactable;
            set => SetVisiblity(value);
         }

        public event Action<bool> VisibilityChange;

        private void Awake()
        {
            Hide();    
        }

        private void Show()
        {
            GetComponent<CanvasGroup>().alpha = 1;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<CanvasGroup>().interactable = true;
        }

        private void SetVisiblity(bool value)
        {
            VisibilityChange?.Invoke(value);
            if (value)
                Show();
            else
                Hide();
        }

        private void Hide()
        {
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            GetComponent<CanvasGroup>().interactable = false;
        }
    }
}
