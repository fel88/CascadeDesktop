// include required OCCT headers
#include <Standard_Version.hxx>
#include <Message_ProgressIndicator.hxx>
#include <Message_ProgressScope.hxx>
//for OCC graphic
#include <Aspect_DisplayConnection.hxx>
#include <WNT_Window.hxx>
#include <OpenGl_GraphicDriver.hxx>
//for object display
#include <V3d_Viewer.hxx>
#include <V3d_View.hxx>
#include <AIS_InteractiveContext.hxx>
#include <AIS_Shape.hxx>
// gprops
#include <GProp_GProps.hxx>
#include <BRepGProp.hxx>
//topology
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_TEdge.hxx>
#include <TopoDS_Compound.hxx>
#include <TopExp_Explorer.hxx>
//brep tools
#include <BRep_Builder.hxx>
#include <BRepTools.hxx>
// iges I/E
#include <IGESControl_Reader.hxx>
#include <IGESControl_Controller.hxx>
#include <IGESControl_Writer.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <Interface_Static.hxx>
//step I/E
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
//for stl export
#include <StlAPI_Writer.hxx>
//for vrml export
#include <VrmlAPI_Writer.hxx>
//wrapper of pure C++ classes to ref classes
#include <NCollection_Haft.h>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <GC_MakeSegment.hxx>
#include <GC_MakeCircle.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepBuilderAPI.hxx>
#include <BOPAlgo_Builder.hxx>
#include < BRepAlgoAPI_Fuse.hxx>
#include < BRepAlgoAPI_Common.hxx>
#include < BRepAlgoAPI_Cut.hxx>
#include <vcclr.h>
#include <BRepBuilderAPI_Transform.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <StdSelect_BRepOwner.hxx>

#include <AIS_ViewCube.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>

#include <Graphic3d_RenderingParams.hxx>
#include <TopExp_Explorer.hxx>
#include <Font_BRepFont.hxx>

#include <Geom_SphericalSurface.hxx>
#include <Geom_CylindricalSurface.hxx>
#include <Geom_SurfaceOfRevolution.hxx>
#include <Geom_ConicalSurface.hxx>
#include <Geom_ToroidalSurface.hxx>
#include <Geom_Line.hxx>
#include <Font_BRepTextBuilder.hxx>

#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <ChFi2d_AnaFilletAlgo.hxx>

// list of required OCCT libraries


using namespace Cascade::Common;

#pragma comment(lib, "TKernel.lib")
#pragma comment(lib, "TKMath.lib")
#pragma comment(lib, "TKBRep.lib")
#pragma comment(lib, "TKXSBase.lib")
#pragma comment(lib, "TKService.lib")
#pragma comment(lib, "TKV3d.lib")
#pragma comment(lib, "TKOpenGl.lib")
#pragma comment(lib, "TKIGES.lib")
#pragma comment(lib, "TKSTEP.lib")
#pragma comment(lib, "TKStl.lib")
#pragma comment(lib, "TKVrml.lib")
#pragma comment(lib, "TKLCAF.lib")
#pragma comment(lib, "TKPrim.lib")
#pragma comment(lib, "TKBin.lib")
#pragma comment(lib, "TKBool.lib")
#pragma comment(lib, "TKFeat.lib")
#pragma comment(lib, "TKOffset.lib")
#pragma comment(lib, "TKDraw.lib")
#pragma comment(lib, "TKGeomAlgo.lib")
#pragma comment(lib, "TKGeomBase.lib")
#pragma comment(lib, "TKG3d.lib")
#pragma comment(lib, "TKMesh.lib")
#pragma comment(lib, "TKService.lib")
#pragma comment(lib, "TKTopAlgo.lib")
#pragma comment(lib, "TKMath.lib")
#pragma comment(lib, "TKBO.lib")
#pragma comment(lib, "TKShHealing.lib")
#pragma comment(lib, "TKFillet.lib")

class membuf : public std::basic_streambuf<char> {
public:
	membuf(const uint8_t* p, size_t l) {
		setg((char*)p, (char*)p, (char*)p + l);
	}
};
class memstream : public std::istream {
public:
	memstream(const uint8_t* p, size_t l) :
		std::istream(&_buffer),
		_buffer(p, l) {
		rdbuf(&_buffer);
	}

private:
	membuf _buffer;
};
struct ObjHandle {
public:
	unsigned __int64 handle;
	unsigned __int64 handleT;
	unsigned __int64 handleF;
};

public ref struct Vector3 {
public:
	Vector3() {}
	Vector3(double x, double y, double z) { X = x;Y = y;Z = z; }
	double X;
	double Y;
	double Z;
};

public enum class CurveType
{
	Line,
	Circle,
	Ellipse,
	Hyperbola,
	Parabola,
	BezierCurve,
	BSplineCurve,
	OffsetCurve,
	OtherCurve
};

public ref class EdgeInfo {
public:
	CurveType CurveType;
	Vector3^ COM;//center of mass
	Vector3^ Start;
	Vector3^ End;
	double Length;
	unsigned __int64 Handle;
	unsigned __int64 THandle;
};

public ref class CircleEdgeInfo : EdgeInfo {
public:
	double Radius;
};

public ref class SurfInfo {
public:
	Vector3^ Position;
	Vector3^ COM;//center of mass
	unsigned __int64 Handle;
	unsigned __int64 THandle;
};

public ref class PlaneSurfInfo : SurfInfo {
public:
	Vector3^ Normal;
};

public ref class SurfOfRevolutionInfo : SurfInfo {
public:
};

public ref class TorusSurfInfo : SurfInfo {
public:
	double MajorRadius;
	double MinorRadius;
};

public ref class ConeSurfInfo : SurfInfo {
public:
	double Radius1;
	double Radius2;
	double SemiAngle;
};

public ref class CylinderSurfInfo :SurfInfo {
public:
	double Radius;
	Vector3^ Axis;
};

public ref class SphereSurfInfo : SurfInfo {
public:
	double Radius;
};

public ref class ManagedObjHandle {
public:
	UINT64 Handle;
	UINT64 HandleT;
	UINT64 HandleF;

	void FromObjHandle(ObjHandle h) {
		Handle = h.handle;
		HandleT = h.handleT;
		HandleF = h.handleF;
	}
	ObjHandle ToObjHandle() {
		ObjHandle h;
		h.handle = Handle;
		h.handleT = HandleT;
		h.handleF = HandleF;
		return h;
	}
};

//! Auxiliary tool for converting C# string into UTF-8 string.
static TCollection_AsciiString toAsciiString(String^ theString)
{
	if (theString == nullptr)
	{
		return TCollection_AsciiString();
	}

	pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
	const wchar_t* aWCharPtr = aPinChars;
	if (aWCharPtr == NULL
		|| *aWCharPtr == L'\0')
	{
		return TCollection_AsciiString();
	}

	return TCollection_AsciiString(aWCharPtr);
}
//! Auxiliary tool for converting C# string into UTF-8 string.
static NCollection_String toNString(String^ theString)
{
	if (theString == nullptr)
	{
		return NCollection_String();
	}

	pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
	const wchar_t* aWCharPtr = aPinChars;
	if (aWCharPtr == NULL
		|| *aWCharPtr == L'\0')
	{
		return NCollection_String();
	}

	return NCollection_String(aWCharPtr);
}
class OCCImpl {
public:
	ObjHandle getSelectedEdge(AIS_InteractiveContext* ctx) {
		auto objs = getSelectedObjectsList(ctx);
		for (auto item : objs) {
			TopoDS_TShape* ptshape = (TopoDS_TShape*)item.handleT;
			TopoDS_TEdge* edge = dynamic_cast<TopoDS_TEdge*>(ptshape);
			if (edge != nullptr) {
				return item;
			}
		}
		return ObjHandle();
	}
	void  GetSelectedEdges(AIS_InteractiveContext* ctx, std::vector<ObjHandle>& list) {
		auto objs = getSelectedObjectsList(ctx);
		for (auto item : objs) {
			TopoDS_TShape* ptshape = (TopoDS_TShape*)item.handleT;
			TopoDS_TEdge* edge = dynamic_cast<TopoDS_TEdge*>(ptshape);
			if (edge != nullptr) {
				list.push_back(item);
			}
		}
	}

	std::vector<double> IteratePoly(ObjHandle h) {
		auto obj = getObject(h);
		const auto& shape = Handle(AIS_Shape)::DownCast(obj)->Shape();

		//std::vector<QVector3D> vertices;
		std::vector<double> ret;
		//std::vector<QVector3D> normals;
		//std::vector<QVector2D> uvs2;
		std::vector<unsigned int> indices;
		unsigned int idxCounter = 0;
		//TopoDS_Shape shape = MakeBottle(100, 300, 20);
		Standard_Real aDeflection = 0.1;

		//BRepMesh_IncrementalMesh(*shape, 1);
		//bm.Perform();
		//auto shape2 = bm.Shape();

		Standard_Integer aIndex = 1, nbNodes = 0;

		//TColgp_SequenceOfPnt aPoints, aPoints1;

		for (TopExp_Explorer aExpFace(shape, TopAbs_FACE); aExpFace.More(); aExpFace.Next())

		{

			TopoDS_Face aFace = TopoDS::Face(aExpFace.Current());

			TopAbs_Orientation faceOrientation = aFace.Orientation();

			TopLoc_Location aLocation;

			Handle(Poly_Triangulation) aTr = BRep_Tool::Triangulation(aFace, aLocation);

			if (!aTr.IsNull())
			{
				//const TColgp_Array1OfPnt& aNodes = aTr->NbNodes();
				const Poly_Array1OfTriangle& triangles = aTr->Triangles();
				//const TColgp_Array1OfPnt2d& uvNodes = aTr->UVNodes();

				TColgp_Array1OfPnt aPoints(1, aTr->NbNodes());
				for (Standard_Integer i = 1; i < aTr->NbNodes() + 1; i++)
					aPoints(i) = aTr->Node(i).Transformed(aLocation);


				Standard_Integer nnn = aTr->NbTriangles();
				Standard_Integer nt, n1, n2, n3;

				for (nt = 1; nt < nnn + 1; nt++)
				{

					triangles(nt).Get(n1, n2, n3);
					gp_Pnt aPnt1 = aPoints(n1);
					gp_Pnt aPnt2 = aPoints(n2);
					gp_Pnt aPnt3 = aPoints(n3);

					/*gp_Pnt2d uv1 = uvNodes(n1);
					gp_Pnt2d uv2 = uvNodes(n2);
					gp_Pnt2d uv3 = uvNodes(n3);*/

					//QVector3D p1, p2, p3;

					if (faceOrientation == TopAbs_Orientation::TopAbs_FORWARD)
					{
						ret.push_back(aPnt1.X());
						ret.push_back(aPnt1.Y());
						ret.push_back(aPnt1.Z());

						ret.push_back(aPnt2.X());
						ret.push_back(aPnt2.Y());
						ret.push_back(aPnt2.Z());

						ret.push_back(aPnt3.X());
						ret.push_back(aPnt3.Y());
						ret.push_back(aPnt3.Z());

						/*p1 = QVector3D(aPnt1.X(), aPnt1.Y(), aPnt1.Z());
						p2 = QVector3D(aPnt2.X(), aPnt2.Y(), aPnt2.Z());
						p3 = QVector3D(aPnt3.X(), aPnt3.Y(), aPnt3.Z());*/
					}
					else
					{
						/*p1 = QVector3D(aPnt3.X(), aPnt3.Y(), aPnt3.Z());
						p2 = QVector3D(aPnt2.X(), aPnt2.Y(), aPnt2.Z());
						p3 = QVector3D(aPnt1.X(), aPnt1.Y(), aPnt1.Z());*/
						ret.push_back(aPnt3.X());
						ret.push_back(aPnt3.Y());
						ret.push_back(aPnt3.Z());

						ret.push_back(aPnt2.X());
						ret.push_back(aPnt2.Y());
						ret.push_back(aPnt2.Z());

						ret.push_back(aPnt1.X());
						ret.push_back(aPnt1.Y());
						ret.push_back(aPnt1.Z());
					}


					/*

					vertices.push_back(p1);
					vertices.push_back(p2);
					vertices.push_back(p3);

					QVector3D dir1 = p2 - p1;
					QVector3D dir2 = p3 - p1;
					QVector3D normal = QVector3D::crossProduct(dir1, dir2);

					normals.push_back(normal);
					normals.push_back(normal);
					normals.push_back(normal);

					uvs2.push_back(QVector2D(uv1.X(), uv1.Y()));
					uvs2.push_back(QVector2D(uv2.X(), uv2.Y()));
					uvs2.push_back(QVector2D(uv3.X(), uv3.Y()));


					indices.push_back(idxCounter++);
					indices.push_back(idxCounter++);
					indices.push_back(idxCounter++);*/

				}

			}

		}
		return ret;
	}

