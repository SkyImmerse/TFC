using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tibia.UI.Shared
{
    public class UIMessageBox : UIWindow
    {

        public delegate void BaseEvent();
        public event BaseEvent OnOk;
        public event BaseEvent OnCancel;

        public Transform ButtonHolder;

        public void Start()
        {

        }

        public void AddButton(string text, Action callback)
        {
            var button = GameObject.Instantiate(Resources.Load<GameObject>("SIUI/Controls/Button"), ButtonHolder);
            button.transform.Find("Text").GetComponent<TMP_Text>().text = text;
            button.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(callback));
        }

        public void Ok()
        {
            Visible = false;
            OnOk?.Invoke();
            Destroy(this.gameObject);

        }

        public void Cancel()
        {
            Visible = false;
            OnCancel?.Invoke();
            Destroy(this.gameObject);

        }
    }
}
