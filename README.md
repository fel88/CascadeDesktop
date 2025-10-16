# CascadeDesktop
OpenCASCADE GUI Viewer/Editor

Simple debug tool based on OpenCASCADE for debugging purposes

![image](https://github.com/fel88/CascadeDesktop/assets/15663687/cc6f5aed-2425-485f-b9ee-647d78fd279e)

![image](https://github.com/fel88/CascadeDesktop/assets/15663687/8bb3410f-2a6b-4073-853e-cea2172b46a4)

Measure tool
<img width="1391" height="914" alt="изображение" src="https://github.com/user-attachments/assets/393d56b2-660f-4f22-965c-1442aaee4cb5" />

ImGui (experimental)
<img width="1504" height="910" alt="изображение" src="https://github.com/user-attachments/assets/09254067-0227-4ace-b0e8-9cc3924b1b23" />

Custom meshes (obj, stl) rendering
<img width="1384" height="910" alt="изображение" src="https://github.com/user-attachments/assets/0031cca2-e5d6-48f6-96e7-1861807fa966" />



<img src="https://github.com/user-attachments/assets/86a773a3-632d-4f28-87d4-546db5710bc0" />


## CSP solver:
<img src="https://github.com/user-attachments/assets/2baf42b5-c543-439d-a48a-caaa69a50664" />
 


# How to use:

## Basic operations (extrude, bool (fuse, subtract), fillet )
![image](https://user-images.githubusercontent.com/15663687/223804454-f6aaa2be-a2b7-4121-a727-db93230d6424.png)

https://www.youtube.com/watch?v=CJIdNRbEsHQ

### Part 1

https://user-images.githubusercontent.com/15663687/223806419-98b7344c-f03d-45d8-85ca-0f639654e019.mp4

### Part 2

https://user-images.githubusercontent.com/15663687/223809990-b63d45bb-d33a-4a0f-89a0-f9e010e793af.mp4



## Making profile pipe with fillet:
<!--https://user-images.githubusercontent.com/15663687/220192496-6f2c0dce-35b9-4d2d-969c-4d66d6ed5dbb.mp4-->
https://github.com/fel88/CascadeDesktop/assets/15663687/446fb389-447d-4e24-8220-a98b91b34a01

## 2D CSP Draft editor using



https://github.com/user-attachments/assets/db8478ee-192c-4962-8cb9-aa01b8007a7f


 > [!NOTE]
    <div><h3>This project uses the Hybrid OCC drawer:</h3>
    1. Render default OCC scene<br/>
    2. Render ImGui overlay<br/>
    3. Render any additional objects (OCC camera space or screen space)<br/>
    4. Render custom freetype labels<br/>
> </div>
   
## How to build:
- Install VC++ Redist 2013 (https://www.microsoft.com/ru-ru/download/details.aspx?id=40784)
- Install OpenCASCADE 7.7 (https://dev.opencascade.org/system/files/occt/OCC_7.7.0_release/opencascade-7.7.0-vc14-64.exe or https://github.com/Open-Cascade-SAS/OCCT/releases/download/V7_7_0/opencascade-7.7.0-vc14-64.exe)
   - Installation path should be : C:\OpenCASCADE-7.7.0-vc14-64
- Install Visual Studio Community edition (https://visualstudio.microsoft.com/ru/vs/community/), with next modules:
  - Desktop development with C++
  - .Net desktop development
- Open soultion Cascade.sln
- Build and run Cascade project (Ctrl+F5)

If you have a HashCode error during compilation of OCCTProxy, you can directly edit NCollection_DefaultHasher.hxx file (just add :: before HashCode) (https://dev.opencascade.org/content/opencascade-760-not-compiling-net-60-class-library-visual-studio-2022-windows-10)

<sub>Some icons by [Yusuke Kamiyamane](http://p.yusukekamiyamane.com/). Licensed under a [Creative Commons Attribution 3.0 License](http://creativecommons.org/licenses/by/3.0/)</sub>
