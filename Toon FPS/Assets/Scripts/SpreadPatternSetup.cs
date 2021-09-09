using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadPatternSetup : MonoBehaviour
{
    public void ChangeSpreadPattern(GameObject pattern)
    {
        foreach (Transform child in gameObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        Instantiate(pattern, gameObject.transform);
    }
}
