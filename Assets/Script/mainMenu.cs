using UnityEngine;
using UnityEngine.SceneManagement; // 記得引用這行才能切換場景

public class MainMenu : MonoBehaviour
{
    public void OnStartGameClick()
    {
        // 確保剛開始是「沒看過影片」的狀態
        GlobalState.hasWatchedIntro = false; 
        
        // 載入遊戲場景 (請確認你的遊戲場景名字是 "GameScene" 或是你自己取的)
        SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitGameClick()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}