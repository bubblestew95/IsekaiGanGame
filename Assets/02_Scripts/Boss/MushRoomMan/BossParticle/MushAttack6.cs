using System.Collections;
using UnityEngine;

public class MushAttack6 : MonoBehaviour
{
    private GameObject boom;
    private GameObject poisionFloor;
    private GameObject poisionSun;

    private ParticleSystem poisionSunParticle;

    private void Awake()
    {
        boom = transform.GetChild(0).gameObject;
        poisionFloor = transform.GetChild(1).gameObject;
        poisionSun = transform.GetChild (2).gameObject;
        poisionSunParticle = poisionSun.GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        poisionSun.SetActive(true);

        StartCoroutine(ChangePoisionSunColor());
    }

    private IEnumerator ChangePoisionSunColor()
    {
        float elapseTime = 0f;
        Color startColor = Color.green;
        Color endColor = Color.black;

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / 10f);

            if (poisionSunParticle != null)
            {
                Color lerpedColor = Color.Lerp(startColor, endColor, t);
                var main = poisionSunParticle.main;
                main.startColor = lerpedColor;
            }

            if (elapseTime >= 10f)
            {
                break;
            }

            yield return null;
        }


        transform.SetParent(null);
        poisionFloor.SetActive(true);
        boom.SetActive(true);
    }
}
