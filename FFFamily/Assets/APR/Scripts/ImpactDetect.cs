using UnityEngine;

public class ImpactDetect : MonoBehaviour
{
    public APRController APR_Player;
    public float ImpactForce;
    public float KnockoutForce;

    public AudioClip[] Impacts;
    public AudioClip[] Hits;
    public AudioSource SoundSource;
    private GameObject hitFX;
    private void Start()
    {
        hitFX = Resources.Load<GameObject>("FX/HitEffect");
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "HitObj")
        {
            var weapon = col.gameObject.GetComponent<Weapon>();
            float p = weapon != null ? weapon.powerOffset : 1;
            ContactPoint point = col.contacts[0];
            if (col.transform.root != transform.root)
            {
                GetComponent<Rigidbody>().AddForceAtPosition(col.relativeVelocity * 0.8f * GetComponent<Rigidbody>().mass, point.point, ForceMode.Impulse);
                APR_Player.GetHurt(gameObject, col.relativeVelocity * 0.8f);
            }
            //击倒

            if (col.relativeVelocity.magnitude*p > KnockoutForce /Mathf.Sqrt(APR_Player.Power) && col.transform.root != transform.root)
            {
                col.transform.root.GetComponent<APRController>().Power += 0.1f;
                APR_Player.ActivateRagdoll();
                GetComponent<Rigidbody>().AddForceAtPosition(col.relativeVelocity * 0.4f * GetComponent<Rigidbody>().mass, point.point, ForceMode.Impulse);
                APR_Player.GetHurt(gameObject, col.relativeVelocity * 0.4f);
                GetComponent<Rigidbody>().AddForce(Vector3.up * col.relativeVelocity.magnitude * 2.5f * GetComponent<Rigidbody>().mass, ForceMode.Impulse);
                APR_Player.GetHurt(gameObject, Vector3.up * col.relativeVelocity.magnitude * 2.5f);
                if(weapon!=null&& !SoundSource.isPlaying)
                {
                    SoundSource.clip = col.gameObject.GetComponent<Weapon>().knockOut;
                    SoundSource.Play();
                }
                else if (!SoundSource.isPlaying)
                {
                    int i = Random.Range(0, Hits.Length);
                    SoundSource.clip = Hits[i];
                    SoundSource.Play();
                }
            }

            //有效打击
            if (col.relativeVelocity.magnitude*p > ImpactForce && col.transform.root != transform.root)
            {
                Instantiate(hitFX, point.point, Quaternion.identity);
                col.transform.root.GetComponent<APRController>().Power += 0.1f;
                if (weapon != null && !SoundSource.isPlaying)
                {
                    SoundSource.clip = col.gameObject.GetComponent<Weapon>().impact;
                    SoundSource.Play();
                }
                else if (!SoundSource.isPlaying)
                {
                    int i = Random.Range(0, Impacts.Length);
                    SoundSource.clip = Impacts[i];
                    SoundSource.Play();
                }
            }
        }
    }
}
