using UnityEngine;
public class Coin : MonoBehaviour
{
    public float value = 5;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CollectCoin(value, transform.position);
            gameObject.SetActive(false);
        }
    }
}