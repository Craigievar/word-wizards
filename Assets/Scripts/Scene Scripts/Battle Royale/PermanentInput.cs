using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PermanentInput : InputField
{
    public override void OnDeselect(BaseEventData eventData)
    {
        return;
    }
}
