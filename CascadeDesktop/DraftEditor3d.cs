using Cascade.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public partial class DraftEditor3d : Form
    {
        public DraftEditor3d()
        {
            InitializeComponent();
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));

            if (glControl.Context.GraphicsMode.Samples == 0)
            {
                glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            }
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);

            Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;


            glControl.MouseDoubleClick += GlControl_MouseDoubleClick;
            glControl.MouseUp += GlControl_MouseUp;

        }
        private void GlControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectedTriangle = pickedTriangle;
            }
        }

        private void GlControl_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (pickedPoint != null)
            {
                camera1.CamTo = pickedPoint.Value;
            }
        }
        public CameraViewManager ViewManager;
        Camera camera1 = new Camera() { IsOrtho = true };
        private EventWrapperGlControl evwrapper;
        GLControl glControl;
        bool drawAxis = true;
        bool pickEnabled = true;
        public List<IHelperItem> Helpers = new List<IHelperItem>();
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }

            Redraw();
        }
        void Redraw()
        {
            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);



            GL.Enable(EnableCap.DepthTest);

            float zz = -500;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();
            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4d.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();
            camera1.Setup(glControl);

            if (drawAxis)
            {
                GL.LineWidth(2);
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(100, 0, 0);
                GL.End();

                GL.Color3(Color.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 100, 0);
                GL.End();

                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 100);
                GL.End();
            }

            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);
            foreach (var item in Helpers)
            {
                item.Draw(null);
            }

            if (pickEnabled)
                PickUpdate();

            glControl.SwapBuffers();
        }
        private double dist(Vector3d pitem, Vector3d start, Vector3d end)
        {
            return (GeometryUtils.point_on_line(start, end, pitem) - pitem).Length;
        }

        TriangleInfo pickedTriangle = null;
        TriangleInfo selectedTriangle = null;
        IHelperItem pickedHelper = null;
        Vector3d? pickedPoint = null;

        void PickUpdate()
        {
            var pos = glControl.PointToClient(System.Windows.Forms.Cursor.Position);
            camera1.UpdateMatricies(glControl);
            MouseRay.UpdateMatrices();

            var ray = new MouseRay(pos.X, pos.Y);
            double? minDist = null;
            double? minDist2 = null;
            Vector3d? p = null;
            pickedTriangle = null;
            foreach (var item in Helpers.OfType<IPointsProvider>())
            {
                if (item is IHelperItem hi && !hi.PickEnabled)
                    continue;

                if (!item.Visible)
                    continue;

                foreach (var pitem in item.GetPoints())
                {
                    var d = dist(pitem, ray.Start, ray.End);
                    if (d > 20)
                        continue;

                    if (minDist == null || d < minDist.Value)
                    {
                        minDist = d;
                        p = pitem;
                    }
                }
            }

            foreach (var item in Helpers.OfType<ITrianglesProvider>())
            {
                if (item is IHelperItem hi && !hi.PickEnabled)
                    continue;

                if (!item.Visible)
                    continue;

                foreach (var pitem in item.GetTriangles())
                {
                    var inter = Intersection.CheckIntersect(ray, new TriangleInfo[] { pitem });

                    if (inter == null)
                        continue;

                    var d = inter.Distance;

                    if (minDist2 == null || d < minDist2.Value)
                    {
                        minDist2 = d;
                        //p = inter.Point;
                        pickedTriangle = inter.Target;
                        pickedHelper = item as IHelperItem;
                    }
                }
            }
            pickedPoint = p;
            GL.Disable(EnableCap.DepthTest);

            if (selectedTriangle != null)
            {
                GL.Color3(Color.LightBlue);
                GL.Begin(PrimitiveType.Triangles);
                foreach (var item in selectedTriangle.Vertices)
                {
                    GL.Vertex3(item.Position);
                }
                GL.End();
            }
            if (minDist != null)
            {
                toolStripStatusLabel3.Text = $"picked point: {p.Value.X} {p.Value.Y} {p.Value.Z}";

                if (pickedTriangle != null)
                {
                    GL.Color3(Color.Red);
                    GL.Begin(PrimitiveType.Triangles);
                    foreach (var item in pickedTriangle.Vertices)
                    {
                        GL.Vertex3(item.Position);
                    }
                    GL.End();
                }
                GL.Color3(Color.Blue);
                GL.PointSize(10);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(p.Value);
                GL.End();
            }
            GL.Enable(EnableCap.DepthTest);
        }

        public Blueprint3d Blueprint = new Blueprint3d();

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            glControl.Invalidate();

        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("x1", "X1");
            d.AddNumericField("y1", "Y1");
            d.AddNumericField("z1", "Z1");
            d.AddNumericField("x2", "X2");
            d.AddNumericField("y2", "Y2");
            d.AddNumericField("z2", "Z2");

            if (!d.ShowDialog())
                return;

            var x1 = d.GetNumericField("x1");
            var y1 = d.GetNumericField("y1");
            var z1 = d.GetNumericField("z1");
            var x2 = d.GetNumericField("x2");
            var y2 = d.GetNumericField("y2");
            var z2 = d.GetNumericField("z2");

            Helpers.Add(new LineHelper()
            {
                Start = new Vector3d(x1, y1, z1),
                End = new Vector3d(x2, y2, z2)
            });
            Blueprint.Items.Add(new Line3D()
            {
                Start = new Vertex(x1, y1,z1),
                End = new Vertex(x2, y2, z2)
            });

        }
        BlueprintContour3d[] ConnectContour(BlueprintItem3d[] items)
        {
            List<BlueprintContour3d> rets = new List<BlueprintContour3d>();
            List<BlueprintItem3d> remains = new List<BlueprintItem3d>();

            BlueprintContour3d ret = new BlueprintContour3d();
            rets.Add(ret);
            ret.Items.Add(items[0]);
            remains.AddRange(items.Skip(1));
            float eps = 1e-5f;
            while (remains.Any())
            {
                var p2 = ret.Items.Last().End;
                var p1 = ret.Items.First().Start;
                BlueprintItem3d todel = null;
                foreach (var item in remains)
                {

                    var dist1 = (item.Start.ToVector3d() - p2.ToVector3d()).Length;
                    if (dist1 < eps)
                    {
                        ret.Items.Add(item);
                        todel = item;
                        break;
                    }
                    var dist2 = (item.End.ToVector3d() - p2.ToVector3d()).Length;
                    if (dist2 < eps)
                    {
                        item.Reverse();
                        ret.Items.Add(item);

                        todel = item;
                        break;
                    }
                }

                if (todel == null)
                {
                    //new contour
                    ret = new BlueprintContour3d();
                    rets.Add(ret);
                    todel = remains[0];
                    ret.Items.Add(remains[0]);
                }

                remains.Remove(todel);
            }

            return rets.ToArray();
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            foreach (var item in Blueprint.Items.OfType<Arc2d>())
            {
                item.UpdateMiddle();
            }
            var contours = ConnectContour(Blueprint.Items.ToArray());

            Blueprint.Items.Clear();
            Blueprint.Contours.AddRange(contours);
        }
    }
}
