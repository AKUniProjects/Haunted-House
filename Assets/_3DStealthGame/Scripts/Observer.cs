using UnityEngine;
using System.Collections.Generic;
public class Observer : MonoBehaviour {
 public Transform player;
 [SerializeField] GameEnding gameEnding;
 readonly HashSet<Collider> contacts=new HashSet<Collider>();
 void Start(){if(!player){var p=FindFirstObjectByType<PlayerMovement>();if(p)player=p.transform;}if(!gameEnding)gameEnding=FindFirstObjectByType<GameEnding>();}
 bool IsPlayer(Collider c)=>player && c.GetComponentInParent<PlayerMovement>()?.transform==player;
 void OnTriggerEnter(Collider other){if(IsPlayer(other))contacts.Add(other);}
 void OnTriggerExit(Collider other){contacts.Remove(other);}
 void OnDisable(){contacts.Clear();}
 void Update(){if(!player || !gameEnding || gameEnding.EndingStarted || contacts.Count==0)return;
  Vector3 direction=player.position+Vector3.up-transform.position;
  if(Physics.Raycast(transform.position,direction,out RaycastHit hit,direction.magnitude+.2f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore) && IsPlayer(hit.collider))gameEnding.CaughtPlayer();
 }
}
