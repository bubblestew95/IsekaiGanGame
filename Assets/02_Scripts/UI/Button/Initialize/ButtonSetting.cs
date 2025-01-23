using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSetting : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public TextMeshProUGUI textMeshPro = null;
    public Button button = null;


    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        images = GetComponentsInChildren<Image>().ToList();
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Start()
    {
        textMeshPro.raycastTarget = false;
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }
        button.gameObject.GetComponent<Image>().raycastTarget = true;
    }

}
