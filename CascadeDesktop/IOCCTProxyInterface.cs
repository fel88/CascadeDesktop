using Cascade.Common;
using System;
using System.Collections.Generic;

namespace CascadeDesktop
{
    public interface IOCCTProxyInterface
    {
        void ResetSelectionMode();
        void SetSelectionMode(OCCTProxy.SelectionModeEnum f);
        void TopView();
        void FrontView();
        ManagedObjHandle GetSelectedObject();
        bool IsObjectSelected();
        void SetDisplayMode(int m);
        void InitOCCTProxy();
        void RedrawView();
        void ShowCube();
        void ZoomAllView();
        List<ManagedObjHandle> ImportStep(string path);
        List<ManagedObjHandle> ImportIges(string path);
        void LeftView();
        void ActivateGrid(bool enabled);
        void AxoView();
        ManagedObjHandle MakeFuse(ManagedObjHandle h1, ManagedObjHandle h2);
        ManagedObjHandle MakeDiff(ManagedObjHandle h1, ManagedObjHandle h2);
        ManagedObjHandle MakeCommon(ManagedObjHandle h1, ManagedObjHandle h2);
        void MoveObject(ManagedObjHandle h1, double x, double y, double z, bool rel);
        void RotateObject(ManagedObjHandle h1, double x, double y, double z, double rot, bool rel);
        void Erase(ManagedObjHandle h1);
        void UpdateView();
        void UpdateCurrentViewer();
        void SetTransparency(ManagedObjHandle managedObjHandle, double v);
        ManagedObjHandle MakePrismFromFace(ManagedObjHandle managedObjHandle, double height);
        List<SurfInfo> GetFacesInfo(ManagedObjHandle managedObjHandle);
        void SetDefaultGradient();
        void BackView();
        void BottomView();
        void RightView();
        ManagedObjHandle MakeCone(double r1, double r2, double h);
        ManagedObjHandle MakeChamfer(ManagedObjHandle so, double r);
        ManagedObjHandle MakePrism(ManagedObjHandle managedObjHandle, double h);
        ManagedObjHandle Clone(ManagedObjHandle so);
        ManagedObjHandle MakeFillet(ManagedObjHandle so, double r);
        void MirrorObject(ManagedObjHandle managedObjHandle, Vector3 dir, Vector3 pnt, bool axis2, bool rel);
        ManagedObjHandle MakeSphere(double r);
        ManagedObjHandle MakeCylinder(double r, double h);
        bool ExportStep(ManagedObjHandle managedObjHandle, string fileName);
        void MoveTo(int x, int y);
        void Pan(int x, int y);
        void Rotation(int x, int y);
        ManagedObjHandle MakeBox(double x, double y, double z, double w, double l, double h);
        void SetBackgroundColor(int r1, int g1, int b1, int r2, int g2, int b2);
        bool InitViewer(IntPtr handle);
        void SetMaterial(int v);
        void Zoom(int v1, int v2, int v3, int v4);
        EdgeInfo GetEdgeInfoPoition(ManagedObjHandle obj);
        SurfInfo GetFaceInfo(ManagedObjHandle obj);
        Vector3 GetVertexPoition(ManagedObjHandle obj);
        void StartRotation(int x, int y);
        void Select(bool v);
        List<Vector3> IteratePoly(ManagedObjHandle managedObjHandle);
        ManagedObjHandle AddWireDraft(double height);
        ManagedObjHandle ImportBlueprint(Blueprint blueprint);
    }
}
