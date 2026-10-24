using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public static partial class HauntedSetup {
 public static Vector3[] PlayerRoute(){var root=GameObject.Find("Level A");var surface=root.GetComponent<NavMeshSurface>();surface.RemoveData();var sources=new List<NavMeshBuildSource>();NavMeshBuilder.CollectSources(root.transform,~0,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);var settings=surface.GetBuildSettings();settings.agentRadius=.36f;settings.agentHeight=1.5f;settings.agentClimb=.1f;settings.overrideVoxelSize=true;settings.voxelSize=.04f;Bounds bounds=new Bounds(Vector3.zero,new Vector3(100,20,100));var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);var instance=NavMesh.AddNavMeshData(data);Vector3[] result=new Vector3[0];
 try{var p=All<PlayerMovement>().Single();var e=All<GameEnding>().Single();var path=new NavMeshPath();bool a=NavMesh.SamplePosition(p.transform.position,out var start,2,NavMesh.AllAreas);bool z=NavMesh.SamplePosition(e.transform.position,out var end,2,NavMesh.AllAreas);bool valid=a&&z&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;if(valid)result=path.corners;File.WriteAllText("Temp/HauntedPlayerRoute.txt","Complete="+valid+"\n"+string.Join(";",path.corners.Select(c=>c.ToString())));}finally{instance.Remove();Object.DestroyImmediate(data);surface.AddData();}return result;}
 public static void Route(){EditorSceneManager.OpenScene(A+"scene/Main.unity");PlayerRoute();}
 public static void Polish(){EditorSceneManager.OpenScene(A+"scene/Main.unity");Profile();SaveCurrent();Menu();}
}

