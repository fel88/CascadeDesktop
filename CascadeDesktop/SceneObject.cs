namespace CascadeDesktop
{
    public class SceneObject
    {
        public static int NewId;
        public SceneObject()
        {
            Name = $"object_{NewId}";
            NewId++;
        }
        public string Name { get; set; }
        public virtual bool Visible { get; protected set; } = true;
    }
}
