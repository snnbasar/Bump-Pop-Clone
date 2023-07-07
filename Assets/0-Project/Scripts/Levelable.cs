using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Levelable : MonoBehaviour
{
    public int maxLevel;
    public int _currentLevel;
    public int currentLevel { get { return _currentLevel; } set
        {
            _currentLevel = value;
            OnLevelUpgrade();
        } }

    public abstract void OnLevelUpgrade();


    [Button]
    public void UpdateMyLevel(int value = 1)
    {
        if (!CheckCanUpdateLevel())
            return;
        currentLevel += value;
    }

    public bool CheckCanUpdateLevel()
    {
        return currentLevel < maxLevel;
    }

    public virtual void ManualSetLevel(int level)
    {
        if (!CheckCanUpdateLevel())
            return;

        for (int i = 0; i < level; i++)
        {
            UpdateMyLevel();

        }
    }

}


public abstract class Buyable : Levelable
{
    public int myPrice;

    [Button]
    public void CheckForUpgrade()
    {
        if (!CheckDoIHaveEnoughMoney())
            return;
        if (!CheckCanUpdateLevel())
            return;
        UpdateMyMoney();
        UpdateMyPrice();
        UpdateMyLevel();
    }

    public bool CheckDoIHaveEnoughMoney()
    {
        return GameManager.Instance.DoIHaveEnoughMoney(myPrice);
    }

    public virtual void UpdateMyPrice()
    {
        myPrice *= 2;
    }

    private void UpdateMyMoney()
    {
        GameManager.Instance.MoneyAdd(-myPrice);
    }

}
