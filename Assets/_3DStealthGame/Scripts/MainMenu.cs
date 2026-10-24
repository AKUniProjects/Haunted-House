using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class MainMenu : MonoBehaviour {
 Button start,bonus,quit;
 void OnEnable(){var root=GetComponent<UIDocument>().rootVisualElement;start=root.Q<Button>("StartButton");quit=root.Q<Button>("ExitButton");bonus=root.Q<Button>("BonusButton");if(start!=null)start.clicked+=StartGame;if(quit!=null)quit.clicked+=Quit;if(bonus!=null)bonus.clicked+=StartBonus;}
 void OnDisable(){if(start!=null)start.clicked-=StartGame;if(quit!=null)quit.clicked-=Quit;if(bonus!=null)bonus.clicked-=StartBonus;}
 public void StartGame(){Time.timeScale=1;SceneManager.LoadScene("Main");}
 public void StartBonus(){Time.timeScale=1;SceneManager.LoadScene("BonusLevel");}
 void Quit(){
#if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
#elif UNITY_WEBGL
 quit.text="Close this browser tab to exit";
#else
 Application.Quit();
#endif
 }
}
