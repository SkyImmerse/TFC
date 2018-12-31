using Assets.Tibia.DAO;
using Assets.Tibia.UI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.GameInterface
{
    public class UIText : MonoBehaviour
    {
        public Vector3 Target
        {
            get => _target;
            set
            {
                _target = value;
                Layer = LayerMask.NameToLayer(Target.z.ToString());
            }
        }
        
        public TMP_Text Text;
        internal string text;
        internal Vector2 offset;
        RectTransform rect;
        private Vector3 _target;
        private int Layer;

        public void Awake()
        {
            var canvas = FindObjectOfType<Canvas>();
            rect = GetComponent<RectTransform>();
        }
        public void SetText(string text, Color color)
        {
            this.text = text;
            Text.color = color;
            Text.text = text;
        }

        public void StartTimer(float time)
        {
            Invoke("Remove", time);
        }
        public void Remove()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {

            //if (Layer != LayerMask.NameToLayer(LocalPlayer.Current.Position.z.ToString()))
            //{
            //    transform.localPosition = new Vector3(float.MaxValue, float.MaxValue);
            //    return;
            //}
            var mousePosition = (Target - LocalPlayer.Current.Sprite.Group.GetThingPosition(LocalPlayer.Current));

            transform.localPosition = UITextController.Instance.Center + (new Vector2(mousePosition.x, mousePosition.y)) * UITextController.Instance.Size;

        }
    }
}
