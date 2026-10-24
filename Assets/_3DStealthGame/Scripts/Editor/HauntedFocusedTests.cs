using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
public static partial class HauntedPlayTests {
 public static void RunFocused(){File.WriteAllText("Temp/HauntedFocusedTests.txt","Focused Play mode tests\n");SessionState.SetString("HauntedTestMode","Focused");SessionState.SetBool("HauntedTests",true);EditorSceneManager.OpenScene("Assets/_3DStealthGame/scene/Main.unity");EditorApplication.isPlaying=true;}
 static IEnumerator Focused(){
  Safe();var player=One<PlayerMovement>();var ending=One<GameEnding>();var ob=All<Observer>().First(x=>x.transform.parent.name.StartsWith("Gargoyle"));
  Check("Vision test placement",PlaceInVision(ob));ob.enabled=false;yield return .15f;
  var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Test vision blocker";var target=player.transform.position+Vector3.up;var ray=target-ob.transform.position;blocker.transform.position=ob.transform.position+ray*.5f;blocker.transform.localScale=new Vector3(.4f,2,.4f);Physics.SyncTransforms();ob.enabled=true;
  Check("Ray hits temporary wall",Physics.Raycast(ob.transform.position,ray,out var hit,ray.magnitude,~0,QueryTriggerInteraction.Ignore)&&hit.collider==blocker.GetComponent<Collider>(),"hit="+(hit.collider?hit.collider.name:"none"));
  yield return .4f;Check("Wall obstructs enemy vision",!ending.EndingStarted);Object.DestroyImmediate(blocker);yield return .4f;Check("Removing obstruction restores detection",ending.EndingStarted);yield return 2f;Safe();
  foreach(var w in All<WaypointPatrol>())w.enabled=false;foreach(var a in All<AIPatrol>())a.StopPatrol();
  var ghost=All<WaypointPatrol>().First(x=>!x.GetComponent<AIPatrol>());var sound=ghost.GetComponent<AudioSource>();foreach(var a in All<AudioSource>())a.Stop();
  var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;var mute=typeof(EditorUtility).GetProperty("audioMasterMute",flags);File.AppendAllText(log,"Audio master mute="+(mute==null?"unavailable":mute.GetValue(null).ToString())+" listenerVolume="+AudioListener.volume+" paused="+AudioListener.pause+" sampleRate="+AudioSettings.outputSampleRate+"\n");if(mute!=null&&mute.CanWrite)mute.SetValue(null,false);
  ghost.transform.position=new Vector3(-7,-.45f,-5.5f);ghost.GetComponent<Rigidbody>().position=ghost.transform.position;sound.priority=0;sound.mute=false;sound.Play();Teleport(new Vector3(-7,-.45f,-6.5f));yield return 1f;
  var data=new float[4096];sound.GetOutputData(data,0);float source=Mathf.Sqrt(data.Sum(v=>v*v)/data.Length);float near=AudioRms();File.AppendAllText(log,"SourceRMS="+source+" virtual="+sound.isVirtual+" time="+sound.time+" load="+sound.clip.loadState+" distance="+Vector3.Distance(sound.transform.position,One<AudioListener>().transform.position)+"\n");
  Teleport(new Vector3(-22,-.45f,-6.5f));yield return 1f;float far=AudioRms();Check("Spatial ghost audio output attenuates",near>.00001f&&far<near*.15f,"near="+near+" far="+far);
 }
}
public static partial class HauntedSetup {public static void FocusedTest(){HauntedPlayTests.RunFocused();}}
