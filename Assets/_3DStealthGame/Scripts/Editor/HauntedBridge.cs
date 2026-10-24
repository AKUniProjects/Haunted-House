using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Text;
[InitializeOnLoad]
public static class HauntedBridge {
 static string root = Path.GetFullPath("Temp/HauntedCommand.txt");
 static HauntedBridge(){EditorApplication.update += Tick;}
 static void Tick(){if(EditorApplication.isCompiling || !File.Exists(root))return; string cmd=File.ReadAllText(root).Trim(); File.Delete(root); try { if(cmd=="audit")Audit(); else if(cmd=="save")EditorSceneManager.SaveOpenScenes(); else { var m=typeof(HauntedBridge).Assembly.GetType("HauntedSetup");m.GetMethod(cmd).Invoke(null,null);} }catch(Exception e){File.WriteAllText("Temp/HauntedResult.txt",e.ToString());Debug.LogException(e);} }
 public static void Audit(){var b=new StringBuilder(); b.AppendLine("ACTIVE "+SceneManager.GetActiveScene().path); foreach(var go in SceneManager.GetActiveScene().GetRootGameObjects()) Walk(go.transform,b); File.WriteAllText("Temp/HauntedAudit.txt",b.ToString());}
 static void Walk(Transform t,StringBuilder b){var cs=t.GetComponents<Component>(); if(t.parent==null || cs.Any(c=>c is Collider || c is Animator || c is PlayerMovement || c is Observer || c is WaypointPatrol) || t.name.Contains("Room")) {b.AppendLine(t.name+" pos="+t.position+" local="+t.localPosition+" rot="+t.eulerAngles+" components="+string.Join(",",cs.Select(c=>c?c.GetType().Name:"MISSING")));foreach(var c in cs){if(c is PlayerMovement || c is Observer || c is WaypointPatrol || c is Animator || c is Rigidbody)b.AppendLine(EditorJsonUtility.ToJson(c));} } foreach(Transform c in t)Walk(c,b);}
}
