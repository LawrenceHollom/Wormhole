using Godot;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

public class Screen : TextureRect
{
    private Vector4 position;
    private Vector4 up, left, back;
    private float theta, phi;

    private float curvatureRadius = 0.5f;
    private float throatLength = 0.5f;

    private ShaderMaterial shader;

    private bool isRunning;
    private bool clickToLook = false;
    private bool isFirstMouseMove;

    private float timeSinceStart;

    private readonly float speed = 0.75f;
    private readonly float angularSpeed = 0.005f;

    public override void _Ready()
    {
        up = Vector4.Up;
        left = Vector4.Left;
        back = Vector4.Back;
        position = Vector4.Back;
        isFirstMouseMove = true;

        timeSinceStart = 0;
                
        shader = Material as ShaderMaterial;

        phi = Mathf.Pi;
        theta = 0;
        shader.SetShaderParam("phi", phi);
        shader.SetShaderParam("theta", theta);
    }

    public void SetIsRunning(bool isRunning)
    {
        this.isRunning = isRunning;
    }

    public void SetTexturePair(TexturePair textures)
    {
        shader.SetShaderParam("home_texture", textures.HomeTexture);
        shader.SetShaderParam("away_texture", textures.AwayTexture);
    }

    public void SetThroatLength(float length)
    {
        float delta = length - throatLength;
        float propn = (position.w - curvatureRadius) / throatLength;
        if (propn >= 1)
            position.w += delta;
        else if (propn > 0)
            position.w += delta * propn;
        throatLength = length;
    }

    public void OnSetClickToLookStatus(bool clickToLook)
    {
        this.clickToLook = clickToLook;
    }

    public void SetCurvatureRadius(float radius)
    {
        float length3 = new Vector3(position.x, position.y, position.z).Length();
        float delta = radius - curvatureRadius;
        float externalRadiusScaler = radius / curvatureRadius;
        float internalRadiusScaler = (1f - radius) / (1f - curvatureRadius);

        if (length3 < 1f && position.w < radius)
        { // In the bottom curve
            Vector4 oldFocus = new Vector4(position.x / length3, position.y / length3, position.z / length3, curvatureRadius);
            Vector4 offset = position - oldFocus;
            position = oldFocus + delta * Vector4.Kata + externalRadiusScaler * offset;
        }
        else if (position.w <= throatLength + radius && position.w >= radius)
        { // In the throat
            position.x *= internalRadiusScaler;
            position.y *= internalRadiusScaler;
            position.z *= internalRadiusScaler;
            position.w += delta;
        }
        else if (position.w > throatLength + radius && length3 < 1f)
        { // In the top curve
            Vector4 oldFocus = new Vector4(position.x / length3, position.y / length3, position.z / length3, throatLength + curvatureRadius);
            Vector4 offset = position - oldFocus;
            position = oldFocus + delta * Vector4.Kata + externalRadiusScaler * offset;
        }
        else
        { // On the top plane
            position.w += 2f * delta;
        }

        curvatureRadius = radius;
    }

    private float Dist(Vector4 pos)
    {
        float length3 = new Vector3(pos.x, pos.y, pos.z).Length();
        float coeff = 1f - 1f / length3;

        if (length3 < 1.0) // In wormhole
        {
            if (pos.w < curvatureRadius)
            {
                float dw = pos.w - curvatureRadius;
                return Mathf.Sqrt(coeff * coeff * length3 * length3 + dw * dw) - curvatureRadius;
            }
            else if (pos.w > curvatureRadius + throatLength)
            {
                float dw = pos.w - throatLength - curvatureRadius;
                return Mathf.Sqrt(coeff * coeff * length3 * length3 + dw * dw) - curvatureRadius;
            }
            else
                return length3 - (1f - curvatureRadius);
        }
        else if (pos.w < curvatureRadius + throatLength / 2f)
            return pos.w;
        else
            return pos.w - (2f * curvatureRadius) - throatLength;
    }

    private Vector4 Normal(Vector4 pos)
    {
        float length3 = new Vector3(pos.x, pos.y, pos.z).Length();
        float coeff = 1f - 1f / length3;

        if (length3 < 0.00001) // Should never happen?
            return Vector4.Right;
        else if (length3 < 1.0) // In wormhole
        {
            if (pos.w < curvatureRadius)
                return new Vector4(coeff * pos.x, coeff * pos.y, coeff * pos.z, pos.w - curvatureRadius);
            else if (pos.w > curvatureRadius + throatLength)
                return new Vector4(coeff * pos.x, coeff * pos.y, coeff * pos.z, pos.w - throatLength - curvatureRadius);
            else
                return new Vector4(pos.x, pos.y, pos.z, 0);
        }
        else
            return Vector4.Kata;
    }

