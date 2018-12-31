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
    [RequireComponent(typeof(LayoutElement))]
    public class UIMiniWindow : UIWindow, IDragHandler, IEndDragHandler
    {
        private bool IsDocked = true;
        public List<Transform> DockPanelTransforms = new List<Transform>();

        private bool IsMinimized = false;
        private float _sizeHolder = 0;

        public float MinimizedSize = 30;

        public TMPro.TMP_Text MinimizeText;
        public void Start()
        {
            DockPanelTransforms.Add(GameObject.Find("RightDockPanel").transform);
            DockPanelTransforms.Add(GameObject.Find("LeftDockPanel").transform);
        }

        public void OnMinimize()
        {
            IsMinimized = !IsMinimized;
            var sizeY = GetComponent<RectTransform>().sizeDelta.y;
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, IsMinimized ? MinimizedSize : _sizeHolder );
            _sizeHolder = sizeY;

            if(!IsMinimized)
            {
                MinimizeText.text = "—";
            }else
            {
                MinimizeText.text = "+";
            }
        }

        public new void OnDrag(PointerEventData eventData)
        {
            if (!Visible) return;

            var position = transform.position;
            if (!IsDocked)
            {
                position += new Vector3(eventData.delta.x, eventData.delta.y);
            }
            else
            {
                transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
                position += new Vector3(eventData.delta.x, eventData.delta.y);
            }

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
            GetComponent<CanvasGroup>().alpha = 0.8f;
        }

        public new void OnEndDrag(PointerEventData eventData)
        {
            if (!Visible) return;

            //Set dragging object on cursor
            var canvas = GameObject.FindObjectOfType<Canvas>().GetComponent<Canvas>();
            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position,
                canvas.worldCamera, out worldPoint);

            foreach (var DockPanelTransform in DockPanelTransforms)
            {

                if (RectTransformUtility.RectangleContainsScreenPoint(DockPanelTransform.GetComponent<RectTransform>(), transform.position))
                {
                    var minDistance = float.PositiveInfinity;
                    var targetIndex = 0;
                    float dist = 0;
                    for (var j = 0; j < DockPanelTransform.childCount; j++)
                    {
                        var c = DockPanelTransform.GetChild(j).GetComponent<RectTransform>();

                        dist = Mathf.Abs(c.position.y - worldPoint.y);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            targetIndex = j;
                        }
                    }
                    transform.SetParent(DockPanelTransform);
                    // move to last
                    if (targetIndex == DockPanelTransform.childCount - 2)
                    {
                        transform.SetAsLastSibling();
                    }
                    else
                    {
                        transform.SetSiblingIndex(targetIndex);
                    }
                    IsDocked = true;
                }
                else
                {
                    IsDocked = false;
                }
            }
            GetComponent<CanvasGroup>().alpha = 1.0f;
        }
    }
}
