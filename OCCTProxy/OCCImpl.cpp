#include <iostream>
#include <optional>
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
#include <BRepOffsetAPI_MakePipeShell.hxx>
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
#include <BRepFilletAPI_MakeFillet2d.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>

#include <Graphic3d_RenderingParams.hxx>
#include <TopExp_Explorer.hxx>
#include <Font_BRepFont.hxx>

#include <Geom_SphericalSurface.hxx>
#include <Geom_CylindricalSurface.hxx>
#include <Geom_BoundedSurface.hxx>
#include <Geom_SweptSurface.hxx>
#include <Geom_SurfaceOfRevolution.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_ConicalSurface.hxx>
#include <Geom_ToroidalSurface.hxx>
#include <Geom_Line.hxx>
#include <Font_BRepTextBuilder.hxx>

#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <ChFi2d_AnaFilletAlgo.hxx>
#include <GCE2d_MakeSegment.hxx>
#include <Geom2d_Ellipse.hxx>
#include <Geom2d_Line.hxx>
#include <Geom_BSplineCurve.hxx>
#include <GeomAPI_PointsToBSpline.hxx>

#include <TopTools_DataMapOfShapeInteger.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <TDF_Label.hxx>
#include <TDF_ChildIterator.hxx>
#include <TDF_Tool.hxx>
#include <TopoDS_Shape.hxx>
#include <TCollection_AsciiString.hxx>

