using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public void SetPlayerColor(Material color)
    {
        meshRenderer.material = color;
    }
}
