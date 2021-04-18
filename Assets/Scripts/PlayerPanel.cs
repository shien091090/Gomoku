using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    public Text stateText; //狀態文字
    public Image chessIcon; //棋子圖示
    public Text turnText; //順序文字(先手&後手)
    public Button readyButton; //就緒按鈕
    public Text readyStateText; //就緒狀態文字

    public void SetReadyButtonState(int stateNum) //0=他玩家 / 1=未激活 / 2=激活
    {
        if (!this.gameObject.activeSelf) this.gameObject.SetActive(true);

        Image buttonImg;

        switch (stateNum)
        {
            case 0:
                buttonImg = this.transform.Find("Ready Button/Not Local").GetComponent<Image>(); //取得就緒按鈕的樣式
                readyStateText = buttonImg.transform.Find("Text").GetComponent<Text>(); //取得就緒狀態文字

                buttonImg.gameObject.SetActive(true); //顯示就緒按鈕
                readyStateText.text = "尚未就緒"; //初始化就緒狀態文字
                break;

            case 1:
                readyButton = this.transform.Find("Ready Button/Local").GetComponent<Button>(); //取得就緒按鈕的樣式
                readyStateText = readyButton.transform.Find("Text").GetComponent<Text>(); //取得就緒狀態文字

                buttonImg = readyButton.GetComponent<Image>();
                buttonImg.color = new Color(1, 1, 1, 0.5f);
                buttonImg.raycastTarget = false;

                readyButton.gameObject.SetActive(true); //顯示就緒按鈕
                readyStateText.text = "尚未就緒"; //初始化就緒狀態文字
                break;

            case 2:
                readyButton = this.transform.Find("Ready Button/Local").GetComponent<Button>(); //取得就緒按鈕的樣式
                readyStateText = readyButton.transform.Find("Text").GetComponent<Text>(); //取得就緒狀態文字

                buttonImg = readyButton.GetComponent<Image>();
                buttonImg.color = new Color(1, 1, 1, 1);
                buttonImg.raycastTarget = true;

                readyButton.gameObject.SetActive(true); //顯示就緒按鈕
                readyStateText.text = "尚未就緒"; //初始化就緒狀態文字
                break;
        }
    }

}
