using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class AIPatrol : MonoBehaviour {
 [SerializeField] float wanderRadius=3f,wanderTimer=1.5f;
 NavMeshAgent agent; float elapsed; Vector3 home; NavMeshPath path;
 public int DestinationsChosen {get;private set;}
 void Start(){agent=GetComponent<NavMeshAgent>();home=transform.position;path=new NavMeshPath();elapsed=wanderTimer;}
 void Update(){if(!agent||!agent.isOnNavMesh)return;elapsed+=Time.deltaTime;if(elapsed<wanderTimer)return;elapsed=0;
  for(int i=0;i<12;i++){var r=Random.insideUnitCircle*wanderRadius;Vector3 candidate=home+new Vector3(r.x,0,r.y);
   if(NavMesh.SamplePosition(candidate,out var hit,1f,agent.areaMask) && Vector3.Distance(home,hit.position)<=wanderRadius+1 && Vector3.Distance(transform.position,hit.position)>.75f && agent.CalculatePath(hit.position,path)&&path.status==NavMeshPathStatus.PathComplete){agent.SetPath(path);DestinationsChosen++;return;}}
 }
 public void StopPatrol(){if(agent&&agent.isOnNavMesh)agent.isStopped=true;enabled=false;}
}

