#include <iostream>
#include <optional>
#include <msclr/marshal_cppstd.h> // For marshal_as
#include <string>
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



#include <AIS_ViewController.hxx>

#include "imgui/imgui_impl_win32.h"
#include "imgui/imgui_impl_opengl3.h"
#include "imgui/imgui_impl_glfw.h"

#include <GLFW/glfw3.h>
#define GLFW_EXPOSE_NATIVE_WIN32
#define GLFW_EXPOSE_NATIVE_WGL
#define GLFW_NATIVE_INCLUDE_NONE
#include <GLFW/glfw3native.h>




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


using namespace Cascade::Common;
using namespace OpenTK::Mathematics;
using namespace System::Collections::Generic;

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

public ref class EdgeInfo {
public:
	CurveType CurveType;
	Vector3d COM;//center of mass
	Vector3d Start;
	Vector3d End;
	double Length;
	int  BindId;
	int  AisShapeBindId;
};

public ref class CircleEdgeInfo : EdgeInfo {
public:
	double Radius;
};

public ref class SurfInfo {
public:
	Vector3d Position;
	Vector3d COM;//center of mass
	int BindId;
	int AisShapeBindId;//parent
};

public ref class VertInfo {
public:
	Vector3d Position;

	int BindId;
	int AisShapeBindId;//parent
};

public ref class PlaneSurfInfo : SurfInfo {
public:
	Vector3d Normal;
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
	Vector3d Axis;
};

public ref class SphereSurfInfo : SurfInfo {
public:
	double Radius;
};

public ref class ManagedObjHandle {//todo make two diffrent handles for top entity and all anothers 
public:

	int BindId;
	int AisShapeBindId;//parent
	int ShapeType;

	void FromObjHandle(ObjHandle h) {

		BindId = h.bindId;
		AisShapeBindId = h.aisShapeBindId;
		ShapeType = h.shapeType;
	}

	ObjHandle ToObjHandle() {
		ObjHandle h;
		h.bindId = BindId;

		h.shapeType = ShapeType;
		h.aisShapeBindId = AisShapeBindId;

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



struct GLFWwindow;

//! GLFWwindow wrapper implementing Aspect_Window interface.
class GlfwOcctWindow : public Aspect_Window
{
	DEFINE_STANDARD_RTTI_INLINE(GlfwOcctWindow, Aspect_Window)
public:
	//! Main constructor.
	GlfwOcctWindow(int theWidth, int theHeight, const TCollection_AsciiString& theTitle);
	GlfwOcctWindow(int theWidth, int theHeight, void* wnd, const TCollection_AsciiString& theTitle);

	//! Close the window.
	virtual ~GlfwOcctWindow() { Close(); }

	//! Close the window.
	void Close();

	//! Return X Display connection.
	const Handle(Aspect_DisplayConnection)& GetDisplay() const { return myDisplay; }

	//! Return GLFW window.
	GLFWwindow* getGlfwWindow() { return myGlfwWindow; }

	//! Return native OpenGL context.
	Aspect_RenderingContext NativeGlContext() const;

	//! Return cursor position.
	Graphic3d_Vec2i CursorPosition() const;

public:

	//! Returns native Window handle
	virtual Aspect_Drawable NativeHandle() const Standard_OVERRIDE;

	//! Returns parent of native Window handle.
	virtual Aspect_Drawable NativeParentHandle() const Standard_OVERRIDE { return 0; }

	//! Applies the resizing to the window <me>
	virtual Aspect_TypeOfResize DoResize() Standard_OVERRIDE;

	//! Returns True if the window <me> is opened and False if the window is closed.
	virtual Standard_Boolean IsMapped() const Standard_OVERRIDE;

	//! Apply the mapping change to the window <me> and returns TRUE if the window is mapped at screen.
	virtual Standard_Boolean DoMapping() const Standard_OVERRIDE { return Standard_True; }

	//! Opens the window <me>.
	virtual void Map() const Standard_OVERRIDE;

	//! Closes the window <me>.
	virtual void Unmap() const Standard_OVERRIDE;

	virtual void Position(Standard_Integer& theX1, Standard_Integer& theY1,
		Standard_Integer& theX2, Standard_Integer& theY2) const Standard_OVERRIDE
	{
		theX1 = myXLeft;
		theX2 = myXRight;
		theY1 = myYTop;
		theY2 = myYBottom;
	}

	//! Returns The Window RATIO equal to the physical WIDTH/HEIGHT dimensions.
	virtual Standard_Real Ratio() const Standard_OVERRIDE
	{
		return Standard_Real(myXRight - myXLeft) / Standard_Real(myYBottom - myYTop);
	}

	//! Return window size.
	virtual void Size(Standard_Integer& theWidth, Standard_Integer& theHeight) const Standard_OVERRIDE
	{
		theWidth = myXRight - myXLeft;
		theHeight = myYBottom - myYTop;
	}

	virtual Aspect_FBConfig NativeFBConfig() const Standard_OVERRIDE { return NULL; }

protected:
	Handle(Aspect_DisplayConnection) myDisplay;
	GLFWwindow* myGlfwWindow;
	Standard_Integer myXLeft;
	Standard_Integer myYTop;
	Standard_Integer myXRight;
	Standard_Integer myYBottom;
};
// ================================================================
// Function : NativeGlContext
// Purpose  :
// ================================================================
Aspect_RenderingContext GlfwOcctWindow::NativeGlContext() const
{
#if defined (__APPLE__)
	return (NSOpenGLContext*)glfwGetNSGLContext(myGlfwWindow);
#elif defined (_WIN32)
	return glfwGetWGLContext(myGlfwWindow);
#else
	return glfwGetGLXContext(myGlfwWindow);
#endif
}
GlfwOcctWindow::GlfwOcctWindow(int theWidth, int theHeight, void* wnd, const TCollection_AsciiString& theTitle)
	: myGlfwWindow((GLFWwindow*)wnd),
	myXLeft(0),
	myYTop(0),
	myXRight(0),
	myYBottom(0)
{
	if (myGlfwWindow != nullptr)
	{
		int aWidth = 0, aHeight = 0;
		glfwGetWindowPos(myGlfwWindow, &myXLeft, &myYTop);
		glfwGetWindowSize(myGlfwWindow, &aWidth, &aHeight);
		myXRight = myXLeft + aWidth;
		myYBottom = myYTop + aHeight;

	}
}

GlfwOcctWindow::GlfwOcctWindow(int theWidth, int theHeight, const TCollection_AsciiString& theTitle)
	: myGlfwWindow(glfwCreateWindow(theWidth, theHeight, theTitle.ToCString(), NULL, NULL)),
	myXLeft(0),
	myYTop(0),
	myXRight(0),
	myYBottom(0)
{
	if (myGlfwWindow != nullptr)
	{
		int aWidth = 0, aHeight = 0;
		glfwGetWindowPos(myGlfwWindow, &myXLeft, &myYTop);
		glfwGetWindowSize(myGlfwWindow, &aWidth, &aHeight);
		myXRight = myXLeft + aWidth;
		myYBottom = myYTop + aHeight;

#if !defined(_WIN32) && !defined(__APPLE__)
		myDisplay = new Aspect_DisplayConnection((Aspect_XDisplay*)glfwGetX11Display());
#endif
	}
}

// ================================================================
// Function : Close
// Purpose  :
// ================================================================
void GlfwOcctWindow::Close()
{
	if (myGlfwWindow != nullptr)
	{
		glfwDestroyWindow(myGlfwWindow);
		myGlfwWindow = nullptr;
	}
}


// ================================================================
// Function : IsMapped
// Purpose  :
// ================================================================
Standard_Boolean GlfwOcctWindow::IsMapped() const
{
	return glfwGetWindowAttrib(myGlfwWindow, GLFW_VISIBLE) != 0;
}


// ================================================================
// Function : DoResize
// Purpose  :
// ================================================================
Aspect_TypeOfResize GlfwOcctWindow::DoResize()
{
	if (glfwGetWindowAttrib(myGlfwWindow, GLFW_VISIBLE) == 1)
	{
		int anXPos = 0, anYPos = 0, aWidth = 0, aHeight = 0;
		glfwGetWindowPos(myGlfwWindow, &anXPos, &anYPos);
		glfwGetWindowSize(myGlfwWindow, &aWidth, &aHeight);
		myXLeft = anXPos;
		myXRight = anXPos + aWidth;
		myYTop = anYPos;
		myYBottom = anYPos + aHeight;
	}
	return Aspect_TOR_UNKNOWN;
}

// ================================================================
// Function : Map
// Purpose  :
// ================================================================
void GlfwOcctWindow::Map() const
{
	glfwShowWindow(myGlfwWindow);
}

// ================================================================
// Function : NativeHandle
// Purpose  :
// ================================================================
Aspect_Drawable GlfwOcctWindow::NativeHandle() const
{
#if defined (__APPLE__)
	return (Aspect_Drawable)glfwGetCocoaWindow(myGlfwWindow);
#elif defined (_WIN32)
	return (Aspect_Drawable)glfwGetWin32Window(myGlfwWindow);
#else
	return (Aspect_Drawable)glfwGetX11Window(myGlfwWindow);
#endif
}
// ================================================================
// Function : Unmap
// Purpose  :
// ================================================================
void GlfwOcctWindow::Unmap() const
{
	glfwHideWindow(myGlfwWindow);
}

// ================================================================
// Function : CursorPosition
// Purpose  :
// ================================================================
Graphic3d_Vec2i GlfwOcctWindow::CursorPosition() const
{
	Graphic3d_Vec2d aPos;
	glfwGetCursorPos(myGlfwWindow, &aPos.x(), &aPos.y());
	return Graphic3d_Vec2i((int)aPos.x(), (int)aPos.y());
}


//! Sample class creating 3D Viewer within GLFW window.
class GlfwOcctView : protected AIS_ViewController
{
public:
	//! Default constructor.
	GlfwOcctView();

	//! Destructor.
	~GlfwOcctView();

	void setAisCtx(opencascade::handle<AIS_InteractiveContext> _ctx) {

		myContext = opencascade::handle<AIS_InteractiveContext>(_ctx);
	}
	//! Clean up before .
	void cleanup();
	//! Main application entry point.
	void run();

	void run(void* wnd);
	void runWnt(IntPtr wnd, IntPtr glctx);
	void runOpenTk(IntPtr wnd, IntPtr glctx);
	void iterate();
	void MouseMove(double thePosX, double thePosY);
	void MouseDown(int btn, double thePosX, double thePosY);
	void MouseScroll(int x, int y, int offset);
	void MouseUp(int btn, double thePosX, double thePosY);
	bool ImGuiMouseUp(int btn, double thePosX, double thePosY);
	bool ImGuiMouseDown(int btn, double thePosX, double thePosY);
	//! Window resize event.
	void onResize(int theWidth, int theHeight);
	bool showTriangle;
	void StartRenderGui();
	void EndRenderGui();
	void ShowDemoWindow();
	void Begin(const char* text);
	void Text(const char* text);
	bool Button(const char* text);
	bool Checkbox(const char* text, bool state);
	void SetNextWindowSizeConstraints(int minx, int miny, int maxx, int maxy);
	void End();

public:

	//! Create GLFW window.
	void initWindow(int theWidth, int theHeight, const char* theTitle);
	void initWindow(int theWidth, int theHeight, void* wnd, const char* theTitle);

	//! Create 3D Viewer.
	void initViewer();
	void initViewer(WNT_Window* wnd, void* glctx);
	void initViewer(void* glctx);
	void ShowStats(bool status);

	//! Init ImGui.
	void initGui();
	void initGui(void* wnd);

	//! Render ImGUI.
	void renderGui();

	//! Fill 3D Viewer with a DEMO items.
	void initDemoScene();

	//! Application event loop.
	void mainloop();



	//! Handle view redraw.
	void handleViewRedraw(const Handle(AIS_InteractiveContext)& theCtx,
		const Handle(V3d_View)& theView) override;

	//! @name GLWF callbacks

private:

	//! Mouse scroll event.
	void onMouseScroll(double theOffsetX, double theOffsetY);

	//! Mouse click event.
	void onMouseButton(int theButton, int theAction, int theMods);

	//! Mouse move event.
	void onMouseMove(int thePosX, int thePosY);

	//! @name GLWF callbacks (static functions)
private:

	//! GLFW callback redirecting messages into Message::DefaultMessenger().
	static void errorCallback(int theError, const char* theDescription);

	//! Wrapper for glfwGetWindowUserPointer() returning this class instance.
	static GlfwOcctView* toView(GLFWwindow* theWin);

	//! Window resize callback.
	static void onResizeCallback(GLFWwindow* theWin, int theWidth, int theHeight)
	{
		toView(theWin)->onResize(theWidth, theHeight);
	}

	//! Frame-buffer resize callback.
	static void onFBResizeCallback(GLFWwindow* theWin, int theWidth, int theHeight)
	{
		toView(theWin)->onResize(theWidth, theHeight);
	}

	//! Mouse scroll callback.
	static void onMouseScrollCallback(GLFWwindow* theWin, double theOffsetX, double theOffsetY)
	{
		toView(theWin)->onMouseScroll(theOffsetX, theOffsetY);
	}

	//! Mouse click callback.
	static void onMouseButtonCallback(GLFWwindow* theWin, int theButton, int theAction, int theMods)
	{
		toView(theWin)->onMouseButton(theButton, theAction, theMods);
	}

	//! Mouse move callback.
	static void onMouseMoveCallback(GLFWwindow* theWin, double thePosX, double thePosY)
	{
		toView(theWin)->onMouseMove((int)thePosX, (int)thePosY);
	}

public:

	Handle(GlfwOcctWindow) myOcctWindow;
	Handle(V3d_View) myView;
	Handle(V3d_Viewer) myViewer;
	Handle(AIS_InteractiveContext) myContext;
	bool myToWaitEvents = true;

};
void GlfwOcctView::initWindow(int theWidth, int theHeight, const char* theTitle)
{
	return;

	glfwSetErrorCallback(GlfwOcctView::errorCallback);
	glfwInit();
	const bool toAskCoreProfile = true;
	if (toAskCoreProfile)
	{
		glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
		glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
#if defined (__APPLE__)
		glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif
		glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
		//glfwWindowHint(GLFW_TRANSPARENT_FRAMEBUFFER, true);
		//glfwWindowHint(GLFW_DECORATED, GL_FALSE);
	}
	myOcctWindow = new GlfwOcctWindow(theWidth, theHeight, theTitle);
	glfwSetWindowUserPointer(myOcctWindow->getGlfwWindow(), this);


	// window callback
	glfwSetWindowSizeCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onResizeCallback);
	glfwSetFramebufferSizeCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onFBResizeCallback);
	// mouse callback
	glfwSetScrollCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseScrollCallback);
	glfwSetMouseButtonCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseButtonCallback);
	glfwSetCursorPosCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseMoveCallback);
}

