using UnityEngine;
using System.Collections;

public class AddLightning : MonoBehaviour 
{
    public GameObject lightningObj;

    float timer = 0f;

    float timeUntilLightning = 5f;



    void Update () 
	{
        timer += Time.deltaTime;

        if (timer > timeUntilLightning)
        {
            timer = 0f;

            timeUntilLightning = Random.Range(5f, 15f);

            //Add a new lightning
            float mapSize = 500f;

            float randomX = Random.Range(-mapSize, mapSize);
            float randomZ = Random.Range(-mapSize, mapSize);

            float y = 230f;

            Vector3 pos = new Vector3(randomX, y, randomZ);

            GameObject newLightning = Instantiate(lightningObj, pos, Quaternion.identity) as GameObject;

            newLightning.transform.parent = transform;
        }
	}
}
