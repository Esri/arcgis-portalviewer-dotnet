# arcgis-portalviewer-dotnet

This project contains source code for the Portal Viewer sample app built using the Windows Store API in the ArcGIS Runtime SDK for .NET.   Source code is available to illustrate best practices for building an application using MVVM design patterns with the ArcGIS Runtime SDK for .NET.  The app includes functionality to view, search, and interact with maps in an ArcGIS Online organization on on-premises Portal for ArcGIS. 

The [ArcGIS app](http://apps.microsoft.com/windows/app/arcgis/db733971-3cc8-4db9-ae5a-865f2853a960) available in the [Windows Store](http://www.windowsstore.com/) is an example of a custom application built using the Portal Viewer source code and enhanced with minor customizations to accommodate for branding and workflow.      

![Image of sample app](/arcgis-portalviewer-dotnet.png "Portal Viewer sample app")

## Features 
- View and search for web maps
- Learn about map contents
- Navigate a map with your fingertips
- Tap to discover information in the map and view results
- Use bookmarks to go to areas of interest
- Measure areas and distances
- Change visibility and opacity of layers
- Change basemaps
- Search for places within the map view
- View the map legend
- Pin a map to the Start screen
- Show your current location
- Login to an ArcGIS Online organization or Portal


## Build
1. Fork and then clone the repo or download the .zip file.
2. The Portal Viewer sample requires the Windows Store API in the ArcGIS Runtime SDK for .NET.  Confirm that your system meets the requirements for building [Windows Store apps](http://developers.arcgis.com/net/store/guide/system-requirements.htm). 
3. Download and install the [ArcGIS Runtime SDK for .NET](http://esriurl.com/dotnetsdk).  Login to the beta community requires an Esri Global account, which can be created for free.
4. The Portal Viewer sample requires the [ArcGIS Runtime .NET Toolkit](https://github.com/Esri/arcgis-toolkit-dotnet).  Only the Windows Store edition of the Toolkit is needed by the Portal Viewer sample app.  You have three options to reference the Toolkit: 
 1. Use submodule
    - The Toolkit is referenced as a submodule in the Portal Viewer repo.  If the source control application you use with GitHub pulls submodule content (eg SourceTree), simply build the solution.   
 2. Add project to solution
    - Follow the build instructions in the repo for the [ArcGIS Runtime .NET Toolkit](https://github.com/Esri/arcgis-toolkit-dotnet).  Add the Toolkit source project (Esri.ArcGISRuntime.Toolkit.csproj) for Windows Store to the Portal Viewer solution, and add\repair the reference in the ArcGISPortalViewer project.  When building the Portal Viewer solution, the Toolkit source will build with the application code.  
 3. Use extension SDK 
    - Follow the build instructions in the repo for the [ArcGIS Runtime .NET Toolkit](https://github.com/Esri/arcgis-toolkit-dotnet) for distributing the Toolkit using a Visual Studio extension installer (VSIX).  Add\repair the Toolkit reference in the ArcGISPortalViewer project. 
5. To build and deploy the Portal Viewer sample app, choose the appropriate platform.  In Visual Studio, go to Build > Configuration Manager.  Choose the active solution platform for your device.  If deploying to Windows RT 8.1, select ARM.  If Windows 8.1 32-bit, select x86.  If Windows 8.1 64-bit, select either x86 or x64.	 

## Configure

To configure the Portal Viewer sample app to use an organization or Portal for ArcGIS instance: 
 1. In the ArcGISPortalViewer project, open App.xaml.
 2. Set the OrganizationUrl property:

    `<x:String x:Key="OrganizationUrl">https://myorg.maps.arcgis.com</x:String>` 
    
 3. Rebuild the Portal Viewer sample app.

## Resources

* [ArcGIS Runtime SDK for .NET](http://esriurl/dotnetsdk)
* [ArcGIS Runtime .NET Toolkit](https://github.com/Esri/arcgis-toolkit-dotnet)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute](CONTRIBUTING.md).  

## Licensing
Copyright 2014 Esri

This source is subject to the Microsoft Public License (Ms-PL).
You may obtain a copy of the License at

https://opensource.org/licenses/ms-pl

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt]( https://raw.github.com/Esri/arcgis-portalviewer-dotnet/master/license.txt) file.

[](Esri Tags: ArcGIS Runtime SDK .NET WinRT WinStore WPF WinPhone C# C-Sharp DotNet XAML)
[](Esri Language: DotNet)



