using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{


    [SerializeField]
    private int moreBallsBaseValue = 5,
        moreBallsIncrementValue = 6,
        incomeBaseValue = 15,
        incomeIncrementValue = 6;
    [SerializeField]
    private float moreBallsExpRatio = 4f,
        incomeExpRatio = 50;

    public static EconomyManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

    }


    public int getIncomePrice(int levelIndex)
    {

        int price = Mathf.FloorToInt(incomeBaseValue + incomeIncrementValue * Mathf.Pow(levelIndex, 2) + ((levelIndex * (levelIndex + 1)) / 2) * incomeExpRatio);

        //print("MERGE XPrice: " + price + " Level: " + levelIndex);
        return price;
    }

    public int getMoreBallsPrice(int levelIndex) 
    {

        int price = Mathf.FloorToInt(moreBallsBaseValue + moreBallsIncrementValue * Mathf.Pow(levelIndex, 2) + ((levelIndex * (levelIndex + 1)) / 2) * moreBallsExpRatio);

       // print("ADD XPrice: " + price +" Level: "+levelIndex);
        return price;    
    }

}
