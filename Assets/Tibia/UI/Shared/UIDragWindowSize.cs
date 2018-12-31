using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Tibia.UI.Shared
{
    public class UIDragWindowSize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        public UIWindow OwnerWindow;

        public bool Invert;

        public Texture2D VerticalCursor;

        public int MinSizeY;
        public int MaxSizeY;

        private Color _holdColor;

        public void OnDrag(PointerEventData eventData)
        {
            var sizeDelta = OwnerWindow.GetComponent<RectTransform>().sizeDelta;
            OwnerWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y - (Invert ? -1 : 1) * eventData.delta.y);
            if (OwnerWindow.GetComponent<RectTransform>().sizeDelta.y < MinSizeY)
            {
                OwnerWindow.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            }
            if (OwnerWindow.GetComponent<RectTransform>().sizeDelta.y > MaxSizeY)
            {
                OwnerWindow.GetComponent<RectTransform>().sizeDelta = sizeDelta;
            }
            GetComponent<RawImage>().CrossFadeColor(Color.green, 0.2f, true, false);
            Cursor.SetCursor(VerticalCursor, new Vector2(0, 0.5f), CursorMode.Auto);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GetComponent<RawImage>().CrossFadeColor(_holdColor, 0.2f, true, false);
            Cursor.SetCursor(null, new Vector2(), CursorMode.Auto);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _holdColor = GetComponent<RawImage>().color;
            GetComponent<RawImage>().CrossFadeColor(Color.green, 0.2f, true, false);
            Cursor.SetCursor(VerticalCursor, new Vector2(0, 0.2f), CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GetComponent<RawImage>().CrossFadeColor(_holdColor, 0.2f, true, false);
            Cursor.SetCursor(null, new Vector2(), CursorMode.Auto);
        }
    }
}
