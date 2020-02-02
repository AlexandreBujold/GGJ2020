using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockShot : Weapon
{
    public float range = 1f;

    [Range(10f, 180f)]
    public float spreadAngle = 120f;
    public float numberOfShotsPerAmmo = 3;

    public float maxAmmo;
    public float ammo = 5;
    [Space]
    public ParticleSystem effect;
    public GameObject hitEffect;

    public List<Ray> rays;
    [Space]
    public LayerMask hitMask;
    public bool drawGizmos = false;
    // Start is called before the first frame update
    void Start()
    {
        ammo = 25f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Activate()
    {
        if (ammo > 0)
        {
            if (StartCooldown()) //Can go on cooldown, which means it can be used
            {
                ammo--;
                //Activate
                Debug.Log("Shock Shot has been fired!!");

                List<Ray> rayShots = new List<Ray>();

                //Get left most spread that the shot will follow, and rotate incrementally by angleBetweenShots
                Vector3 leftDirection = Quaternion.Euler(0, -(spreadAngle / 2), 0) * transform.forward;
                //Iterate through and raycast each shot
                for (int i = 0; i < numberOfShotsPerAmmo; i++)
                {
                    float angleBetweenShots = numberOfShotsPerAmmo == 1 ? spreadAngle / 2 : (spreadAngle / (numberOfShotsPerAmmo - 1)) * i;
                    Vector3 newShotDirection = Quaternion.Euler(0, angleBetweenShots, 0) * leftDirection;

                    RaycastHit hit;
                    Physics.Raycast(transform.position, newShotDirection, out hit, range, hitMask);

                    rayShots.Add(new Ray(transform.position, newShotDirection));


                    if (hit.collider != null)
                    {
                        Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
                        if (enemy != null)
                        {
                            Debug.Log(enemy.gameObject.name + " KILLED!", enemy.gameObject);
                            if (hitEffect != null)
                            {
                                Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
                            }
                            enemy.Kill();
                        }
                    }
                }
                rays = rayShots;
                if (effect != null)
                {
                    effect.Play();
                }
            }
        }

        
    }

    public override void Deactivate()
    {
        if (effect != null)
        {
            effect.Stop();
        }
    }

    public void UpdateAmmo()
    {
        ammo = maxAmmo += 5f;
        maxAmmo = ammo;
    }

    private void OnDrawGizmos() {
        if (drawGizmos)
        {
            if (rays != null)
            {
                foreach (Ray ray in rays)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(ray.GetPoint(range), 0.5f);
                }
            }
            
            Gizmos.color = Color.yellow;
            float theta1 = - (spreadAngle / 2);
            float theta2 =  (spreadAngle / 2);

            Ray leftRay = new Ray(transform.position, transform.forward);
            Ray rightRay = new Ray(transform.position, transform.forward);

        
            leftRay.direction = Quaternion.Euler(0, theta1, 0) * leftRay.direction;
            rightRay.direction = Quaternion.Euler(0, theta2, 0) * rightRay.direction;

            Gizmos.DrawRay(transform.position, leftRay.direction * 10f);
            Gizmos.DrawRay(transform.position, rightRay.direction * 10f);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * 10f);
        }
    }
}
