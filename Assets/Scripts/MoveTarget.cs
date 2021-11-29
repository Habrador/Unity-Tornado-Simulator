using UnityEngine;
using System.Collections;

//Moves the target the tornado is chasing to random positons
public class MoveTarget : MonoBehaviour
{
    //The torna speed is 48 - should this be the same?
    float speed = 55f;

    //The target the goal is moving towards
    Vector3 goal;

    float mapHalfSize = 500f;



	void Start ()
    {
        goal = new Vector3(Random.Range(-mapHalfSize, mapHalfSize), 0f, Random.Range(-mapHalfSize, mapHalfSize));
	}
	
	

	void Update ()
    {
        //Move towards the goal
        transform.LookAt(goal);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        //Did we reach the goal?
        if ((transform.position - goal).sqrMagnitude < 2f)
        {
            goal = new Vector3(Random.Range(-mapHalfSize, mapHalfSize), 0f, Random.Range(-mapHalfSize, mapHalfSize));
        }
	}
}
