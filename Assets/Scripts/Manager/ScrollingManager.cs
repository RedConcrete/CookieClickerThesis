using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingManager : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;
    [SerializeField] private float limit;

    // Update is called once per frame
    void Update()
    {
        // Überprüfung und Umkehr der Werte von _x und _y, wenn der Schwellenwert erreicht wird
        if (Mathf.Abs(_img.uvRect.position.x) >= limit) _x = -_x;
        if (Mathf.Abs(_img.uvRect.position.y) >= limit) _y = -_y;

        // Aktualisierung des UV-Rechtecks
        _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
    }
}