// ================================================================
// Function : initWindow
// Purpose  :
// ================================================================
void GlfwOcctView::initWindow(int theWidth, int theHeight, void* wnd, const char* theTitle)
{
	glfwSetErrorCallback(GlfwOcctView::errorCallback);
	glfwInit();
	const bool toAskCoreProfile = true;
	if (toAskCoreProfile)
	{
		glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
		glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
#if defined (__APPLE__)
		glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif
		glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
		//glfwWindowHint(GLFW_TRANSPARENT_FRAMEBUFFER, true);
		//glfwWindowHint(GLFW_DECORATED, GL_FALSE);
	}
	myOcctWindow = new GlfwOcctWindow(theWidth, theHeight, wnd, theTitle);
	glfwSetWindowUserPointer(myOcctWindow->getGlfwWindow(), this);

	// window callback
	glfwSetWindowSizeCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onResizeCallback);
	glfwSetFramebufferSizeCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onFBResizeCallback);

	// mouse callback
	glfwSetScrollCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseScrollCallback);
	glfwSetMouseButtonCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseButtonCallback);
	glfwSetCursorPosCallback(myOcctWindow->getGlfwWindow(), GlfwOcctView::onMouseMoveCallback);
}



// ================================================================
// Function : GlfwOcctView
// Purpose  :
// ================================================================
GlfwOcctView::GlfwOcctView()
{
}

// ================================================================
// Function : ~GlfwOcctView
// Purpose  :
// ================================================================
GlfwOcctView::~GlfwOcctView()
{
}

// ================================================================
// Function : errorCallback
// Purpose  :
// ================================================================
void GlfwOcctView::errorCallback(int theError, const char* theDescription)
{
	Message::DefaultMessenger()->Send(TCollection_AsciiString("Error") + theError + ": " + theDescription, Message_Fail);
}
// ================================================================
// Function : toView
// Purpose  :
// ================================================================
GlfwOcctView* GlfwOcctView::toView(GLFWwindow* theWin)
{
	return static_cast<GlfwOcctView*>(glfwGetWindowUserPointer(theWin));
}
// ================================================================
// Function : run
// Purpose  :
// ================================================================
void GlfwOcctView::run()
{
	initWindow(800, 600, "OCCT IMGUI");
	initViewer();
	initDemoScene();
	if (myView.IsNull())
	{
		return;
	}

	myView->MustBeResized();
	myOcctWindow->Map();
	initGui();
	//mainloop();
	//cleanup();
}

void GlfwOcctView::run(void* wnd)
{
	initWindow(800, 600, wnd, "OCCT IMGUI");
	initViewer();
	initDemoScene();
	if (myView.IsNull())
	{
		return;
	}

	myView->MustBeResized();
	myOcctWindow->Map();
	initGui();
	//mainloop();
	//cleanup();
}

void GlfwOcctView::runOpenTk(IntPtr wnd, IntPtr glctx)
{
	initWindow(800, 600, wnd.ToPointer(), "OCCT IMGUI");

	initViewer(glctx.ToPointer());
	initDemoScene();
	if (myView.IsNull())
		return;


	myView->MustBeResized();
	//myOcctWindow->Map();
	initGui();
	//mainloop();
	//cleanup();
}

void GlfwOcctView::runWnt(IntPtr wnd, IntPtr glctx)
{
	initWindow(800, 600, "OCCT IMGUI");
	Handle(WNT_Window) pWNTWindow = new WNT_Window(reinterpret_cast<HWND> (wnd.ToPointer()));

	initViewer(pWNTWindow.get(), glctx.ToPointer());
	initDemoScene();
	if (myView.IsNull())
		return;


	myView->MustBeResized();
	//myOcctWindow->Map();
	initGui(wnd.ToPointer());
	//mainloop();
	//cleanup();
}



// ================================================================
// Function : initViewer
// Purpose  :
// ================================================================
void GlfwOcctView::initViewer()
{
	if (myOcctWindow.IsNull()
		|| myOcctWindow->getGlfwWindow() == nullptr)
	{
		return;
	}

	Handle(OpenGl_GraphicDriver) aGraphicDriver
		= new OpenGl_GraphicDriver(myOcctWindow->GetDisplay(), Standard_False);
	aGraphicDriver->SetBuffersNoSwap(Standard_True);

	Handle(V3d_Viewer) aViewer = new V3d_Viewer(aGraphicDriver);
	aViewer->SetDefaultLights();
	aViewer->SetLightOn();
	aViewer->SetDefaultTypeOfView(V3d_PERSPECTIVE);
	aViewer->ActivateGrid(Aspect_GT_Rectangular, Aspect_GDM_Lines);
	myView = aViewer->CreateView();
	//myView->SetImmediateUpdate(Standard_False);
	auto ctx = myOcctWindow->NativeGlContext();
	myView->SetWindow(myOcctWindow, ctx);
	myView->ChangeRenderingParams().ToShowStats = Standard_True;

	myContext = new AIS_InteractiveContext(aViewer);

	Handle(AIS_ViewCube) aCube = new AIS_ViewCube();
	aCube->SetSize(55);
	aCube->SetFontHeight(12);
	aCube->SetAxesLabels("", "", "");
	aCube->SetTransformPersistence(new Graphic3d_TransformPers(Graphic3d_TMF_TriedronPers, Aspect_TOTP_LEFT_LOWER, Graphic3d_Vec2i(100, 100)));
	aCube->SetViewAnimation(this->ViewAnimation());
	aCube->SetFixedAnimationLoop(false);
	myContext->Display(aCube, false);
}

void GlfwOcctView::initViewer(WNT_Window* wnd, void* glctx)
{

	Handle(Aspect_DisplayConnection) aDisplayConnection;
	Handle(OpenGl_GraphicDriver) aGraphicDriver
		= new OpenGl_GraphicDriver(aDisplayConnection);

	aGraphicDriver->SetBuffersNoSwap(Standard_True);

	Handle(V3d_Viewer) aViewer = new V3d_Viewer(aGraphicDriver);
	aViewer->SetDefaultLights();
	aViewer->SetLightOn();
	aViewer->SetDefaultTypeOfView(V3d_PERSPECTIVE);
	aViewer->ActivateGrid(Aspect_GT_Rectangular, Aspect_GDM_Lines);
	myView = aViewer->CreateView();
	//myView->SetImmediateUpd/ate(Standard_False);

	myView->SetWindow(wnd, glctx);
	myView->ChangeRenderingParams().ToShowStats = Standard_True;

	myContext = new AIS_InteractiveContext(aViewer);

	Handle(AIS_ViewCube) aCube = new AIS_ViewCube();
	aCube->SetSize(55);
	aCube->SetFontHeight(12);
	aCube->SetAxesLabels("", "", "");
	aCube->SetTransformPersistence(new Graphic3d_TransformPers(Graphic3d_TMF_TriedronPers, Aspect_TOTP_LEFT_LOWER, Graphic3d_Vec2i(100, 100)));
	aCube->SetViewAnimation(this->ViewAnimation());
	aCube->SetFixedAnimationLoop(false);
	myContext->Display(aCube, false);
}

