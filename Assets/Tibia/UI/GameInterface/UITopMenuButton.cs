using Assets.Tibia.UI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.GameInterface
{
    [RequireComponent(typeof(Toggle))]
    public class UITopMenuButton : MonoBehaviour
    {
        public UIWindow Window;

        void Start()
        {
            if (Window != null)
            {
                Window.Visible = GetComponent<Toggle>().isOn;
                Window.GetComponent<LayoutElement>().ignoreLayout = !Window.Visible;
            }
        }
        public void OnChange(bool v)
        {
            if (Window != null)
            {
                Window.Visible = GetComponent<Toggle>().isOn;
                Window.GetComponent<LayoutElement>().ignoreLayout = !Window.Visible;
            }
        } 

        public void Hide()
        {
            if(Window!=null) Window.Visible = false;
            GetComponent<Toggle>().isOn = false;
            Window.GetComponent<LayoutElement>().ignoreLayout = !Window.Visible;
        }
    }
}
