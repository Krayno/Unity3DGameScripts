using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateXColours : MonoBehaviour
{
    public GameObject ColourButtonPrefab;
    public int AmountOfColours;

    List<Color32> Colours;

    void Awake()
    {
        //Initialise Lists.
        Colours = new List<Color32>();

        float IncrementBy = (1f / 360f) * (360f / AmountOfColours);
        for (float i = IncrementBy; i < 1; i+= IncrementBy)
        {
            Colours.Add(Color.HSVToRGB(i, 1f, 1f));
            if (i + IncrementBy > 1)
            {
                break;
            }
        }

        //Instantiate all colours with the ColourButtonPrefab.
        foreach(Color32 Colour in Colours)
        {
            GameObject ColourButton = Instantiate(ColourButtonPrefab, gameObject.transform);
            ColourButton.transform.GetChild(0).GetComponent<Image>().color = Colour;
        }
    }
}
