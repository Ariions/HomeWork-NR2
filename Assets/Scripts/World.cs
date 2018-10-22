using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;


// Entry point to the simulations. Technically there could be multiple worlds
// that are be completely isolated from each other
public class World : MonoBehaviour 
{	
	public GameObject templateObject;
	public int entityCount = 10;
	public Rect worldBounds = new Rect(-10f, -5f, 20f, 10f);
	public Vector2 gravity = Vector2.down * 9.81f;
	public Vector2 wind = new Vector2( 0.0f, 0.0f);
    public Vector2 mouseClickDirection;
    public float force = 0;

    static public Vector2 spawnLocation = new Vector2(-9.0f , 0.0f);

    public LineRenderer lineRenderer;

	[NonSerialized]
	public Entities entities;

	protected List<ISystemInterface> systems;
	
	// Use this for initialization
	void Start () 
	{
        lineRenderer.SetWidth(0.2f, 0);

		Profiler.BeginSample("World.Start");
		systems = new List<ISystemInterface>();
		entities = new Entities();
		
		entities.Init(entityCount);

		// System addition order matters, they will run in the same order
		systems.Add(new GravitySystem());
		systems.Add(new ForceSystem());
		systems.Add(new MoveSystem());	
		systems.Add(new CollisionSystem());
		systems.Add(new WorldBoundsSystem());
		systems.Add(new RenderingSystem());
		systems.Add(new WindSystem());

		foreach (var system in systems)
		{
			Profiler.BeginSample(system.GetType().Name);
			system.Start(this);
			Profiler.EndSample();
		}
		
		Profiler.EndSample();
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (Input.GetButton("Fire1"))
        {
            if (force <= 19)
                force += 0.3f;
            mouseClickDirection = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - spawnLocation).normalized;

            
            lineRenderer.SetPosition(1, new Vector3(mouseClickDirection.x * (force * 0.2f) -9, mouseClickDirection.y * (force * 0.2f) , 0.0f));
            


        }

        if (Input.GetMouseButtonUp(0)) {
            
            ShootEntity();
            force = 0;
            Invoke("ChangeWind", 2);
            lineRenderer.SetPosition(1, new Vector3(-9.0f, 0.0f, 0.0f));
        }

        Profiler.BeginSample("World.Update");
		foreach (var system in systems)
		{
			Profiler.BeginSample(system.GetType().Name);
			system.Update(this, Time.timeSinceLevelLoad, Time.deltaTime);
			Profiler.EndSample();
		}
		Profiler.EndSample();
	}

    void ShootEntity()
    {
        int id = entities.Shoot();

        if (entities.flags[id].HasFlag(EntityFlags.kFlagPosition))
        {
            entities.AddComponent(new Entity(id), EntityFlags.kFlagGravity);
            entities.AddComponent(new Entity(id), EntityFlags.kFlagForce);
            entities.AddComponent(new Entity(id), EntityFlags.kFlagMove);
            entities.AddComponent(new Entity(id), EntityFlags.kFlagCollision);
            entities.AddComponent(new Entity(id), EntityFlags.kFlagWorldBounds);

            //gravity - empty

            //force
            var forceComponent = new ForceComponent() { massInverse = Random.Range(1f, 5f), force = Vector2.zero };
            entities.forceComponents[id] = forceComponent;

            //movement
            var moveComponent = entities.moveComponents[id];
            moveComponent.velocity = new Vector2 (mouseClickDirection.x, mouseClickDirection.y) * force;
            
            //moveComponent.velocity = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
            entities.moveComponents[id] = moveComponent;

            //collition
            var collisionComponent = new CollisionComponent();
            if (entities.forceComponents[id].massInverse > 1e-6f)
                collisionComponent.radius = 1.0f / entities.forceComponents[id].massInverse;

            collisionComponent.coeffOfRestitution = Random.Range(0.1f, 0.9f);

            entities.collisionComponents[id] = collisionComponent;

            //worldboudns - empty


            // add force in selected direction
            
        }     
    }

    void ChangeWind()
    {
        wind = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
    }
}
