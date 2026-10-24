using UnityEngine;
public class Key : MonoBehaviour {
 [SerializeField] string keyName="StorageKey";bool collected;
 void OnTriggerEnter(Collider other){var inventory=other.GetComponentInParent<PlayerInventory>();if(collected||!inventory)return;collected=true;inventory.AddKey(keyName);FindFirstObjectByType<GameEnding>()?.ShowMessage(keyName+" collected — find its locked door.");gameObject.SetActive(false);}
}

