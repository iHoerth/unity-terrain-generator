using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public WorldGenerator world;
    public Slider loadingBarSlider;
    public TextMeshProUGUI loadingBarValueText;
    public Image crosshair;

    int value = 0;
    int maxValue = 0;
    float otroValue = 0;

    void Awake()
    {
        crosshair.gameObject.SetActive(false);
        loadingBarSlider.maxValue = world.possibleActiveChunks;
        loadingBarSlider.value = world.totalActiveChunks;
        StartCoroutine(FillBar());
    }

    // Update is called once per frame
    IEnumerator FillBar()
    {
        while(world.loading)
        {
            value = world.totalActiveChunks;
            maxValue = world.possibleActiveChunks;
            
            otroValue = Mathf.FloorToInt(Mathf.InverseLerp(0, maxValue, value) * 100);

            loadingBarValueText.text = otroValue.ToString() + "/100";
            loadingBarSlider.value = world.totalActiveChunks;
            loadingBarSlider.maxValue = world.possibleActiveChunks;

            yield return new WaitForSeconds(0.2f);
        }

        gameObject.SetActive(false);
        crosshair.gameObject.SetActive(true);
    }

    void Update()
    {
       
    }
}