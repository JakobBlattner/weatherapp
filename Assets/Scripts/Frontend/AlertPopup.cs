using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Assets.Scripts.Data;
using System.Text.RegularExpressions;

public class AlertPopup : MonoBehaviour
{
public TextMeshProUGUI textDescription;
public TextMeshProUGUI textFrom;

    public void SetText(Alert alert)
    {
        textDescription.text = Regex.Replace(alert.description, @"\t|\n|\r", "");
        textFrom.text = Regex.Replace(alert.sender_name, @"\t|\n|\r", "");
    }
}