    private void PassDirsToShader(Vector4 normal)
    {
        shader.SetShaderParam("up", up.ToPlane());
        shader.SetShaderParam("left", left.ToPlane());
        shader.SetShaderParam("forward", back.Negate().ToPlane());
        shader.SetShaderParam("position", position.ToPlane());
        shader.SetShaderParam("normal", normal.ToPlane());

        shader.SetShaderParam("throat_length", throatLength);
        shader.SetShaderParam("radius", curvatureRadius);
    }

    private void PassMatrixToShader(Matrix4x4 Aet)
    {
        // columns of the matrix
        shader.SetShaderParam("aet0", new Vector4(Aet.M11, Aet.M21, Aet.M31, Aet.M41).ToPlane());
        shader.SetShaderParam("aet1", new Vector4(Aet.M12, Aet.M22, Aet.M32, Aet.M42).ToPlane());
        shader.SetShaderParam("aet2", new Vector4(Aet.M13, Aet.M23, Aet.M33, Aet.M43).ToPlane());
        shader.SetShaderParam("aet3", new Vector4(Aet.M14, Aet.M24, Aet.M34, Aet.M44).ToPlane());
    }

    private Matrix4x4 MakeRotationMatrix()
    {
        return Matrix4x4.Transpose(new Matrix4x4(
            Mathf.Cos(phi), 0f, -Mathf.Sin(phi), 0f, 
            Mathf.Sin(phi) * Mathf.Sin(theta), Mathf.Cos(theta), Mathf.Cos(phi) * Mathf.Sin(theta), 0f, 
            Mathf.Sin(phi) * Mathf.Cos(theta), -Mathf.Sin(theta), Mathf.Cos(phi) * Mathf.Cos(theta), 0f, 
            0f, 0f, 0f, 1f));
    }

    private void ProcessInput(float delta, Matrix4x4 Tea, Matrix4x4 Aet)
    {
        Matrix4x4 Conv = Tea * MakeRotationMatrix() * Aet;
        if (Input.IsActionPressed("left"))
            position += delta * speed * (Conv * left);
        if (Input.IsActionPressed("right"))
            position -= delta * speed * (Conv * left);
        if (Input.IsActionPressed("up"))
            position += delta * speed * (Conv * up);
        if (Input.IsActionPressed("down"))
            position -= delta * speed * (Conv * up);
        if (Input.IsActionPressed("forward"))
            position += delta * speed * (Conv * back);
        if (Input.IsActionPressed("back"))
            position -= delta * speed * (Conv * back);
    }

    private void PrintDebugInfo(Matrix4x4 Tea, Matrix4x4 Aet)
    {
        Matrix4x4 Conv = Tea * MakeRotationMatrix() * Aet;
        GD.Print(" ~~~~  DEBUG  ~~~~ ");
        GD.Print("LEFT: " + left);
        GD.Print("  UP: " + up);
        GD.Print("BACK: " + back);
        GD.Print("THETA: " + theta);
        GD.Print("  PHI: " + phi);
        GD.Print("CONVERTED LEFT: " + (Conv * left));
        GD.Print("CONVERTED BACK: " + (Conv * back));
    }

    private void HackGif(float delta)
    {
        isRunning = false;
        timeSinceStart += delta;
        phi = timeSinceStart;
        theta = 0f;
        shader.SetShaderParam("phi", phi);
        shader.SetShaderParam("theta", theta);
        position = new Vector4(-1.5f * Mathf.Sin(phi), 0, -1.5f * Mathf.Cos(phi), 0f);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Vector4 normal = Normal(position);
        //position = position.RemoveComponent(normal);
        position = position - Dist(position) * normal;
        up = up.RemoveComponent(normal).Normalize();
        left = left.RemoveComponent(normal).Normalize();
        back = back.RemoveComponent(normal).Normalize();

        PassDirsToShader(normal);

        Matrix4x4 Tea = new Matrix4x4(left.x, up.x, -back.x, normal.x, left.y, up.y, -back.y, normal.y,
            left.z, up.z, -back.z, normal.z, left.w, up.w, -back.w, normal.w);
        Matrix4x4 Aet;

        if (Matrix4x4.Invert(Tea, out Aet))
            PassMatrixToShader(Aet);

        if (isRunning)
        {
            ProcessInput(delta, Tea, Aet);

            if (Input.IsActionJustPressed("debug"))
                PrintDebugInfo(Tea, Aet);
        }
        //HackGif(delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (isRunning && (!clickToLook || Input.IsActionPressed("click")) && @event is InputEventMouseMotion)
        {
            if (isFirstMouseMove)
            {
                isFirstMouseMove = false;
            }
            else
            {
                Vector2 delta = (@event as InputEventMouseMotion).Relative;
                phi += delta.x * angularSpeed;
                theta = Mathf.Clamp(theta - delta.y * angularSpeed, -Mathf.Pi / 2f, Mathf.Pi / 2f);
                shader.SetShaderParam("phi", phi);
                shader.SetShaderParam("theta", theta);
            }
        }
    }
}
