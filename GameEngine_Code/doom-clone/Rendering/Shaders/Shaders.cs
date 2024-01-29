
namespace doom_clone.Rendering.Shaders
{
    public class Shaders
    {
        public static readonly Shader DEFAULT_SHADER;
        public static readonly Shader UI_SHADER;
        static Shaders()
        {
            DEFAULT_SHADER = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            UI_SHADER = new Shader("Shaders/shader.vert", "Shaders/shader_ui.frag");
        }
    }
}
