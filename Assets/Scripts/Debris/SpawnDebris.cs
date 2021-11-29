using UnityEngine;
using System.Collections;

public class SpawnDebris : MonoBehaviour 
{
    public GameObject sphereDebris;
	


	void Start () 
	{
        float mapWidth = 400f;

        for (int i = 0; i < 40; i++)
        {
            float x = Random.Range(-mapWidth, mapWidth);
            float y = sphereDebris.transform.position.y;
            float z = Random.Range(-mapWidth, mapWidth);

            Instantiate(sphereDebris, new Vector3(x, y, z), Quaternion.identity);
        }
	}
	
	

	void Update () 
	{
	
	}
}
