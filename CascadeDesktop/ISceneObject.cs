namespace CascadeDesktop
{
    public interface ISceneObject
    {
        void Draw(GpuDrawingContext ctx);
        int Id { get; set; }

    }
}