	std::vector<ObjHandle> getSelectedObjectsList(AIS_InteractiveContext* ctx) {
		std::vector<ObjHandle> ret;
		for (ctx->InitSelected(); ctx->MoreSelected(); ctx->NextSelected())
		{
			ObjHandle h;
			Handle(SelectMgr_EntityOwner) owner = ctx->SelectedOwner();
			Handle(SelectMgr_SelectableObject) so = owner->Selectable();
			Handle(StdSelect_BRepOwner) brepowner = Handle(StdSelect_BRepOwner)::DownCast(owner);

			if (brepowner.IsNull())
				break;

			const TopoDS_Shape& shape = brepowner->Shape();

			TopoDS_TShape* ptshape = shape.TShape().get();

			Handle(AIS_InteractiveObject) selected = ctx->SelectedInteractive();
			Handle(AIS_InteractiveObject) self = ctx->SelectedInteractive();
			h.handle = (unsigned __int64)(self.get());
			h.handleT = (unsigned __int64)(ptshape);
			ret.push_back(h);
		}
		return ret;
	}

	ObjHandle getSelectedObject(AIS_InteractiveContext* ctx) {
		ObjHandle h;
		for (ctx->InitSelected(); ctx->MoreSelected(); ctx->NextSelected())
		{
			Handle(SelectMgr_EntityOwner) owner = ctx->SelectedOwner();
			Handle(SelectMgr_SelectableObject) so = owner->Selectable();
			Handle(StdSelect_BRepOwner) brepowner = Handle(StdSelect_BRepOwner)::DownCast(owner);

			if (brepowner.IsNull())
				break;

			const TopoDS_Shape& shape = brepowner->Shape();

			TopoDS_TShape* ptshape = shape.TShape().get();

			Handle(AIS_InteractiveObject) selected = ctx->SelectedInteractive();
			Handle(AIS_InteractiveObject) self = ctx->SelectedInteractive();
			h.handle = (unsigned __int64)(self.get());
			h.handleT = (unsigned __int64)(ptshape);
			h.handleF = (unsigned __int64)(&shape);
			break;
		}
		return h;
	}
	ObjHandle getDetectedObject(AIS_InteractiveContext* ctx) {
		ObjHandle h;
		for (ctx->InitDetected(); ctx->MoreDetected(); ctx->NextDetected())
		{
			Handle(SelectMgr_EntityOwner) owner = ctx->DetectedOwner();
			Handle(SelectMgr_SelectableObject) so = owner->Selectable();
			Handle(StdSelect_BRepOwner) brepowner = Handle(StdSelect_BRepOwner)::DownCast(owner);

			if (brepowner.IsNull())
				break;

			const TopoDS_Shape& shape = brepowner->Shape();

			TopoDS_TShape* ptshape = shape.TShape().get();

			Handle(AIS_InteractiveObject) selected = ctx->SelectedInteractive();
			Handle(AIS_InteractiveObject) self = ctx->SelectedInteractive();
			h.handle = (unsigned __int64)(self.get());
			h.handleT = (unsigned __int64)(ptshape);
			h.handleF = (unsigned __int64)(&shape);
			break;
		}
		return h;
	}
	AIS_InteractiveObject* getObject(const ObjHandle& handle) const {
		return reinterpret_cast<AIS_InteractiveObject*> (handle.handle);
	}

	TopoDS_Shape* getShapeFromObject(const ObjHandle& handle) const {
		return reinterpret_cast<TopoDS_Shape*> (handle.handleF);
	}

	TopoDS_Shape MakeBoolDiff(ObjHandle h1, ObjHandle h2, bool fixShape = true) {
		const auto* obj1 = getObject(h1);
		const auto* obj2 = getObject(h2);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(obj1)->Shape();
		shape0 = shape0.Located(obj1->LocalTransformation());
		TopoDS_Shape shape1 = Handle(AIS_Shape)::DownCast(obj2)->Shape();
		shape1 = shape1.Located(obj2->LocalTransformation());
		const TopoDS_Shape shape = BRepAlgoAPI_Cut(shape0, shape1);

		if (fixShape) {
			ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
			unif.Build();
			auto shape2 = unif.Shape();
			return shape2;
		}
		return shape;
	}

	TopoDS_Shape MakeBoolFuse(ObjHandle h1, ObjHandle h2, bool fixShape = true) {
		const auto* obj1 = getObject(h1);
		const auto* obj2 = getObject(h2);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(obj1)->Shape();
		shape0 = shape0.Located(obj1->LocalTransformation());
		TopoDS_Shape shape1 = Handle(AIS_Shape)::DownCast(obj2)->Shape();
		shape1 = shape1.Located(obj2->LocalTransformation());
		const TopoDS_Shape shape = BRepAlgoAPI_Fuse(shape0, shape1);

		if (fixShape) {
			ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
			unif.Build();
			auto shape2 = unif.Shape();
			return shape2;
		}
		return shape;
	}

	TopoDS_Shape MakeBoolCommon(ObjHandle h1, ObjHandle h2, bool fixShape = true) {
		const auto* obj1 = getObject(h1);
		const auto* obj2 = getObject(h2);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(obj1)->Shape();
		shape0 = shape0.Located(obj1->LocalTransformation());
		TopoDS_Shape shape1 = Handle(AIS_Shape)::DownCast(obj2)->Shape();
		shape1 = shape1.Located(obj2->LocalTransformation());
		const TopoDS_Shape shape = BRepAlgoAPI_Common(shape0, shape1);

		if (fixShape) {
			ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
			unif.Build();
			auto shape2 = unif.Shape();
			return shape2;
		}
		return shape;
	}
};

