using Assets.Tibia.UI;
using UnityEngine;

[RequireComponent(typeof(UIVisibleToggle))]
public class LoadingCircle : MonoBehaviour
{
    public static LoadingCircle Global;

    void Awake()
    {
        Global = GameObject.Find("GlobalLoadingProgress").GetComponent<LoadingCircle>();
    }
    public RectTransform Progress;
    private float rotateSpeed = 200f;
    bool isRun = false;
    public bool Visible
    {
        get {
            return isRun;
        }
        set
        {
            isRun = value;
            GetComponent<UIVisibleToggle>().Visible = isRun;
        }
    }

    private void Update()
    {
        if (isRun)
            Progress.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}