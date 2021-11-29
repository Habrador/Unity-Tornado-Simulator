using UnityEngine;
using System.Collections;

public class TestSpin : MonoBehaviour
{
    public GameObject spinSpehere;

    public float rotationSpeed = 5f;
    //This is the upwards speed, which is not the same as the rotation speed?
    public float speedY = 1f; 
    public float radius = 15f;

    float angle = 0f;


	void Start ()
    {
        Vector3 startPos = transform.position + Vector3.right * radius;

        startPos.y = 0f;

        spinSpehere.transform.position = startPos;
	}
	
	

	void Update ()
    {
    ////Spin the sphere around the cylinder
    ////The current position of the sphere
    //Vector3 currentPos = spinSpehere.transform.position;

    ////Increase the angle with the rotationSpeed
    //angle -= rotationSpeed * Time.deltaTime;

    ////The center pos of what we are rotating around
    //Vector3 centerPos = transform.position;

    ////Calculate the new positions
    //float newX = centerPos.x + radius * Mathf.Sin(angle);
    //float newY = currentPos.y + speedY * Time.deltaTime;
    //float newZ = centerPos.z + radius * Mathf.Cos(angle);

    //Vector3 newPos = new Vector3(newX, newY, newZ);

    ////Debug.Log(newPos);

    //spinSpehere.transform.position = newPos;

    //Make sure the radius is constant no matter how the object we are rotating around is positioned
    //http://gamedev.stackexchange.com/questions/61981/unity3d-orbit-around-orbiting-object-transform-rotatearound
        Vector3 center = transform.position;

        Vector3 bottom = center - transform.up * 100f;
        Vector3 top = center + transform.up * 100f;

        float progress = TornadoMath.CalculateProgress(bottom, top, spinSpehere.transform.position);

       

        //Debug.Log(progress);

        //spinSpehere.transform.position = 

        //Get the coordinate at the progress center line
        Vector3 progressCoordinate = progress * (top - bottom) + bottom;
        
        //Get the current radius
        float currentRadius = (spinSpehere.transform.position - progressCoordinate).magnitude;

        //The direction from the sphere to the center line progress coordinates
        Vector3 direction = (spinSpehere.transform.position - progressCoordinate).normalized;

        //Debug.Log((radius - currentRadius) * direction);

        spinSpehere.transform.position += (radius - currentRadius) * direction;

        spinSpehere.transform.RotateAround(transform.position, transform.up, rotationSpeed * Time.deltaTime);

        spinSpehere.transform.Translate(transform.up * speedY * Time.deltaTime);


        if (progress > 1f)
        {
            spinSpehere.transform.position = bottom + Vector3.right * radius;
        }
    }




    
}
