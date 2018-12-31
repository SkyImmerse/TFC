using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Tibia.UI.Shared
{
    [RequireComponent(typeof(UIVisibleToggle))]
    public class UIWindow : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public bool Visible
        {
            get => GetComponent<UIVisibleToggle>().Visible;
            set => GetComponent<UIVisibleToggle>().Visible = value;
        }

        public bool VisibleOnStart;

        void Awake()
        {
            GetComponent<UIVisibleToggle>().VisibilityChange += UIWindow_VisibilityChange;
        }

        void Start()
        {
            Visible = VisibleOnStart;
        }

        private void UIWindow_VisibilityChange(bool obj)
        {
            if(obj)
            {
                OnShow();
            }
        }

        void OnShow()
        {
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Visible) return;
            GetComponent<CanvasGroup>().alpha = 0.8f;
            var position = transform.position;
            position += new Vector3(eventData.delta.x, eventData.delta.y);
            if(position.y <= 0)
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

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Visible) return;
            GetComponent<CanvasGroup>().alpha = 1.0f;
        }


    }
}