void GlfwOcctView::ShowStats(bool status)
{
	myView->ChangeRenderingParams().ToShowStats = status;
}

void GlfwOcctView::initViewer(void* glctx)
{

	Handle(Aspect_DisplayConnection) aDisplayConnection;
	Handle(OpenGl_GraphicDriver) aGraphicDriver
		= new OpenGl_GraphicDriver(aDisplayConnection);

	aGraphicDriver->SetBuffersNoSwap(Standard_True);

	Handle(V3d_Viewer) aViewer = new V3d_Viewer(aGraphicDriver);
	myViewer = aViewer;
	aViewer->SetDefaultLights();
	aViewer->SetLightOn();
	//aViewer->SetDefaultTypeOfView(V3d_PERSPECTIVE);
	aViewer->ActivateGrid(Aspect_GT_Rectangular, Aspect_GDM_Lines);
	myView = aViewer->CreateView();
	//myView->SetImmediateUpd/ate(Standard_False);

	myView->SetWindow(myOcctWindow, glctx);
	//myView->ChangeRenderingParams().ToShowStats = Standard_True;

	myContext = new AIS_InteractiveContext(aViewer);

	Handle(AIS_ViewCube) aCube = new AIS_ViewCube();
	aCube->SetSize(55);
	aCube->SetFontHeight(12);
	aCube->SetAxesLabels("", "", "");
	aCube->SetTransformPersistence(new Graphic3d_TransformPers(Graphic3d_TMF_TriedronPers, Aspect_TOTP_LEFT_LOWER, Graphic3d_Vec2i(100, 100)));
	aCube->SetViewAnimation(this->ViewAnimation());
	aCube->SetFixedAnimationLoop(false);



	myContext->Display(aCube, false);
}

void GlfwOcctView::initGui(void* wnd)
{
	IMGUI_CHECKVERSION();
	ImGui::CreateContext();

	ImGuiIO& aIO = ImGui::GetIO();
	aIO.ConfigFlags |= ImGuiConfigFlags_DockingEnable;
	//aIO.ConfigFlags |= ImGuiConfigFlags_ViewportsEnable;


	ImGui_ImplWin32_Init(wnd);
	ImGui_ImplOpenGL3_Init("#version 330");

	// Setup Dear ImGui style.
	//ImGui::StyleColorsClassic();
}

void GlfwOcctView::initGui()
{
	IMGUI_CHECKVERSION();
	ImGui::CreateContext();

	ImGuiIO& aIO = ImGui::GetIO();
	aIO.ConfigFlags |= ImGuiConfigFlags_DockingEnable;

	ImGui_ImplGlfw_InitForOpenGL(myOcctWindow->getGlfwWindow(), Standard_True);
	ImGui_ImplOpenGL3_Init("#version 330");

	// Setup Dear ImGui style.
	//ImGui::StyleColorsClassic();
}

bool show_dialog = true;

void GlfwOcctView::StartRenderGui()
{

	ImGuiIO& aIO = ImGui::GetIO();

	ImGui_ImplOpenGL3_NewFrame();
	//ImGui_ImplWin32_NewFrame();
	ImGui_ImplGlfw_NewFrame();

	ImGui::NewFrame();

}

void GlfwOcctView::EndRenderGui()
{
	ImGui::Render();

	ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());

	//glfwSwapBuffers(myOcctWindow->getGlfwWindow());
}

void GlfwOcctView::ShowDemoWindow()
{
	ImGui::ShowDemoWindow();
}

void GlfwOcctView::Begin(const char* text)
{
	ImGui::Begin(text);
}

bool GlfwOcctView::Button(const char* text)
{
	return ImGui::Button(text);
}

bool GlfwOcctView::Checkbox(const char* text, bool state)
{
	ImGui::Checkbox(text, &state);
	return state;
}

void GlfwOcctView::SetNextWindowSizeConstraints(int minx, int miny, int maxx, int maxy)
{
	ImGui::SetNextWindowSizeConstraints(ImVec2(minx, miny), ImVec2(maxx, maxy));

}

void GlfwOcctView::Text(const char* text)
{
	ImGui::Text(text);
}

void GlfwOcctView::End()
{
	ImGui::End();

}
void GlfwOcctView::renderGui()
{
	// Hello IMGUI.
	ImGui::SetNextWindowSizeConstraints(ImVec2(220, 120), ImVec2(350, 200));
	ImGui::Begin("Hello");
	ImGui::Text("Hello ImGui!");

	ImGui::Checkbox("show triangle", &showTriangle);

	ImGui::Text("Hello OpenCASCADE!");
	if (ImGui::Button("OK")) {
		show_dialog = true;
	}
	ImGui::SameLine();
	ImGui::Button("Cancel");
	ImGui::End();

	if (show_dialog) {
		// If using a regular window/dialog:
		if (ImGui::Begin("My Dialog", &show_dialog, ImGuiWindowFlags_AlwaysAutoResize))
		{
			ImGui::Text("This is my dialog content.");
			if (ImGui::Button("Cancel"))
			{
				show_dialog = false;
			}
			ImGui::SameLine();
			if (ImGui::Button("OK"))
			{
				// Handle OK action
				show_dialog = false;
			}

		}
		ImGui::End();
	}

}

// ================================================================
// Function : initDemoScene
// Purpose  :
// ================================================================
void GlfwOcctView::initDemoScene()
{
	if (myContext.IsNull())
	{
		return;
	}

	myView->TriedronDisplay(Aspect_TOTP_LEFT_LOWER, Quantity_NOC_GOLD, 0.08, V3d_WIREFRAME);

	gp_Ax2 anAxis;
	anAxis.SetLocation(gp_Pnt(0.0, 0.0, 0.0));
	Handle(AIS_Shape) aBox = new AIS_Shape(BRepPrimAPI_MakeBox(anAxis, 50, 50, 50).Shape());
	myContext->Display(aBox, AIS_Shaded, 0, false);
	anAxis.SetLocation(gp_Pnt(25.0, 125.0, 0.0));
	Handle(AIS_Shape) aCone = new AIS_Shape(BRepPrimAPI_MakeCone(anAxis, 25, 0, 50).Shape());
	myContext->Display(aCone, AIS_Shaded, 0, false);

	TCollection_AsciiString aGlInfo;
	{
		TColStd_IndexedDataMapOfStringString aRendInfo;
		myView->DiagnosticInformation(aRendInfo, Graphic3d_DiagnosticInfo_Basic);
		for (TColStd_IndexedDataMapOfStringString::Iterator aValueIter(aRendInfo); aValueIter.More(); aValueIter.Next())
		{
			if (!aGlInfo.IsEmpty()) { aGlInfo += "\n"; }
			aGlInfo += TCollection_AsciiString("  ") + aValueIter.Key() + ": " + aValueIter.Value();
		}
	}
	Message::DefaultMessenger()->Send(TCollection_AsciiString("OpenGL info:\n") + aGlInfo, Message_Info);
}

// ================================================================
// Function : handleViewRedraw
// Purpose  :
// ================================================================
void GlfwOcctView::handleViewRedraw(const Handle(AIS_InteractiveContext)& theCtx,
	const Handle(V3d_View)& theView)
{
	AIS_ViewController::handleViewRedraw(theCtx, theView);
	myToWaitEvents = !myToAskNextFrame;
}

// ================================================================
// Function : mainloop
// Purpose  :
// ================================================================
void GlfwOcctView::mainloop()
{
	while (!glfwWindowShouldClose(myOcctWindow->getGlfwWindow()))
	{
		// glfwPollEvents() for continuous rendering (immediate return if there are no new events)
		// and glfwWaitEvents() for rendering on demand (something actually happened in the viewer)
		if (myToWaitEvents)
		{
			glfwWaitEvents();
		}
		else
		{
			glfwPollEvents();
		}
		if (!myView.IsNull())
		{
			myView->InvalidateImmediate(); // redraw view even if it wasn't modified
			FlushViewEvents(myContext, myView, Standard_True);

			//renderGui();
		}
	}
}

void GlfwOcctView::MouseMove(double x, double y)
{
	onMouseMove(x, y);
}

void GlfwOcctView::MouseScroll(int x, int y, int theOffsetY) {
	ImGuiIO& aIO = ImGui::GetIO();
	if (myView.IsNull() || aIO.WantCaptureMouse)
	{
		aIO.AddMouseWheelEvent(0, theOffsetY / 120);
		return;
	}

	Graphic3d_Vec2i aPos(x, y);
	UpdateZoom(Aspect_ScrollDelta(aPos, int(theOffsetY / 8.0)));

}

bool GlfwOcctView::ImGuiMouseDown(int btn, double x, double y) {
	ImGuiIO& aIO = ImGui::GetIO();
	if (myView.IsNull() || aIO.WantCaptureMouse)
	{
		aIO.AddMousePosEvent((float)x, (float)y);
		aIO.AddMouseButtonEvent(btn - 1, true);

		return true;
	}
	return false;
}

void GlfwOcctView::MouseDown(int btn, double x, double y)
{
	if (ImGuiMouseDown(btn, x, y))
		return;

	Graphic3d_Vec2i aPos(x, y);
	auto bb = Aspect_VKeyMouse_LeftButton;
	switch (btn) {
	case 1:
		bb = Aspect_VKeyMouse_LeftButton;
		break;
	case 2:
		bb = Aspect_VKeyMouse_RightButton;
		break;
	case 3:
		bb = Aspect_VKeyMouse_MiddleButton;
		break;
	}
	PressMouseButton(aPos, bb, Aspect_VKeyFlags_NONE, false);
}

bool GlfwOcctView::ImGuiMouseUp(int btn, double x, double y)
{
	ImGuiIO& aIO = ImGui::GetIO();
	if (myView.IsNull() || aIO.WantCaptureMouse)
	{
		aIO.AddMousePosEvent((float)x, (float)y);
		aIO.AddMouseButtonEvent(btn - 1, false);
		return true;
	}
	return false;
}

