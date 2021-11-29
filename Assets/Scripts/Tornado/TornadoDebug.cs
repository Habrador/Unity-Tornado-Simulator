using UnityEngine;
using System.Collections;

public class TornadoDebug : MonoBehaviour 
{
    //Cylinder the tornado is made up of for debug
    public GameObject skeletonCylinder;
    //Tornado's goal position and this object is chasing the real target
    public GameObject chaseObj;
    public GameObject chaseRotateObj;
    public GameObject targetObj;

    //Tornado data
    [Header("Tornado data")]
    //Most tornadoes have an average speed of 30 mph (48km/h), but the speed could be between 0-70 mph (0-112 km/h)
    public float tornadoSpeed = 10f;
    //How fast the tornado is rotating towards the goal it's chasing
    public float tornadoRotSpeed = 1f;
    //How high the tornado is
    public float tornadoHeight = 50f;
    //How many pieces does the tornado consists of = its resolution
    public int pieces = 10;
    //Data that will determines the spring like behavior of the tornado
    public float m;
    //These will be automatically set from m
    public float k;
    public float c;
    //The pieces should depend more on the above piece to get a more realistic tornado
    public float aboveFactor = 1f; // 0.6
    public float belowFactor = 1f;
    public float chaseFactor = 1f;


    //Array with all skeleton pieces
    [System.NonSerialized]
    public Transform[] skeletonPiecesArray;

    //Arrays needed for the integration to make the tornado's shape
    private Vector3[] posNew;
    private Vector3[] posOld;
    private Vector3[] velArray;

    Vector3 oldChasePos;



    void Start () 
	{
        //Build the tornado
        BuildTornado();

        //Initialize the arrays
        posNew = new Vector3[pieces];
        posOld = new Vector3[pieces];
        velArray = new Vector3[pieces];
        //averagePos = new Vector3[pieces];

        //Add init values to the arrays
        for (int i = 0; i < pieces; i++)
        {
            posNew[i] = Vector3.zero;
            posOld[i] = skeletonPiecesArray[i].position;
            velArray[i] = Vector3.zero;
            //averagePos[i] = skeletonPiecesArray[i].position;
        }

        //Move the chase object to the tornado's start position
        chaseObj.transform.position = transform.position;
    }



    void Update()
    {
        MoveTornado();
    }



    void FixedUpdate () 
	{
        TornadoDynamics();
	}



    void BuildTornado()
    {
        skeletonPiecesArray = new Transform[pieces];

        //particlesArray = new Transform[pieces];

        float pieceHeight = tornadoHeight / pieces;

        float currentY = 0f;

        for (int i = 0; i < pieces; i++)
        {
            //
            //Add skeleton piece
            //
            GameObject newPiece = Instantiate(skeletonCylinder) as GameObject;

            //Change scale
            Vector3 newScale = skeletonCylinder.transform.localScale;

            newScale.z = pieceHeight / 2f;

            newPiece.transform.localScale = newScale;

            //Change position
            Vector3 position = transform.position;

            position.y = currentY;

            newPiece.transform.position = position;

            //Parent it to the tornado
            newPiece.transform.parent = transform;

            //Add it to the array
            skeletonPiecesArray[i] = newPiece.transform;

            //Increment height for next update
            currentY += pieceHeight;
        }
    }



