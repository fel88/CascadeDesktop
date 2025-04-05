using System.Reflection;

namespace CSPLib
{
    public class IntFieldEditor : IName
    {
        public IntFieldEditor(PropertyInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public PropertyInfo Field;
        public int Value
        {
            get
            {
                return ((int)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
}
