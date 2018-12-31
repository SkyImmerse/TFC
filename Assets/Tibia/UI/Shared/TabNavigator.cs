using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Tibia.UI
{
    public class TabNavigator : MonoBehaviour
    {
        private EventSystem system;

        private void Start()
        {
            system = GetComponent<EventSystem>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (system.currentSelectedGameObject != null && system.currentSelectedGameObject.GetComponent<Selectable>()!=null)
                {
                    Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                    if (next != null)
                    {
                        InputField inputfield = next.GetComponent<InputField>();
                        if (inputfield != null)
                        {
                            inputfield.OnPointerClick(new PointerEventData(system));
                        }

                        system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                    }
                }
            }
        }
    }
}
