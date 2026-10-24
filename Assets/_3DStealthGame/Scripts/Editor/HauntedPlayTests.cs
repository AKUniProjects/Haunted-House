using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
[InitializeOnLoad]
public static partial class HauntedPlayTests {
 static IEnumerator routine;static float resume;static Keyboard keyboard;
 static string log="Temp/HauntedPlayTests.txt";
 static HauntedPlayTests(){EditorApplication.playModeStateChanged+=State;EditorApplication.update+=Tick;}
 public static void Run(){SessionState.SetString("HauntedTestMode","");log="Temp/HauntedPlayTests.txt";File.WriteAllText(log,"Haunted House actual Play mode integration tests\n");SessionState.SetBool("HauntedTests",true);EditorSceneManager.OpenScene("Assets/_3DStealthGame/scene/MainMenu.unity");EditorApplication.isPlaying=true;}
 static void State(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("HauntedTests",false)){bool focused=SessionState.GetString("HauntedTestMode","")=="Focused";log=focused?"Temp/HauntedFocusedTests.txt":"Temp/HauntedPlayTests.txt";routine=focused?Focused():Suite();resume=Time.realtimeSinceStartup+1;Application.logMessageReceived+=Log;}
 if(s==PlayModeStateChange.ExitingPlayMode){Application.logMessageReceived-=Log;routine=null;if(keyboard!=null){InputSystem.RemoveDevice(keyboard);keyboard=null;}SessionState.SetBool("HauntedTests",false);}}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Warning)File.AppendAllText(log,type+": "+message+"\n");}
 static void Tick(){if(routine==null||!EditorApplication.isPlaying||Time.realtimeSinceStartup<resume)return;try{if(!routine.MoveNext()){routine=null;File.AppendAllText(log,"TEST RUN FINISHED\n");EditorApplication.isPlaying=false;return;}resume=Time.realtimeSinceStartup+(routine.Current is float f?f:0f);}catch(System.Exception e){File.AppendAllText(log,"HARNESS ERROR "+e+"\n");routine=null;EditorApplication.isPlaying=false;}}
 static T One<T>()where T:Object=>Object.FindFirstObjectByType<T>();
 static T[] All<T>()where T:Object=>Object.FindObjectsByType<T>(FindObjectsSortMode.None);
 static void Check(string name,bool pass,string detail=""){File.AppendAllText(log,(pass?"PASS ":"FAIL ")+name+" "+detail+"\n");}
 static void Press(params UnityEngine.InputSystem.Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
 static void Teleport(Vector3 p){var rb=One<PlayerMovement>().GetComponent<Rigidbody>();rb.transform.position=p;rb.position=p;rb.linearVelocity=Vector3.zero;Physics.SyncTransforms();}
 static void Safe(){foreach(var o in All<Observer>())o.enabled=false;}
 static bool PlaceInVision(Observer observer){var pc=One<PlayerMovement>().GetComponent<Collider>();var vision=observer.GetComponent<Collider>();
  Vector3 origin=observer.transform.position;foreach(float distance in new[]{1f,1.5f,2f,.7f}){Vector3 p=origin+observer.transform.forward*distance;p.y=One<PlayerMovement>().transform.position.y;
   var target=p+Vector3.up;var dir=target-origin;if(Physics.Raycast(origin,dir,out var hit,dir.magnitude,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)&&hit.collider!=pc)continue;
   Teleport(p);if(vision.bounds.Intersects(pc.bounds)){observer.enabled=true;return true;}}
  observer.enabled=true;return false;
 }
 static float AudioRms(){var samples=new float[4096];AudioListener.GetOutputData(samples,0);return Mathf.Sqrt(samples.Sum(x=>x*x)/samples.Length);}
 static IEnumerator Suite(){
  Random.InitState(402);keyboard=InputSystem.AddDevice<Keyboard>();
  Check("MainMenu scene opens",SceneManager.GetActiveScene().name=="MainMenu"&&One<MainMenu>());
  var menuDoc=One<UIDocument>();Check("Menu buttons exist",menuDoc.rootVisualElement.Q<Button>("StartButton")!=null&&menuDoc.rootVisualElement.Q<Button>("BonusButton")!=null);
  One<MainMenu>().StartGame();yield return 1f;
  Check("Start Game loads Main",SceneManager.GetActiveScene().name=="Main");
  Safe();var player=One<PlayerMovement>();var ending=One<GameEnding>();var doc=One<UIDocument>();var start=player.transform.position;var camera=Camera.main;var cameraStart=camera.transform.position;
  Check("Single active Audio Listener",All<AudioListener>().Count(x=>x.enabled)==1);
  Check("UI starts hidden",doc.rootVisualElement.Q("EndScreen").resolvedStyle.display==DisplayStyle.None&&doc.rootVisualElement.Q("CaughtScreen").resolvedStyle.display==DisplayStyle.None);
  Check("Ambient loops and plays",GameObject.Find("Ambient_AudioSource").GetComponent<AudioSource>().isPlaying&&GameObject.Find("Ambient_AudioSource").GetComponent<AudioSource>().loop);
  var ghost=All<WaypointPatrol>().First(x=>x.enabled);var ghostStart=ghost.transform.position;var ai=One<AIPatrol>();var aiStart=ai.transform.position;
  Press(UnityEngine.InputSystem.Key.W);yield return .8f;
  Check("WASD movement",Vector3.Distance(start,player.transform.position)>.3f,player.transform.position.ToString());
  Check("Walk animation",player.GetComponent<Animator>().GetBool("IsWalking"));Check("Footsteps playing",player.GetComponent<AudioSource>().isPlaying);
  Check("Camera follows",Vector3.Distance(cameraStart,camera.transform.position)>.1f);
  Press();yield return .25f;
  Check("Idle and footsteps stop",!player.GetComponent<Animator>().GetBool("IsWalking")&&!player.GetComponent<AudioSource>().isPlaying);
  Check("Player remains on floor",player.transform.position.y>-.8f&&player.transform.position.y<.3f);
  yield return 2f;
  Check("Waypoint ghost patrol moves",Vector3.Distance(ghostStart,ghost.transform.position)>.2f);
  float travel=0;for(int n=0;n<8;n++){var prev=ai.transform.position;yield return .5f;travel+=Vector3.Distance(prev,ai.transform.position);}Check("NavMesh ghost patrol moves",ai.GetComponent<NavMeshAgent>().isOnNavMesh&&ai.DestinationsChosen>0&&travel>.3f,"travel="+travel+" destinations="+ai.DestinationsChosen);
  Check("Ghost audio spatial attenuation configured",All<WaypointPatrol>().All(x=>x.GetComponent<AudioSource>().spatialBlend==1&&x.GetComponent<AudioSource>().maxDistance==10&&x.GetComponent<AudioSource>().isPlaying));
  Check("Elapsed timer increases",ending.ElapsedTime>2&&float.TryParse(doc.rootVisualElement.Q<Label>("TimerLabel").text,out _));
  var sounds=All<AudioSource>();var originalMutes=sounds.Select(x=>x.mute).ToArray();var spatial=ghost.GetComponent<AudioSource>();foreach(var sound in sounds)sound.mute=sound!=spatial;ghost.enabled=false;Teleport(ghost.transform.position+Vector3.right);yield return .7f;float nearRms=AudioRms();Teleport(ghost.transform.position+Vector3.right*15);yield return .7f;float farRms=AudioRms();Check("Spatial ghost audio output attenuates",nearRms>0.00001f&&farRms<nearRms*.15f,"near="+nearRms+" far="+farRms);for(int i=0;i<sounds.Length;i++)sounds[i].mute=originalMutes[i];Teleport(start);yield return .2f;
  var gargoyle=All<Observer>().First(x=>x.transform.parent.name.StartsWith("Gargoyle"));Check("Gargoyle visible test location",PlaceInVision(gargoyle));var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Test vision blocker";blocker.transform.position=(gargoyle.transform.position+player.transform.position+Vector3.up)*.5f;blocker.transform.localScale=new Vector3(.5f,2,.5f);Physics.SyncTransforms();yield return .3f;Check("Wall obstructs enemy vision",!ending.EndingStarted);Object.DestroyImmediate(blocker);yield return .5f;
  Check("Gargoyle detection causes loss",ending.EndingStarted&&!ending.Escaped);
  var finalTime=ending.ElapsedTime;float opacity=doc.rootVisualElement.Q("CaughtScreen").resolvedStyle.opacity;
  Check("Caught UI fading",opacity>0&&opacity<1,opacity.ToString());Check("Lose audio once",ending.EndingSoundCount==1&&GameObject.Find("Caught_AudioSource").GetComponent<AudioSource>().isPlaying);
  ending.CaughtPlayer();ending.BeginEnding(true);Check("Ending is exclusive",!ending.Escaped&&ending.EndingSoundCount==1);yield return .3f;Check("Timer freezes",Mathf.Approximately(finalTime,ending.ElapsedTime));
  yield return 2f;Check("Loss reloads Main",SceneManager.GetActiveScene().name=="Main"&&!One<GameEnding>().EndingStarted);Safe();
  Check("Restart resets timer and movement",One<GameEnding>().ElapsedTime<2&&One<PlayerMovement>().MovementEnabled);
  var observer=All<Observer>().First(x=>x.transform.parent.GetComponent<WaypointPatrol>());foreach(var patrol in All<WaypointPatrol>())patrol.enabled=false;
  Check("Ghost visible test location",PlaceInVision(observer));yield return .5f;Check("Ghost detection causes loss",One<GameEnding>().EndingStarted);yield return 2f;Safe();
  ending=One<GameEnding>();Teleport(ending.transform.position);yield return .4f;
  Check("Finish trigger wins",ending.EndingStarted&&ending.Escaped);Check("Win audio once",ending.EndingSoundCount==1&&GameObject.Find("Escape_AudioSource").GetComponent<AudioSource>().isPlaying);yield return 2f;
  Check("Win remains visible",ending&&ending.Escaped&&One<UIDocument>().rootVisualElement.Q("EndScreen").resolvedStyle.opacity>.99f);
  Check("Win stops movement and footsteps",!One<PlayerMovement>().MovementEnabled&&!One<PlayerMovement>().GetComponent<AudioSource>().isPlaying);
  ending.ReturnToMenu();yield return .6f;Check("Win return to menu",SceneManager.GetActiveScene().name=="MainMenu");One<MainMenu>().StartBonus();yield return 1f;Safe();
  Check("BonusLevel loads",SceneManager.GetActiveScene().name=="BonusLevel");
  player=One<PlayerMovement>();var inventory=player.GetComponent<PlayerInventory>();var door=One<Door>();var doorCollider=door.GetComponent<BoxCollider>();
  Check("Door initially blocks",!door.Unlocked&&doorCollider.enabled&&!doorCollider.isTrigger);
  inventory.AddKey("UnrelatedKey");Check("Unrelated key rejected",!door.TryUnlock(inventory)&&doorCollider.enabled);
  Teleport(new Vector3(6,.05f,8));Press(UnityEngine.InputSystem.Key.W);yield return 1.4f;Press();Check("Physical locked door blocks player",player.transform.position.z<8.8f,player.transform.position.ToString());
  var key=One<Key>();Teleport(key.transform.position-new Vector3(0,.5f,0));yield return .3f;Check("Key pickup stored and hidden",inventory.OwnKey("StorageKey")&&!key.gameObject.activeSelf);
  Teleport(new Vector3(6,.05f,8));Press(UnityEngine.InputSystem.Key.W);yield return 1.7f;Press();Check("Matching key opens door on contact",door.Unlocked&&!doorCollider.enabled);Check("Player passes unlocked door",player.transform.position.z>9.05f,player.transform.position.ToString());
  // Traverse all room connections using the actual movement input and physics, with detection disabled only to isolate layout validation.
  foreach(var enemy in All<WaypointPatrol>())enemy.GetComponent<Collider>().enabled=false;foreach(var enemy in All<AIPatrol>())enemy.GetComponent<Collider>().enabled=false;
  Teleport(new Vector3(0,.05f,0));Press(UnityEngine.InputSystem.Key.W);yield return 6.2f;Press();Check("Start to living room walkable",player.transform.position.z>5.5f,player.transform.position.ToString());
  Press(UnityEngine.InputSystem.Key.D);yield return 6.2f;Press();Check("Living to storage walkable",player.transform.position.x>5.5f,player.transform.position.ToString());
  Press(UnityEngine.InputSystem.Key.A);yield return 1f;Press();Press(UnityEngine.InputSystem.Key.W);yield return 7.8f;Press();Check("Bonus route reaches finish",One<GameEnding>().Escaped,player.transform.position.ToString());
  yield return 2f;Check("Bonus win screen",One<UIDocument>().rootVisualElement.Q("EndScreen").resolvedStyle.opacity>.99f);  SceneManager.LoadScene("Main");yield return 1f;Safe();
  foreach(var enemy in All<WaypointPatrol>())enemy.GetComponent<Collider>().enabled=false;foreach(var enemy in All<AIPatrol>())enemy.GetComponent<Collider>().enabled=false;
  var route=HauntedSetup.PlayerRoute();Check("Main player-sized route exists",route.Length>0);
  var pad=InputSystem.AddDevice<Gamepad>();player=One<PlayerMovement>();player.MoveAction.Disable();player.MoveAction.AddBinding("<Gamepad>/leftStick");player.MoveAction.Enable();Time.timeScale=3;
  float deadline=Time.realtimeSinceStartup+80;int corner=1;
  while(corner<route.Length && Time.realtimeSinceStartup<deadline&&!One<GameEnding>().EndingStarted){Vector3 delta=route[corner]-player.transform.position;delta.y=0;if(delta.magnitude<.08f){corner++;continue;}var direction=delta.normalized;InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(direction.x,direction.z)});yield return 0f;}
  InputSystem.QueueStateEvent(pad,new GamepadState());Time.timeScale=1;yield return .5f;Check("Main physical start-to-finish traversal",One<GameEnding>().Escaped,"corner="+corner+"/"+route.Length+" position="+player.transform.position);InputSystem.RemoveDevice(pad);
 }
}
public static partial class HauntedSetup { public static void Test(){HauntedPlayTests.Run();} }




