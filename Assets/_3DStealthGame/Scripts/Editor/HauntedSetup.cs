using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using System.Linq;
using System.Text;
public static partial class HauntedSetup { public static void SaveCurrent(){foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true)){if(PrefabUtility.IsPartOfPrefabInstance(t.gameObject))PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);foreach(var c in t.GetComponents<Component>())if(c && PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);}EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());}
 const string A="Assets/_3DStealthGame/";
 public static T Ensure<T>(GameObject g) where T:Component {var c=g.GetComponent<T>();return c?c:g.AddComponent<T>();}
 public static T[] All<T>() where T:Object=>Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
 public static void Ref(Object obj,string field,Object value){var so=new SerializedObject(obj);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
 static GameObject Spawn(string path,Vector3 position,Transform parent=null){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(A+path));go.transform.SetParent(parent,true);go.transform.position=position;return go;}
 static AudioSource Audio(GameObject g,string clip,float volume,bool loop,float spatial=0){var a=Ensure<AudioSource>(g);a.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(A+"Audio/"+clip+".wav");a.priority=clip=="SFXWin"||clip=="SFXLose"?0:clip=="SFXFootstepsLooping"?32:clip=="SFXGhostMove"?64:96;a.volume=volume;a.loop=loop;a.playOnAwake=loop;a.spatialBlend=spatial;return a;}
 static void Player(GameObject go){var body=go.GetComponent<Rigidbody>();body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.Continuous;body.constraints=RigidbodyConstraints.FreezeRotation;Ensure<PlayerInventory>(go);Ensure<AudioListener>(go);var a=Audio(go,"SFXFootstepsLooping",.65f,true);a.playOnAwake=false;Ref(go.GetComponent<PlayerMovement>(),"footsteps",a);foreach(var an in go.GetComponentsInChildren<Animator>())if(an.gameObject!=go && !an.runtimeAnimatorController)an.enabled=false;}
 static void Ghost(GameObject go){var a=Audio(go,"SFXGhostMove",.4f,true,1);a.maxDistance=10;a.minDistance=1;a.rolloffMode=AudioRolloffMode.Custom;a.SetCustomCurve(AudioSourceCurveType.CustomRolloff,new AnimationCurve(new Keyframe(0,1),new Keyframe(.3f,.45f),new Keyframe(1,0)));var rb=go.GetComponent<Rigidbody>();if(rb){rb.useGravity=false;rb.isKinematic=true;rb.interpolation=RigidbodyInterpolation.Interpolate;}var ob=go.GetComponentInChildren<Observer>();if(ob){ob.transform.localPosition=new Vector3(0,1.1f,.4f);ob.transform.localRotation=Quaternion.identity;}var model=go.transform.Find("EyeGhost_Model");if(model)model.localRotation=Quaternion.identity;}
 public static void Inspect(){var b=new StringBuilder();foreach(var p in All<WaypointPatrol>())b.AppendLine(p.name+" waypoints="+string.Join(";",p.waypoints.Select(w=>w?w.name+":"+w.position:"NULL")));foreach(var o in All<Observer>())b.AppendLine(o.transform.parent.name+" player="+(o.player?o.player.name:"NULL"));foreach(var path in new[]{"Prefabs/Environment/Rooms/Room Blanks/Floor 6 x 6.prefab","Prefabs/Environment/Rooms/Room Blanks/Room 6 x 6.prefab","Prefabs/Environment/Walls/Wall_Door.prefab","Prefabs/Environment/Decorations/Corridors/Door.prefab"}){var g=Spawn(path,Vector3.zero);b.AppendLine(path);foreach(var c in g.GetComponentsInChildren<Collider>())b.AppendLine(c.name+" "+c.bounds);Object.DestroyImmediate(g);}File.WriteAllText("Temp/HauntedInspect.txt",b.ToString());}
 public static void Repair(){ EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
 SaveCurrent();
 foreach(var pair in new[]{"player","Ghost"}){var path=A+"Prefabs/Characters/"+pair+".prefab";var go=PrefabUtility.LoadPrefabContents(path);if(pair=="player")Player(go);else Ghost(go);PrefabUtility.SaveAsPrefabAsset(go,path);PrefabUtility.UnloadPrefabContents(go);}
 var p=All<PlayerMovement>().Single();Player(p.gameObject);
 foreach(var a in All<AudioListener>())a.enabled=a.gameObject==p.gameObject;
 foreach(var w in All<WaypointPatrol>()){Ghost(w.gameObject);if(w.waypoints.Length>0 && w.waypoints[0]){var pos=w.transform.position;pos.y=w.waypoints[0].position.y;w.transform.position=pos;}}
 foreach(var o in All<Observer>())o.player=p.transform;
 foreach(var light in All<Light>())if(light.type!=LightType.Directional)light.shadows=LightShadows.None;
 SaveCurrent();
 // Transfer the user's completed gameplay into the existing, previously empty Main scene.
 EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),A+"scene/Main.unity");EditorSceneManager.OpenScene(A+"scene/Main.unity");
 p=All<PlayerMovement>().Single();foreach(var o in All<Observer>())o.player=p.transform;
 foreach(var w in All<WaypointPatrol>()){for(int i=0;i<w.waypoints.Length;i++){if(w.waypoints[i])continue;string prefix=w.name=="Ghost"?"Waypoint_1":w.name=="Ghost (1)"?"Waypoint_2":w.name=="Ghost (2)"?"Waypoint_3":"Waypoint_4";string suffix=i==0?"_Start":i==w.waypoints.Length-1?"_End":"_mid"+i;w.waypoints[i]=GameObject.Find(prefix+suffix)?.transform;}}
 var cm=All<Unity.Cinemachine.CinemachineCamera>().FirstOrDefault();if(cm)cm.Target.TrackingTarget=p.transform;
 SaveCurrent();File.WriteAllText("Temp/HauntedResult.txt","Repair complete");Inspect();
 }
 public static void Integrate(){
 var p=All<PlayerMovement>().Single();Player(p.gameObject);
 var ui=GameObject.Find("FaderCanvas")??new GameObject("FaderCanvas");var doc=Ensure<UIDocument>(ui);doc.visualTreeAsset=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(A+"UI/MainUI.uxml");doc.panelSettings=AssetDatabase.LoadAssetAtPath<PanelSettings>(A+"UI/PanelSettings.asset");
 var end=GameObject.Find("GameEnding")??new GameObject("GameEnding");end.transform.position=new Vector3(17,-.1f,.2f);var box=Ensure<BoxCollider>(end);box.isTrigger=true;box.size=new Vector3(3,2,3);var ending=Ensure<GameEnding>(end);Ref(ending,"player",p.gameObject);Ref(ending,"uiDocument",doc);
 var audio=GameObject.Find("Audio")??new GameObject("Audio");foreach(var n in new[]{"Ambient","Escape","Caught"}){var go=GameObject.Find(n+"_AudioSource")??new GameObject(n+"_AudioSource");go.transform.SetParent(audio.transform);go.transform.position=Vector3.zero;var a=Audio(go,n=="Ambient"?"SFXHouseAmbience":n=="Escape"?"SFXWin":"SFXLose",n=="Ambient"?.5f:1,n=="Ambient");if(n!="Ambient")Ref(ending,n=="Escape"?"exitAudio":"caughtAudio",a);}
 foreach(var o in All<Observer>()){o.player=p.transform;Ref(o,"gameEnding",ending);}
 Profile();SaveCurrent();File.WriteAllText("Temp/HauntedResult.txt","Integration complete");
 }
 static void Profile(){string path=A+"Art/PostProcessing/Haunted Custom.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){AssetDatabase.CopyAsset(A+"Art/PostProcessing/Scary Volume Profile.asset",path);profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);}
 var bloom=Get<Bloom>(profile);bloom.intensity.Override(.25f);bloom.threshold.Override(1f);var vignette=Get<Vignette>(profile);vignette.intensity.Override(.28f);var color=Get<ColorAdjustments>(profile);color.postExposure.Override(.15f);color.contrast.Override(12);color.saturation.Override(-12);var grain=Get<FilmGrain>(profile);grain.intensity.Override(.12f);var dof=Get<DepthOfField>(profile);dof.active=true;dof.mode.Override(DepthOfFieldMode.Gaussian);dof.gaussianStart.Override(12);dof.gaussianEnd.Override(22);dof.gaussianMaxRadius.Override(.5f);
 foreach(var c in profile.components)EditorUtility.SetDirty(c);EditorUtility.SetDirty(profile);
 var vols=All<Volume>();foreach(var v in vols)v.enabled=false;var go=GameObject.Find("Global post-processing")??new GameObject("Global post-processing");var volume=Ensure<Volume>(go);volume.enabled=true;volume.isGlobal=true;volume.sharedProfile=profile;foreach(var cam in All<Camera>())Ensure<UniversalAdditionalCameraData>(cam.gameObject).renderPostProcessing=true;AssetDatabase.SaveAssets();
 }
 static T Get<T>(VolumeProfile p)where T:VolumeComponent{if(p.TryGet<T>(out var c))return c;c=p.Add<T>(true);AssetDatabase.AddObjectToAsset(c,p);return c;}
}





