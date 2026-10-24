using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public static partial class HauntedSetup { public static void ConfigureAll(){Repair();Integrate();MainNavigation();Bonus();Menu();File.WriteAllText("Temp/HauntedResult.txt","ALL SCENES SAVED");}
 static void Bake(GameObject environment,string name){
  var surface=Ensure<NavMeshSurface>(environment);surface.collectObjects=CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.overrideVoxelSize=true;surface.voxelSize=.08f;
  var sources=new List<NavMeshBuildSource>();NavMeshBuilder.CollectSources(environment.transform,~0,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
  var settings=surface.GetBuildSettings();settings.agentRadius=.5f;settings.agentClimb=.1f;settings.agentHeight=1.5f;settings.overrideVoxelSize=true;settings.voxelSize=.08f;
  Bounds bounds=new Bounds(environment.transform.position,Vector3.zero);foreach(var c in environment.GetComponentsInChildren<Collider>())if(c.enabled&&!c.isTrigger)bounds.Encapsulate(c.bounds);bounds.Expand(4);
  var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(!data)throw new System.Exception("Navmesh bake failed");
  string path=A+"scene/"+name+"_NavMesh.asset";var old=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);surface.RemoveData();if(old){EditorUtility.CopySerialized(data,old);Object.DestroyImmediate(data);data=old;}else AssetDatabase.CreateAsset(data,path);surface.navMeshData=data;surface.AddData();EditorUtility.SetDirty(surface);
 }
 static GameObject AI(Vector3 position,GameEnding ending,PlayerMovement player){var go=GameObject.Find("GhostAI");if(!go)go=Spawn("Prefabs/Characters/Ghost.prefab",position);go.name="GhostAI";Ghost(go);var wp=go.GetComponent<WaypointPatrol>();if(wp)wp.enabled=false;var agent=Ensure<NavMeshAgent>(go);agent.radius=.5f;agent.height=1.5f;agent.speed=.8f;agent.angularSpeed=180;agent.acceleration=4;agent.stoppingDistance=.15f;agent.baseOffset=0;
  if(NavMesh.SamplePosition(position,out var hit,4,NavMesh.AllAreas))go.transform.position=hit.position;else throw new System.Exception("No navmesh at GhostAI spawn "+position);
  Ensure<AIPatrol>(go);var ob=go.GetComponentInChildren<Observer>();ob.player=player.transform;Ref(ob,"gameEnding",ending);
  string path=A+"Prefabs/Characters/GhostAI.prefab";if(!AssetDatabase.LoadAssetAtPath<GameObject>(path)) {var copy=Object.Instantiate(go);copy.name="GhostAI";var co=copy.GetComponentInChildren<Observer>();co.player=null;Ref(co,"gameEnding",null);PrefabUtility.SaveAsPrefabAsset(copy,path);Object.DestroyImmediate(copy);}return go;
 }
 static void KeyDoor(Vector3 keyPos,Vector3 doorPos,Quaternion rotation){
  Directory.CreateDirectory(A+"Prefabs/Interactions");var key=GameObject.Find("StorageKey");if(!key){key=Spawn("Tutorial_Demo/Demo_Prefabs/Key&Doors/Key.prefab",keyPos);PrefabUtility.UnpackPrefabInstance(key,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);foreach(var c in key.GetComponents<MonoBehaviour>())Object.DestroyImmediate(c);key.name="StorageKey";Ensure<Key>(key);var box=Ensure<BoxCollider>(key);box.isTrigger=true;box.size=new Vector3(2,2,8);PrefabUtility.SaveAsPrefabAsset(key,A+"Prefabs/Interactions/StorageKey.prefab");}
  var door=GameObject.Find("LockedStorageDoor");if(!door){door=new GameObject("LockedStorageDoor");door.transform.SetPositionAndRotation(doorPos,rotation);var visual=Spawn("Prefabs/Environment/Decorations/Corridors/Door.prefab",doorPos,door.transform);visual.transform.localRotation=Quaternion.identity;foreach(var c in visual.GetComponentsInChildren<Collider>())c.enabled=false;var box=door.AddComponent<BoxCollider>();box.size=new Vector3(2.05f,2.4f,.25f);box.center=new Vector3(0,1.2f,0);door.AddComponent<Door>();PrefabUtility.SaveAsPrefabAsset(door,A+"Prefabs/Interactions/LockedStorageDoor.prefab");}
 }
 public static void MainNavigation(){Bake(GameObject.Find("Level A"),"Main");AI(new Vector3(6,-.49f,7.5f),All<GameEnding>().Single(),All<PlayerMovement>().Single());SaveCurrent();File.WriteAllText("Temp/HauntedResult.txt","Main navigation complete");}
 public static void Bonus(){
  SaveCurrent();EditorSceneManager.OpenScene(A+"scene/Main.unity");EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),A+"scene/BonusLevel.unity");
  Object.DestroyImmediate(GameObject.Find("Level A"));Object.DestroyImmediate(GameObject.Find("Enemies"));Object.DestroyImmediate(GameObject.Find("Waypoints"));var oldAI=GameObject.Find("GhostAI");if(oldAI)Object.DestroyImmediate(oldAI);
  var root=new GameObject("CustomHauntedLevel");root.transform.position=Vector3.zero;
  // Four six-metre rooms, arranged as an L. Imported floor modules remain at their authored scale.
  var cells=new[]{new Vector2Int(0,0),new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(1,2)};
  string[] names={"Start Room","Living Room","Storage Room","Finish Room"};
  for(int i=0;i<cells.Length;i++){
   Vector3 center=new Vector3(cells[i].x*6,0,cells[i].y*6);
   var room=Spawn("Prefabs/Environment/Rooms/Room Blanks/Room 6 x 6.prefab",center,root.transform);room.name=names[i];
   var dirs=new[]{new Vector2Int(0,1),new Vector2Int(1,0),new Vector2Int(0,-1),new Vector2Int(-1,0)};
   foreach(var d in dirs){if(!cells.Contains(cells[i]+d))continue;
    foreach(var c in room.GetComponentsInChildren<Collider>()){
     if(!c.name.StartsWith("Wall"))continue;var local=c.bounds.center-center;
     float normal=local.x*d.x+local.z*d.y;float tangent=local.x*d.y-local.z*d.x;
     if(normal>2.7f && Mathf.Abs(tangent)<1.05f)c.gameObject.SetActive(false);
    }
   }
   var rug=Spawn("Prefabs/Environment/Decorations/Bedroom/Rug_Bedroom.prefab",center+Vector3.up*.015f,room.transform);foreach(var c in rug.GetComponentsInChildren<Collider>())c.enabled=false;
   Spawn("Prefabs/Environment/Decorations/General/ArmChair.prefab",center+new Vector3(-2,0,1.8f),room.transform);
   Spawn("Prefabs/Environment/Decorations/General/SmallTable_A.prefab",center+new Vector3(2,0,1.8f),room.transform);
   var light=new GameObject("Room Lantern");light.transform.SetParent(room.transform);light.transform.position=center+Vector3.up*2.5f;var l=light.AddComponent<Light>();l.type=LightType.Point;l.range=7;l.intensity=2;l.color=new Color(1,.73f,.4f);l.shadows=LightShadows.None;
  }
  PrefabUtility.SaveAsPrefabAsset(root,A+"Prefabs/Levels/CustomHauntedLevel.prefab");
  var player=All<PlayerMovement>().Single();player.transform.position=new Vector3(0,.1f,-1.5f);
  var end=All<GameEnding>().Single();end.transform.position=new Vector3(6,1,13.4f);end.GetComponent<BoxCollider>().size=new Vector3(3,2,1.4f);
  var gargoyle=Spawn("Prefabs/Characters/Gargoyle.prefab",new Vector3(-2,0,7),root.transform);gargoyle.transform.rotation=Quaternion.Euler(0,90,0);
  var ghost=Spawn("Prefabs/Characters/Ghost.prefab",new Vector3(6,0,7.5f));Ghost(ghost);var patrol=ghost.GetComponent<WaypointPatrol>();patrol.moveSpeed=.7f;patrol.waypoints=new Transform[2];for(int i=0;i<2;i++){var w=new GameObject("BonusWaypoint"+i);w.transform.position=new Vector3(4.5f+3*i,0,7.5f);patrol.waypoints[i]=w.transform;}
  KeyDoor(new Vector3(1, .6f,1),new Vector3(6,0,9),Quaternion.identity);
  Bake(root,"BonusLevel");AI(new Vector3(7,0,11.5f),end,player);
  foreach(var ob in All<Observer>()){ob.player=player.transform;Ref(ob,"gameEnding",end);}
  var cm=All<Unity.Cinemachine.CinemachineCamera>().Single();cm.Target.TrackingTarget=player.transform;
  SaveCurrent();AssetDatabase.SaveAssets();File.WriteAllText("Temp/HauntedResult.txt","Bonus created");
 }
 public static void Menu(){
  SaveCurrent();EditorSceneManager.OpenScene(A+"Tutorial_Demo/Demo_Scenes/MainMenu.unity");EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),A+"scene/MainMenu.unity");
  foreach(var m in All<StealthGame.MainMenu>())Object.DestroyImmediate(m);
  var doc=All<UIDocument>().First();doc.visualTreeAsset=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(A+"UI/MainMenu.uxml");Ensure<MainMenu>(doc.gameObject);
  var panel=Object.Instantiate(doc.panelSettings);string pp=A+"UI/MenuPanelSettings.asset";if(!AssetDatabase.LoadAssetAtPath<PanelSettings>(pp))AssetDatabase.CreateAsset(panel,pp);else Object.DestroyImmediate(panel);panel=AssetDatabase.LoadAssetAtPath<PanelSettings>(pp);panel.scaleMode=PanelScaleMode.ScaleWithScreenSize;panel.referenceResolution=new Vector2Int(1920,1080);doc.panelSettings=panel;EditorUtility.SetDirty(panel);
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(A+"scene/MainMenu.unity",true),new EditorBuildSettingsScene(A+"scene/Main.unity",true),new EditorBuildSettingsScene(A+"scene/BonusLevel.unity",true)};
  SaveCurrent();AssetDatabase.SaveAssets();File.WriteAllText("Temp/HauntedResult.txt","Menu complete");
 }
}





