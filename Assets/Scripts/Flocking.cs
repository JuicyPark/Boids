using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Flocking : MonoBehaviour
{
    [SerializeField]
    Vector3 _baseRotation;

    [Range(0, 10), SerializeField]
    float _maxSpeed = 1f;

    [Range(.1f, .5f), SerializeField]
    float _maxForce = 0.03f;

    [Range(1, 10), SerializeField]
    float _neighborhoodRadius = 3f;

    [Range(0, 3), SerializeField]
    float _separateWeight = 1f;

    [Range(0, 3), SerializeField]
    float _cohesionWeight = 1f;

    [Range(0, 3), SerializeField]
    float _alignmentWeight = 1f;

    [SerializeField]
    Vector2 _acceleration;
    [SerializeField]
    Vector2 _velocity;

    Vector2 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    void Start()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + _baseRotation);
        _velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void Update()
    {
        var boidColliders = Physics2D.OverlapCircleAll(Position, _neighborhoodRadius);
        var boids = boidColliders.Select(o => o.GetComponent<Flocking>()).ToList();
        boids.Remove(this);

        Flock(boids);
        UpdateVelocity();
        UpdatePosition();
        UpdateRotation();
        Warp();
    }

    void Flock(IEnumerable<Flocking> boids)
    {
        var alignment = Alignment(boids);
        var separation = Separation(boids);
        var cohesion = Cohesion(boids);

        _acceleration = _alignmentWeight * alignment + _cohesionWeight * cohesion + _separateWeight * separation;
    }

    void UpdateVelocity()
    {
        _velocity += _acceleration;
        _velocity = LimitMagnitude(_velocity, _maxSpeed);
    }

    void UpdatePosition()
    {
        Position += _velocity * Time.deltaTime;
    }

    void UpdateRotation()
    {
        var angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + _baseRotation);
    }

    Vector2 Alignment(IEnumerable<Flocking> boids)
    {
        var velocity = Vector2.zero;
        if (!boids.Any())
            return velocity;

        foreach (var boid in boids)
        {
            velocity += boid._velocity;
        }
        velocity /= boids.Count();

        var steer = Steer(velocity.normalized * _maxSpeed);
        return steer;
    }

    Vector2 Cohesion(IEnumerable<Flocking> boids)
    {
        if (!boids.Any())
            return Vector2.zero;

        var sumPositions = Vector2.zero;
        foreach (var boid in boids)
        {
            sumPositions += boid.Position;
        }
        var average = sumPositions / boids.Count();
        var direction = average - Position;

        var steer = Steer(direction.normalized * _maxSpeed);
        return steer;
    }

    Vector2 Separation(IEnumerable<Flocking> boids)
    {
        var direction = Vector2.zero;
        boids = boids.Where(o => DistanceTo(o) <= _neighborhoodRadius / 2);
        if (!boids.Any())
            return direction;

        foreach (var boid in boids)
        {
            var difference = Position - boid.Position;
            direction += difference.normalized / difference.magnitude;
        }
        direction /= boids.Count();

        var steer = Steer(direction.normalized * _maxSpeed);
        return steer;
    }

    Vector2 Steer(Vector2 desired)
    {
        var steer = desired - _velocity;
        steer = LimitMagnitude(steer, _maxForce);

        return steer;
    }

    float DistanceTo(Flocking boid)
    {
        return Vector3.Distance(boid.transform.position, Position);
    }

    Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
    {
        if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            baseVector = baseVector.normalized * maxMagnitude;
        }
        return baseVector;
    }

    void Warp()
    {
        if (Position.x < -14)
            Position = new Vector2(14, Position.y);
        if (Position.y < -8)
            Position = new Vector2(Position.x, 8);
        if (Position.x > 14)
            Position = new Vector2(-14, Position.y);
        if (Position.y > 8)
            Position = new Vector2(Position.x, -8);
    }
}