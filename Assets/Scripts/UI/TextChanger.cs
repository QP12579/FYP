using System.Collections;
using UnityEngine;
using TMPro;

public class TextChanger : MonoBehaviour
{
    public TMP_Text textComponent;
    private string[] phrases = {
        "Fire Tornado", "Double Damage", "Guess Who I Am", "Set the Bomb", "Give Up Treatment",
        "Being a Poor Person", "Donald Trump", "Enemies From The Sky"
    };

    private int currentIndex = 0; 

    private void Start()
    {
        StartCoroutine(ChangeText());
    }

    private IEnumerator ChangeText()
    {
        while (true)
        {
            string phrase = phrases[currentIndex];
            string fullText = $"Press C to speak: <color=yellow></color>";
            textComponent.text = fullText;

            for (int i = 0; i < phrase.Length; i++)
            {
                textComponent.text = $"Press C to speak:\n <color=yellow>{phrase.Substring(0, i + 1)}</color>";
                yield return new WaitForSeconds(0.05f); 
            }

            yield return new WaitForSeconds(3);

            currentIndex = (currentIndex + 1) % phrases.Length;
        }
    }
}