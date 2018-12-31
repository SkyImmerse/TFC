using Assets.Tibia.UI.GameInterface;
using Game.DAO;
using SkyImmerseEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tibia.DAO
{
    public class AnimatedText : Thing
    {
        UIText uiText;
        internal string Text
        {
            get => uiText.text;
            set
            {
                if(uiText==null)
                {
                    StartTicks = (long)Time.realtimeSinceStartup * 1000;
                    if (Map.Current.GetTile(RealPosition) != null)
                        uiText = UITextController.Instance.PoolText(Map.Current.GetTile(RealPosition).RealPosition, value, ByteColorConverter.from8bit(Color), (float)TileMaps.ANIMATED_TEXT_DURATION / 1000f);
                } else
                {
                    uiText.SetText(value, ByteColorConverter.from8bit(Color));
                }
            }
        }
        int _color;
        internal int Color
        {
            get => _color;
            set
            {
                _color = value;
                if (uiText != null)
                {
                    uiText.SetText(Text, ByteColorConverter.from8bit(Color));
                }
            }
        }
        internal Vector2 Offset
        {
            get => uiText.offset;
            set => uiText.offset = value;
        }

        public long StartTicks = (long)Time.realtimeSinceStartup * 1000;
        public long TicksElapsed => ((long)Time.realtimeSinceStartup * 1000) - StartTicks;

        internal bool Merge(AnimatedText other)
        {
            if (other.Color != Color)
                return false;

            if (TicksElapsed > (float)TileMaps.ANIMATED_TEXT_DURATION / 2.5f)
                return false;

            try
            {
                int number = int.Parse(Text);
                int otherNumber = int.Parse(other.Text);

                var _text = string.Format("{0}", number + otherNumber);
                Text = _text;
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal void Destroy()
        {
            if (uiText != null) uiText.Remove();
        }
    }
}
