using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class GlobalShaderVariables : MonoBehaviour 
{
    public Transform tornadoTrans;
    
    Camera thisCamera;

    private void Awake()
    {
        //Shader.SetGlobalTexture("_NoiseOffsets", this.noiseOffsetTexture);

        thisCamera = GetComponent<Camera>();
    }



    private void OnPreRender()
    {
        //The camera positions we need to determines the global position of the shader
        Shader.SetGlobalVector("_CamPos",     this.transform.position);
        Shader.SetGlobalVector("_CamRight",   this.transform.right);
        Shader.SetGlobalVector("_CamUp",      this.transform.up);
        Shader.SetGlobalVector("_CamForward", this.transform.forward);

        Shader.SetGlobalFloat("_AspectRatio", (float)Screen.width / (float)Screen.height);
        Shader.SetGlobalFloat("_FieldOfView", Mathf.Tan(thisCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 2f);

        //The position of the death star - we have an empty game object flying around the screen to easier set rotation
        //The the shader is using this position to paint the death star where its supposed to be
        //Shader.SetGlobalVector("_StarPos", deathStar.transform.position);

        //The position of the tornado
        Shader.SetGlobalVector("_TornadoPos", tornadoTrans.position);
    }


    //void Update()
    //{
    //    //The position of the tornado
    //    Shader.SetGlobalVector("_TornadoPos", tornadoTrans.position);
    //}
}
