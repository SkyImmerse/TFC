using Assets.Tibia.ASPR.Graphics;
using Assets.Tibia.DAO;
using Assets.Tibia.UI.Shared;
using SkyImmerseEngine.Graphics;
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
    public class UICreatureInformation : MonoBehaviour
    {
        public SkyImmerseEngine.Graphics.SpriteCreature Target
        {
            get => _target;
            set
            {
                _target = value;
                if (Target != null)
                {
                    Layer = Target.Group.Layer;
                    if (Target.Thing != null)
                    {
                        if (Target.Thing is LocalPlayer)
                        {
                            ManaBar.gameObject.SetActive(true);
                        }
                    }

                    GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x * Target.ThingType.Size.width, GetComponent<RectTransform>().sizeDelta.y * Target.ThingType.Size.width);
                }

            }
        }

        public TMP_Text CreatureSay;
        public TMP_Text Name;
        public UIProgress HpBar;
        public UIProgress ManaBar;
        public Image Frame;
        private int Layer;
        private SpriteCreature _target;

        public void Awake()
        {
            var canvas = FindObjectOfType<Canvas>();
            Clear();
        }
        public void SetName(string name)
        {
            Name.text = name;
        }

        public void SetInfo(Color c, float hp, float mana = -1)
        {
            HpBar.SetProgress((double)hp);
            HpBar.ProgressValue.GetChild(0).GetComponent<Image>().color = c;
            Name.color = c;
            if (mana > -1)
                ManaBar.SetProgress((double)mana);

        }

        public void Clear()
        {
            Target = null;
            SetInfo(Color.white, 0);
            SetName("");
        }

        public void Remove()
        {
            Clear();
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (Target == null || Target.Thing == null || Target.Group == null || LocalPlayer.Current == null || LocalPlayer.Current.Sprite == null)
            {
                Remove();
                return;
            }

            if(Layer != LayerMask.NameToLayer(LocalPlayer.Current.Position.z.ToString()))
            {
                transform.localPosition = new Vector3(float.MaxValue, float.MaxValue);
                return;
            }
            var mousePosition = (Target.Group.GetThingPosition(Target.Thing) - LocalPlayer.Current.Sprite.Group.GetThingPosition(LocalPlayer.Current));
            transform.localPosition = UITextController.Instance.Center + (new Vector2(mousePosition.x, mousePosition.y)) * UITextController.Instance.Size;
        }
    }
}
