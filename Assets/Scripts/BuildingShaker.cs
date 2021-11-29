using UnityEngine;
using System.Collections;

public class BuildingShaker : MonoBehaviour 
{
    //Drags
    public Transform[] floorArray;

    //The ground that will shake so we can shake the building
    private Transform groundObj;

    //Needed in the Euler forward
    private Vector3[] posNew;
    private Vector3[] posOld;
    private Vector3[] velArray;

    //Building parameters
    //float m = 5000f; // kg / floor
    //float k = 10000f; // kg/s^2
    //float c = 1000f; // kg/s

    public float m;
    float k;
    float c;

    //Save the start position of the building in global coordinates
    //relative to the center of the ground
    //so the building can be positioned everywhere
    Vector3 startPos;



	void Start() 
	{
	    //Get the ground
        groundObj = GameObject.FindGameObjectWithTag("Ground").transform;

        startPos = transform.position - groundObj.position;

        //Initialize the arrays
        posNew = new Vector3[floorArray.Length];
        posOld = new Vector3[floorArray.Length];
        velArray = new Vector3[floorArray.Length];

        //Add init values to the arrays
        for (int i = 0; i < posNew.Length; i++) 
        {
            posNew[i] = Vector3.zero;
            posOld[i] = floorArray[i].position;
            velArray[i] = Vector3.zero;
        }

        k = 2f * m;
        c = k / 10f;
	}
	
	

    //Should be in fixed update because timestep is always 0.02
	void FixedUpdate() 
	{
        //Move the building with the ground
        transform.position = groundObj.position + startPos;
        
        ShakeBuilding();
	}



    void ShakeBuilding()
    {        
        //Time.deltatime might give an unstable result because we are using Euler forward
        float h = 0.02f;

        //Iterate through the floors to calculate the new position and velocity
        for (int i = 0; i < floorArray.Length; i++)
        {
            Vector3 oldPosVec = posOld[i];
            
            //
            //Calculate the floor's acceleration
            //
            Vector3 accVec = Vector3.zero;

            //First floor
            if (i == 0) 
            {
                accVec = (-k * (oldPosVec - transform.position) + k * (posOld[i + 1] - oldPosVec)) / m;
            }
            //Last floor
            else if (i == floorArray.Length - 1)
            {
                //m = 500f; //If the last floor is smaller
                accVec = (-k * (oldPosVec - posOld[i - 1])) / m;
            }
            //Middle floors
            else 
            {
                accVec = (-k * (oldPosVec - posOld[i - 1]) + k * (posOld[i + 1] - oldPosVec)) / m;
            }

            //Add damping to the final acceleration
            accVec -= (c * velArray[i]) / m;


            //
            //Euler forward
            //
            //Add the new position
            posNew[i] = oldPosVec + h * velArray[i];
            //Add the new velocity
            velArray[i] = velArray[i] + h * accVec;
        }
        

        //Add the new coordinates to the floors
        for (int i = 0; i < floorArray.Length; i++)
        {
            //Assume no spring-like behavior in y direction
            Vector3 newPos = new Vector3(
                posNew[i].x,
                floorArray[i].position.y,
                posNew[i].z);

            floorArray[i].position = newPos;

            //Transfer the values from this update to the next
            posOld[i] = posNew[i];
        }
    }
}
