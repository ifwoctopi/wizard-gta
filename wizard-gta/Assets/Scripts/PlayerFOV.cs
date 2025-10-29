using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFOV : MonoBehaviour
{

    private Mesh mesh;
    private Material visionMaterial;
    private Vector3 origin;
    private float startingAngle;

    [Header("Vision Settings")] public float fov = 90f;
    public float viewDistance = 10f;
    public int rayCount = 50;
    public LayerMask obstacleMask;
    public Transform enemy;

    [Header("Visualization")] public bool showCone = true; // toggle this in the inspector

    [Header("Detection")] public float partialDetectionTime = 2f;
    private float detectionTimer = 0f;
    private bool isAlert = false;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Set up material
        visionMaterial = GetComponent<MeshRenderer>().material;
        visionMaterial.color = Color.black; // default color
    }

    void LateUpdate()
    {
        DrawVisionCone();
        CheckPlayerDetection();
    }

    private void DrawVisionCone()
    {

        if (!showCone)
        {
            mesh.Clear(); // hide the mesh
            return;
        }

        origin = Vector3.zero; // local space
        float angle = startingAngle; // start at half FOV to the right
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;
        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 dir = GetVectorFromAngle(angle);
            Vector3 vertex = origin + dir * viewDistance;

            RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, dir, viewDistance, obstacleMask);
            if (hit.collider != null)
            {
                vertex = transform.InverseTransformPoint(hit.point); // local space
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void CheckPlayerDetection()
    {
        if (enemy == null) return;

        // Direction and distance to player
        Vector3 directionToPlayer = enemy.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Skip if out of range
        if (distanceToPlayer > viewDistance)
        {
            detectionTimer = 0f;
            visionMaterial.color = Color.black;
            return;
        }

        // Get the cone's forward in world space
        Vector3 coneForward = transform.right; // adjust to transform.right if your mesh points along X

        // Angle between forward and player direction
        float angleToPlayer = Vector2.Angle(new Vector2(coneForward.x, coneForward.y),
            new Vector2(directionToPlayer.x, directionToPlayer.y));

        // Check if player is inside the cone
        if (angleToPlayer <= fov / 2f)
        {
            // Check line-of-sight with obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer,
                obstacleMask);
            if (hit.collider == null || hit.collider.transform == enemy) // allow detection if player is hit
            {
                detectionTimer += Time.deltaTime;

                if (detectionTimer >= partialDetectionTime)
                {
                    visionMaterial.color = Color.black;
                    if (showCone)
                        Debug.Log("Chasing");
                    isAlert = true;
                }
                else
                {
                    visionMaterial.color = Color.black;
                    if (showCone)
                        Debug.Log("Confused");
                    isAlert = false;
                }

                return; // player detected
            }
        }

        // Player blocked or outside cone
        detectionTimer = 0f;
        visionMaterial.color = Color.black;
        if (showCone)
            Debug.Log("Not detected");
        isAlert = false;
    }


   

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = GetAngleFromVectorFloat(aimDirection) - fov / 2f;
        
    }
    
    public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n =  Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }

}


