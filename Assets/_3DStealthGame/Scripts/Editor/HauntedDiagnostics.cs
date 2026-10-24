using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Unity.AI.Navigation;
using System.IO;
using System.Linq;
using System.Text;
public static partial class HauntedSetup {
 public static void PlayMain(){EditorSceneManager.OpenScene(A+"scene/Main.unity");EditorApplication.isPlaying=true;}
 public static void PlayBonus(){EditorSceneManager.OpenScene(A+"scene/BonusLevel.unity");EditorApplication.isPlaying=true;}
 public static void Stop(){EditorApplication.isPlaying=false;}
 public static void Snapshot(){var b=new StringBuilder();b.AppendLine("Scene "+SceneManager.GetActiveScene().path+" playing="+EditorApplication.isPlaying);foreach(var ai in All<AIPatrol>()){var agent=ai.GetComponent<NavMeshAgent>();b.AppendLine(ai.name+" enabled="+ai.enabled+" pos="+ai.transform.position+" agent="+agent.enabled+" onMesh="+agent.isOnNavMesh+" choices="+ai.DestinationsChosen+" updatePosition="+agent.updatePosition+" rb="+ai.GetComponent<Rigidbody>().position);if(agent.isOnNavMesh)b.AppendLine("destination="+agent.destination+" velocity="+agent.velocity+" remaining="+agent.remainingDistance+" stopped="+agent.isStopped+" hasPath="+agent.hasPath+" status="+agent.pathStatus+" next="+agent.nextPosition);}
 foreach(var s in All<NavMeshSurface>())b.AppendLine("surface "+s.name+" data="+s.navMeshData+" enabled="+s.enabled);
 foreach(var p in All<PlayerMovement>())b.AppendLine("player="+p.transform.position);
 foreach(var o in All<Observer>())b.AppendLine("observer="+o.transform.parent.name+" player="+(o.player?o.player.name:"NULL"));
 b.AppendLine("triangles="+NavMesh.CalculateTriangulation().indices.Length);File.WriteAllText("Temp/HauntedSnapshot.txt",b.ToString());}
 public static void Validate(){var b=new StringBuilder();foreach(var name in new[]{"MainMenu","Main","BonusLevel"}){EditorSceneManager.OpenScene(A+"scene/"+name+".unity");b.AppendLine("SCENE "+name);int missing=0;foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);b.AppendLine("Missing scripts="+missing);
 b.AppendLine("Listeners="+All<AudioListener>().Count(a=>a.enabled));foreach(var w in All<WaypointPatrol>())if(w.enabled)b.AppendLine("Patrol "+w.name+" valid="+(w.waypoints.Length>0&&w.waypoints.All(t=>t)));
 foreach(var o in All<Observer>()){var so=new SerializedObject(o);b.AppendLine("Observer "+o.transform.parent.name+" references="+(o.player && so.FindProperty("gameEnding").objectReferenceValue));}
 foreach(var a in All<AudioSource>())b.AppendLine("Audio "+a.name+" clip="+(a.clip?a.clip.name:"MISSING"));
 foreach(var s in All<NavMeshSurface>())b.AppendLine("Navmesh "+s.name+" saved="+(s.navMeshData && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(s.navMeshData))));
 if(name=="Main"){var p=All<PlayerMovement>().Single();var e=All<GameEnding>().Single();bool a=NavMesh.SamplePosition(p.transform.position,out var start,2,NavMesh.AllAreas);bool z=NavMesh.SamplePosition(e.transform.position,out var end,2,NavMesh.AllAreas);var path=new NavMeshPath();b.AppendLine("Start-finish NavMesh route="+(a&&z&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete));if(path.corners.Length>0)b.AppendLine("Route="+string.Join(";",path.corners.Select(c=>c.ToString())));}
 }
 EditorSceneManager.OpenScene(A+"scene/MainMenu.unity");File.WriteAllText("Temp/HauntedValidation.txt",b.ToString());}
}
