using Assets.Tibia.DAO;
using Assets.Tibia.UI.GameInterface;
using Game.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.ASPR.Graphics
{
    public class MouseSpriteSelect : MonoBehaviour
    {
        public static MouseSpriteSelect Instance;
        public void Awake()
        {
            Instance = this;
        }
        public SkyImmerseEngine.Sprite Selected;

        public float TileScreenSize = 32;
        public float YSign = 1;
        public Vector3 Center = new Vector3(0, 0);
        public Vector3 worldPos;
        public Vector3 mousePosition;
        public Camera cam;
        public Vector2 ViewportOffset = new Vector2(-20, -20);
        public Vector2 Viewport = new Vector2(1, 1);
        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }
        void Update()
        {
            if (LocalPlayer.Current == null) return;
            if (LocalPlayer.Current.Sprite == null) return;
            if (LocalPlayer.Current.Tile == null) return;
            mousePosition = (Input.mousePosition- new Vector3(Screen.width / 2, Screen.height / 2)) / TileScreenSize;
            worldPos = new Vector3(LocalPlayer.Current.Tile.RealPosition.x, LocalPlayer.Current.Tile.RealPosition.y, LocalPlayer.Current.Position.z) + Center + new Vector3(mousePosition.x + 0.5f, YSign * mousePosition.y - 0.5f, 0);
            worldPos = new Vector3(Mathf.Floor(worldPos.x), Mathf.Floor(worldPos.y), worldPos.z);
        }
        public void Test()
        {
            if (LocalPlayer.Current == null) return;
            if (LocalPlayer.Current.Sprite == null) return;


            var list = MapRenderer.GetSpriteByPos(Map.Current, worldPos, -1, 1);

            if (list != null)
                if (list.Count > 0)
                {
                    var sprite = list.FirstOrDefault();
                    Selected = sprite.Value;

                    Selected.EnableSelected();
                }
                else
                {
                    if (Selected != null)
                        Selected.DisableSelected();
                    Selected = null;
                }
            else
            {
                if (Selected != null)
                    Selected.DisableSelected();
                Selected = null;
            }

        }

    }
}
