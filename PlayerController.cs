using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public Vector3 characterSpawnPoint; 
    public NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>(); 

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SelectedCharacter.OnValueChanged += OnSelectedCharacterChanged;

        SpawnCharacter();
    }

    void OnDestroy()
    {
        SelectedCharacter.OnValueChanged -= OnSelectedCharacterChanged;
    }

    private void OnSelectedCharacterChanged(int previousValue, int newValue)
    {
        if (IsLocalPlayer)
        {
            SpawnCharacter();
        }
    }

    private void SpawnCharacter()
    {
        if (characterSpawnPoint == null || CharacterSelectionMenu.Instance.characterPrefabs.Length == 0)
        {
            Debug.LogError("Character spawn point or character prefabs not set.");
            return;
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        characterSpawnPoint = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

        GameObject characterPrefab = CharacterSelectionMenu.Instance.characterPrefabs[SelectedCharacter.Value];
        GameObject character = Instantiate(characterPrefab, characterSpawnPoint, Quaternion.identity);
        character.transform.parent = transform;
    }
}
