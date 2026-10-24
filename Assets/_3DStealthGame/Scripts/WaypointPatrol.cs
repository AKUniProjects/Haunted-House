using UnityEngine;
public class WaypointPatrol : MonoBehaviour {
 public float moveSpeed=1f; public Transform[] waypoints;
 Rigidbody body; int index;
 void Awake(){body=GetComponent<Rigidbody>();}
 void FixedUpdate(){if(!body||waypoints==null||waypoints.Length==0)return; if(!waypoints[index]){index=(index+1)%waypoints.Length;return;}
  Vector3 target=waypoints[index].position; Vector3 delta=target-body.position;
  if(delta.sqrMagnitude<.01f){index=(index+1)%waypoints.Length;return;}
  Vector3 forward=new Vector3(delta.x,0,delta.z);if(forward.sqrMagnitude>.0001f)body.MoveRotation(Quaternion.LookRotation(forward));
  body.MovePosition(Vector3.MoveTowards(body.position,target,moveSpeed*Time.fixedDeltaTime));
 }
}
