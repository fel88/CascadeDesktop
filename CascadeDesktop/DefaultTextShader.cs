namespace CascadeDesktop
{
    public class DefaultTextShader : Shader
    {
        public DefaultTextShader()
        {
            InitFromResources("text.vs", "text.fs");
        }
    }
}