/// <summary>
/// Proxy class encapsulating calls to OCCT C++ classes within 
/// C++/CLI class visible from .Net (CSharp)
/// </summary>
public ref class OCCTProxy
{


public:
	static OCCImpl* impl = new OCCImpl();
	// ============================================
	// Viewer functionality
	// ============================================

	/// <summary>
	///Initialize a viewer
	/// </summary>
	/// <param name="theWnd">System.IntPtr that contains the window handle (HWND) of the control</param>
	bool InitViewer(System::IntPtr theWnd)
	{
		try
		{
			Handle(Aspect_DisplayConnection) aDisplayConnection;
			myGraphicDriver() = new OpenGl_GraphicDriver(aDisplayConnection);
		}
		catch (Standard_Failure)
		{
			return false;
		}

		myViewer() = new V3d_Viewer(myGraphicDriver());
		myViewer()->SetDefaultLights();

		myViewer()->SetLightOn();
		//myViewer()->DefaultShadingModel();
		myView() = myViewer()->CreateView();



		/*
		Graphic3d_RenderingParams& aParams = myView()->ChangeRenderingParams();
		aParams.Method = Graphic3d_RM_RASTERIZATION;
		aParams.RaytracingDepth = 3;
		aParams.IsShadowEnabled = true;
		aParams.IsReflectionEnabled = true;
		aParams.IsAntialiasingEnabled = true;
		aParams.IsTransparentShadowEnabled = false;
		aParams.ToReverseStereo = true;
		aParams.StereoMode = Graphic3d_StereoMode::Graphic3d_StereoMode_QuadBuffer;
		aParams.AnaglyphFilter = Graphic3d_RenderingParams::Anaglyph::Anaglyph_RedCyan_Optimized;
		aParams.FrustumCullingState = Graphic3d_RenderingParams::FrustumCulling::FrustumCulling_On;
		aParams.LineFeather = 1.0;
		aParams.NbMsaaSamples = 4;
		myView()->Redraw();
		myViewer()->SetViewOn(myView());*/
		myView()->SetBgGradientColors(Quantity_Color(0.5, 0.5, 0.5, Quantity_TOC_RGB),
			Quantity_Color(0.3, 0.3, 0.3, Quantity_TOC_RGB),
			Aspect_GFM_VER,
			Standard_True);

		//add8e6
		//f0f8ff
		SetDefaultGradient();
		myView()->SetLightOn();
		myView()->SetLightOff();

		Handle(WNT_Window) aWNTWindow = new WNT_Window(reinterpret_cast<HWND> (theWnd.ToPointer()));
		myView()->SetWindow(aWNTWindow);
		if (!aWNTWindow->IsMapped())
		{
			aWNTWindow->Map();
		}
		myAISContext() = new AIS_InteractiveContext(myViewer());

		myAISContext()->UpdateCurrentViewer();
		myView()->Redraw();
		myView()->MustBeResized();
		SetDefaultDrawerParams();
		return true;
	}

	void SetDefaultGradient() {
		myView()->SetBgGradientColors(
			Quantity_Color(0xAD / (float)0xFF - 0.2f, 0xD8 / (float)0xFF - 0.2f, 0xE6 / (float)0xFF, Quantity_TOC_RGB),
			Quantity_Color(0xF0 / (float)0xFF - 0.3f, 0xF8 / (float)0xFF - 0.3f, 0xFF / (float)0xFF - 0.3f, Quantity_TOC_RGB),
			Aspect_GFM_DIAG2);
	}

	ManagedObjHandle^ GetSelectedObject() {
		auto ret = impl->getSelectedObject(myAISContext().get());
		ManagedObjHandle^ hh = gcnew ManagedObjHandle();
		hh->FromObjHandle(ret);
		return hh;
	}

	ManagedObjHandle^ GetDetectedObject() {
		auto ret = impl->getDetectedObject(myAISContext().get());
		ManagedObjHandle^ hh = gcnew ManagedObjHandle();
		hh->FromObjHandle(ret);
		return hh;
	}

	void SetDefaultDrawerParams() {
		auto ais = myAISContext();
		auto drawer = ais->DefaultDrawer();
		drawer->SetFaceBoundaryDraw(true);
		drawer->SetColor(Quantity_NOC_BLACK);
		myAISContext()->EnableDrawHiddenLine();
		//drawer->SetLineAspect()
		/*
		* raphic3d_RenderingParams& aParams = aView->ChangeRenderingParams();
// specifies rendering mode
aParams.Method = Graphic3d_RM_RAYTRACING;
// maximum ray-tracing depth
aParams.RaytracingDepth = 3;
// enable shadows rendering
aParams.IsShadowEnabled = true;
// enable specular reflections
aParams.IsReflectionEnabled = true;
// enable adaptive anti-aliasing
aParams.IsAntialiasingEnabled = true;
// enable light propagation through transparent media
aParams.IsTransparentShadowEnabled = true;
// update the view
aView->Update();
		*/
		Graphic3d_RenderingParams& rp = myView()->ChangeRenderingParams();
		rp.RenderResolutionScale = 2;
		rp.IsShadowEnabled = false;
		// enable specular reflections
		rp.IsReflectionEnabled = false;
		// enable adaptive anti-aliasing
		rp.IsAntialiasingEnabled = false;
		/*Graphic3d_RenderingParams& rayp = myView()->ChangeRenderingParams();
		rayp.Method = Graphic3d_RM_RAYTRACING;
		rayp.IsShadowEnabled = Standard_True;
		rayp.IsReflectionEnabled = Standard_True;

		myAISContext()->UpdateCurrentViewer();*/
	}

	/// <summary>
	/// Make dump of current view to file
	/// </summary>
	/// <param name="theFileName">Name of dump file</param>
	bool Dump(const TCollection_AsciiString& theFileName)
	{
		if (myView().IsNull())
		{
			return false;
		}
		myView()->Redraw();
		return myView()->Dump(theFileName.ToCString()) != Standard_False;
	}

	/// <summary>
	///Redraw view
	/// </summary>
	void RedrawView(void)
	{
		if (!myView().IsNull())
		{
			myView()->Redraw();
		}
	}

	/// <summary>
	///Update view
	/// </summary>
	void UpdateView(void)
	{
		if (!myView().IsNull())
		{
			myView()->MustBeResized();
		}
	}

	/// <summary>
	///Set computed mode in false
	/// </summary>
	void SetDegenerateModeOn(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetComputedMode(Standard_False);
			myView()->Redraw();
		}
	}

	/// <summary>
	///Set computed mode in true
	/// </summary>
	void SetDegenerateModeOff(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetComputedMode(Standard_True);
			myView()->Redraw();
		}
	}

	/// <summary>
	///Fit all
	/// </summary>
	void WindowFitAll(int theXmin, int theYmin, int theXmax, int theYmax)
	{
		if (!myView().IsNull())
		{
			myView()->WindowFitAll(theXmin, theYmin, theXmax, theYmax);
		}
	}

	/// <summary>
	///Current place of window
	/// </summary>
	/// <param name="theZoomFactor">Current zoom</param>
	void Place(int theX, int theY, float theZoomFactor)
	{
		Standard_Real aZoomFactor = theZoomFactor;
		if (!myView().IsNull())
		{
			myView()->Place(theX, theY, aZoomFactor);
		}
	}

	/// <summary>
	///Set Zoom
	/// </summary>
	void Zoom(int theX1, int theY1, int theX2, int theY2)
	{
		if (!myView().IsNull())
		{
			myView()->Zoom(theX1, theY1, theX2, theY2);
		}
	}


	void ZoomAtPoint(int theX1, int theY1, int theX2, int theY2)
	{
		if (!myView().IsNull())
		{
			myView()->ZoomAtPoint(theX1, theY1, theX2, theY2);
		}
	}

	void StartZoomAtPoint(int theX1, int theY1)
	{
		if (!myView().IsNull())
		{
			myView()->StartZoomAtPoint(theX1, theY1);
		}
	}

	/// <summary>
	///Set Pan
	/// </summary>
	void Pan(int theX, int theY)
	{
		if (!myView().IsNull())
		{
			myView()->Pan(theX, theY);
		}
	}

	/// <summary>
	///Rotation
	/// </summary>
	void Rotation(int theX, int theY)
	{
		if (!myView().IsNull())
		{
			myView()->Rotation(theX, theY);
		}
	}

	/// <summary>
	///Start rotation
	/// </summary>
	void StartRotation(int theX, int theY)
	{
		if (!myView().IsNull())
		{
			myView()->StartRotation(theX, theY);
		}
	}

	Vector3^ GetGravityPoint()
	{
		if (!myView().IsNull())
		{
			auto ret = myView()->GravityPoint();
			Vector3^ v = gcnew Vector3();
			v->X = ret.X();
			v->X = ret.Y();
			v->X = ret.Z();
			return v;
		}
		return nullptr;
	}
	Vector3^ GetEye()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Eye();
			Vector3^ v = gcnew Vector3();
			v->X = ret.X();
			v->X = ret.Y();
			v->X = ret.Z();
			return v;
		}
		return nullptr;
	}

	Vector3^ GetCenter()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Center();
			Vector3^ v = gcnew Vector3();
			v->X = ret.X();
			v->X = ret.Y();
			v->X = ret.Z();
			return v;
		}
		return nullptr;
	}

	Vector3^ GetUp()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Up();
			Vector3^ v = gcnew Vector3();
			v->X = ret.X();
			v->X = ret.Y();
			v->X = ret.Z();
			return v;
		}
		return nullptr;
	}
	/// <summary>
	///Select by rectangle
	/// </summary>
	void Select(int theX1, int theY1, int theX2, int theY2)
	{
		if (!myAISContext().IsNull())
		{
			myAISContext()->SelectRectangle(Graphic3d_Vec2i(theX1, theY1),
				Graphic3d_Vec2i(theX2, theY2),
				myView());
			myAISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Select by click
	/// </summary>
	void Select(bool xorSelect)
	{
		if (myAISContext().IsNull())
			return;

		if (xorSelect)
			myAISContext()->SelectDetected(AIS_SelectionScheme_XOR);
		else
			myAISContext()->SelectDetected(AIS_SelectionScheme_Replace);

		myAISContext()->UpdateCurrentViewer();

	}

	/// <summary>
	///Move view
	/// </summary>
	void MoveTo(int theX, int theY)
	{
		if ((!myAISContext().IsNull()) && (!myView().IsNull()))
		{
			myAISContext()->MoveTo(theX, theY, myView(), Standard_True);
		}
	}

	/// <summary>
	///Select by rectangle with pressed "Shift" key
	/// </summary>
	void ShiftSelect(int theX1, int theY1, int theX2, int theY2)
	{
		if ((!myAISContext().IsNull()) && (!myView().IsNull()))
		{
			myAISContext()->SelectRectangle(Graphic3d_Vec2i(theX1, theY1),
				Graphic3d_Vec2i(theX2, theY2),
				myView(),
				AIS_SelectionScheme_XOR);
			myAISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Select by "Shift" key
	/// </summary>
	void ShiftSelect(void)
	{
		if (!myAISContext().IsNull())
		{
			myAISContext()->SelectDetected(AIS_SelectionScheme_XOR);
			myAISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Set background color
	/// </summary>
	void BackgroundColor(int& theRed, int& theGreen, int& theBlue)
	{
		Standard_Real R1;
		Standard_Real G1;
		Standard_Real B1;
		if (!myView().IsNull())
		{
			myView()->BackgroundColor(Quantity_TOC_RGB, R1, G1, B1);
		}
		theRed = (int)R1 * 255;
		theGreen = (int)G1 * 255;
		theBlue = (int)B1 * 255;
	}

	/// <summary>
	///Get background color Red
	/// </summary>
	int GetBGColR(void)
	{

		int aRed, aGreen, aBlue;
		BackgroundColor(aRed, aGreen, aBlue);
		return aRed;
	}

	/// <summary>
	///Get background color Green
	/// </summary>
	int GetBGColG(void)
	{
		int aRed, aGreen, aBlue;
		BackgroundColor(aRed, aGreen, aBlue);
		return aGreen;
	}

	/// <summary>
	///Get background color Blue
	/// </summary>
	int GetBGColB(void)
	{
		int aRed, aGreen, aBlue;
		BackgroundColor(aRed, aGreen, aBlue);
		return aBlue;
	}

	/// <summary>
	///Update current viewer
	/// </summary>
	void UpdateCurrentViewer(void)
	{
		if (!myAISContext().IsNull())
		{
			myAISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Front side
	/// </summary>
	void FrontView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Yneg);
		}
	}

	/// <summary>
	///Top side
	/// </summary>
	void TopView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Zpos);
		}
	}

	/// <summary>
	///Left side
	/// </summary>
	void LeftView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Xneg);
		}
	}

	/// <summary>
	///Back side
	/// </summary>
	void BackView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Ypos);
		}
	}

	/// <summary>
	///Right side
	/// </summary>
	void RightView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Xpos);
		}
	}

	/// <summary>
	///Bottom side
	/// </summary>
	void BottomView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_Zneg);
		}
	}

	/// <summary>
	///Axo side
	/// </summary>
	void AxoView(void)
	{
		if (!myView().IsNull())
		{
			myView()->SetProj(V3d_XposYnegZpos);
		}
	}

	/// <summary>
	///Scale
	/// </summary>
	float Scale(void)
	{
		if (myView().IsNull())
		{
			return -1;
		}
		else
		{
			return (float)myView()->Scale();
		}
	}

	/// <summary>
	///Zoom in all view
	/// </summary>
	void ZoomAllView(void)
	{
		if (!myView().IsNull())
		{
			myView()->FitAll();
			myView()->ZFitAll();
		}
	}

	/// <summary>
	///Reset view
	/// </summary>
	void Reset(void)
	{
		if (!myView().IsNull())
		{
			myView()->Reset();
		}
	}

	void ShowCube() {
		auto cube = new AIS_ViewCube();
		cube->SetDrawAxes(true);
		cube->SetSize(40);
		cube->SetBoxFacetExtension(100 * 0.1);

		cube->SetResetCamera(true);
		cube->SetFitSelected(false);


		//cube->SetViewAnimation();
		cube->SetDuration(0.5);

		myAISContext()->Display(cube, false);
	}
	void ActivateGrid(bool en) {
		if (en)
			myViewer()->ActivateGrid(Aspect_GT_Rectangular, Aspect_GDM_Lines);
		else
			myViewer()->DeactivateGrid();
	}
	/// <summary>
	///Set display mode of objects
	/// </summary>
	/// <param name="theMode">Set current mode</param>
	void SetDisplayMode(int theMode)
	{
		if (myAISContext().IsNull())
		{

			return;
		}
		AIS_DisplayMode aCurrentMode;
		if (theMode == 0)
		{
			aCurrentMode = AIS_WireFrame;
		}
		else
		{
			aCurrentMode = AIS_Shaded;
		}

		if (myAISContext()->NbSelected() == 0)
		{
			myAISContext()->SetDisplayMode(aCurrentMode, Standard_False);
		}
		else
		{
			for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
			{
				myAISContext()->SetDisplayMode(myAISContext()->SelectedInteractive(), theMode, Standard_False);
			}
		}
		myAISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Set color
	/// </summary>
	void SetColor(int theR, int theG, int theB)
	{
		if (myAISContext().IsNull())
		{
			return;
		}
		Quantity_Color aCol = Quantity_Color(theR / 255., theG / 255., theB / 255., Quantity_TOC_RGB);
		for (; myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			myAISContext()->SetColor(myAISContext()->SelectedInteractive(), aCol, Standard_False);
		}
		myAISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Get object color red
	/// </summary>
	int GetObjColR(void)
	{
		int aRed, aGreen, aBlue;
		ObjectColor(aRed, aGreen, aBlue);
		return aRed;
	}

	/// <summary>
	///Get object color green
	/// </summary>
	int GetObjColG(void)
	{
		int aRed, aGreen, aBlue;
		ObjectColor(aRed, aGreen, aBlue);
		return aGreen;
	}

	/// <summary>
	///Get object color R/G/B
	/// </summary>
	void ObjectColor(int& theRed, int& theGreen, int& theBlue)
	{
		if (myAISContext().IsNull())
		{
			return;
		}
		theRed = 255;
		theGreen = 255;
		theBlue = 255;
		Handle(AIS_InteractiveObject) aCurrent;
		myAISContext()->InitSelected();
		if (!myAISContext()->MoreSelected())
		{
			return;
		}
		aCurrent = myAISContext()->SelectedInteractive();
		if (aCurrent->HasColor())
		{
			Quantity_Color anObjCol;
			myAISContext()->Color(aCurrent, anObjCol);
			Standard_Real r1, r2, r3;
			anObjCol.Values(r1, r2, r3, Quantity_TOC_RGB);
			theRed = (int)r1 * 255;
			theGreen = (int)r2 * 255;
			theBlue = (int)r3 * 255;
		}
	}

	/// <summary>
	///Get object color blue
	/// </summary>
	int GetObjColB(void)
	{
		int aRed, aGreen, aBlue;
		ObjectColor(aRed, aGreen, aBlue);
		return aBlue;
	}

	/// <summary>
	///Set background color R/G/B
	/// </summary>
	void SetBackgroundColor(int theRed, int theGreen, int theBlue)
	{
		if (!myView().IsNull())
		{
			myView()->SetBackgroundColor(Quantity_TOC_RGB, theRed / 255., theGreen / 255., theBlue / 255.);
		}
	}

	void SetBackgroundColor(int red1, int green1, int blue1, int red2, int green2, int blue2) {
		myView()->SetBgGradientColors(
			Quantity_Color(red1 / 255., green1 / 255., blue1 / 255., Quantity_TOC_RGB),
			Quantity_Color(red2 / 255., green2 / 255., blue2 / 255., Quantity_TOC_RGB),
			Aspect_GFM_DIAG2);
	}

	/// <summary>
	///Erase objects
	/// </summary>
	void EraseObjects(void)
	{
		if (myAISContext().IsNull())
		{
			return;
		}

		myAISContext()->EraseSelected(Standard_False);
		myAISContext()->ClearSelected(Standard_True);
	}

	/// <summary>
	///Get version
	/// </summary>
	float GetOCCVersion(void)
	{
		return (float)OCC_VERSION;
	}

	/// <summary>
	///set material
	/// </summary>
	void SetMaterial(int theMaterial)
	{
		if (myAISContext().IsNull())
		{
			return;
		}
		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			myAISContext()->SetMaterial(myAISContext()->SelectedInteractive(), (Graphic3d_NameOfMaterial)theMaterial, Standard_False);
		}
		myAISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///set transparency
	/// </summary>
	void SetTransparency(ManagedObjHandle^ h, double theTrans)
	{
		if (myAISContext().IsNull())
			return;

		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		myAISContext()->SetTransparency(o, theTrans, AutoViewerUpdate);
	}

	void SetAutoViewerUpdate(bool v)
	{
		AutoViewerUpdate = v;
	}

	/// <summary>
	///set color
	/// </summary>
	void SetColor(ManagedObjHandle^ h, int red, int green, int blue)
	{
		if (myAISContext().IsNull())
			return;

		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		Quantity_Color c(red / 255., green / 255., blue / 255., Quantity_TOC_RGB);
		myAISContext()->SetColor(o, c, AutoViewerUpdate);
	}

	/// <summary>
	///set transparency
	/// </summary>
	void SetTransparency(int theTrans)
	{
		if (myAISContext().IsNull())
		{
			return;
		}
		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			myAISContext()->SetTransparency(myAISContext()->SelectedInteractive(), ((Standard_Real)theTrans) / 10.0, Standard_False);
		}
		myAISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Return true if object is selected
	/// </summary>
	bool IsObjectSelected(void)
	{
		if (myAISContext().IsNull())
		{
			return false;
		}
		myAISContext()->InitSelected();
		return myAISContext()->MoreSelected() != Standard_False;
	}

	/// <summary>
	///Return display mode
	/// </summary>
	int DisplayMode(void)
	{
		if (myAISContext().IsNull())
		{
			return -1;
		}
		int aMode = -1;
		bool OneOrMoreInShading = false;
		bool OneOrMoreInWireframe = false;
		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			if (myAISContext()->IsDisplayed(myAISContext()->SelectedInteractive(), 1))
			{
				OneOrMoreInShading = true;
			}
			if (myAISContext()->IsDisplayed(myAISContext()->SelectedInteractive(), 0))
			{
				OneOrMoreInWireframe = true;
			}
		}
		if (OneOrMoreInShading && OneOrMoreInWireframe)
		{
			aMode = 10;
		}
		else if (OneOrMoreInShading)
		{
			aMode = 1;
		}
		else if (OneOrMoreInWireframe)
		{
			aMode = 0;
		}

		return aMode;
	}

	/// <summary>
	///Create new view
	/// </summary>
	/// <param name="theWnd">System.IntPtr that contains the window handle (HWND) of the control</param>
	void CreateNewView(System::IntPtr theWnd)
	{
		if (myAISContext().IsNull())
		{
			return;
		}
		myView() = myAISContext()->CurrentViewer()->CreateView();
		if (myGraphicDriver().IsNull())
		{
			myGraphicDriver() = new OpenGl_GraphicDriver(Handle(Aspect_DisplayConnection)());
		}
		Handle(WNT_Window) aWNTWindow = new WNT_Window(reinterpret_cast<HWND> (theWnd.ToPointer()));
		myView()->SetWindow(aWNTWindow);
		Standard_Integer w = 100, h = 100;
		aWNTWindow->Size(w, h);
		if (!aWNTWindow->IsMapped())
		{
			aWNTWindow->Map();
		}
	}

	/// <summary>
	///Set AISContext
	/// </summary>
	bool SetAISContext(OCCTProxy^ theViewer)
	{
		this->myAISContext() = theViewer->GetContext();
		if (myAISContext().IsNull())
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///Get AISContext
	/// </summary>
	Handle(AIS_InteractiveContext) GetContext(void)
	{
		return myAISContext();
	}

public:
	// ============================================
	// Import / export functionality
	// ============================================

	/// <summary>
	///Import BRep file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportBrep(System::String^ theFileName)
	{
		return ImportBrep(toAsciiString(theFileName));
	}

	/// <summary>
	///Import BRep file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportBrep(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Shape aShape;
		BRep_Builder aBuilder;
		Standard_Boolean isResult = BRepTools::Read(aShape, theFileName.ToCString(), aBuilder);
		if (!isResult)
		{
			return false;
		}

		myAISContext()->Display(new AIS_Shape(aShape), Standard_True);
		return true;
	}
	//#include <msclr/marshal_cppstd.h>

	System::Collections::Generic::List<ManagedObjHandle^>^ ImportStep(System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ImportStep(aFilename);
	}



	System::Collections::Generic::List<ManagedObjHandle^>^ ImportStep(System::String^ name, System::Collections::Generic::List<System::Byte>^ bts)
	{
		auto buf = new uint8_t[bts->Count];
		for (int i = 0;i < bts->Count;i++) {
			buf[i] = bts[i];
		}
		memstream s(buf, bts->Count);

		//char b;

		/*do {
			s.read(&b, 1);
			std::cout << "read: " << (int)b << std::endl;
		} while (s.good());*/

		const TCollection_AsciiString aFilename = toAsciiString(name);
		return ImportStep(aFilename, s);
	}

	System::Collections::Generic::List<ManagedObjHandle^>^ ImportIges(System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ImportIges(aFilename);
	}

	/// <summary>
	///Import Step file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	System::Collections::Generic::List<ManagedObjHandle^>^ ImportStep(const TCollection_AsciiString& theFileName)
	{
		System::Collections::Generic::List<ManagedObjHandle^>^ ret = gcnew System::Collections::Generic::List<ManagedObjHandle^>();
		STEPControl_Reader aReader;

		IFSelect_ReturnStatus aStatus = aReader.ReadFile(theFileName.ToCString());
		if (aStatus == IFSelect_RetDone)
		{
			bool isFailsonly = false;
			aReader.PrintCheckLoad(isFailsonly, IFSelect_ItemsByEntity);

			int aNbRoot = aReader.NbRootsForTransfer();
			aReader.PrintCheckTransfer(isFailsonly, IFSelect_ItemsByEntity);
			for (Standard_Integer n = 1; n <= aNbRoot; n++)
			{
				Standard_Boolean ok = aReader.TransferRoot(n);
				int aNbShap = aReader.NbShapes();
				if (aNbShap > 0)
				{
					for (int i = 1; i <= aNbShap; i++)
					{
						TopoDS_Shape aShape = aReader.Shape(i);

						auto ais = new AIS_Shape(aShape);


						ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

						auto hn = GetHandle(*ais);
						hhh->FromObjHandle(hn);
						ret->Add(hhh);

						myAISContext()->Display(ais, Standard_False);
					}

					myAISContext()->UpdateCurrentViewer();
				}
			}
		}
		/*else
		{
			return false;
		}*/

		return ret;
	}

	/// <summary>
	///Import Step stream
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	System::Collections::Generic::List<ManagedObjHandle^>^ ImportStep(const TCollection_AsciiString& name, std::istream& stream)
	{
		System::Collections::Generic::List<ManagedObjHandle^>^ ret = gcnew System::Collections::Generic::List<ManagedObjHandle^>();
		STEPControl_Reader aReader;
		IFSelect_ReturnStatus aStatus = aReader.ReadStream(name.ToCString(), stream);

		if (aStatus == IFSelect_RetDone)
		{
			bool isFailsonly = false;
			aReader.PrintCheckLoad(isFailsonly, IFSelect_ItemsByEntity);

			int aNbRoot = aReader.NbRootsForTransfer();
			aReader.PrintCheckTransfer(isFailsonly, IFSelect_ItemsByEntity);
			for (Standard_Integer n = 1; n <= aNbRoot; n++)
			{
				Standard_Boolean ok = aReader.TransferRoot(n);
				int aNbShap = aReader.NbShapes();
				if (aNbShap > 0)
				{
					for (int i = 1; i <= aNbShap; i++)
					{
						TopoDS_Shape aShape = aReader.Shape(i);

						auto ais = new AIS_Shape(aShape);


						ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

						auto hn = GetHandle(*ais);
						hhh->FromObjHandle(hn);
						ret->Add(hhh);

						myAISContext()->Display(ais, Standard_False);
					}

					//myAISContext()->UpdateCurrentViewer();
				}
			}
		}
		/*else
		{
			return false;
		}*/

		return ret;
	}

	/// <summary>
	///Import Iges file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	System::Collections::Generic::List<ManagedObjHandle^>^ ImportIges(const TCollection_AsciiString& theFileName)
	{
		System::Collections::Generic::List<ManagedObjHandle^>^ ret = gcnew System::Collections::Generic::List<ManagedObjHandle^>();

		IGESControl_Reader aReader;
		int aStatus = aReader.ReadFile(theFileName.ToCString());

		if (aStatus == IFSelect_RetDone)
		{
			aReader.TransferRoots();
			TopoDS_Shape aShape = aReader.OneShape();
			auto ais = new AIS_Shape(aShape);


			ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

			auto hn = GetHandle(*ais);
			hhh->FromObjHandle(hn);
			ret->Add(hhh);

			myAISContext()->Display(ais, Standard_False);


		}
		/*else
		{
			return false;
		}*/

		myAISContext()->UpdateCurrentViewer();
		return ret;
	}

	/// <summary>
	///Export BRep file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportBRep(const TCollection_AsciiString& theFileName)
	{
		myAISContext()->InitSelected();
		if (!myAISContext()->MoreSelected())
		{
			return false;
		}

		Handle(AIS_InteractiveObject) anIO = myAISContext()->SelectedInteractive();
		Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
		return BRepTools::Write(anIS->Shape(), theFileName.ToCString()) != Standard_False;
	}

	bool ExportStep(System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ExportStep(aFilename);
	}

	bool ExportStep(ManagedObjHandle^ h, System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ExportStep(h, aFilename);
	}

	bool ExportStep(ManagedObjHandle^ h, const TCollection_AsciiString& theFileName)
	{
		STEPControl_StepModelType aType = STEPControl_AsIs;
		IFSelect_ReturnStatus aStatus;
		STEPControl_Writer aWriter;

		//auto anIO = impl->getObject(h);
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		auto anIO = o;
		Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
		TopoDS_Shape aShape = anIS->Shape();
		aStatus = aWriter.Transfer(aShape, aType);
		if (aStatus != IFSelect_RetDone)
		{
			return false;
		}

		aStatus = aWriter.Write(theFileName.ToCString());
		if (aStatus != IFSelect_RetDone)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	///Export Step file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	System::Collections::Generic::List<System::Byte>^ ExportStepStream(ManagedObjHandle^ h)
	{
		STEPControl_StepModelType aType = STEPControl_AsIs;
		IFSelect_ReturnStatus aStatus;
		STEPControl_Writer aWriter;

		//auto anIO = impl->getObject(h);
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		auto anIO = o;

		Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
		TopoDS_Shape aShape = anIS->Shape();
		aStatus = aWriter.Transfer(aShape, aType);

		if (aStatus != IFSelect_RetDone)
			return nullptr;

		auto bts = gcnew System::Collections::Generic::List<System::Byte>();
		std::stringstream  ostr;
		aStatus = aWriter.WriteStream(ostr);
		unsigned char n2;
		while (!ostr.eof()) {
			ostr.read((char*)&n2, sizeof(n2));
			bts->Add(n2);
		}

		if (aStatus != IFSelect_RetDone)
			return nullptr;

		return bts;
	}



	ManagedObjHandle^ Text2Brep(System::String^ str, double aFontHeight, double anExtrusion) {
		const auto aText = toNString(str);
		// text2brep
		//const double aFontHeight = 20.0;
		Font_BRepFont aFont(Font_NOF_SANS_SERIF, Font_FontAspect_Bold, aFontHeight);
		Font_BRepTextBuilder aBuilder;
		TopoDS_Shape aTextShape2d = aBuilder.Perform(aFont, aText);

		// prism
		//const double anExtrusion = 5.0;
		BRepPrimAPI_MakePrism aPrismTool(aTextShape2d, gp_Vec(0, 0, 1) * anExtrusion);
		TopoDS_Shape aTextShape3d = aPrismTool.Shape();
		//aTextShape3d.SetLocation(); // move where needed

		//BRepMesh_IncrementalMesh mesh(shape,0.00001);
		/*bool fixShape = true;
		if (fixShape) {
			ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
			unif.Build();
			auto shape2 = unif.Shape();
			shape = shape2;
		}*/
		auto ais = new AIS_Shape(aTextShape3d);
		myAISContext()->Display(ais, true);


		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;

		// bopcut
		//TopoDS_Shape theMainShape; // defined elsewhere
		//BRepAlgoAPI_Cut aCutTool(theMainShape, aTextShape3d);
		//if (!aCutTool.IsDone()) { error }
		//TopoDS_Shape aResult = aCutTool.Shape();
	}

	/// <summary>
	///Export Step file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportStep(const TCollection_AsciiString& theFileName)
	{
		STEPControl_StepModelType aType = STEPControl_AsIs;
		IFSelect_ReturnStatus aStatus;
		STEPControl_Writer aWriter;
		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			Handle(AIS_InteractiveObject) anIO = myAISContext()->SelectedInteractive();
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
			TopoDS_Shape aShape = anIS->Shape();
			aStatus = aWriter.Transfer(aShape, aType);
			if (aStatus != IFSelect_RetDone)
			{
				return false;
			}
		}

		aStatus = aWriter.Write(theFileName.ToCString());
		if (aStatus != IFSelect_RetDone)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	///Export Iges file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportIges(const TCollection_AsciiString& theFileName)
	{
		IGESControl_Controller::Init();
		IGESControl_Writer aWriter(Interface_Static::CVal("XSTEP.iges.unit"),
			Interface_Static::IVal("XSTEP.iges.writebrep.mode"));

		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			Handle(AIS_InteractiveObject) anIO = myAISContext()->SelectedInteractive();
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
			TopoDS_Shape aShape = anIS->Shape();
			aWriter.AddShape(aShape);
		}

		aWriter.ComputeModel();
		return aWriter.Write(theFileName.ToCString()) != Standard_False;
	}

	/// <summary>
	///Export Vrml file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportVrml(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Compound aRes;
		BRep_Builder aBuilder;
		aBuilder.MakeCompound(aRes);

		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			Handle(AIS_InteractiveObject) anIO = myAISContext()->SelectedInteractive();
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
			TopoDS_Shape aShape = anIS->Shape();
			if (aShape.IsNull())
			{
				return false;
			}

			aBuilder.Add(aRes, aShape);
		}

		VrmlAPI_Writer aWriter;
		aWriter.Write(aRes, theFileName.ToCString());

		return true;
	}

	/// <summary>
	///Export Stl file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportStl(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Compound aComp;
		BRep_Builder aBuilder;
		aBuilder.MakeCompound(aComp);

		for (myAISContext()->InitSelected(); myAISContext()->MoreSelected(); myAISContext()->NextSelected())
		{
			Handle(AIS_InteractiveObject) anIO = myAISContext()->SelectedInteractive();
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
			TopoDS_Shape aShape = anIS->Shape();
			if (aShape.IsNull())
			{
				return false;
			}
			aBuilder.Add(aComp, aShape);
		}

		StlAPI_Writer aWriter;
		aWriter.Write(aComp, theFileName.ToCString());
		return true;
	}
	enum class SelectionModeEnum {
		None = -1,
		Shape = 0,
		Vertex = 1,
		Edge = 2,
		Wire = 3,
		Face = 4
	};

	void ResetSelectionMode() {
		myAISContext()->Deactivate();
	}

	void SetSelectionMode(SelectionModeEnum t) {
		if (t != SelectionModeEnum::None) {
			myAISContext()->Activate((int)t, true);
		}
		//currentMode=t;
	}

	void Erase(ManagedObjHandle^ h) {
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		myAISContext()->Erase(o, true);
	}

	void Remove(ManagedObjHandle^ h) {
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		myAISContext()->Remove(o, true);
	}

	void Display(ManagedObjHandle^ h, bool wireframe) {
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		myAISContext()->Display(o, false);
		myAISContext()->SetDisplayMode(o, wireframe ? AIS_WireFrame : AIS_Shaded, true);
	}

	gp_Trsf GetObjectMatrix(ManagedObjHandle^ h) {
		AIS_InteractiveObject* p = (AIS_InteractiveObject*)(h->Handle);
		auto trans = p->Transformation();
		return trans;
	}

	System::Collections::Generic::List<double>^ GetObjectMatrixValues(ManagedObjHandle^ h) {
		System::Collections::Generic::List<double>^ ret = gcnew System::Collections::Generic::List<double>();
		AIS_InteractiveObject* p = (AIS_InteractiveObject*)(h->Handle);
		auto trans = p->Transformation();
		for (size_t i = 1; i <= 3; i++)
		{
			for (size_t j = 1; j <= 4; j++)
			{
				ret->Add(trans.Value(i, j));
			}
		}
		return ret;
	}

	void MoveObject(ManagedObjHandle^ h, double x, double y, double z, bool rel)
	{
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		gp_Trsf tr;
		tr.SetValues(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z);
		if (rel) {
			auto mtr = GetObjectMatrix(h);
			tr.Multiply(mtr);
		}

		TopLoc_Location p(tr);
		myAISContext()->SetLocation(o, p);
	}

	void SetMatrixValues(ManagedObjHandle^ h, System::Collections::Generic::List<double>^ m)
	{
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		gp_Trsf tr;
		double a11 = m[0];
		double a12 = m[1];
		double a13 = m[2];
		double a14 = m[3];

		double a21 = m[0 + 4];
		double a22 = m[1 + 4];
		double a23 = m[2 + 4];
		double a24 = m[3 + 4];

		double a31 = m[0 + 8];
		double a32 = m[1 + 8];
		double a33 = m[2 + 8];
		double a34 = m[3 + 8];
		tr.SetValues(a11, a12, a13, a14,
			a21, a22, a23, a24,
			a31, a32, a33, a34
		);

		TopLoc_Location p(tr);
		myAISContext()->SetLocation(o, p);

	}

	void RotateObject(ManagedObjHandle^ h, double x, double y, double z, double ang, bool rel)
	{
		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		gp_Trsf tr;
		gp_Ax1 ax(gp_Pnt(0, 0, 0), gp_Dir(x, y, z));
		tr.SetRotation(ax, ang);

		if (rel) {
			auto mtr = GetObjectMatrix(h);
			tr.Multiply(mtr);
		}

		TopLoc_Location p(tr);
		myAISContext()->SetLocation(o, p);
	}

	ManagedObjHandle^ MirrorObject(ManagedObjHandle^ h, Vector3^ dir, Vector3^ pnt, bool axis2, bool rel)
	{
		ObjHandle oh = h->ToObjHandle();
		const auto* object1 = impl->getObject(oh);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();

		Handle(AIS_InteractiveObject) o;
		o.reset((AIS_InteractiveObject*)h->Handle);
		gp_Trsf tr;
		if (axis2) {
			gp_Ax2 ax2(gp_Pnt(pnt->X, pnt->Y, pnt->Z), gp_Dir(dir->X, dir->Y, dir->Z));
			tr.SetMirror(ax2);
		}
		else {
			gp_Ax1 ax(gp_Pnt(pnt->X, pnt->Y, pnt->Z), gp_Dir(dir->X, dir->Y, dir->Z));
			tr.SetMirror(ax);
		}


		if (rel) {
			auto mtr = GetObjectMatrix(h);
			tr.Multiply(mtr);
		}

		auto shape = BRepBuilderAPI_Transform(shape0, tr, Standard_True);
		//TopLoc_Location p(tr);		
		//myAISContext()->SetLocation(o, p);	
		auto ais = new AIS_Shape(shape.Shape());
		//myAISContext()->Display(new AIS_Shape(shape.Shape()), true);
		myAISContext()->Display(ais, true);

		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	System::Collections::Generic::List<Vector3^>^
		IteratePoly(ManagedObjHandle^ h) {


		System::Collections::Generic::List<Vector3^>^ ret = gcnew System::Collections::Generic::List<Vector3^>();
		ObjHandle hc = h->ToObjHandle();

		auto pp = impl->IteratePoly(hc);
		for (size_t i = 0; i < pp.size(); i += 3)
		{
			Vector3^ v = gcnew Vector3();
			v->X = pp[i];
			v->Y = pp[i + 1];
			v->Z = pp[i + 2];
			ret->Add(v);
		}

		return ret;
	}

	ManagedObjHandle^ MakeDiff(ManagedObjHandle^ mh1, ManagedObjHandle^ mh2) {
		ObjHandle h1 = mh1->ToObjHandle();
		ObjHandle h2 = mh2->ToObjHandle();
		const auto ret = impl->MakeBoolDiff(h1, h2);
		auto ais = new AIS_Shape(ret);
		myAISContext()->Display(ais, true);



		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakeFuse(ManagedObjHandle^ mh1, ManagedObjHandle^ mh2) {
		ObjHandle h1 = mh1->ToObjHandle();
		ObjHandle h2 = mh2->ToObjHandle();
		const auto ret = impl->MakeBoolFuse(h1, h2);


		auto ais = new AIS_Shape(ret);
		myAISContext()->Display(ais, true);



		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;

	}

	ManagedObjHandle^ Clone(ManagedObjHandle^ m) {
		BRepBuilderAPI_Copy copy;
		ObjHandle h = m->ToObjHandle();

		const auto* object1 = impl->getObject(h);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());
		copy.Perform(shape0);

		auto shapeCopy = copy.Shape();

		auto ais = new AIS_Shape(shapeCopy);
		myAISContext()->Display(ais, true);

		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;

	}

	ManagedObjHandle^ MakePrism(ManagedObjHandle^ m, double height) {
		ObjHandle h = m->ToObjHandle();
		BRepBuilderAPI_MakeFace bface;
		const auto* object1 = impl->getObject(h);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		auto compound = TopoDS::Compound(shape0);

		int counter = 0;
		for (TopExp_Explorer aExpFace(shape0, TopAbs_WIRE); aExpFace.More(); aExpFace.Next())
		{
			const auto ttt = aExpFace.Current();
			const auto& edgee = TopoDS::Wire(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);
			auto ttt4 = (unsigned __int64)(&ttt);

			if (edgee.IsNull()) {
				continue;
			}
			//if (ttt3 == h.handleT) 
			//{
			TopoDS_Wire wire = TopoDS::Wire(aExpFace.Current());
			BRepBuilderAPI_MakeFace face1(wire);
			if (counter > 0) {

				//wire.Reverse();
				bface.Add(wire);
			}
			else
				bface.Init(face1);

			counter++;
			//}					
		}

		auto profile = bface.Face();
		//myAISContext()->Display(new AIS_Shape(profile), true);

		gp_Vec vec(0, 0, height);
		auto body = BRepPrimAPI_MakePrism(profile, vec);

		auto shape = body.Shape();
		//BRepMesh_IncrementalMesh mesh(shape,0.00001);
		/*bool fixShape = true;
		if (fixShape) {
			ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
			unif.Build();
			auto shape2 = unif.Shape();
			shape = shape2;
		}*/
		auto ais = new AIS_Shape(shape);
		myAISContext()->Display(ais, true);

		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;


		//const auto ret = impl->MakeBoolFuse(h1, h2, fixShape);
		//myAISContext()->Display(new AIS_Shape(ret), true);
	}
	ManagedObjHandle^ MakePrismFromFace(ManagedObjHandle^ m, double height) {
		ObjHandle h = m->ToObjHandle();
		BRepBuilderAPI_MakeFace bface;
		const auto* object1 = impl->getObject(h);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());
		int counter = 0;
		for (TopExp_Explorer aExpFace(shape0, TopAbs_FACE); aExpFace.More(); aExpFace.Next())
		{
			const auto ttt = aExpFace.Current();
			const auto& edgee = TopoDS::Face(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);
			auto ttt4 = (unsigned __int64)(&ttt);

			if (edgee.IsNull()) {
				continue;
			}
			if (ttt3 == h.handleT) {


				TopLoc_Location aLocation;
				Handle(Geom_Surface) aSurf = BRep_Tool::Surface(edgee, aLocation);

				auto plane = Handle(Geom_Plane)::DownCast(aSurf);

				auto pln = (*plane).Pln();

				float aU = 0;
				float aV = 0;
				gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
				Vector3^ pos = gcnew Vector3();
				Vector3^ nrm = gcnew Vector3();

				PlaneSurfInfo^ ret = gcnew PlaneSurfInfo();
				GProp_GProps massProps;
				BRepGProp::SurfaceProperties(ttt, massProps);
				gp_Pnt gPt = massProps.CentreOfMass();

				pos->X = aPnt.X();
				pos->Y = aPnt.Y();
				pos->Z = aPnt.Z();

				auto orient = edgee.Orientation();

				auto dir = pln.Axis().Direction().Transformed(aLocation.Transformation());
				if (orient == TopAbs_REVERSED) {
					dir.Reverse();
				}
				gp_Vec vec(dir.X(), dir.Y(), dir.Z());
				vec *= height;

				auto body = BRepPrimAPI_MakePrism(edgee, vec);

				auto shape = body.Shape();
				//BRepMesh_IncrementalMesh mesh(shape,0.00001);
				/*bool fixShape = true;
				if (fixShape) {
					ShapeUpgrade_UnifySameDomain unif(shape, true, true, false);
					unif.Build();
					auto shape2 = unif.Shape();
					shape = shape2;
				}*/
				auto ais = new AIS_Shape(shape);
				myAISContext()->Display(ais, true);

				ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

				auto hn = GetHandle(*ais);
				hhh->FromObjHandle(hn);
				return hhh;
			}

		}


		return nullptr;


		//const auto ret = impl->MakeBoolFuse(h1, h2, fixShape);
		//myAISContext()->Display(new AIS_Shape(ret), true);
	}
	ManagedObjHandle^ MakeCommon(ManagedObjHandle^ mh1, ManagedObjHandle^ mh2) {
		ObjHandle h1 = mh1->ToObjHandle();
		ObjHandle h2 = mh2->ToObjHandle();
		const auto ret = impl->MakeBoolCommon(h1, h2);


		auto ais = new AIS_Shape(ret);
		myAISContext()->Display(ais, true);



		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	void MakeBool() {
		gp_Ax2 anAxis;
		anAxis.SetLocation(gp_Pnt(0.0, 100.0, 0.0));

		TopoDS_Shape aTopoBox = BRepPrimAPI_MakeBox(anAxis, 3.0, 4.0, 5.0);
		TopoDS_Shape aTopoSphere = BRepPrimAPI_MakeSphere(anAxis, 2.5);
		TopoDS_Shape aFusedShape = BRepAlgoAPI_Fuse(aTopoBox, aTopoSphere);

		gp_Trsf aTrsf;
		aTrsf.SetTranslation(gp_Vec(8.0, 0.0, 0.0));
		BRepBuilderAPI_Transform aTransform(aFusedShape, aTrsf);

		Handle_AIS_Shape anAisBox = new AIS_Shape(aTopoBox);
		Handle_AIS_Shape anAisSphere = new AIS_Shape(aTopoSphere);
		Handle_AIS_Shape anAisFusedShape = new AIS_Shape(aTransform.Shape());

		anAisBox->SetColor(Quantity_NOC_SPRINGGREEN);
		anAisSphere->SetColor(Quantity_NOC_STEELBLUE);
		anAisFusedShape->SetColor(Quantity_NOC_ROSYBROWN);

		myAISContext()->Display(anAisBox, true);
		myAISContext()->Display(anAisSphere, true);
		myAISContext()->Display(anAisFusedShape, true);
	}

	ManagedObjHandle^ AddWireDraft(double height) {
		BRepBuilderAPI_MakeFace bface;

		BRepBuilderAPI_MakeWire wire;
		std::vector<gp_Pnt> pnts;
		pnts.push_back(gp_Pnt(0, 0, 0));
		pnts.push_back(gp_Pnt(100, 0, 0));
		pnts.push_back(gp_Pnt(100, 100, 0));
		pnts.push_back(gp_Pnt(0, 100, 0));
		for (size_t i = 1; i <= pnts.size(); i++)
		{
			Handle(Geom_TrimmedCurve) seg1 = GC_MakeSegment(pnts[i - 1], pnts[i % pnts.size()]);
			auto edge = BRepBuilderAPI_MakeEdge(seg1);
			wire.Add(edge);
		}

		auto seg2 = GC_MakeCircle(gp_Ax1(gp_Pnt(50, 50, 0), gp_Dir(0, 0, 1)), 10).Value();
		auto edge1 = BRepBuilderAPI_MakeEdge(seg2);
		auto wb = BRepBuilderAPI_MakeWire(edge1).Wire();
		wb.Reverse();

		//myAISContext()->Display(new AIS_Shape(wire.Wire()), true);
		//return;
		BRepBuilderAPI_MakeFace face1(wire.Wire());
		bface.Init(face1);
		bface.Add(wb);
		auto profile = bface.Face();
		//myAISContext()->Display(new AIS_Shape(profile), true);

		gp_Vec vec(0, 0, height);
		auto body = BRepPrimAPI_MakePrism(profile, vec);

		auto shape = body.Shape();
		auto ais = new AIS_Shape(shape);
		myAISContext()->Display(ais, true);

		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	Vector3^ GetVertexPosition(ManagedObjHandle^ h1)
	{
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		for (TopExp_Explorer exp(shape0, TopAbs_VERTEX); exp.More(); exp.Next()) {
			const auto ttt = exp.Current();
			const auto& edgee = TopoDS::Vertex(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);
			auto ttt4 = (unsigned __int64)(&ttt);

			if (edgee.IsNull()) {
				continue;
			}
			if (ttt3 == hh.handleT) {
				Vector3^ ret = gcnew Vector3();

				gp_Pnt p = BRep_Tool::Pnt(edgee);
				ret->X = p.X();
				ret->Y = p.Y();
				ret->Z = p.Z();
				return ret;
			}
		}
		return nullptr;
	}

	EdgeInfo^ GetEdgeInfoPosition(ManagedObjHandle^ h1)
	{
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		for (TopExp_Explorer exp(shape0, TopAbs_EDGE); exp.More(); exp.Next()) {
			const auto ttt = exp.Current();
			const auto& edgee = TopoDS::Edge(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);
			auto ttt4 = (unsigned __int64)(&ttt);

			if (edgee.IsNull()) {
				continue;
			}
			if (ttt3 == hh.handleT) {

				GProp_GProps massProps;
				BRepGProp::LinearProperties(ttt, massProps);
				auto len = massProps.Mass();

				gp_Pnt gPt = massProps.CentreOfMass();

				//Analysis of Edge
				Standard_Real First, Last;
				Handle(Geom_Curve) curve = BRep_Tool::Curve(edgee, First, Last); //Extract the curve from the edge
				GeomAdaptor_Curve aAdaptedCurve(curve);
				GeomAbs_CurveType curveType = aAdaptedCurve.GetType();

				gp_Pnt pnt1, pnt2;
				aAdaptedCurve.D0(First, pnt1);
				aAdaptedCurve.D0(Last, pnt2);
				int nPoles = 2;
				if (curveType == GeomAbs_BezierCurve || curveType == GeomAbs_BSplineCurve)
					nPoles = aAdaptedCurve.NbPoles();


				EdgeInfo^ ret = nullptr;
				if (curveType == GeomAbs_Circle) {
					auto ret2 = gcnew CircleEdgeInfo();
					Handle(Geom_Circle) C2 = Handle(Geom_Circle)::DownCast(curve);
					if (!C2.IsNull())
					{
						ret2->Radius = C2->Circ().Radius();
					}
					ret = ret2;
				}
				else
					ret = gcnew EdgeInfo();

				ret->Length = len;
				ret->CurveType = (CurveType)curveType;
				ret->Start = gcnew Vector3();
				ret->End = gcnew Vector3();

				ret->COM = gcnew Vector3();
				ret->COM->X = gPt.X();
				ret->COM->Y = gPt.Y();
				ret->COM->Z = gPt.Z();

				ret->Start->X = pnt1.X();
				ret->Start->Y = pnt1.Y();
				ret->Start->Z = pnt1.Z();

				ret->End->X = pnt2.X();
				ret->End->Y = pnt2.Y();
				ret->End->Z = pnt2.Z();
				return ret;
			}
		}
		return nullptr;
	}

	CylinderSurfInfo^ ExtractCylinderSurface(TopoDS_Shape ttt) {
		auto loc = ttt.Location();

		const auto& aFace = TopoDS::Face(ttt);
		auto orient = aFace.Orientation();

		TopLoc_Location aLocation;
		Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);
		auto cyl = Handle(Geom_CylindricalSurface)::DownCast(aSurf);
		auto rad = (*cyl).Radius();

		auto dir = cyl->Axis().Direction().Transformed(aLocation.Transformation());
		if (orient == TopAbs_REVERSED) {
			dir.Reverse();
		}
		float aU = 0;
		float aV = 0;

		gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
		Vector3^ pos = gcnew Vector3();
		Vector3^ nrm = gcnew Vector3();

		CylinderSurfInfo^ ret = gcnew CylinderSurfInfo();

		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();

		nrm->X = dir.X();
		nrm->Y = dir.Y();
		nrm->Z = dir.Z();


		pos->X = aPnt.X();
		pos->Y = aPnt.Y();
		pos->Z = aPnt.Z();

		ret->COM = gcnew Vector3();
		ret->COM->X = gPt.X();
		ret->COM->Y = gPt.Y();
		ret->COM->Z = gPt.Z();
		ret->Position = pos;
		ret->Radius = rad;
		ret->Axis = nrm;
		return ret;
	}

	SphereSurfInfo^ ExtractSphereSurface(TopoDS_Shape ttt) {
		auto loc = ttt.Location();

		const auto& aFace = TopoDS::Face(ttt);
		auto orient = aFace.Orientation();

		TopLoc_Location aLocation;
		Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);
		auto cyl = Handle(Geom_SphericalSurface)::DownCast(aSurf);
		auto rad = (*cyl).Radius();

		float aU = 0;
		float aV = 0;

		gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
		Vector3^ pos = gcnew Vector3();

		SphereSurfInfo^ ret = gcnew SphereSurfInfo();

		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();


		pos->X = aPnt.X();
		pos->Y = aPnt.Y();
		pos->Z = aPnt.Z();

		ret->COM = gcnew Vector3();
		ret->COM->X = gPt.X();
		ret->COM->Y = gPt.Y();
		ret->COM->Z = gPt.Z();
		ret->Position = pos;
		ret->Radius = rad;

		return ret;
	}
	PlaneSurfInfo^ ExtractPlaneSurface(TopoDS_Shape ttt) {
		auto loc = ttt.Location();

		const auto& aFace = TopoDS::Face(ttt);
		auto orient = aFace.Orientation();

		TopLoc_Location aLocation;
		Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);

		auto plane = Handle(Geom_Plane)::DownCast(aSurf);

		auto pln = (*plane).Pln();

		float aU = 0;
		float aV = 0;
		gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
		Vector3^ pos = gcnew Vector3();
		Vector3^ nrm = gcnew Vector3();

		PlaneSurfInfo^ ret = gcnew PlaneSurfInfo();
		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();

		pos->X = aPnt.X();
		pos->Y = aPnt.Y();
		pos->Z = aPnt.Z();

		auto dir = pln.Axis().Direction().Transformed(aLocation.Transformation());
		if (orient == TopAbs_REVERSED) {
			dir.Reverse();
		}

		nrm->X = dir.X();
		nrm->Y = dir.Y();
		nrm->Z = dir.Z();

		ret->COM = gcnew Vector3();
		ret->COM->X = gPt.X();
		ret->COM->Y = gPt.Y();
		ret->COM->Z = gPt.Z();
		ret->Position = pos;
		ret->Normal = nrm;

		return ret;
	}

	SurfInfo^ GetFaceInfo(ManagedObjHandle^ h1) {
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		auto temp1 = Handle(AIS_Shape)::DownCast(object1);
		if (temp1.IsNull()) {
			return nullptr;
		}
		TopoDS_Shape shape0 = temp1->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		for (TopExp_Explorer exp(shape0, TopAbs_FACE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();

			const auto& aFace = TopoDS::Face(ttt);
			auto orient = aFace.Orientation();

			TopLoc_Location aLocation;
			Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);

			GeomAdaptor_Surface theGASurface(aSurf);

			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);

			if (aFace.IsNull()) {
				continue;
			}
			if (ttt3 == hh.handleT) {
				switch (theGASurface.GetType())
				{
				case GeomAbs_Plane:
				{
					return ExtractPlaneSurface(ttt);
				}
				case GeomAbs_Cylinder:
				{
					return ExtractCylinderSurface(ttt);
				}
				case GeomAbs_Sphere:
				{
					return ExtractSphereSurface(ttt);
				}
				}
			}
		}
		return nullptr;
	}

	System::Collections::Generic::List<SurfInfo^>^ GetFacesInfo(ManagedObjHandle^ h1) {
		System::Collections::Generic::List<SurfInfo^>^ rett = gcnew System::Collections::Generic::List<SurfInfo^>();
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		for (TopExp_Explorer exp(shape0, TopAbs_FACE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();

			const auto& aFace = TopoDS::Face(ttt);
			auto orient = aFace.Orientation();

			TopLoc_Location aLocation;
			Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);

			GeomAdaptor_Surface theGASurface(aSurf);

			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);

			if (aFace.IsNull()) {
				continue;
			}

			auto tp = theGASurface.GetType();
			switch (theGASurface.GetType()) {
			case GeomAbs_Plane:
			{
				rett->Add(ExtractPlaneSurface(ttt));
			}
			break;
			case GeomAbs_Cylinder:
			{
				rett->Add(ExtractCylinderSurface(ttt));
			}
			break;
			case GeomAbs_Sphere:
			{
				rett->Add(ExtractSphereSurface(ttt));
			}
			break;
			}
		}
		return rett;
	}

	ManagedObjHandle^ MakeChamfer(ManagedObjHandle^ h1, double s)
	{
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(myAISContext().get(), edges);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		BRepFilletAPI_MakeChamfer chamferOp(shape0);

		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_EDGE); edgeExplorer.More(); edgeExplorer.Next()) {
			const auto ttt = edgeExplorer.Current();
			const auto& edgee = TopoDS::Edge(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);

			if (edgee.IsNull()) {
				continue;
			}
			for (auto edge : edges) {
				if (ttt3 == edge.handleT) {
					chamferOp.Add(s, edgee);
					b = true;
					break;
				}
			}
		}

		if (!b)
			return nullptr;

		chamferOp.Build();
		auto shape = chamferOp.Shape();
		auto ais = new AIS_Shape(shape);
		myAISContext()->Display(ais, false);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ Sphere(double x1, double y1, double z1,double size) {
		return Sphere(gp_Pnt(x1, y1, z1), size);
	}

	ManagedObjHandle^ Sphere(gp_Pnt center, double radius) {

		auto	sphere = BRepPrimAPI_MakeSphere(center, radius).Shape();

		auto ais = new AIS_Shape(sphere);
		myAISContext()->Display(ais, true);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ Pipe(double x1, double y1, double z1, double x2, double y2, double z2, double size) {
		return Pipe(gp_Pnt(x1, y1, z1), gp_Pnt(x2, y2, z2), size);
	}

	ManagedObjHandle^ Pipe(gp_Pnt point1, gp_Pnt point2, double size) {


		auto	makeWire = BRepBuilderAPI_MakeWire();
		auto edge = BRepBuilderAPI_MakeEdge(point1, point2).Edge();
		makeWire.Add(edge);
		makeWire.Build();
		auto wire = makeWire.Wire();

		auto dir = gp_Dir(point2.X() - point1.X(), point2.Y() - point1.Y(), point2.Z() - point1.Z());
		auto circle = gp_Circ(gp_Ax2(point1, dir), size);
		auto profile_edge = BRepBuilderAPI_MakeEdge(circle).Edge();
		auto profile_wire = BRepBuilderAPI_MakeWire(profile_edge).Wire();
		auto profile_face = BRepBuilderAPI_MakeFace(profile_wire).Face();
		auto pipe = BRepOffsetAPI_MakePipe(wire, profile_face).Shape();

		auto ais = new AIS_Shape(pipe);
		myAISContext()->Display(ais, true);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakePipe(ManagedObjHandle^ h1, double s)
	{
		auto pipe1 = Pipe(gp_Pnt(0, 0, 0), gp_Pnt(0, 0, 1), s);
		auto sphere1 = Sphere(gp_Pnt(0, 0, 1), s);
		auto fuse1 = MakeFuse(pipe1, sphere1);
		Erase(pipe1);
		Erase(sphere1);

		auto pipe2 = Pipe(gp_Pnt(0, 0, 1), gp_Pnt(0, 1, 2), s);

		auto fuse2 = MakeFuse(pipe2, fuse1);
		Erase(pipe2);
		Erase(fuse1);

		auto sphere2 = Sphere(gp_Pnt(0, 1, 2), s);

		auto fuse3 = MakeFuse(sphere2, fuse2);
		Erase(sphere2);
		Erase(fuse2);

		auto pipe3 = Pipe(gp_Pnt(0, 1, 2), gp_Pnt(0, 2, 2), s);

		auto fuse4 = MakeFuse(pipe3, fuse3);
		Erase(pipe3);
		Erase(fuse3);

		return fuse4;

		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(myAISContext().get(), edges);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		gp_Pnt p1(0, 0, 0),
			p2(0, 1, 0),
			p3(1, 2, 0),
			p4(2, 2, 0);
		//# the edges
		auto ed1 = BRepBuilderAPI_MakeEdge(p1, p2).Edge();
		auto	ed2 = BRepBuilderAPI_MakeEdge(p2, p3).Edge();
		auto	ed3 = BRepBuilderAPI_MakeEdge(p3, p4).Edge();


		ChFi2d_AnaFilletAlgo f;
		double radius = 0.3;
		f.Init(ed1, ed2, gp_Pln());
		f.Perform(radius);
		auto res = f.Result(ed1, ed2);


		/*
	auto ais = new AIS_Shape(shape);
	myAISContext()->Display(ais, false);
	ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

	auto hn = GetHandle(*ais);
	hhh->FromObjHandle(hn);
	return hhh;*/
		return nullptr;
	}

	ManagedObjHandle^ MakeFillet(ManagedObjHandle^ h1, double s)
	{
		auto hh = h1->ToObjHandle();
		const auto* object1 = impl->getObject(hh);
		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(myAISContext().get(), edges);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		BRepFilletAPI_MakeFillet filletOp(shape0);

		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_EDGE); edgeExplorer.More(); edgeExplorer.Next()) {
			const auto ttt = edgeExplorer.Current();
			const auto& edgee = TopoDS::Edge(ttt);
			auto tt = ttt.TShape();
			TopoDS_TShape* ptshape = tt.get();
			auto ttt3 = (unsigned __int64)(ptshape);

			if (edgee.IsNull()) {
				continue;
			}
			for (auto edge : edges) {
				if (ttt3 == edge.handleT) {
					filletOp.Add(s, edgee);
					b = true;
					break;
				}
			}


		}

		if (!b)
			return nullptr;

		filletOp.Build();
		auto shape = filletOp.Shape();
		auto ais = new AIS_Shape(shape);
		myAISContext()->Display(ais, false);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakeBox(double x, double y, double z, double w, double h, double l) {

		ManagedObjHandle^ hh = gcnew ManagedObjHandle();
		//myAISContext()->SetDisplayMode(prs, AIS_Shaded, false);

		gp_Pnt p1(x, y, z);
		gp_Pnt p2(w, h, l);
		BRepPrimAPI_MakeBox box(p1, p2);
		box.Build();
		auto solid = box.Solid();
		auto shape = new AIS_Shape(solid);
		myAISContext()->Display(shape, Standard_True);
		myAISContext()->SetDisplayMode(shape, AIS_Shaded, false);
		auto hn = GetHandle(*shape);
		hh->FromObjHandle(hn);
		return hh;
	}

	ManagedObjHandle^ MakeCylinder(double r, double h) {

		ManagedObjHandle^ hh = gcnew ManagedObjHandle();

		BRepPrimAPI_MakeCylinder cyl(r, h);
		cyl.Build();
		auto solid = cyl.Solid();
		auto shape = new AIS_Shape(solid);
		myAISContext()->Display(shape, Standard_True);
		myAISContext()->SetDisplayMode(shape, AIS_Shaded, false);
		auto hn = GetHandle(*shape);
		hh->FromObjHandle(hn);
		return hh;
	}

	ManagedObjHandle^ MakeSphere(double r) {

		ManagedObjHandle^ hh = gcnew ManagedObjHandle();

		BRepPrimAPI_MakeSphere s(r);
		s.Build();
		auto solid = s.Solid();
		auto shape = new AIS_Shape(solid);
		myAISContext()->Display(shape, Standard_True);
		myAISContext()->SetDisplayMode(shape, AIS_Shaded, false);
		auto hn = GetHandle(*shape);
		hh->FromObjHandle(hn);
		return hh;
	}


	ManagedObjHandle^ MakeCone(double r1, double r2, double h) {

		ManagedObjHandle^ hh = gcnew ManagedObjHandle();

		BRepPrimAPI_MakeCone s(r1, r2, h);
		s.Build();
		auto solid = s.Solid();
		auto shape = new AIS_Shape(solid);
		myAISContext()->Display(shape, Standard_True);
		myAISContext()->SetDisplayMode(shape, AIS_Shaded, false);
		auto hn = GetHandle(*shape);
		hh->FromObjHandle(hn);
		return hh;
	}

	static void ImportElement(BRep_Builder& builder, TopoDS_Compound& compound, BlueprintItem^ item) {
		Line3D^ line = dynamic_cast<Line3D^>	(item);

		if (line != nullptr) {
			AttachLineToCompound(builder, compound, line->Start->X, line->Start->Y, line->Start->Z, line->End->X, line->End->Y, line->End->Z);
		}
	}
	ManagedObjHandle^ ImportBlueprint(Blueprint^ blueprint) {
		TopoDS_Compound compound;
		BRep_Builder builder;

		builder.MakeCompound(compound);

		for (size_t i = 0; i < blueprint->Contours->Count; i++)
		{
			BRepBuilderAPI_MakeWire wire;
			for (size_t j = 0; j < blueprint->Contours[i]->Items->Count; j++)
			{
				auto p = blueprint->Contours[i]->Items[j];
				Line2D^ line = dynamic_cast<Line2D^>	(p);
				BlueprintPolyline^ polyline = dynamic_cast<BlueprintPolyline^>	(p);
				Arc2d^ arc = dynamic_cast<Arc2d^>	(p);

				if (polyline != nullptr) {
					for (size_t p = 1; p < polyline->Points->Count; p++)
					{
						gp_Pnt pnt1(polyline->Points[p - 1]->X, polyline->Points[p - 1]->Y, 0);
						gp_Pnt pnt2(polyline->Points[p]->X, polyline->Points[p]->Y, 0);
						Handle(Geom_TrimmedCurve) seg1 = GC_MakeSegment(pnt1, pnt2);
						auto edge = BRepBuilderAPI_MakeEdge(seg1);
						wire.Add(edge);
					}
				}
				else				if (line != nullptr) {
					gp_Pnt pnt1(line->Start->X, line->Start->Y, 0);
					gp_Pnt pnt2(line->End->X, line->End->Y, 0);
					Handle(Geom_TrimmedCurve) seg1 = GC_MakeSegment(pnt1, pnt2);
					auto edge = BRepBuilderAPI_MakeEdge(seg1);
					wire.Add(edge);
				}
				else
					if (arc != nullptr && arc->IsCircle) {
						gp_Pnt cen(arc->Center->X, arc->Center->Y, 0);

						auto seg1 = GC_MakeCircle(gp_Ax1(cen, gp_Dir(0, 0, 1)), arc->Radius).Value();
						auto edge = BRepBuilderAPI_MakeEdge(seg1);
						TopoDS_Edge e = TopoDS::Edge(edge);
						if (arc->CCW)
							e.Reverse();

						wire.Add(e);
						/*auto wb = BRepBuilderAPI_MakeWire(edge).Wire();
						wb.Reverse();

						wire.Add(wb);*/

					}
					else
						if (arc != nullptr) {

							gp_Pnt pnt1(arc->Start->X, arc->Start->Y, 0);
							gp_Pnt pnt2(arc->End->X, arc->End->Y, 0);
							gp_Pnt pnt3(arc->Middle->X, arc->Middle->Y, 0);

							auto seg1 = GC_MakeArcOfCircle(pnt1, pnt3, pnt2).Value();
							auto edge = BRepBuilderAPI_MakeEdge(seg1);

							wire.Add(edge);
						}
			}

			builder.Add(compound, wire.Wire());

			//ImportElement(builder, compound, blueprint->Items[i]);
		}

		auto shape = new AIS_Shape(compound);
		auto h = GetHandle(*shape);
		myAISContext()->Display(shape, true);

		ManagedObjHandle^ ret = gcnew ManagedObjHandle();
		ret->FromObjHandle(h);
		return ret;
	}
	ManagedObjHandle^ ImportBlueprint(Blueprint3d^ blueprint) {
		TopoDS_Compound compound;
		BRep_Builder builder;

		builder.MakeCompound(compound);

		for (size_t i = 0; i < blueprint->Contours->Count; i++)
		{
			BRepBuilderAPI_MakeWire wire;
			for (size_t j = 0; j < blueprint->Contours[i]->Items->Count; j++)
			{
				auto p = blueprint->Contours[i]->Items[j];
				Line3D^ line = dynamic_cast<Line3D^>	(p);
				Arc3d^ arc = dynamic_cast<Arc3d^>	(p);

				if (line != nullptr) {
					gp_Pnt pnt1(line->Start->X, line->Start->Y, line->Start->Z);
					gp_Pnt pnt2(line->Start->X, line->Start->Y, line->Start->Z);
					Handle(Geom_TrimmedCurve) seg1 = GC_MakeSegment(pnt1, pnt2);
					auto edge = BRepBuilderAPI_MakeEdge(seg1);
					wire.Add(edge);
				}
				else
					if (arc != nullptr && arc->AngleSweep == 360) {
						gp_Pnt cen(arc->Center->X, arc->Center->Y, arc->Center->Z);

						auto seg1 = GC_MakeCircle(gp_Ax1(cen, gp_Dir(0, 0, 1)), arc->Radius).Value();
						auto edge = BRepBuilderAPI_MakeEdge(seg1);
						wire.Add(edge);
					}
			}

			builder.Add(compound, wire.Wire());

			//ImportElement(builder, compound, blueprint->Items[i]);
		}

		auto shape = new AIS_Shape(compound);
		auto h = GetHandle(*shape);
		myAISContext()->Display(shape, true);

		ManagedObjHandle^ ret = gcnew ManagedObjHandle();
		ret->FromObjHandle(h);
		return ret;
	}

	static void AttachLineToCompound(BRep_Builder& builder, TopoDS_Compound& compound, double x1, double y1, double z1, double x2, double y2, double z2)
	{
		Handle(Geom_TrimmedCurve) segment = GC_MakeSegment(gp_Pnt(x1, y1, z1), gp_Pnt(x2, y2, z2));
		TopoDS_Edge edge = BRepBuilderAPI_MakeEdge(segment);

		TopoDS_Wire wire = BRepBuilderAPI_MakeWire(edge);

		BRepBuilderAPI_MakeWire topWire;
		topWire.Add(wire);
		auto result = topWire.Wire();
		builder.Add(compound, topWire);
	}


	ObjHandle GetHandle(const AIS_Shape& ais_shape) {
		const TopoDS_Shape& shape = ais_shape.Shape();
		TopoDS_TShape* ptshape = shape.TShape().get();
		ObjHandle h;
		h.handleF = (unsigned __int64)(&shape);
		h.handleT = (unsigned __int64)ptshape;
		h.handle = (unsigned __int64)(&ais_shape);
		return h;
	}

	/*/// <summary>
	///Define which Import/Export function must be called
	/// </summary>
	/// <param name="theFileName">Name of Import/Export file</param>
	/// <param name="theFormat">Determines format of Import/Export file</param>
	/// <param name="theIsImport">Determines is Import or not</param>
	bool TranslateModel(System::String^ theFileName, int theFormat, bool theIsImport)
	{
		bool isResult;

		const TCollection_AsciiString aFilename = toAsciiString(theFileName);
		if (theIsImport)
		{
			switch (theFormat)
			{
			case 0:
				isResult = ImportBrep(aFilename);
				break;
			case 1:
				isResult = ImportStep(aFilename);
				break;
			case 2:
				isResult = ImportIges(aFilename);
				break;
			default:
				isResult = false;
			}
		}
		else
		{
			switch (theFormat)
			{
			case 0:
				isResult = ExportBRep(aFilename);
				break;
			case 1:
				isResult = ExportStep(aFilename);
				break;
			case 2:
				isResult = ExportIges(aFilename);
				break;
			case 3:
				isResult = ExportVrml(aFilename);
				break;
			case 4:
				isResult = ExportStl(aFilename);
				break;
			case 5:
				isResult = Dump(aFilename);
				break;
			default:
				isResult = false;
			}
		}
		return isResult;
	}*/

	/// <summary>
	///Initialize OCCTProxy
	/// </summary>
	void InitOCCTProxy(void)
	{
		myGraphicDriver() = NULL;
		myViewer() = NULL;
		myView() = NULL;
		myAISContext() = NULL;
	}

private:
	// fields
	bool AutoViewerUpdate;
	NCollection_Haft<Handle(V3d_Viewer)> myViewer;
	NCollection_Haft<Handle(V3d_View)> myView;
	NCollection_Haft<Handle(AIS_InteractiveContext)> myAISContext;
	NCollection_Haft<Handle(OpenGl_GraphicDriver)> myGraphicDriver;
};

