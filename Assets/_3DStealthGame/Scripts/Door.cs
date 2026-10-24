using UnityEngine;
public class Door : MonoBehaviour {
 [SerializeField] string requiredKey="StorageKey";
 public bool Unlocked{get;private set;}
 void OnCollisionEnter(Collision c){TryUnlock(c.collider.GetComponentInParent<PlayerInventory>());}
 void OnCollisionStay(Collision c){TryUnlock(c.collider.GetComponentInParent<PlayerInventory>());}
 public bool TryUnlock(PlayerInventory inventory){if(Unlocked)return true;if(!inventory)return false;if(!inventory.OwnKey(requiredKey)){FindFirstObjectByType<GameEnding>()?.ShowMessage("Locked — requires "+requiredKey);return false;}Unlocked=true;var obstacle=GetComponent<UnityEngine.AI.NavMeshObstacle>();if(obstacle)obstacle.enabled=false;FindFirstObjectByType<GameEnding>()?.ShowMessage("Door unlocked — the exit is ahead.");foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;return true;}
}