    void TornadoDynamics()
    {
        //Which position is the tornado chasing
        //Vector3 chasePos = Camera.main.transform.position;

        //Vector3 chasePos = transform.position;

        Vector3 chasePos = chaseObj.transform.position;
        Vector3 rotatePos = chaseRotateObj.transform.position;

        //Vector3 chasePos = rotateChaseObj.transform.position;

        chasePos.y = 0f;

        //The integration time step
        float h = 0.02f;

        //Iterate through the pieces to calculate the new position and velocity
        for (int i = 0; i < pieces; i++)
        {
            //Change m depending on height so the upper part will move slower which is more tornado looking
            //float mMod = m * (1f - (((float)i + 1f) / (float)pieces));

            float mMod = m;

            //k = 2f * mMod;
            //c = k / 10f;


            Vector3 oldPosVec = posOld[i];

            //
            //Calculate the floor's acceleration
            //
            Vector3 accVec = Vector3.zero;


            //First floor
            if (i == 0)
            {
                //accVec = (-k * (oldPosVec - chasePos) + k * (posOld[i + 1] - oldPosVec) * aboveFactor) / m;
                accVec = (-k * (oldPosVec - posOld[i + 1]) * aboveFactor - k * (oldPosVec - rotatePos) * chaseFactor) / m;
                //accVec = (-k * (oldPosVec - posOld[i + 1]) * aboveFactor) / m;
            }
            //Last floor
            else if (i == pieces - 1)
            {
                //accVec = (-k * (oldPosVec - posOld[i - 1])) / m;
                accVec = (-k * (oldPosVec - chasePos) + k * (posOld[i - 1] - oldPosVec) * belowFactor) / m;
            }
            //Middle floors
            else
            {
                //accVec = (-k * (oldPosVec - posOld[i - 1]) + k * (posOld[i + 1] - oldPosVec) * aboveFactor) / m;
                accVec = (-k * (oldPosVec - posOld[i - 1]) * belowFactor + k * (posOld[i + 1] - oldPosVec) * aboveFactor) / m;
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



        //Add the new coordinates to the skeleton pieces
        for (int i = 0; i < pieces; i++)
        {
            Vector3 newPos = posNew[i];

            //Assume no spring-like behavior in y direction
            newPos.y = skeletonPiecesArray[i].position.y;

            //Add to the averagepos
            //averagePos[i] = averagePos[i] + ((newPos - averagePos[i]) / 20f);

            skeletonPiecesArray[i].position = newPos;

            //Transfer the values from this update to the next
            posOld[i] = posNew[i];
        }



        //Make the skeleton pieces look at each other to make it easier to see the final shape of the tornado
        //Important to update the lookat after we updated the positions because
        //they depend on the upper skeleton piece
        for (int i = 0; i < pieces; i++)
        {
            //The skeleton pieces are looking at each other
            //if (i > pieces - 1)
            //{
            //    skeletonPiecesArray[i].LookAt(skeletonPiecesArray[i + 1].position);
            //}
            //else
            //{
            //    //Look straight up
            //    skeletonPiecesArray[i].LookAt(skeletonPiecesArray[i].position + Vector3.up);
            //}

            if (i > 0)
            {
                skeletonPiecesArray[i].LookAt(skeletonPiecesArray[i - 1].position);
            }
            else
            {
                //Look straight up
                skeletonPiecesArray[i].LookAt(skeletonPiecesArray[i].position + -Vector3.up);
            }
        }
    }



    void MoveTornado()
    {
        //The tornado skeleton is chasing an object which is moving against the real target with the same speed as the tornado
        //chaseObj.transform.LookAt(Camera.main.transform);

        //Rotate slowly towards the target
        Vector3 targetDir = targetObj.transform.position - chaseObj.transform.position;

        Vector3 newDir = Vector3.RotateTowards(chaseObj.transform.forward, targetDir, tornadoRotSpeed * Time.deltaTime, 0.0F);

        chaseObj.transform.rotation = Quaternion.LookRotation(newDir);


        //Move towards the target
        chaseObj.transform.Translate(Vector3.forward * tornadoSpeed * Time.deltaTime);



        //The obj that's rotating
        float rotationSpeed = 40f;
        float radius = 300f;

        Vector3 chasePos = chaseObj.transform.position;
        Vector3 rotatePos = chaseRotateObj.transform.position;

        chaseRotateObj.transform.position += (chasePos - oldChasePos);

        float currentRadius = (rotatePos - chasePos).magnitude;

        Vector3 dir = (rotatePos - chasePos).normalized;

        chaseRotateObj.transform.position += (radius - currentRadius) * dir;

        chaseRotateObj.transform.RotateAround(chasePos, transform.up, rotationSpeed * Time.deltaTime);

        oldChasePos = chasePos;
    }
}