void GlfwOcctView::MouseUp(int btn, double x, double y)
{
	if (ImGuiMouseUp(btn, x, y))
		return;

	Graphic3d_Vec2i aPos(x, y);
	auto bb = Aspect_VKeyMouse_LeftButton;

	switch (btn) {
	case 1:
		bb = Aspect_VKeyMouse_LeftButton;
		break;
	case 2:
		bb = Aspect_VKeyMouse_RightButton;
		break;
	case 3:
		bb = Aspect_VKeyMouse_MiddleButton;
		break;
	}
	ReleaseMouseButton(aPos, bb, Aspect_VKeyFlags_NONE, false);
}

void GlfwOcctView::iterate()
{
	// glfwPollEvents() for continuous rendering (immediate return if there are no new events)
	// and glfwWaitEvents() for rendering on demand (something actually happened in the viewer)
	if (myToWaitEvents)
	{
		//glfwWaitEvents();
	}
	else
	{
		//	glfwPollEvents();
	}
	if (!myView.IsNull())
	{
		myView->InvalidateImmediate(); // redraw view even if it wasn't modified
		FlushViewEvents(myContext, myView, Standard_True);

		//renderGui();
	}
}

// ================================================================
// Function : cleanup
// Purpose  :
// ================================================================
void GlfwOcctView::cleanup()
{
	// Cleanup IMGUI.
	//ImGui_ImplOpenGL3_Shutdown();
	//ImGui_ImplGlfw_Shutdown();
	//ImGui::DestroyContext();

	if (!myView.IsNull())
	{
		myView->Remove();
	}
	if (!myOcctWindow.IsNull())
	{
		myOcctWindow->Close();
	}

	glfwTerminate();
}

// ================================================================
// Function : onResize
// Purpose  :
// ================================================================
void GlfwOcctView::onResize(int theWidth, int theHeight)
{
	if (theWidth != 0
		&& theHeight != 0
		&& !myView.IsNull())
	{
		myView->Window()->DoResize();
		myView->MustBeResized();
		myView->Invalidate();
		FlushViewEvents(myContext, myView, true);
		//renderGui();
	}
}

// ================================================================
// Function : onMouseScroll
// Purpose  :
// ================================================================
void GlfwOcctView::onMouseScroll(double theOffsetX, double theOffsetY)
{
	ImGuiIO& aIO = ImGui::GetIO();
	if (!myView.IsNull() && !aIO.WantCaptureMouse)
	{
		UpdateZoom(Aspect_ScrollDelta(myOcctWindow->CursorPosition(), int(theOffsetY * 8.0)));
	}
}

//! Convert GLFW mouse button into Aspect_VKeyMouse.
static Aspect_VKeyMouse mouseButtonFromGlfw(int theButton)
{
	switch (theButton)
	{
	case GLFW_MOUSE_BUTTON_LEFT:   return Aspect_VKeyMouse_LeftButton;
	case GLFW_MOUSE_BUTTON_RIGHT:  return Aspect_VKeyMouse_RightButton;
	case GLFW_MOUSE_BUTTON_MIDDLE: return Aspect_VKeyMouse_MiddleButton;
	}
	return Aspect_VKeyMouse_NONE;
}

//! Convert GLFW key modifiers into Aspect_VKeyFlags.
static Aspect_VKeyFlags keyFlagsFromGlfw(int theFlags)
{
	Aspect_VKeyFlags aFlags = Aspect_VKeyFlags_NONE;
	if ((theFlags & GLFW_MOD_SHIFT) != 0)
	{
		aFlags |= Aspect_VKeyFlags_SHIFT;
	}
	if ((theFlags & GLFW_MOD_CONTROL) != 0)
	{
		aFlags |= Aspect_VKeyFlags_CTRL;
	}
	if ((theFlags & GLFW_MOD_ALT) != 0)
	{
		aFlags |= Aspect_VKeyFlags_ALT;
	}
	if ((theFlags & GLFW_MOD_SUPER) != 0)
	{
		aFlags |= Aspect_VKeyFlags_META;
	}
	return aFlags;
}
// ================================================================
// Function : onMouseButton
// Purpose  :
// ================================================================
void GlfwOcctView::onMouseButton(int theButton, int theAction, int theMods)
{
	//ImGuiIO& aIO = ImGui::GetIO();
	//if (myView.IsNull() || aIO.WantCaptureMouse)
	{
		//	return;
	}

	const Graphic3d_Vec2i aPos = myOcctWindow->CursorPosition();
	if (theAction == GLFW_PRESS)
	{
		PressMouseButton(aPos, mouseButtonFromGlfw(theButton), keyFlagsFromGlfw(theMods), false);
	}
	else
	{
		ReleaseMouseButton(aPos, mouseButtonFromGlfw(theButton), keyFlagsFromGlfw(theMods), false);
	}
}

