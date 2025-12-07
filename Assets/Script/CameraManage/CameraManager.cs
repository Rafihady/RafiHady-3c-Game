using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Action OnChangePerpective;
    [SerializeField] public CameraState _CameraState;
    [SerializeField] CinemachineVirtualCamera _fpsCamera;
    [SerializeField] CinemachineFreeLook _tpsCamera;
    [SerializeField] InputManager _inputPOV;


    private void Start()
    {
        _inputPOV.OnChangePOV += Camerachange;  
    }
    private void OnDestroy()
    {
        _inputPOV.OnChangePOV -= Camerachange;

    }

    public void SetTpsFOV (float fov)
    {
        _tpsCamera.m_Lens.FieldOfView = fov;
    }

    public void setFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        CinemachinePOV pov = _fpsCamera.GetCinemachineComponent<CinemachinePOV>();

        if (isClamped)
        {
            pov.m_HorizontalAxis.m_Wrap = false;
            pov.m_HorizontalAxis.m_MinValue = playerRotation.y - 45;
            pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + 45;
        }
        else
        {
            pov.m_HorizontalAxis.m_MinValue = -180;
            pov.m_HorizontalAxis.m_MaxValue = +180;
            pov.m_HorizontalAxis.m_Wrap = true;

        }
    }

    public void Camerachange ()
    {
        OnChangePerpective();
        if (_CameraState == CameraState.ThrirdPerson)
        {
            _CameraState = CameraState.FirstPerson;
            _tpsCamera.gameObject .SetActive(false);
            _fpsCamera.gameObject .SetActive(true);
        }
        else
        {
            _CameraState = CameraState.ThrirdPerson;
            _tpsCamera.gameObject .SetActive(true);
            _fpsCamera.gameObject .SetActive(false);
        }
    }
}
