using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using System.Reflection;
public static partial class HauntedSetup {
 public static void FinalAdjustments(){
  foreach(var name in new[]{"player","Ghost","GhostAI"}){string path=A+"Prefabs/Characters/"+name+".prefab";var go=PrefabUtility.LoadPrefabContents(path);if(name=="player")Player(go);else Ghost(go);PrefabUtility.SaveAsPrefabAsset(go,path);PrefabUtility.UnloadPrefabContents(go);}
  foreach(var name in new[]{"Main","BonusLevel"}){EditorSceneManager.OpenScene(A+"scene/"+name+".unity");foreach(var audio in All<AudioSource>()){string clip=audio.clip?audio.clip.name:"";audio.priority=clip=="SFXWin"||clip=="SFXLose"?0:clip=="SFXFootstepsLooping"?32:clip=="SFXGhostMove"?64:clip=="SFXHouseAmbience"?96:200;}
   foreach(var door in All<Door>()){var box=door.GetComponent<BoxCollider>();var obstacle=Ensure<NavMeshObstacle>(door.gameObject);obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;PrefabUtility.SaveAsPrefabAsset(door.gameObject,A+"Prefabs/Interactions/LockedStorageDoor.prefab");}
   SaveCurrent();}
  EditorSceneManager.OpenScene(A+"scene/MainMenu.unity");AssetDatabase.SaveAssets();File.WriteAllText("Temp/HauntedResult.txt","Final adjustments saved");
 }
 public static void ConsoleClear(){typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",BindingFlags.Static|BindingFlags.Public).Invoke(null,null);}
 public static void ConsoleCounts(){var args=new object[]{0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",BindingFlags.Static|BindingFlags.Public).Invoke(null,args);File.WriteAllText("Temp/HauntedConsole.txt","Errors="+args[0]+" Warnings="+args[1]+" Logs="+args[2]+" Compiling="+EditorApplication.isCompiling+" Playing="+EditorApplication.isPlaying);}
 public static void PlayMenu(){EditorSceneManager.OpenScene(A+"scene/MainMenu.unity");EditorApplication.isPlaying=true;}
}
