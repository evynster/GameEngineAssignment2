using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClickScore : MonoBehaviour
{
    public static event Action clicked;

    private void OnMouseDown() {
        ScoreText.Instance.AddScore(1);

        # region observer
        clicked?.Invoke();
        # endregion
    }

}
