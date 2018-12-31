using Assets.Tibia.DAO;
using Game.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.UI.GameInterface
{
    public class UITextController : MonoBehaviour
    {
        public Vector2 Center;
        public Vector2 Size =Vector2.one;
        public GameObject InfoPrefab;
        public GameObject TextPrefab;

        public static UITextController Instance;

        public List<UIText> texts;
        public List<UICreatureInformation> infos;

        void Start()
        {
            Instance = this;
            texts = new List<UIText>();
            infos = new List<UICreatureInformation>();

            for (int i = 0; i < 50; i++)
            {
                var info = CreateCreatureInfo();
                info.gameObject.SetActive(false);
                infos.Add(info);
            }
            for (int i = 0; i < 50; i++)
            {
                var text = CreateText(Vector3.zero, "", Color.white, 99999);
                text.gameObject.SetActive(false);
                texts.Add(text);
            }
        }
        public UIText PoolText(Vector3 worldPos, string message, Color color, float timer = (float)TileMaps.ANIMATED_TEXT_DURATION / 1000f)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i].gameObject.activeInHierarchy == false)
                {
                    texts[i].Target = worldPos;
                    texts[i].SetText(message, color);
                    texts[i].Update();
                    texts[i].StartTimer(timer);
                    texts[i].gameObject.SetActive(true);
                    return texts[i];
                }
            }
            var text = CreateText(Vector3.zero, message, color, timer);
            text.StartTimer(timer);
            texts.Add(text);
            return texts.Last();
           

        }

        public UICreatureInformation PoolCreatureInfo()
        {
            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].gameObject.activeInHierarchy == false)
                {
                    infos[i].gameObject.SetActive(true);
                    return infos[i];
                }
            }
            var info = CreateCreatureInfo();
            infos.Add(info);
            return infos.Last();


        }


        private UICreatureInformation CreateCreatureInfo()
        {
            return Instantiate<GameObject>(InfoPrefab, transform).GetComponent<UICreatureInformation>();
        }
        private UIText CreateText(Vector3 worldPos, string message, Color color, float timer)
        {
            var text = Instantiate<GameObject>(TextPrefab, transform).GetComponent<UIText>();
            text.Target = worldPos;
            text.SetText(message, color);
            
            return text;
        }
    }
}