// ================================================================
// Function : onMouseMove
// Purpose  :
// ================================================================
void GlfwOcctView::onMouseMove(int thePosX, int thePosY)
{
	if (myView.IsNull())
	{
		return;
	}

	ImGuiIO& aIO = ImGui::GetIO();
	aIO.AddMousePosEvent((float)thePosX, (float)thePosY);

	if (aIO.WantCaptureMouse)
	{
		myView->Redraw();
	}
	else
	{
		const Graphic3d_Vec2i aNewPos(thePosX, thePosY);
		UpdateMousePosition(aNewPos, PressedMouseButtons(), LastMouseFlags(), Standard_False);
	}
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

	void  GetDetectedVertices(std::vector<ObjHandle>& list) {
		auto objs = getDetectedObjectsList(TopAbs_ShapeEnum::TopAbs_VERTEX);
		for (auto item : objs) {

			list.push_back(item);

		}
	}
	std::vector<double> IteratePoly(ObjHandle h, bool useLocalTransform) {
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
			auto face = aExpFace.Current();
			if (useLocalTransform)
				face = face.Located(obj->LocalTransformation());

			TopoDS_Face aFace = TopoDS::Face(face);

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

	std::vector<ObjHandle> getDetectedObjectsList(std::optional<TopAbs_ShapeEnum> type = std::nullopt) {
		std::vector<ObjHandle> ret;

		for (ctx->InitDetected(); ctx->MoreDetected(); ctx->NextDetected())
		{
			ObjHandle h;
			Handle(SelectMgr_EntityOwner) owner = ctx->DetectedOwner();

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

				ret.push_back(h);
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


			if (_map_shape_int.Contains(shape)) {
				h.bindId = _map_shape_int.FindIndex(shape);
			}
			else {

				h.bindId = _map_shape_int.Add(shape);
			}

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


/// <summary>
/// Proxy class encapsulating calls to OCCT C++ classes within 
/// C++/CLI class visible from .Net (CSharp)
/// </summary>
public ref class OCCTProxy
{


public:

	void runOpenTk(IntPtr wnd, IntPtr glctx)
	{
		gview->initWindow(800, 600, wnd.ToPointer(), "OCCT IMGUI");

		gview->initViewer(glctx.ToPointer());

		initDemoScene();
		myView() = gview->myView;
		myViewer() = gview->myViewer;
		myAISContext() = gview->myContext;
		impl->setAisCtx(myAISContext());
		SetDefaultDrawerParams();
		SetDefaultGradient();
		if (myView().IsNull())
			return;


		myView()->MustBeResized();
		//myOcctWindow->Map();
		initGui();
		//mainloop();
		//cleanup();
	}
	void MouseMove(int x, int y) {
		gview->MouseMove(x, y);
	}
	void Resize(int x, int y) {
		gview->onResize(x, y);
	}

	void ShowStats(bool status) {
		gview->ShowStats(status);
	}

	void MouseDown(int btn, int x, int y) {
		gview->MouseDown(btn, x, y);
	}

	void MouseUp(int btn, int x, int y) {
		gview->MouseUp(btn, x, y);
	}

	bool ImGuiMouseUp(int btn, int x, int y) {
		return gview->ImGuiMouseUp(btn, x, y);
	}

	bool ImGuiMouseDown(int btn, int x, int y) {
		return gview->ImGuiMouseDown(btn, x, y);
	}

	void MouseScroll(int x, int y, int offset) {
		gview->MouseScroll(x, y, offset);
	}
	void iterate() {
		gview->iterate();
	}
	void StartRenderGui() {
		gview->StartRenderGui();
	}

	void EndRenderGui() {
		gview->EndRenderGui();
	}

	void ShowDemoWindow() {
		gview->ShowDemoWindow();
	}

	void Begin(System::String^ str) {

		// Convert to std::string
		std::string nativeString = msclr::interop::marshal_as<std::string>(str);

		// Get const char*
		const char* cstr_const = nativeString.c_str();
		gview->Begin(cstr_const);
	}

	void Text(System::String^ str) {
		// Convert to std::string
		std::string nativeString = msclr::interop::marshal_as<std::string>(str);

		// Get const char*
		const char* cstr_const = nativeString.c_str();
		gview->Text(cstr_const);
	}

	bool Button(System::String^ str) {
		// Convert to std::string
		std::string nativeString = msclr::interop::marshal_as<std::string>(str);

		// Get const char*
		const char* cstr_const = nativeString.c_str();
		return gview->Button(cstr_const);
	}

	bool Checkbox(System::String^ str, bool state) {
		// Convert to std::string
		std::string nativeString = msclr::interop::marshal_as<std::string>(str);

		// Get const char*
		const char* cstr_const = nativeString.c_str();
		return gview->Checkbox(cstr_const, state);
	}
	void SetNextWindowSizeConstraints(int minx, int miny, int maxx, int maxy) {

		return gview->SetNextWindowSizeConstraints(minx, miny, maxx, maxy);
	}

	void End() {
		gview->End();
	}
	void cleanup()
	{
		gview->cleanup();

	}
	void initDemoScene()
	{
		if (myAISContext().IsNull())
		{
			return;
		}

		myView()->TriedronDisplay(Aspect_TOTP_LEFT_LOWER, Quantity_NOC_GOLD, 0.08, V3d_WIREFRAME);

		gp_Ax2 anAxis;
		anAxis.SetLocation(gp_Pnt(0.0, 0.0, 0.0));
		Handle(AIS_Shape) aBox = new AIS_Shape(BRepPrimAPI_MakeBox(anAxis, 50, 50, 50).Shape());
		myAISContext()->Display(aBox, AIS_Shaded, 0, false);
		anAxis.SetLocation(gp_Pnt(25.0, 125.0, 0.0));
		Handle(AIS_Shape) aCone = new AIS_Shape(BRepPrimAPI_MakeCone(anAxis, 25, 0, 50).Shape());
		myAISContext()->Display(aCone, AIS_Shaded, 0, false);

		TCollection_AsciiString aGlInfo;
		{
			TColStd_IndexedDataMapOfStringString aRendInfo;
			myView()->DiagnosticInformation(aRendInfo, Graphic3d_DiagnosticInfo_Basic);
			for (TColStd_IndexedDataMapOfStringString::Iterator aValueIter(aRendInfo); aValueIter.More(); aValueIter.Next())
			{
				if (!aGlInfo.IsEmpty()) { aGlInfo += "\n"; }
				aGlInfo += TCollection_AsciiString("  ") + aValueIter.Key() + ": " + aValueIter.Value();
			}
		}
		Message::DefaultMessenger()->Send(TCollection_AsciiString("OpenGL info:\n") + aGlInfo, Message_Info);
	}

	void initGui()
	{
		IMGUI_CHECKVERSION();
		ImGui::CreateContext();

		ImGuiIO& aIO = ImGui::GetIO();
		aIO.ConfigFlags |= ImGuiConfigFlags_DockingEnable;

		ImGui_ImplGlfw_InitForOpenGL(gview->myOcctWindow->getGlfwWindow(), Standard_True);
		ImGui_ImplOpenGL3_Init("#version 330");

		// Setup Dear ImGui style.
		//ImGui::StyleColorsClassic();
	}

	static OCCImpl* impl = new OCCImpl();
	static GlfwOcctView* gview = new GlfwOcctView();
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

		impl->setAisCtx(myAISContext());
		gview->setAisCtx(myAISContext());

		myAISContext()->UpdateCurrentViewer();
		myView()->Redraw();
		myView()->MustBeResized();
		SetDefaultDrawerParams();
		return true;
	}
	/// <summary>
	///Initialize a viewer
	/// </summary>
	/// <param name="theWnd">System.IntPtr that contains the window handle (HWND) of the control</param>
	bool InitViewer2(System::IntPtr glctx)
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



		myView()->SetBgGradientColors(Quantity_Color(0.5, 0.5, 0.5, Quantity_TOC_RGB),
			Quantity_Color(0.3, 0.3, 0.3, Quantity_TOC_RGB),
			Aspect_GFM_VER,
			Standard_True);

		//add8e6
		//f0f8ff
		SetDefaultGradient();
		myView()->SetLightOn();
		myView()->SetLightOff();

		myView()->SetWindow(gview->myOcctWindow, glctx.ToPointer());
		myView()->ChangeRenderingParams().ToShowStats = Standard_True;

		myAISContext() = new AIS_InteractiveContext(myViewer());

		impl->setAisCtx(myAISContext());
		gview->setAisCtx(myAISContext());

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

	List<ManagedObjHandle^>^ GetSelectedObjects() {
		auto objs = impl->getSelectedObjectsList();
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
		for (size_t i = 0; i < objs.size(); i++)
		{
			ManagedObjHandle^ hh = gcnew ManagedObjHandle();
			hh->FromObjHandle(objs[i]);
			ret->Add(hh);
		}
		return ret;
	}

	List<ManagedObjHandle^>^ GetDetectedObjects() {
		auto objs = impl->getDetectedObjectsList();
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
		for (size_t i = 0; i < objs.size(); i++)
		{
			ManagedObjHandle^ hh = gcnew ManagedObjHandle();
			hh->FromObjHandle(objs[i]);
			ret->Add(hh);
		}
		return ret;
	}

	ManagedObjHandle^ GetSelectedEdge() {
		auto ret = impl->getSelectedEdge();

		ManagedObjHandle^ hh = gcnew ManagedObjHandle();
		hh->FromObjHandle(ret);
		return hh;
	}

	List<ManagedObjHandle^>^ GetSelectedEdges() {

		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(edges);
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
		for (size_t i = 0; i < edges.size(); i++)
		{
			ManagedObjHandle^ hh = gcnew ManagedObjHandle();
			hh->FromObjHandle(edges[i]);
			ret->Add(hh);
		}


		return ret;
	}
	List<ManagedObjHandle^>^ GetDetectedVertices() {

		std::vector<ObjHandle> verts;
		impl->GetDetectedVertices(verts);
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
		for (size_t i = 0; i < verts.size(); i++)
		{
			ManagedObjHandle^ hh = gcnew ManagedObjHandle();
			hh->FromObjHandle(verts[i]);
			ret->Add(hh);
		}


		return ret;
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
		Graphic3d_RenderingParams& aParams = myView()->ChangeRenderingParams();
		aParams.RenderResolutionScale = 2;
		aParams.IsShadowEnabled = false;
		// enable specular reflections
		aParams.IsReflectionEnabled = false;
		// enable adaptive anti-aliasing
		aParams.IsAntialiasingEnabled = false;
		/*Graphic3d_RenderingParams& rayp = myView()->ChangeRenderingParams();
		rayp.Method = Graphic3d_RM_RAYTRACING;
		rayp.IsShadowEnabled = Standard_True;
		rayp.IsReflectionEnabled = Standard_True;

		myAISContext()->UpdateCurrentViewer();*/

		//aParams.Method = Graphic3d_RM_RASTERIZATION;
		//aParams.RaytracingDepth = 3;
		//aParams.IsShadowEnabled = true;
		//aParams.IsReflectionEnabled = true;
		//aParams.IsAntialiasingEnabled = true;
		//aParams.IsTransparentShadowEnabled = false;
		//aParams.ToReverseStereo = true;
		//aParams.StereoMode = Graphic3d_StereoMode::Graphic3d_StereoMode_QuadBuffer;
		//aParams.AnaglyphFilter = Graphic3d_RenderingParams::Anaglyph::Anaglyph_RedCyan_Optimized;
		//aParams.FrustumCullingState = Graphic3d_RenderingParams::FrustumCulling::FrustumCulling_On;
		//aParams.LineFeather = 1.0;
		//aParams.NbMsaaSamples = 4;
		//auto IsSamplingOn = true;
		//if (IsSamplingOn)
		//	aParams.NbMsaaSamples = 32;
		//else
		//	aParams.NbMsaaSamples = 16;

		//aParams.Method = Graphic3d_RM_RAYTRACING;
		//aParams.IsShadowEnabled = true;
		//aParams.IsReflectionEnabled = true;
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

	Nullable<Vector3d> GetGravityPoint()
	{
		if (!myView().IsNull())
		{
			auto ret = myView()->GravityPoint();
			Vector3d v;
			v.X = ret.X();
			v.X = ret.Y();
			v.X = ret.Z();
			return v;
		}
		return {};
	}

	Nullable<float> ProjectionFOVy()
	{
		if (myView().IsNull())
			return {};

		return myView()->Camera()->FOVy();
	}

	Nullable<float> ProjectionAspect()
	{
		if (myView().IsNull())
			return {};

		return myView()->Camera()->Aspect();
	}

	Nullable<float> ProjectionZNear()
	{
		if (myView().IsNull())
			return {};

		return myView()->Camera()->ZNear();
	}

	Nullable<float> ProjectionZFar()
	{
		if (myView().IsNull())
			return {};

		return myView()->Camera()->ZFar();
	}

	Nullable<float> ProjectionScale()
	{
		if (myView().IsNull())
			return {};

		return (int)myView()->Camera()->Scale();
	}

	Nullable<int> ProjectionType()
	{
		if (myView().IsNull())
			return {};

		return (int)myView()->Camera()->ProjectionType();
	}

	/*
	Graphic3d_Camera::ProjectionType aProjectionType = aCamera->ProjectionType();
	Standard_Real aFovY = aCamera->FOVy(); // For perspective
	Standard_Real anAspect = aCamera->Aspect();
	Standard_Real aZNear = aCamera->ZNear();
	Standard_Real aZFar = aCamera->ZFar();*/

	Nullable<Matrix4> ProjectionMatrix()
	{
		if (myView().IsNull())
			return {};

		auto ret = myView()->Camera()->ProjectionMatrix();
		Matrix4 v;
		auto row0 = ret.GetRow(0);
		auto row1 = ret.GetRow(1);
		auto row2 = ret.GetRow(2);
		auto row3 = ret.GetRow(3);
		v.Row0 = Vector4(row0.x(), row0.y(), row0.z(), row0.w());
		v.Row1 = Vector4(row1.x(), row1.y(), row1.z(), row1.w());
		v.Row2 = Vector4(row2.x(), row2.y(), row2.z(), row2.w());
		v.Row3 = Vector4(row3.x(), row3.y(), row3.z(), row3.w());

		return v;
	}

	Nullable<Matrix4> OrientationMatrix()
	{
		if (myView().IsNull())
			return {};

		auto ret = myView()->Camera()->OrientationMatrix();
		Matrix4 v;
		auto row0 = ret.GetRow(0);
		auto row1 = ret.GetRow(1);
		auto row2 = ret.GetRow(2);
		auto row3 = ret.GetRow(3);
		v.Row0 = Vector4(row0.x(), row0.y(), row0.z(), row0.w());
		v.Row1 = Vector4(row1.x(), row1.y(), row1.z(), row1.w());
		v.Row2 = Vector4(row2.x(), row2.y(), row2.z(), row2.w());
		v.Row3 = Vector4(row3.x(), row3.y(), row3.z(), row3.w());

		return v;
	}

	Nullable<Vector3d> GetEye()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Eye();
			Vector3d v;
			v.X = ret.X();
			v.Y = ret.Y();
			v.Z = ret.Z();
			return v;
		}
		return {};
	}

	Nullable<Vector3d>  GetCenter()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Center();
			Vector3d v;
			v.X = ret.X();
			v.Y = ret.Y();
			v.Z = ret.Z();
			return v;
		}

		return {};
	}

	Nullable<Vector3d>  GetUp()
	{
		if (!myView().IsNull())
		{

			auto ret = myView()->Camera()->Up();
			Vector3d v;
			v.X = ret.X();
			v.Y = ret.Y();
			v.Z = ret.Z();
			return v;
		}
		return {};
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

		auto o = impl->findObject(h->BindId);
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

		auto o = impl->findObject(h->BindId);
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

	List<ManagedObjHandle^>^ ImportStep(System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ImportStep(aFilename);
	}



	List<ManagedObjHandle^>^ ImportStep(System::String^ name, List<System::Byte>^ bts)
	{
		auto buf = new uint8_t[bts->Count];
		for (int i = 0; i < bts->Count; i++) {
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

	List<ManagedObjHandle^>^ ImportIges(System::String^ str)
	{
		const TCollection_AsciiString aFilename = toAsciiString(str);
		return ImportIges(aFilename);
	}

	/// <summary>
	///Import Step file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	List<ManagedObjHandle^>^ ImportStep(const TCollection_AsciiString& theFileName)
	{
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
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
	List<ManagedObjHandle^>^ ImportStep(const TCollection_AsciiString& name, std::istream& stream)
	{
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();
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
	List<ManagedObjHandle^>^ ImportIges(const TCollection_AsciiString& theFileName)
	{
		List<ManagedObjHandle^>^ ret = gcnew List<ManagedObjHandle^>();

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
		auto anIO = impl->findObject(h->BindId);

		//auto anIO = impl->getObject(h);

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
	List<System::Byte>^ ExportStepStream(ManagedObjHandle^ h)
	{
		STEPControl_StepModelType aType = STEPControl_AsIs;
		IFSelect_ReturnStatus aStatus;
		STEPControl_Writer aWriter;

		//auto anIO = impl->getObject(h);
		auto o = impl->findObject(h->BindId);
		auto anIO = o;

		Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(anIO);
		TopoDS_Shape aShape = anIS->Shape();
		aStatus = aWriter.Transfer(aShape, aType);

		if (aStatus != IFSelect_RetDone)
			return nullptr;

		auto bts = gcnew List<System::Byte>();
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
		Handle(AIS_InteractiveContext) theCtx = myAISContext();

		AIS_ListOfInteractive aDispList;
		theCtx->DisplayedObjects(aDispList);
		for (const Handle(AIS_InteractiveObject)& aPrsIter : aDispList)
		{
			Handle(AIS_Shape) aShape = Handle(AIS_Shape)::DownCast(aPrsIter);
			if (aShape.IsNull())
				continue;

			theCtx->Deactivate(aShape);

		}
		//myAISContext()->Deactivate();
	}

	void SetSelectionMode(SelectionModeEnum t) {
		if (t == SelectionModeEnum::None)
			return;
		Handle(AIS_InteractiveContext) theCtx = myAISContext();
		int aSelMode = (int)t;

		AIS_ListOfInteractive aDispList;
		theCtx->DisplayedObjects(aDispList);
		for (const Handle(AIS_InteractiveObject)& aPrsIter : aDispList)
		{
			Handle(AIS_Shape) aShape = Handle(AIS_Shape)::DownCast(aPrsIter);
			if (aShape.IsNull()) { continue; }

			theCtx->Deactivate(aShape);
			theCtx->Activate(aShape, aSelMode, true);
		}
		//myAISContext()->Activate((int)t, true);

	//currentMode=t;
	}

	void Erase(ManagedObjHandle^ h, bool updateViewer) {
		auto o = impl->findObject(h->BindId);
		myAISContext()->Erase(o, updateViewer);
	}

	void Erase(ManagedObjHandle^ h) {
		Erase(h, true);
	}

	void Remove(ManagedObjHandle^ h) {
		auto o = impl->findObject(h->BindId);
		myAISContext()->Remove(o, true);
	}

	void Display(ManagedObjHandle^ h, bool wireframe) {
		auto o = impl->findObject(h->BindId);
		myAISContext()->Display(o, false);
		myAISContext()->SetDisplayMode(o, wireframe ? AIS_WireFrame : AIS_Shaded, true);
	}

	gp_Trsf GetObjectMatrix(ManagedObjHandle^ h) {
		auto p = impl->findObject(h->BindId);
		auto trans = p->Transformation();
		return trans;
	}

	List<double>^ GetObjectMatrixValues(ManagedObjHandle^ h) {
		List<double>^ ret = gcnew List<double>();
		auto p = impl->findObject(h->BindId);
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
		auto o = impl->findObject(h->BindId);
		gp_Trsf tr;
		tr.SetValues(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z);
		if (rel) {
			auto mtr = o->Transformation();
			tr.Multiply(mtr);
		}

		TopLoc_Location p(tr);
		myAISContext()->SetLocation(o, p);
	}

	void SetMatrixValues(ManagedObjHandle^ h, List<double>^ m)
	{
		auto o = impl->findObject(h->BindId);
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
		auto o = impl->findObject(h->BindId);
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

	ManagedObjHandle^ MirrorObject(ManagedObjHandle^ h, Vector3d dir, Vector3d pnt, bool axis2, bool rel)
	{
		ObjHandle oh = h->ToObjHandle();
		const auto object1 = impl->findObject(oh);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();


		gp_Trsf tr;
		if (axis2) {
			gp_Ax2 ax2(gp_Pnt(pnt.X, pnt.Y, pnt.Z), gp_Dir(dir.X, dir.Y, dir.Z));
			tr.SetMirror(ax2);
		}
		else {
			gp_Ax1 ax(gp_Pnt(pnt.X, pnt.Y, pnt.Z), gp_Dir(dir.X, dir.Y, dir.Z));
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

	List<List<Vector3d>^>^ IteratePoly(ManagedObjHandle^ h, bool useLocalTransform) {

		List<List<Vector3d>^>^ ret = gcnew List<List<Vector3d>^>();

		List<Vector3d>^ verts = gcnew List<Vector3d>();
		List<Vector3d>^ norms = gcnew List<Vector3d>();
		ObjHandle hc = h->ToObjHandle();

		auto pp = impl->IteratePoly(hc, useLocalTransform);
		for (size_t i = 0; i < pp.size(); i += 6)
		{
			Vector3d v;
			v.X = pp[i];
			v.Y = pp[i + 1];
			v.Z = pp[i + 2];
			verts->Add(v);

			Vector3d v2;
			v2.X = pp[i + 3];
			v2.Y = pp[i + 4];
			v2.Z = pp[i + 5];
			norms->Add(v2);
		}

		ret->Add(verts);
		ret->Add(norms);

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
		myAISContext()->Display(ais, false);

		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;

	}

	ManagedObjHandle^ Clone(ManagedObjHandle^ m) {
		BRepBuilderAPI_Copy copy;
		ObjHandle h = m->ToObjHandle();

		const auto object1 = impl->findObject(h);

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
		auto			 object1 = impl->findObject(h);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		auto compound = TopoDS::Compound(shape0);

		int counter = 0;
		for (TopExp_Explorer aExpFace(shape0, TopAbs_WIRE); aExpFace.More(); aExpFace.Next())
		{
			const auto ttt = aExpFace.Current();
			const auto& edgee = TopoDS::Wire(ttt);
			auto tt = ttt.TShape();

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

	ManagedObjHandle^ MakePrismFromFace(ManagedObjHandle^ m, double height)
	{
		return MakePrismFromFace(m->AisShapeBindId, m, height);
	}

	ManagedObjHandle^ MakePrismFromFace(int parentId, ManagedObjHandle^ m, double height) {
		ObjHandle h = m->ToObjHandle();
		BRepBuilderAPI_MakeFace bface;
		const auto object1 = impl->findObject(parentId);

		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();

		int counter = 0;
		for (TopExp_Explorer aExpFace(shape0, TopAbs_FACE); aExpFace.More(); aExpFace.Next())
		{
			auto ttt = aExpFace.Current();
			auto ind = AddOrGetShapeIndex(ttt);

			ttt = ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Face(ttt);


			if (edgee.IsNull()) {
				continue;
			}
			if (ind == h.bindId) {


				TopLoc_Location aLocation;
				Handle(Geom_Surface) aSurf = BRep_Tool::Surface(edgee, aLocation);

				auto plane = Handle(Geom_Plane)::DownCast(aSurf);

				auto pln = (*plane).Pln();

				float aU = 0;
				float aV = 0;
				gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
				Vector3d pos;
				Vector3d nrm;

				PlaneSurfInfo^ ret = gcnew PlaneSurfInfo();
				GProp_GProps massProps;
				BRepGProp::SurfaceProperties(ttt, massProps);
				gp_Pnt gPt = massProps.CentreOfMass();

				pos.X = aPnt.X();
				pos.Y = aPnt.Y();
				pos.Z = aPnt.Z();

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

	Nullable<Vector3d> GetVertexPosition(ManagedObjHandle^ h1)
	{
		return GetVertexPosition(h1->AisShapeBindId, h1);
	}

	Nullable<Vector3d> GetVertexPosition(int parentId, ManagedObjHandle^ h1)
	{
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(parentId);

		auto temp1 = Handle(AIS_Shape)::DownCast(object1);
		if (temp1.IsNull()) {
			return {};
		}
		TopoDS_Shape shape0 = temp1->Shape();



		for (TopExp_Explorer exp(shape0, TopAbs_VERTEX); exp.More(); exp.Next()) {
			auto ttt = exp.Current();
			auto ind = GetShapeIndex(ttt);
			ttt = ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Vertex(ttt);


			if (edgee.IsNull()) {
				continue;
			}
			if (ind == hh.bindId) {
				Vector3d ret;

				gp_Pnt p = BRep_Tool::Pnt(edgee);
				ret.X = p.X();
				ret.Y = p.Y();
				ret.Z = p.Z();
				return ret;
			}
		}
		return {};
	}

	EdgeInfo^ GetEdgeInfoPosition(ManagedObjHandle^ h1)
	{
		return GetEdgeInfoPosition(h1->AisShapeBindId, h1);
	}

	EdgeInfo^ GetEdgeInfoPosition(int parentId, ManagedObjHandle^ h1)
	{
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(parentId);
		if (!object1)
			return {};

		auto temp1 = Handle(AIS_Shape)::DownCast(object1);
		if (temp1.IsNull()) {
			return nullptr;
		}
		TopoDS_Shape shape0 = temp1->Shape();




		for (TopExp_Explorer exp(shape0, TopAbs_EDGE); exp.More(); exp.Next()) {
			auto ttt = exp.Current();
			auto ind = AddOrGetShapeIndex(ttt);
			ttt = ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Edge(ttt);

			if (edgee.IsNull())
				continue;

			if (ind == hh.bindId) {

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
				ret->BindId = ind;
				ret->AisShapeBindId = parentId;
				ret->Length = len;
				ret->CurveType = (CurveType)curveType;

				ret->COM.X = gPt.X();
				ret->COM.Y = gPt.Y();
				ret->COM.Z = gPt.Z();

				ret->Start.X = pnt1.X();
				ret->Start.Y = pnt1.Y();
				ret->Start.Z = pnt1.Z();

				ret->End.X = pnt2.X();
				ret->End.Y = pnt2.Y();
				ret->End.Z = pnt2.Z();
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

		auto bsurf = Handle(Geom_BoundedSurface)::DownCast(aSurf);
		auto swept = Handle(Geom_SweptSurface)::DownCast(aSurf);
		auto cyl = Handle(Geom_CylindricalSurface)::DownCast(aSurf);
		auto rev = Handle(Geom_SurfaceOfRevolution)::DownCast(aSurf);
		auto rtrimmed = Handle(Geom_RectangularTrimmedSurface)::DownCast(aSurf);
		if (rtrimmed) {
			auto basis = (*rtrimmed).BasisSurface();
			auto cyl1 = Handle(Geom_CylindricalSurface)::DownCast(basis);
			if (cyl1) {
				cyl = cyl1;
			}
		}

		double rad = 0;
		if (cyl) {
			rad = (*cyl).Radius();
		}
		else if (rev) {
			rad = (*cyl).Radius();
		}
		else if (swept) {
			rad = (*cyl).Radius();
		}
		else if (bsurf) {
			rad = (*cyl).Radius();
		}


		auto dir = cyl->Axis().Direction().Transformed(aLocation.Transformation());
		if (orient == TopAbs_REVERSED) {
			dir.Reverse();
		}
		float aU = 0;
		float aV = 0;

		gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
		Vector3d pos;
		Vector3d nrm;

		CylinderSurfInfo^ ret = gcnew CylinderSurfInfo();

		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();

		nrm.X = dir.X();
		nrm.Y = dir.Y();
		nrm.Z = dir.Z();

		pos.X = aPnt.X();
		pos.Y = aPnt.Y();
		pos.Z = aPnt.Z();

		ret->COM.X = gPt.X();
		ret->COM.Y = gPt.Y();
		ret->COM.Z = gPt.Z();
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

		auto rtrimmed = Handle(Geom_RectangularTrimmedSurface)::DownCast(aSurf);
		if (rtrimmed) {
			auto basis = (*rtrimmed).BasisSurface();
			auto cyl1 = Handle(Geom_SphericalSurface)::DownCast(basis);
			if (cyl1) {
				cyl = cyl1;
			}

		}

		double rad = 0;
		if (cyl) {
			rad = (*cyl).Radius();
		}

		float aU = 0;
		float aV = 0;

		gp_Pnt aPnt = aSurf->Value(aU, aV).Transformed(aLocation.Transformation());
		Vector3d pos;

		SphereSurfInfo^ ret = gcnew SphereSurfInfo();

		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();


		pos.X = aPnt.X();
		pos.Y = aPnt.Y();
		pos.Z = aPnt.Z();


		ret->COM.X = gPt.X();
		ret->COM.Y = gPt.Y();
		ret->COM.Z = gPt.Z();
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

		Vector3d pos;
		Vector3d nrm;

		PlaneSurfInfo^ ret = gcnew PlaneSurfInfo();
		GProp_GProps massProps;
		BRepGProp::SurfaceProperties(ttt, massProps);
		gp_Pnt gPt = massProps.CentreOfMass();

		pos.X = aPnt.X();
		pos.Y = aPnt.Y();
		pos.Z = aPnt.Z();

		auto dir = pln.Axis().Direction().Transformed(aLocation.Transformation());
		if (orient == TopAbs_REVERSED) {
			dir.Reverse();
		}

		nrm.X = dir.X();
		nrm.Y = dir.Y();
		nrm.Z = dir.Z();

		ret->COM.X = gPt.X();
		ret->COM.Y = gPt.Y();
		ret->COM.Z = gPt.Z();
		ret->Position = pos;
		ret->Normal = nrm;

		return ret;
	}

	SurfInfo^ GetFaceInfo(ManagedObjHandle^ h1) {
		return GetFaceInfo(h1->AisShapeBindId, h1);
	}

	SurfInfo^ GetFaceInfo(int parentId, ManagedObjHandle^ h1) {
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(parentId);
		auto temp1 = Handle(AIS_Shape)::DownCast(object1);
		if (temp1.IsNull()) {
			return nullptr;
		}
		TopoDS_Shape shape0 = temp1->Shape();

		for (TopExp_Explorer exp(shape0, TopAbs_FACE); exp.More(); exp.Next())
		{
			auto ttt = exp.Current();
			auto ind = GetShapeIndex(ttt);
			ttt = ttt.Located(object1->LocalTransformation());

			auto loc = ttt.Location();

			const auto& aFace = TopoDS::Face(ttt);
			auto orient = aFace.Orientation();

			TopLoc_Location aLocation;
			Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);

			GeomAdaptor_Surface theGASurface(aSurf);



			if (aFace.IsNull())
				continue;

			if (ind == hh.bindId) {
				switch (theGASurface.GetType())
				{
				case GeomAbs_Plane:
				{
					auto ret = ExtractPlaneSurface(ttt);
					ret->BindId = ind;
					ret->AisShapeBindId = parentId;
					return ret;
				}
				case GeomAbs_Cylinder:
				{
					auto ret = ExtractCylinderSurface(ttt);
					ret->BindId = ind;
					ret->AisShapeBindId = parentId;

					return ret;
				}
				case GeomAbs_Sphere:
				{
					auto ret = ExtractSphereSurface(ttt);
					ret->BindId = ind;
					ret->AisShapeBindId = parentId;

					return ret;
				}
				}
			}
		}
		return nullptr;
	}

	List<VertInfo^>^ GetVertsInfo(ManagedObjHandle^ h1) {
		List<VertInfo^>^ rett = gcnew List<VertInfo^>();
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		auto indp = AddOrGetShapeIndex(shape0);

		for (TopExp_Explorer exp(shape0, TopAbs_VERTEX); exp.More(); exp.Next())
		{
			auto ttt = exp.Current();
			auto ind = AddOrGetShapeIndex(ttt);

			ttt = ttt.Located(object1->LocalTransformation());

			auto loc = ttt.Location();

			const auto& aVert = TopoDS::Vertex(ttt);
			auto orient = aVert.Orientation();

			if (aVert.IsNull())
				continue;

			VertInfo^ toAdd = gcnew VertInfo();

			if (toAdd != nullptr) {
				toAdd->BindId = ind;
				toAdd->AisShapeBindId = indp;
				rett->Add(toAdd);
			}
		}
		return rett;
	}

	List<SurfInfo^>^ GetFacesInfo(ManagedObjHandle^ h1) {
		List<SurfInfo^>^ rett = gcnew List<SurfInfo^>();
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		auto indp = AddOrGetShapeIndex(shape0);

		for (TopExp_Explorer exp(shape0, TopAbs_FACE); exp.More(); exp.Next())
		{
			auto ttt = exp.Current();
			auto ind = AddOrGetShapeIndex(ttt);

			ttt = ttt.Located(object1->LocalTransformation());

			auto loc = ttt.Location();

			const auto& aFace = TopoDS::Face(ttt);
			auto orient = aFace.Orientation();

			TopLoc_Location aLocation;
			Handle(Geom_Surface) aSurf = BRep_Tool::Surface(aFace, aLocation);

			GeomAdaptor_Surface theGASurface(aSurf);

			auto tt = ttt.TShape();


			if (aFace.IsNull())
				continue;


			auto tp = theGASurface.GetType();
			SurfInfo^ toAdd = nullptr;
			switch (theGASurface.GetType()) {
			case GeomAbs_Plane:
			{
				toAdd = ExtractPlaneSurface(ttt);
			}
			break;
			case GeomAbs_Cylinder:
			{
				toAdd = ExtractCylinderSurface(ttt);
			}
			break;
			case GeomAbs_Sphere:
			{
				toAdd = ExtractSphereSurface(ttt);
			}
			break;
			}
			if (toAdd != nullptr) {
				toAdd->BindId = ind;
				toAdd->AisShapeBindId = indp;
				rett->Add(toAdd);
			}
		}
		return rett;
	}

	List<EdgeInfo^>^ GetEdgesInfo(ManagedObjHandle^ h1) {
		List<EdgeInfo^>^ rett = gcnew List<EdgeInfo^>();
		auto hh = h1->ToObjHandle();
		auto object1 = impl->findObject(hh);
		//const auto* object1 = impl->getObject(hh);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		//shape0 = shape0.Located(object1->LocalTransformation());
		int indp = AddOrGetShapeIndex(shape0);

		for (TopExp_Explorer exp(shape0, TopAbs_EDGE); exp.More(); exp.Next()) {
			const auto _ttt = exp.Current();
			int ind = AddOrGetShapeIndex(_ttt);
			auto ttt = _ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Edge(ttt);

			if (edgee.IsNull())
				continue;

			GProp_GProps massProps;
			BRepGProp::LinearProperties(ttt, massProps);
			auto len = massProps.Mass();

			gp_Pnt gPt = massProps.CentreOfMass();

			//Analysis of Edge
			Standard_Real First, Last;
			Handle(Geom_Curve) curve = BRep_Tool::Curve(edgee, First, Last); //Extract the curve from the edge

			if (curve.IsNull())
				continue;

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

			ret->BindId = ind;
			ret->AisShapeBindId = indp;

			ret->Length = len;
			ret->CurveType = (CurveType)curveType;

			ret->COM.X = gPt.X();
			ret->COM.Y = gPt.Y();
			ret->COM.Z = gPt.Z();

			ret->Start.X = pnt1.X();
			ret->Start.Y = pnt1.Y();
			ret->Start.Z = pnt1.Z();

			ret->End.X = pnt2.X();
			ret->End.Y = pnt2.Y();
			ret->End.Z = pnt2.Z();

			rett->Add(ret);

		}
		return rett;
	}

	ManagedObjHandle^ MakeChamfer(ManagedObjHandle^ h1, double s)
	{
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);
		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(edges);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();

		BRepFilletAPI_MakeChamfer chamferOp(shape0);

		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_EDGE); edgeExplorer.More(); edgeExplorer.Next()) {
			auto ttt = edgeExplorer.Current();
			auto ind = AddOrGetShapeIndex(ttt);
			ttt = ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Edge(ttt);


			if (edgee.IsNull()) {
				continue;
			}
			for (auto edge : edges) {
				//todo fix
				//if (ttt3 == edge.handleT) 
				{

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

	ManagedObjHandle^ Sphere(double x1, double y1, double z1, double size) {
		return Sphere(gp_Pnt(x1, y1, z1), size);
	}

	ManagedObjHandle^ Sphere(gp_Pnt center, double radius) {

		auto	sphere = BRepPrimAPI_MakeSphere(center, radius).Shape();

		auto ais = new AIS_Shape(sphere);
		myAISContext()->Display(ais, false);
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
		myAISContext()->Display(ais, false);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakePipe(ManagedObjHandle^ h1, double s)
	{

		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);


		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());
		gp_Pnt p1(0, 0, 0);
		auto dir = gp_Dir(0, 1, 0);

		//get first point of wire
		for (TopExp_Explorer exp(shape0, TopAbs_EDGE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();

			const auto& edge = TopoDS::Edge(ttt);
			//Analysis of Edge
			Standard_Real First, Last;
			Handle(Geom_Curve) curve = BRep_Tool::Curve(edge, First, Last); //Extract the curve from the edge
			GeomAdaptor_Curve aAdaptedCurve(curve);
			GeomAbs_CurveType curveType = aAdaptedCurve.GetType();

			gp_Pnt pnt1, pnt2;
			aAdaptedCurve.D0(First, pnt1);
			aAdaptedCurve.D0(First + 0.01, pnt2);
			p1 = pnt1;
			dir = gp_Dir(pnt2.X() - pnt1.X(), pnt2.Y() - pnt1.Y(), pnt2.Z() - pnt1.Z());
		}


		auto circle = gp_Circ(gp_Ax2(p1, dir), s);
		auto profile_edge = BRepBuilderAPI_MakeEdge(circle).Edge();
		auto profile_wire = BRepBuilderAPI_MakeWire(profile_edge).Wire();
		auto profile_face = BRepBuilderAPI_MakeFace(profile_wire).Face();
		for (TopExp_Explorer exp(shape0, TopAbs_WIRE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();

			const auto& baseWire = TopoDS::Wire(ttt);

			//BRepOffsetAPI_MakePipeShell makePipe(baseWire, profile_face);
			BRepOffsetAPI_MakePipeShell makePipe(baseWire);
			makePipe.SetMode(true);
			makePipe.Add(profile_wire, true, true);
			makePipe.SetTransitionMode(BRepBuilderAPI_RightCorner);
			makePipe.Build();

			makePipe.MakeSolid();
			auto ais = new AIS_Shape(makePipe.Shape());
			myAISContext()->Display(ais, false);
			ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

			auto hn = GetHandle(*ais);
			hhh->FromObjHandle(hn);
			return hhh;
		}

		return nullptr;
	}

	ManagedObjHandle^ HelixWire(double radius, double radius2, double height, double turns) {

		std::vector<gp_Pnt> vec;
		auto ang = turns * M_PI * 2;
		double step = 0.1;
		auto radDelta = radius2 - radius;
		for (double i = 0; i <= ang; i += step)
		{
			auto t = (i / ang);
			auto xx = (radius + t * radDelta) * cos(i);
			auto yy = (radius + t * radDelta) * sin(i);
			//	auto xx = i;
			//	auto yy = 0;
			vec.push_back(gp_Pnt(xx, yy, height * t));

		}

		TColgp_Array1OfPnt aPoints(0,
			vec.size() - 1);
		for (size_t i = 0; i < vec.size(); i++)
		{
			aPoints(i) = vec[i];
		}


		gp_Pnt center = gp::Origin();
		gp_Dir axis = gp::DZ();

		Handle(Geom_CylindricalSurface)
			cyl = new Geom_CylindricalSurface(gp_Ax2(center, axis), radius);
		GeomAPI_PointsToBSpline approx(aPoints);
		Handle(Geom_BSplineCurve)  c = approx.Curve();

		/*TopoDS_Edge                 edge1 = BRepBuilderAPI_MakeEdge(c, cyl,
			0.0,
			10.0);*/
		TopoDS_Edge ts2 = BRepBuilderAPI_MakeEdge(c);
		auto profile_wire = BRepBuilderAPI_MakeWire(ts2).Wire();

		//BRepLib::BuildCurves3d(ts2);
		auto ais2 = new AIS_Shape(profile_wire);
		myAISContext()->Display(ais2, false);

		ManagedObjHandle^ hhh2 = gcnew ManagedObjHandle();

		auto hn2 = GetHandle(*ais2);
		hhh2->FromObjHandle(hn2);
		return hhh2;//	myContext->Display(new AIS_Shape(ts)
	}

	ManagedObjHandle^ MakeFillet2d(ManagedObjHandle^ h1, double s)
	{
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);
		std::vector<ObjHandle> vertices;
		impl->GetSelectedVertices(vertices);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		BRepFilletAPI_MakeFillet2d filletOp;
		for (TopExp_Explorer exp(shape0, TopAbs_WIRE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();

			const auto& baseWire = TopoDS::Wire(ttt);
			bool isPlannar = true;
			TopoDS_Face baseFace = BRepBuilderAPI_MakeFace(baseWire, isPlannar);
			filletOp.Init(baseFace);
		}



		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_VERTEX); edgeExplorer.More(); edgeExplorer.Next()) {
			const auto ttt = edgeExplorer.Current();
			const auto& _vertex = TopoDS::Vertex(ttt);
			auto tt = ttt.TShape();

			if (_vertex.IsNull()) {
				continue;
			}

			for (int i = 0; i < vertices.size(); i++)
			{
				auto& vertex = vertices[i];
				//todo fix
				//if (ttt3 == vertex.handleT) 
				{
					filletOp.AddFillet(_vertex, s);
					b = true;
					vertices.erase(vertices.begin() + i);
					break;
				}
			}
		}

		if (!b)
			return nullptr;

		filletOp.Build();


		//Extract new face bound wire
		TopExp_Explorer explorer(filletOp.Shape(), TopAbs_WIRE);
		//Normally ony one wire exists
		TopoDS_Wire filletWire = TopoDS::Wire(explorer.Current());
		auto ais = new AIS_Shape(filletWire);
		myAISContext()->Display(ais, false);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakeFillet2d_new(ManagedObjHandle^ h1, double s)
	{
		//not tested
		auto hh = h1->ToObjHandle();
		const auto object1 = impl->findObject(hh);
		std::vector<ObjHandle> vertices;
		impl->GetSelectedVertices(vertices);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();
		shape0 = shape0.Located(object1->LocalTransformation());

		BRepFilletAPI_MakeFillet2d filletOp;
		for (TopExp_Explorer exp(shape0, TopAbs_WIRE); exp.More(); exp.Next())
		{
			const auto ttt = exp.Current();
			auto loc = ttt.Location();
			const auto& _wire = TopoDS::Wire(ttt);

			if (_wire.IsNull())
				continue;

			std::vector<  std::reference_wrapper<const TopoDS_Edge>> edges;

			for (TopExp_Explorer edgeExplorer(_wire, TopAbs_EDGE); edgeExplorer.More(); edgeExplorer.Next()) {
				const auto ttt = edgeExplorer.Current();
				auto& _edge = TopoDS::Edge(ttt);

				if (_edge.IsNull())
					continue;

				for (TopExp_Explorer edgeExplorer(_edge, TopAbs_VERTEX); edgeExplorer.More(); edgeExplorer.Next()) {
					const auto ttt = edgeExplorer.Current();
					const auto& _vertex = TopoDS::Vertex(ttt);


					if (_vertex.IsNull())
						continue;

					for (int i = 0; i < vertices.size(); i++)
					{
						auto& vertex = vertices[i];
						//todo fix
						//if (ttt3 == vertex.handleT) 
						{
							edges.push_back(std::reference_wrapper<const TopoDS_Edge>(_edge));

							break;
						}
					}
					if (edges.size() == 2)
						break;
				}

				if (edges.size() == 2)
					break;
			}
			BRepBuilderAPI_MakeWire wire(edges[0], edges[1]);

			bool isPlannar = true;
			TopoDS_Face baseFace = BRepBuilderAPI_MakeFace(wire, isPlannar);
			filletOp.Init(baseFace);

		}



		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_VERTEX); edgeExplorer.More(); edgeExplorer.Next()) {
			const auto ttt = edgeExplorer.Current();
			const auto& _vertex = TopoDS::Vertex(ttt);


			if (_vertex.IsNull()) {
				continue;
			}

			for (int i = 0; i < vertices.size(); i++)
			{
				auto& vertex = vertices[i];
				//todo fix
			//	if (ttt3 == vertex.handleT) 
				{
					filletOp.AddFillet(_vertex, s);
					b = true;
					vertices.erase(vertices.begin() + i);
					break;
				}
			}
		}

		if (!b)
			return nullptr;

		filletOp.Build();


		//Extract new face bound wire
		TopExp_Explorer explorer(filletOp.Shape(), TopAbs_WIRE);
		//Normally ony one wire exists
		TopoDS_Wire filletWire = TopoDS::Wire(explorer.Current());
		auto ais = new AIS_Shape(filletWire);
		myAISContext()->Display(ais, false);
		ManagedObjHandle^ hhh = gcnew ManagedObjHandle();

		auto hn = GetHandle(*ais);
		hhh->FromObjHandle(hn);
		return hhh;
	}

	ManagedObjHandle^ MakeFillet(ManagedObjHandle^ h1, double s)
	{
		auto hh = h1->ToObjHandle();
		//const auto* object1 = impl->getObject(hh);
		const auto object1 = impl->findObject(hh);
		std::vector<ObjHandle> edges;
		impl->GetSelectedEdges(edges);
		//auto edge = impl->getSelectedEdge(myAISContext().get());

		//const auto* object2 = impl->getObject(edge);
		TopoDS_Shape shape0 = Handle(AIS_Shape)::DownCast(object1)->Shape();

		BRepFilletAPI_MakeFillet filletOp(shape0);

		bool b = false;
		for (TopExp_Explorer edgeExplorer(shape0, TopAbs_EDGE); edgeExplorer.More(); edgeExplorer.Next()) {
			const auto _ttt = edgeExplorer.Current();
			auto ind = GetShapeIndex(_ttt);
			//auto ttt = _ttt.Located(object1->LocalTransformation());

			const auto& edgee = TopoDS::Edge(_ttt);

			if (edgee.IsNull()) {
				continue;
			}

			for (auto edge : edges) {
				//if (ttt3 == edge.handleT) 
				if (ind == edge.bindId)
					// 
				{
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
		auto trsf = GetObjectMatrix(h1);
		shape = BRepBuilderAPI_Transform(shape, trsf, Standard_True);

		auto ais = new AIS_Shape(shape);
		//myAISContext()->SetLocation(ais, myAISContext()->Location(object1));

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
				Line2D^ line = dynamic_cast<Line2D^>(p);
				BlueprintPolyline^ polyline = dynamic_cast<BlueprintPolyline^>(p);
				Arc2d^ arc = dynamic_cast<Arc2d^>(p);

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
				Line3D^ line = dynamic_cast<Line3D^>(p);
				Arc3d^ arc = dynamic_cast<Arc3d^>(p);

				if (line != nullptr) {
					gp_Pnt pnt1(line->Start->X, line->Start->Y, line->Start->Z);
					gp_Pnt pnt2(line->End->X, line->End->Y, line->End->Z);
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

	int GetShapeIndex(const TopoDS_Shape& shape) {

		if (impl->_map_shape_int.Contains(shape)) {
			return impl->_map_shape_int.FindIndex(shape);
		}
		return -1;
	}

	int AddOrGetShapeIndex(const TopoDS_Shape& shape) {

		if (impl->_map_shape_int.Contains(shape)) {
			return impl->_map_shape_int.FindIndex(shape);
		}
		return impl->_map_shape_int.Add(shape);
	}

	ObjHandle GetHandle(const AIS_Shape& ais_shape) {
		const TopoDS_Shape& shape = ais_shape.Shape();
		ObjHandle h;
		if (impl->_map_shape_int.Contains(shape)) {
			h.bindId = impl->_map_shape_int.FindIndex(shape);
		}
		else {
			h.bindId = impl->_map_shape_int.Add(shape);
		}


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

