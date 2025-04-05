namespace CSPLib.Interfaces
{
    public interface IDraftHelper : IDrawable
    {

        Draft DraftParent { get; }
        bool Enabled { get; set; }

        void Draw(IDrawingContext ctx);

    }
}
