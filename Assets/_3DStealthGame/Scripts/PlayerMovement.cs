using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMovement : MonoBehaviour {
 public InputAction MoveAction;
 public float walkSpeed=1f, turnSpeed=20f;
 [SerializeField] AudioSource footsteps;
 Rigidbody body; Animator animator;
 public bool IsWalking {get; private set;}
 public bool MovementEnabled {get;set;}=true;
 void Awake(){body=GetComponent<Rigidbody>(); animator=GetComponent<Animator>(); if(!footsteps)footsteps=GetComponent<AudioSource>();}
 void OnEnable(){MoveAction?.Enable();}
 void OnDisable(){MoveAction?.Disable(); StopWalking();}
 void FixedUpdate(){Vector2 input=MovementEnabled && Time.timeScale>0 ? MoveAction.ReadValue<Vector2>():Vector2.zero; ApplyMovement(input);}
 public void ApplyMovement(Vector2 input){
  Vector3 movement=Vector3.ClampMagnitude(new Vector3(input.x,0,input.y),1);
  IsWalking=MovementEnabled && movement.sqrMagnitude>0.001f;
  animator.SetBool("IsWalking",IsWalking);
  if(!IsWalking){if(footsteps && footsteps.isPlaying)footsteps.Stop();return;}
  if(footsteps && !footsteps.isPlaying)footsteps.Play();
  Vector3 forward=Vector3.RotateTowards(transform.forward,movement,turnSpeed*Time.fixedDeltaTime,0);
  body.MoveRotation(Quaternion.LookRotation(forward));
  body.MovePosition(body.position+movement*walkSpeed*Time.fixedDeltaTime);
 }
 public void StopWalking(){IsWalking=false;if(animator)animator.SetBool("IsWalking",false);if(footsteps)footsteps.Stop();if(body&&!body.isKinematic)body.linearVelocity=Vector3.zero;}
}
