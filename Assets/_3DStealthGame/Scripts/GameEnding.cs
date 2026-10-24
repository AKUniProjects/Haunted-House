using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class GameEnding : MonoBehaviour {
 [SerializeField] float fadeDuration=1f, displayImageDuration=1f;
 [SerializeField] GameObject player;
 [SerializeField] UIDocument uiDocument;
 [SerializeField] AudioSource exitAudio,caughtAudio;
 VisualElement endScreen,caughtScreen,activeScreen; Label timerLabel; Label statusLabel;
 float fadeProgress,displayTime; bool playerEscaped,playerCaught,endingStarted,finished;
 public bool EndingStarted=>endingStarted;
 public bool Escaped=>playerEscaped;
 public float ElapsedTime {get;private set;}
 public int EndingSoundCount {get;private set;}
 void Start(){Time.timeScale=1; if(!player){var p=FindFirstObjectByType<PlayerMovement>();if(p)player=p.gameObject;} if(!uiDocument)uiDocument=FindFirstObjectByType<UIDocument>();
  if(uiDocument){var root=uiDocument.rootVisualElement;endScreen=root.Q("EndScreen");caughtScreen=root.Q("CaughtScreen");timerLabel=root.Q<Label>("TimerLabel");statusLabel=root.Q<Label>("StatusLabel");if(statusLabel!=null)statusLabel.text=gameObject.scene.name=="BonusLevel"?"WASD / Arrows • Find the StorageKey, unlock the door, and escape." : "WASD / Arrows • Avoid the watchers and find the exit.";Hide(endScreen);Hide(caughtScreen);}
  if(endScreen==null || caughtScreen==null || timerLabel==null)Debug.LogError("GameEnding requires EndScreen, CaughtScreen and TimerLabel.",this);
 }
 static void Hide(VisualElement e){if(e==null)return;e.style.display=DisplayStyle.None;e.style.opacity=0;}
 void OnTriggerEnter(Collider other){if(player && other.GetComponentInParent<PlayerMovement>()?.gameObject==player)BeginEnding(true);}
 public void CaughtPlayer(){BeginEnding(false);}
 public void BeginEnding(bool escaped){if(endingStarted)return;endingStarted=true;playerEscaped=escaped;playerCaught=!escaped;activeScreen=escaped?endScreen:caughtScreen;
  if(activeScreen!=null){activeScreen.style.display=DisplayStyle.Flex;activeScreen.style.opacity=0;}
  var movement=player?player.GetComponent<PlayerMovement>():null;if(movement){movement.MovementEnabled=false;movement.StopWalking();}
  foreach(var p in FindObjectsByType<WaypointPatrol>(FindObjectsSortMode.None))p.enabled=false;
  foreach(var p in FindObjectsByType<AIPatrol>(FindObjectsSortMode.None))p.StopPatrol();
  var sound=escaped?exitAudio:caughtAudio;if(sound){sound.Play();EndingSoundCount++;}
 }
 void Update(){if(!endingStarted){ElapsedTime+=Time.deltaTime;if(timerLabel!=null)timerLabel.text=ElapsedTime.ToString("F2",System.Globalization.CultureInfo.InvariantCulture);return;}
  if(finished)return;
  fadeProgress+=Time.unscaledDeltaTime; if(activeScreen!=null)activeScreen.style.opacity=Mathf.Clamp01(fadeProgress/Mathf.Max(.01f,fadeDuration));
  if(fadeProgress<fadeDuration)return;displayTime+=Time.unscaledDeltaTime;if(displayTime<displayImageDuration)return;finished=true;
  if(playerCaught){Time.timeScale=1;SceneManager.LoadScene(gameObject.scene.path);}
  else if(playerEscaped && activeScreen!=null){var button=new Button(ReturnToMenu){text="Return to Main Menu"};button.style.alignSelf=Align.Center;button.style.fontSize=24;button.style.marginBottom=35;activeScreen.Add(button);}
 }
 public void ShowMessage(string message){if(statusLabel!=null)statusLabel.text=message;}
 public void ReturnToMenu(){Time.timeScale=1;SceneManager.LoadScene("MainMenu");}
}

