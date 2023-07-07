using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyThings : Buyable
{
    public int myId;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public Button button;
    public CanvasGroup canvasGroup;
    public UIShiny uishiny;

    private void Awake()
    {
        button = GetComponent<Button>();
        SetText();
        uishiny = GetComponent<UIShiny>();
    }
    private void Start()
    {
        int level = SaveManager.LoadFromDictionary(myId, "buyThingsSave", 0);
        ManualSetLevel(level);
    }


    private void Update()
    {
        bool sts = CheckDoIHaveEnoughMoney() && CheckCanUpdateLevel();
        button.interactable = sts;
        canvasGroup.interactable = sts;
        canvasGroup.alpha = sts ? 1 : 0.7f;
        uishiny.enabled = sts;

    }

    public override void OnLevelUpgrade()
    {
        if(myId == 0)
        {
            GameManager.Instance.oneBallIncome += GameManager.Instance.incomeIncrementalAmountPerLevel;
        }
        else if (myId == 1)
        {
            GameManager.Instance.ballSpawnCount = 5 + currentLevel;
        }
        SaveManager.SaveToDictionary(myId, currentLevel, "buyThingsSave");
        SetText();

    }

    public override void UpdateMyPrice()
    {
        base.UpdateMyPrice();

    }

    private void SetText()
    {
        if (CheckCanUpdateLevel())
        {
            costText.text = "$" + myPrice;
            levelText.text = "LEVEL " + (currentLevel + 1);
        }
        else
        {
            costText.text = "-";
            levelText.text = "MAX";

        }
    }

}
