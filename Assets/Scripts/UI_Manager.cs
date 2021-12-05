using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] SO_PlayerData playerData;
    [SerializeField] GameObject HPBar;
    [SerializeField] GameObject ChargesBar;
    [SerializeField] TextMeshProUGUI LevelCounter;
    [SerializeField] AudioSource audio;
    Image[] HPList;
    Image[] ChargesList;

    bool blinking = false;
    // Start is called before the first frame update
    void Start()
    {
        HPList = HPBar.GetComponentsInChildren<Image>();
        ChargesList = ChargesBar.GetComponentsInChildren<Image>();
    }

    public void UpdateHP()
    {
        for (int i = 0; i < HPList.Length; i++)
        {
            if (i > playerData.hitPoints - 1)
            {
                HPList[i].enabled = false;
            }
            else
            {
                HPList[i].enabled = true;
            }

            Vector3 current = Vector3.Lerp(Vector3.right, Vector3.up, (float)playerData.hitPoints / 10).normalized;

            HPList[i].color = new Color(current.x, current.y, current.z);
        }

        if (playerData.hitPoints == 1)
        {
            blinking = true;
            StartCoroutine(Blink(HPList[0]));
        }

        if (playerData.hitPoints <= 0)
        {
            blinking = false;
        }

    }

    public void UpdateCharges()
    {
        for (int i = 0; i < ChargesList.Length; i++)
        {
            if (i > playerData.charges - 1)
            {
                ChargesList[i].enabled = false;
            }
            else
            {
                ChargesList[i].enabled = true;
            }
        }
    }

    IEnumerator Blink(Image img)
    {
        float onTime = 0.3f;
        float offTime = 0.2f;

        bool on = true;

        while (blinking)
        {
            if (on)
            {
                img.enabled = true;
                yield return new WaitForSeconds(onTime);
                on = false;
                AudioHandler.instance.PlaySound("Health_Low", audio);
                continue;
            }
            else
            {
                img.enabled = false;
                yield return new WaitForSeconds(offTime);
                on = true;
                continue;
            }
        }
        img.enabled = false;
    }

    public void UpdateLevel()
    {
        LevelCounter.text = GameHandler.instance.levelNumber.ToString("-000");
    }
}
