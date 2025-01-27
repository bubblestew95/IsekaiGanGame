using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TestDecal : MonoBehaviour
{
    public DecalProjector project;
    public float size;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(ScaleUp());
        }
    }

    private IEnumerator ScaleUp()
    {
        float elapseTime = 0f;

        while (elapseTime < 1f)
        {
            elapseTime += Time.deltaTime;
            project.size = new Vector3(size * elapseTime, size * elapseTime, 1f);

            yield return null;
        }

        yield return null;
    }
}