#include <BRepLib.hxx>
// list of required OCCT libraries



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
#pragma comment(lib, "TKG2d.lib")
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

	int bindId;
	int aisShapeBindId;
	int shapeType;
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
	OCCImpl() {

	}
	TopTools_IndexedMapOfShape _map_shape_int;
	opencascade::handle<AIS_InteractiveContext> ctx;


	ObjHandle getSelectedEdge() {
		auto objs = getSelectedObjectsList(TopAbs_ShapeEnum::TopAbs_EDGE);
		for (auto item : objs) {

			return item;

		}
		return ObjHandle();
	}

	void  GetSelectedEdges(std::vector<ObjHandle>& list) {
		auto objs = getSelectedObjectsList(TopAbs_ShapeEnum::TopAbs_EDGE);
		for (auto item : objs) {

			list.push_back(item);

		}
	}

	void  GetSelectedVertices(std::vector<ObjHandle>& list) {
		auto objs = getSelectedObjectsList(TopAbs_ShapeEnum::TopAbs_VERTEX);
		for (auto item : objs) {

			list.push_back(item);

		}
	}

	std::vector<double> IteratePoly(ObjHandle h) {
		auto obj = findObject(h);
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
				NCollection_Array1<gp_Dir> aNormals(1, aTr->NbNodes());

				for (Standard_Integer i = 1; i < aTr->NbNodes() + 1; i++) {
					aPoints(i) = aTr->Node(i).Transformed(aLocation);
					aNormals(i) = aTr->Normal(i).Transformed(aLocation);
				}


				Standard_Integer nnn = aTr->NbTriangles();
				Standard_Integer nt, n1, n2, n3;

				for (nt = 1; nt < nnn + 1; nt++)
				{

					triangles(nt).Get(n1, n2, n3);
					gp_Pnt aPnt1 = aPoints(n1);
					gp_Pnt aPnt2 = aPoints(n2);
					gp_Pnt aPnt3 = aPoints(n3);

					gp_Dir aDir1 = aNormals(n1);
					gp_Dir aDir2 = aNormals(n2);
					gp_Dir aDir3 = aNormals(n3);

					/*gp_Pnt2d uv1 = uvNodes(n1);
					gp_Pnt2d uv2 = uvNodes(n2);
					gp_Pnt2d uv3 = uvNodes(n3);*/

					//QVector3D p1, p2, p3;

					//if (faceOrientation == TopAbs_Orientation::TopAbs_FORWARD)
					{
						ret.push_back(aPnt1.X());
						ret.push_back(aPnt1.Y());
						ret.push_back(aPnt1.Z());

						ret.push_back(aDir1.X());
						ret.push_back(aDir1.Y());
						ret.push_back(aDir1.Z());

						ret.push_back(aPnt2.X());
						ret.push_back(aPnt2.Y());
						ret.push_back(aPnt2.Z());

						ret.push_back(aDir2.X());
						ret.push_back(aDir2.Y());
						ret.push_back(aDir2.Z());

						ret.push_back(aPnt3.X());
						ret.push_back(aPnt3.Y());
						ret.push_back(aPnt3.Z());

						ret.push_back(aDir3.X());
						ret.push_back(aDir3.Y());
						ret.push_back(aDir3.Z());

						/*p1 = QVector3D(aPnt1.X(), aPnt1.Y(), aPnt1.Z());
						p2 = QVector3D(aPnt2.X(), aPnt2.Y(), aPnt2.Z());
						p3 = QVector3D(aPnt3.X(), aPnt3.Y(), aPnt3.Z());*/
					}
					/*else
					{
						//p1 = QVector3D(aPnt3.X(), aPnt3.Y(), aPnt3.Z());
						//p2 = QVector3D(aPnt2.X(), aPnt2.Y(), aPnt2.Z());
						//p3 = QVector3D(aPnt1.X(), aPnt1.Y(), aPnt1.Z());
						ret.push_back(aPnt3.X());
						ret.push_back(aPnt3.Y());
						ret.push_back(aPnt3.Z());

						ret.push_back(aPnt2.X());
						ret.push_back(aPnt2.Y());
						ret.push_back(aPnt2.Z());

						ret.push_back(aPnt1.X());
						ret.push_back(aPnt1.Y());
						ret.push_back(aPnt1.Z());
					}*/


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

	std::vector<ObjHandle> getSelectedObjectsList(std::optional<TopAbs_ShapeEnum> type = std::nullopt) {
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

			if (!type.has_value() || shape.ShapeType() == type.value())
			{
				if (_map_shape_int.Contains(shape)) {
					h.bindId = _map_shape_int.FindIndex(shape);
				}
				else {

					h.bindId = _map_shape_int.Add(shape);
				}
				TopoDS_TShape* ptshape = shape.TShape().get();

				Handle(AIS_InteractiveObject) selected = ctx->SelectedInteractive();
				Handle(AIS_InteractiveObject) self = ctx->SelectedInteractive();
				//h.handle = (unsigned __int64)(self.get());
				//h.handleT = (unsigned __int64)(ptshape);
				ret.push_back(h);
			}
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
			h.shapeType = shape.ShapeType();

			TopoDS_TShape* ptshape = shape.TShape().get();

			Handle(AIS_InteractiveObject) selected = ctx->SelectedInteractive();
			Handle(AIS_InteractiveObject) self = ctx->SelectedInteractive();

			if (_map_shape_int.Contains(shape)) {
				h.bindId = _map_shape_int.FindIndex(shape);
			}
			else {

				h.bindId = _map_shape_int.Add(shape);
			}
			//h.handle = (unsigned __int64)(self.get());
			//h.handleT = (unsigned __int64)(ptshape);
			//h.handleF = (unsigned __int64)(&shape);
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
		//	h.handle = (unsigned __int64)(self.get());
		//	h.handleT = (unsigned __int64)(ptshape);
		//	h.handleF = (unsigned __int64)(&shape);
			break;
		}
		return h;
	}


	void CreateOcafDoc() {
		// 1. Create a document and obtain the main label (root)
		/*Handle(TDocStd_Document) doc = new TDocStd_Document("MyDocument");
		Handle(TDF_Label) rootLabel = doc->Main();

		// 2. Get the ShapeTool from the document
		Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(doc->Main());

		// 2. Get the ShapeTool from the document
		Handle(XCAFDoc_ShapeTool) shapeTool = XCAFDoc_DocumentTool::ShapeTool(doc->Main());

		// 3. Create some geometric shapes (e.g., using ModelingData module)
		TopoDS_Shape boxShape = BRepBuilderAPI_MakeBox(gp_Ax2(), 10, 10, 10).Shape();

		// 4. Create a label for the shape
		TDF_Label boxLabel;
		shapeTool->AddShape(boxShape, boxLabel); // Add the shape to the framework

		// 5. Assign a name to the shape's label
		TDF_Label nameLabel;
		shapeTool->SetName(boxLabel, "MyBox");
		*/
		// 6. Assign a color to the shape's label
		// (Details for color management would be in separate code)

		/*TopoDS_Shape shape;
		int shape_id = 123;
		_map_shape_int.Bind(shape, shape_id);
		int back_shape_id = 0;
		if (_map_shape_int.IsBound(shape)) {
			back_shape_id = _map_shape_int(shape);
		}*/

	}
	// Assuming 'theLabel' is a TDF_Label and 'aShapeTool' is a Handle(XCAFDoc_ShapeTool)
	// and 'shape' is the TopoDS_Shape (e.g., TopoDS_Edge) you are looking for.

	TCollection_AsciiString findName(const TDF_Label& theLabel, Handle(XCAFDoc_ShapeTool) aShapeTool, const TopoDS_Shape& shape) {
		for (TDF_ChildIterator Father(theLabel, Standard_True); Father.More(); Father.Next()) {
			TDF_Label Child = Father.Value();
			if (aShapeTool->IsShape(Child)) {
				TopoDS_Shape aShape = aShapeTool->GetShape(Child);
				if (shape.IsSame(aShape)) {
					TCollection_AsciiString node;
					TDF_Tool::Entry(Child, node); // Get the entry string (tag list) of the label
					return node;
				}
			}
		}
		return "null"; // Or handle not found case appropriately
	}
	/*AIS_InteractiveObject* getObject(const ObjHandle& handle) const {
		return reinterpret_cast<AIS_InteractiveObject*> (handle.handle);
	}*/

	void setAisCtx(opencascade::handle<AIS_InteractiveContext> _ctx) {

		ctx = opencascade::handle<AIS_InteractiveContext>(_ctx);
	}
	const TopoDS_Shape& findShape(const ObjHandle& handle) const {
		auto theIndex = handle.bindId;
		if (theIndex < 1 || theIndex > _map_shape_int.Extent())
			return {};

		return _map_shape_int.FindKey(handle.bindId);
	}

	opencascade::handle<AIS_InteractiveObject> OCCImpl::findObject(const ObjHandle& handle) const {
		return findObject(handle.bindId);
	}

	opencascade::handle<AIS_InteractiveObject> OCCImpl::findObject(int bindId) const {

		AIS_ListOfInteractive aList;
		ctx->DisplayedObjects(aList);
		AIS_ListIteratorOfListOfInteractive it(aList);
		//iterate on list:
		while (it.More())
		{
			auto sin = it.Value();

			Handle(AIS_Shape) aAIS_Shape = Handle(AIS_Shape)::DownCast(sin);
			if (!aAIS_Shape.IsNull()) {
				TopoDS_Shape aTopoDS_Shape = aAIS_Shape->Shape();
				// Use aTopoDS_Shape here

				if (_map_shape_int.Contains(aTopoDS_Shape)) {
					if (_map_shape_int.FindIndex(aTopoDS_Shape) == bindId)
						return sin;
				}
			}

			//do something with the current item : it.Value ()
			it.Next();
		}
		return {};
		//return reinterpret_cast<AIS_InteractiveObject*> (handle.handle);
	}

	/*TopoDS_Shape* getShapeFromObject(const ObjHandle& handle) const {
		return reinterpret_cast<TopoDS_Shape*> (handle.handleF);
	}*/

	TopoDS_Shape MakeBoolDiff(ObjHandle h1, ObjHandle h2, bool fixShape = true) {
		auto obj1 = findObject(h1);
		auto obj2 = findObject(h2);

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
		//const auto* obj1 = getObject(h1);
		auto obj1 = findObject(h1);
		auto obj2 = findObject(h2);
		//const auto* obj2 = getObject(h2);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(obj1)->Shape();
		auto trsf = obj1->Transformation();
		shape0 = BRepBuilderAPI_Transform(shape0, trsf, Standard_True);

		//shape0 = shape0.Located(obj1->LocalTransformation());
		TopoDS_Shape shape1 = Handle(AIS_Shape)::DownCast(obj2)->Shape();
		//	shape1 = shape1.Located(obj2->LocalTransformation());
		auto trsf2 = obj2->Transformation();
		shape1 = BRepBuilderAPI_Transform(shape1, trsf2, Standard_True);


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
		auto obj1 = findObject(h1);
		auto obj2 = findObject(h2);

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

