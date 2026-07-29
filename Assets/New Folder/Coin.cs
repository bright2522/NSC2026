using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinGameManager.Instance.AddScore();
            Destroy(gameObject);
        }
    }
}