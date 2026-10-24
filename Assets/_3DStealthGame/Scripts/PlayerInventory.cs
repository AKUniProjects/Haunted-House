using System.Collections.Generic;
using UnityEngine;
public class PlayerInventory : MonoBehaviour {
 readonly HashSet<string> keys=new HashSet<string>();
 public bool AddKey(string keyName){return !string.IsNullOrWhiteSpace(keyName)&&keys.Add(keyName);}
 public bool OwnKey(string keyName)=>keys.Contains(keyName);
}
