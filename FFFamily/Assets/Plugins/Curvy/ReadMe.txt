// =====================================================================
// Copyright 2013-2018 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

GETTING STARTED
===============
Visit https://curvyeditor.com to access documentation, tutorials and references

EXAMPLE SCENES
==============
Checkout the example scenes at  "Packages/Curvy Examples/Scenes"!

NEED FURTHER HELP
=================
Visit our support forum at https://forum.curvyeditor.com

VERSION HISTORY
===============

Note: Starting from version 3.0.0, Curvy is using the versioning standard “Semantic Versioning 2.0.0”. More about it here : https://semver.org/

6.1.0
	Curvy Generator
		[FIXED] Generators break when having other generators as children
		[FIXED] The Auto Refresh parameters where ignored when modifying an input spline that is not a child of an input module.
		[FIXED] Volume Caps module: end cap not using the right volume scaling
	Others
		[FIXED] Controllers: Offset feature still applies even when it should not i.e. when the Orientation mode is set to None.
		[FIXED] The UI is broken in some places when using new UI system introduced by Unity 2019.3
		[FIXED] When using Unity 2019.3, you can't assign a dynamic event handler to events inherited from parent class
		[FIXED] When using Unity 2019.3, warnings are displayed when opening some example scenes
		[FIXED] In rare cases, singletons can be duplicated
		[API/CHANGE] Controllers have now some previously private members exposed to inheriting classes. This is to make it easier to make custom controllers.

6.0.1
	[OPTIM] Reduced cost of drawing a spline's gizmos
	[FIXED] The Create GameObject CG module, when instantiating a prefab, breaks the link between the instantiated objects and their prefab

6.0.0
	Curvy Generator
		[NEW] Variable Mix Shapes module: Interpolates between two shapes in a way that varies along the shape extrusion. This module is used in the example scene 27.
		[OPTIM] Modules that are out of the view are now culled
		[OPTIM] Optimized shapes interpolation
		[CHANGE] Shape Extrusion module: The shape used in the extrusion can now vary along the extrusion. To do so, you should link as a shape input a module that provides a varying shape. This is the case of the Variable Mix Shapes module.
		[CHANGE] Made TRS modules larger to show more digits
		[CHANGE] Better error messages when something goes wrong with resources creation
		[FIXED] Mix Shapes module: Normals interpolation in not correctly computed
		[FIXED] Null reference exceptions happen when having as inputs splines with no Control Points
		[FIXED] Mix Shapes and Mix Paths modules: The mixing slider did not considered the mixed paths/shapes in the same order consistently. The order was sometimes inverted depending on the complexity of said paths/shapes
		[FIXED] When a scene has input spline path or input spline shape module, renaming objects from the hierarchy or though the F2 shortcut does not work
		[FIXED] Null references when feeding the output path of BuildRasterizedPath or the output volume of BuildShapeExtrusion to the input of a module that takes a CGPath as an input
		[FIXED] Volume Spots module: Null reference exception when using random distribution on items all of them of weight 0
		[FIXED] Create Mesh module: Null reference exception when unloading a scene having a Create Mesh module which mesh was removed by hand through the inspector
		[API/NEW] New types used by shape extrusions with variable shapes: CGDataRequestShapeRasterization, ShapeOutputSlotInfo and SlotInfo.SlotArrayType
		[API/CHANGE] ModifierMixShapes.InterpolateShape has now an optional ignoreWarnings parameter
	Control Point's Metadata
		[CHANGE] Displays warnings when a Control Point has multiple instances of the same Metadata type
		[FIXED] MetaData gets duplicated
		[API/CHANGE] Started refactoring Metadata API. Obsolete MetaData API will be removed in version 7.0.0. More details here: https://forum.curvyeditor.com/thread-706.html
	Others
		[OPTIM] Various minor optimizations
		[CHANGE] Corrected scene 24's name
		[FIXED] CurvyUISpline's documentation link leads to the wrong page
		[FIXED] UITextSplineController have wrong position when its canvas is scaled
		[FIXED] CurvyLineRenderer not updating when attached to an object that is not a CurvSpline
		[FIXED] CurvyLineRenderer not updating when a spline moves
		[FIXED] The View Examples button in the About Curvy Window does not find the example scenes
		[FIXED] Shapes resetting themselves (and loosing all modifications done outside the Shapes menu) when Curvy reload its assemblies
		[API/NEW] Added an InterpolateAndGetTangentFast method to CurvySpline and CurvySplineSegment
		[API/CHANGE] Removed various obsolete members

5.2.2
	[FIXED] Curvy Generator: In some cases when using multiple material groups, properties of some material groups get overridden by the properties of other material groups
	[FIXED] In some complex Curvy Generators, some modules don't get updated when needed
	[FIXED] Normal computation is wrong in TRSShape
	[API/NEW] Added method SamplePointsMaterialGroup.Clone

5.2.1
	[FIXED] Build error when using Unity 2019

5.2.0
	Curvy Generator
		[NEW] Added a new module: Debug Rasterized Path
		[NEW] Input Spline Path and Input Shape Path modules: Added a "Use Global Space" option
		[CHANGE] CG module's reset now resets the listeners too
		[CHANGE] Modified the error message shown when failing to destroy a resource that is part of a prefab to make it more helpful
		[CHANGE] Debug modules now show a warning if the generator's debug mode is not active
		[CHANGE] DebugVolume now displays the path's normals too
		[OPTIM] Optimized the CG resources creation and destruction process
		[FIXED] In edit mode, automatic refresh is not respecting the set "Refresh Delay Editor" value
		[FIXED] Resetting some CG modules does not reset all their properties
		[FIXED] Newly created Templates not showing on the templates list until you close and reopen the CG window
		[FIXED] Reordering CG modules in Play mode displays an error
		[FIXED] TRS Mesh module not applying the TRS matrix as it should
		[FIXED] Operations on meshes' transforms do not update the meshes' normals and tangents correctly
		[FIXED] Shape Extrusion module: The Cross's "Shift" option doesn't work properly when its value is set to "By Orientation"
		Create Mesh CG module
			[FIXED] Exported meshes (to asset files) keep being updated even after the export operation
			[FIXED] Removing an exported mesh removes the mesh from the Curvy generator too
	Example scene 51_InfiniteTrack
		[FIXED] The generated track has discontinuities between its sections
		[FIXED] The track is material rendered weirdly
	Others
		[NEW] Added an example scene to showcase the usage of the Conform Path CG module
		[API/NEW] Added a OnGlobalCoordinatesChanged delegate to CurvySpline
	
5.1.1
	[FIXED] Projects don't build when using Unity 2017.2 or newer
	
5.1.0
	Curvy Connection
		[CHANGE] Reworked the inspector
		[CHANGE] Gizmo labels now have the same color than the icon
		[FIXED] Inspector is not displayed inside the Curvy Spline Segment inspector when deleting then re-adding a connection
		[FIXED] When having some of the control points synchronized, and some not, moving a not synchronized control point make the synchronized ones move too.
		[FIXED] When using Synchronization presets, the synchronized position/rotation is not the one of the connection's game object as it should be, but one of the synchronized control points
		[FIXED] Gizmo labels are not drawn at the right position under some circumstances
		[FIXED] Updating the synchronization options from the inspector don't apply them right away, but until one of the synchronized transforms is modified
		[FIXED] Synchronization options not working properly when moving multiple connected control points at once
		[FIXED] Gizmos are sometime drawn using the wrong color
	Curvy Generator
		[OPTIM] Various optimizations related to Shape Extrusion and Input Spline modules
		[API/FIXED] CGShape.Bounds is not filled by SplineInputModuleBase when rasterization mode is equal to GDataRequestRasterization.ModeEnum.Even
		[API/CHANGE] Made CGShape.Bounds obsolete
		[FIXED] Shape Extrusion CG module: when Optimize is true, in certain conditions the following computations give wrong results: advanced scaling, volume splitting, UV coordinates
	Controllers
		[CHANGE] "Update In" is now taken into consideration even when in Edit Mode. Note: Since in Edit Mode there no fixed updates, setting "Update In" to "Fixed Update" will be equivalent to setting it to "Late Update" while in Edit Mode
		[FIXED] Controllers not updating frequently enough while in Edit Mode.
	Others
		[FIXED] When using Unity 2018.3, opening Curvy preferences from the toolbar does not work
		[CHANGE] Made Curvy Global Manager execute before anything Curvy
		[FIXED] You can end up with multiple Curvy Global Managers when opening multiple scenes at once

5.0.0
	Import/Export splines:
		[FIXED] Deserializing JSON files with missing fields do not assign the correct default values
		[FIXED] An error is logged when closing the file selection window without selecting a file
	Curvy Preferences window:
		[FIXED] The "Reset to defaults" button int the does not reset all the preferences
		[FIXED] A warning appears when building Curvy asking to use Unity's new [SettingsProvider] attribute
	Curvy Generator:
		[NEW] Added new CG module PathRelativeTranslation. It translates a Path relatively to it's direction, instead of relatively to the world as does the TRS Path module
		[CHANGE] Volume Spots CG module: Extended the valid range of Position Offset's values to [-1,1]. Now all the volume can be covered
		[CHANGE] The "Delete Resource" button now results in an undoable action. The confirmation message was updated to reflect this change.
		[FIXED] Volume Spots CG module: crash when bounds are too small
		[FIXED] Shape Extrusion CG module: resetting the module do not reset its scaling multiplier curves
		[FIXED] TRS Path not transforming the Normals correctly
		[FIXED] When using Unity 2018.3, showing debug information of a CG module stopped it from drawing
		[FIXED] When using Unity 2018.3, error messages are logged when unloading scenes containing "Input Spline Path" or "Input Spline Shape" CG modules
		[API/NEW] Shape Extrusion CG module: Exposed the scale multiplier curves
		[API/CHANGE] Removed obsolete CGData.TimeStamp
		[API/CHANGE] CGSplineResourceLoader.Destroy and CGShapeResourceLoader.Destroy do not register undoing actions anymore. This is to be coherent with other classes implementing ICGResourceLoader
	Controllers:
		[NEW] When setting "Orientation Mode" to None, the new "Lock Rotation" option allows you to enforce the rotation to stay the same. It is active by default, to keep the same behavior than previous versions
		[CHANGE] Spline controller: Added better error messages, especially when dealing with splines having segments of length 0
	Others:
		[OPTIM] Various minor optimizations, mostly in Curvy Generator

4.1.1
	[FIXED] Subdivision of a Bézier spline does not update the Bézier handles to keep the original spline's shape
	[FIXED] In Unity 2018.3, Curvy Generators that are an instance of a prefab don't show properly in the inspector

4.1.0
	UI Text Spline Controller
		[NEW] Added a new property, named "Static Orientation", to make the text keep it's orientation regardless of the spline it follows
		[FIXED] Inspector displays unused properties
	Curvy Generator
		[NEW] Create Mesh CG module: Exposed GameObject.tag in the "General" tab
		[NEW] Create Mesh CG module: Exposed MeshCollider.cookingOptions in the "Collider" tab
		[CHANGE] A slightly better modules reordering algorithm
		[FIXED] Saving a scene just after reordering modules does not save the modules positions
	Others:
		[OPTIM] Some splines related operations are faster thanks to C# 7.2, available when using Unity 2018.3 or above with .Net 4.x scripting runtime
		[FIXED] Incompatibility issues with Unity 2018.3 beta
		
4.0.1
	[FIXED] UI icons not showing in projects using .Net 3.5

4.0.0
	Create Mesh CG module		
		[CHANGE] Combining meshes now always produce continuous meshes
		[CHANGE] Enhanced the warnings when combining big meshes
		[OPTIM] Memory and CPU savings, especially when combining (merging) meshes and/or generating a Mesh Collider
		[FIXED] Null reference exception when combining big meshes
	Volume Spots CG module
		[NEW] Added warnings when the module is misconfigured
		[CHANGE] The Use Volume option is now hidden when irrelevant
		[CHANGE] The first available item is always added when creating groups
		[CHANGE] When using objects with no bounds (like point lights), the module will assume a depth of 1 Unity unit for those objects
		[FIXED] Various bugs and crashes happening in some situations where the module has null or empty input bounds and/or groups
		[FIXED] Invalid spots computation when using objects with no bounds (like point lights) mixed with objects with bounds		
		[FIXED] Object's bounds computation sometimes not considering the colliders
	Others:
		[FIXED] Normalize tool failed to normalize properly some Bezier splines
		[FIXED] Curvy Line Renderer not working properly when attached to an object that is not a spline
		[FIXED] Removed obsolete warnings when compiling Curvy
		[FIXED] In example scene 04_PaintSpline, the painted path is not positioned correctly
		[API/CHANGE] Made CGData.Timestamp obsolete
		[API/CHANGE] Removed the deprecated method VolumeController.ConvertObsoleteCrossPosition
		[API/FIXED] CGBounds copy constructor do not copy the source name
		
3.0.0
	Starting from this version, Curvy is using the versioning standard "Semantic Versioning 2.0.0". More about it here: https://semver.org/
	In a nutshell, and given a version number MAJOR.MINOR.PATCH:
	 -  An increase in MAJOR is done when there are non backwards-compatible changes.
	 -  An increase in MINOR is done when there are backwards-compatible changes that adds new features/API member, or modifies existing ones.
	 -  An increase in PATCH is done when there are backwards-compatible bug fixes.
	Whenever a number is increased, the numbers to its right are set to 0.

	Curvy Generator:
		[NEW] Added a  Reorder Modules" in the Curvy Generator toolbar. This will automatically sort the position of modules to make the graph easily readable
		[CHANGE] Mix Shapes and Mix Paths CG modules show warning when mixing inputs with incompatible unmixable properties
		[FIXED] Error when validating Input Spline Path or Input Spline Shape CG modules when they have a null source
		[FIXED] UI warnings in some CG modules are never cleared, even when the reason for the warning is fixed
		[FIXED] Mix Shapes CG module not working for shapes of different points counts
		[FIXED] Mix Shapes and Mix Paths CG modules giving wrong normals, directions, length, F array and bounds
		[FIXED] Create Mesh CG module can't generate a flat mesh collider
		[API/FIXED] Mix Shapes and Mix Paths CG modules produce CGShape and CGPath objects with most field set to default value
		[API/FIXED] CGShape and CGPath interpolation methods sometime returns non normalized vectors

	Others:
		[CHANGE] Removed features previously marked as deprecated
		[OPTIM] Dynamic orientaion computation optimized. Up to 20% performance increase
		[FIXED] For splines long enough with control points close enough, a Spline Controller could fail to handle connections and/or send events for some control points
		[FIXED] Control Points created using pooling get potentially a different orientation from ones created without pooling
		[FIXED] Compiling Curvy shows a compilation warning
		[API/CHANGE] Corrected the typo in CurvySpline.GetSegementIndex method
		[API/CHANGE] Removed deprecated API members
		[API/CHANGE] Merged CurvySplineBase with CurvySpline
	
2.3.0
	Controllers:
		All controllers:
			[NEW] Added an OnInitialized Unity event
			[NEW] Controller's position can now be animated via Unity's Animation window, or any other animation tool
			[DEPRECATED] The "Animate" option is removed. Use Unity's Animation window or any other animation tool instead
			[DEPRECATED] "Adapt On Change" is removed, but it's behavior is still there. Controllers will now always "adapt" (keeping their relative or absolute position, depending on the Position Mode value) when there source changes.
			[DEPRECATED] The Space parameter is no more used. The controller now works correctly without the need to set the correct Space value.
			[CHANGE] Direction is no more set from the sign of the Speed, but through a new value, Direction
			[CHANGE] When orientation's source has a value of "None", any rotation previously applied by the controller will be reverted
			[CHANGE] When orientation's source has a value of "None", Target and Ignore Direction are no more displayed in the inspector
			[CHANGE] When OrientationMode is set to None, controller now uses the object's rotation on initialization instead of keeping the last assigned rotation
			[FIXED] Wrong controller orientation when Target has a value of Backward
			[FIXED] When orientation's source has a value of "Tangent", the orientation of the spline is ignored
			[FIXED] Position is using Move Mode instead of Position Mode
			[FIXED] Wrong controller's position for 0 length splines
			[FIXED] Damping causes infinite rotations when the controller is parented to a rotated object, and has "Self" as a Space value
			[FIXED] Very high values of OffsetRadius sometimes lead to the controller stopping
			[FIXED] Offset compensation is always a frame late
			[FIXED] Offset compensation is is computed based on world speed even when move mode is set to "Relative"
			[FIXED] When OrientationMode is set to "None", the controller in some cases compensates the offset even when no offset is applied
			[API/NEW] TeleportBy and TeleportTo methods
			[API/CHANGE] Stop method now always rewinds the controller to its position it had when starting playing
			[API/CHANGE] Some properties are no more virtual
			[API/CHANGE] Moved all controllers related code inside the FluffyUnderware.Curvy.Controllers namespace
			[API/FIXED] UserAfterUpdate not being called like its documentation states
			[API/FIXED] Wrap method does not take Offset into consideration
			[API/FIXED] Apply method does not take Offset into consideration
		Spline Controller:
			[NEW] Added options in the inspector to define the controller's behavior when reaching a Connection: continue on the current spline, use the Follow-Up, ...
			[FIXED] Adding a listener to a SplineController's event is ignored if that event had no listener when the controller is initialized
			[FIXED] Spline Controller not working properly with Catmull-Rom splines having Auto End Tangents to false
			[FIXED] Switching splines ignores Offset
			[FIXED] Switching splines ignores Damping
			[FIXED] Switch duration is ignored when On Switch event is listened to
			[FIXED] Controller has wrong position when following a moved spline while having "Self" as a Space
			[FIXED] When spline and spline controller are parented to a rotated object, rotation of the controller is wrong when having "World" as a Space
			[FIXED] When spline and spline controller are parented to a moved object, controller switching between splines has wrong position when having "World" as a Space
			[API/NEW] Added FinishCurrentSwitch and CancelCurrentSwitch methods
			[API/CHANGE] SwitchTo method now raises an error if called on a stopped controller
			[API/CHANGE] CurvySplineMoveEventArgs fields are now read only. To modify a controller movement, modify the controller properties instead of CurvySplineMoveEventArgs fields
			[API/CHANGE] CurvySplineMoveEventArgs: sender is now always a SplineController
		UI Text Spline controller:
			[FIXED] Invalid position when having "World" as a Space
		Path and Volume Controllers
			[FIXED] "Adapt On Change" option has no effect
	Curvy Splines:
		[OPTIM] Real time splines modification takes tens of percents less CPU
		[CHANGE] Transforming a spline into a shape will remove all its connections
		[CHANGE] GetTangentFast now uses slerp instead of lerp to interpolate between cached tangents
		[FIXED] Connection inspector forbids setting some Heading values when control point is connected to a closed spline's first control point
		[FIXED] Slowdowns due to some connected control points synchronizing unnecessarily their transforms every frame
		[FIXED] Events are triggered before the spline is refreshed
		[FIXED] OnAfterControlPointChanges event was sometimes send with a null Spline property
		[FIXED] OnAfterControlPointAdd and OnAfterControlPointChanges events where sometimes called with the wrong value of CurvyControlPointEventArgs.Mode
		[API/NEW] InsertBefore and InsertAfter methods's overrides that set the created control point's position
		[API/NEW] CurvySpline.GlobalCoordinatesChangedThisFrame
		[API/CHANGE] CurvySpline.DistanceToTF will now return 0 (instead of 1) when spline's length is 0
		[API/CHANGE] InsertBefore, InsertAfter and Delete methods now have an optional parameter to make them not call the Refresh method, and not trigger events
		[API/CHANGE] Start now calls Refresh, which means OnRefresh event can be send in Start method
		[API/CHANGE] Adding and deleting control points events are no more cancelable
		[API/CHANGE/FIXED] Made methods that add or removes control points from splines coherent between themselves in the following ways:
			They now all call Refresh at their end
			OnAfterControlPointChanges will always send a CurvySplineEvent
			Methods that add or remove multiple control points will now always send only one event of each relevant type
		[API/FIXED] TFToDistance and DistanceToTF behave differently when handling the TF and Distance of the last Control Point of a spline
		[API/FIXED] CurvySpline.GetTangentFast returns non normalized vectors
		[API/FIXED] DistanceToSegment returns different result that TFToSegment for the same segment
	Misc:
		[NEW] Added support for compiler symbol: CURVY_SANITY_CHECKS. It activates various sanity checks in Curvy's code. Very useful to debug your code when using the Curvy API. This symbol is not set by default. For performance reasons, please do not set this symbol when building your release binary.
		[CHANGE] Set script execution order for some script: CurvyConection < CurvySplineSegment < CurvySpline < CurvyController
		[CHANGE] CurvySplineExportWizard do not support CurvySplineGroups anymore (but still supports multiple CurvySplines)
		[CHANGE] Moved files to the Plugins folder
		[FIXED] Draw splines tool: undoing the creation of a connected Control Point gives console errors
		[FIXED] Draw Splines tool: the "Add & Smart Connect" action generates splines that have incorrect shapes
		[FIXED] CG options handled incorrectly when extruding a mesh using a Cross with non null Start PC and End CP
		[FIXED] Namespace conflict with Oculus example project
		[FIXED] ThreadPoolWorker does not use all available cores in some cases.
		
2.2.4.2
	[OPTIM] Huge optimizations related to splines modification. Real time splines modification can take up to an order of magnitude less time
	[OPTIM] Reduced multi threading computational and memory overheads
	[OPTIM] Optimized splines interpolation
	[CHANGE] Made splines initialize earlier, in Start instead of the first Update/FixedUpdate/LateUpdate
	[CHANGE] Resetting a Control Point will now keep it's connections
	[CHANGE] Connection inspector: to no allow invalid Heading settings
	[CHANGE] Curvy Spline Segment inspector: OrientationAnchor is no more visible when it's value is irrelevant
	[CHANGE] Made materials in example scenes compatible with Unity 2018 (dropping of Substance based ones)
	[CHANGE] Transformation from text to number of user provided values now uses the invariant culture instead of the culture on the user's machine. More about invariant culture here: https://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.invariantculture%28v=vs.110%29.aspx
	[FIXED] Exceptions thrown when updating splines with "Use Threading" and "Bake Orientation" in one of the control points set to true
	[FIXED] Occasional exceptions thrown when undoing some operations
	[FIXED] In Meta CG Options' inspector, setting First U from neighbors lead to thrown execptions or invalid values
	[FIXED] Spline Controller does not work when setting its spline from code
	[FIXED] Changing spline controller's speed through the SetFromString does not work
	[FIXED] Controller's orientation can be wrong when TimeScale is set to 0 or when unpausing the game
	[FIXED] If a Control Point is used as a Follow-Up, moving it sometimes does not update the connected spline
	[FIXED] Control points that have already a next or previous Control point are allowed to have Follow-Ups
	[FIXED] A spline does not get updated when its first Control Point has a Follow-Up that moves
	[FIXED] Changing a spline's Auto End Tangents do not refresh following up spline
	[FIXED] Connection between control points at the end or start of a spline prevent those control points to become invisible when Auto End Tangents is set to false
	[FIXED] Connection persists after deletion
	[FIXED] Connections still existing even if all the connected control points are removed
	[FIXED] When changing Auto End Tangents from false to true, the Orientation Anchor value of the second control point is always overridden to true
	[FIXED] Selecting an orphan control point from a loaded scene leads to null references
	[FIXED] Spline does not get updated when one of its children Control Points is moved elsewhere via the Hierarchy window
	[FIXED] First tangent of a segment is sometimes not up to date
	[FIXED] When switching from Orientation Static or Dynamic to None, orientation for some control points is not recomputed
	[FIXED] The orientation of the last segment's cached points is sometimes not up to date
	[FIXED] A spline's Length can have a negative value when removing all control points
	[FIXED] A spline keeps a non zero value for Cache Size and Segments count even after removing all control points
	[FIXED] Sometimes, when enabling a disabled spline, the control points hierarchy is not reflecting the actual splines' control points
	[FIXED] Changing the number of points in a Spiral (in the Shape wizard) sometimes make the spiral "entangled"
	[FIXED] "Max Points Per Unit" is not correctly imported when using the splines import/export tool
	[FIXED] Resetting a CurvySpline component will set invalid value to its "Max Points Per Unit"
	[API/CHANGE] CurvySpline.Create now returns an initialized splines
	[API/CHANGE] Now all CurvySplineSegments have approximation arrays of a length of CacheSize + 1
	[API/CHANGE] Made CurvySplineBase abstract
	[API/CHANGE] CurvySplineSegment's ControlPointIndex can no longer be set. It is now handled by CurvySpline
	[API/CHANGE] Made components stripping systematic when pushing CurvySplineSegments to pools
	[API/FIXED] IsLastSegment returns true even if Control Point was not a segment
	[API/FIXED] When using Follow-Ups, GetPreviousControlPoint returns a Control Point that is not a segment even when segmentsOnly is true
	[API/FIXED] Poolable objects do not get their OnBeforePush method called before being pushed into a pool
	[API/FIXED] Setters of CurvySpline.MaxPointsPerUnit, CurvySplineGroup.Splines and BuildShapeExtrusion.CrossShiftValue did not update these values correctly
	Changes since 2.2.4.1
		[FIXED] StartCP and EndCP in InputSplinePath and InputSplineShape modules are set automatically to null.
		
2.2.3
Reminder: at each Curvy update, please delete the old Curvy folders before installing the new version.
	[NEW] Assembly definitions support
		Curvy was restructured and modified to support Unity's assembly definitions. This feature is disabled by default in this release. To enable it, search in Curvy installed folders for files with ".asmdef.disabled" extension, and rename the extension to ".asmdef".
	[NEW] CG module slots can be renamed without breaking the generator's data
	[CHANGE] Spline sampling parameters were modified:
		- The global "Max Cache PPU" is now obsolete and replaced with a per spline "Max Max Points Per Unit"
		- The "Min Distance" parameter of the Curvy Generators is now obsolete
		For more details please go here https://forum.curvyeditor.com/thread-526.html	
	[CHANGE] Fixed an example script's namespace
	[CHANGE] Added relevant dlls generated by asmdefs to link.xml
	[CHANGE] 0 is no more a valid value for the Resolution parameter in "Rasterize Path" and "Shape Extrusion" CG modules
	[CHANGE] Modified some CG module slots names to reflect the need for a rasterized path, in opposition to a non rasterized one.
	[FIXED] Some CurvyUISpline properties do not get reset properly when resetting the component
	[FIXED] Spline's level of detail is different between the editor and the build.
	[FIXED] Extruded shapes become two dimensional if small enough
	[FIXED] The "Use Cache" parameter in the spline input CG modules is ignored when linked to "Shape Extrusion" or "Rasterize Path" CG modules having "Optimize" set to true. 
	[FIXED] The rasterization resolution in the "Rasterize Path" CG module is modified if the module's Length parameter is modified.
	[FIXED] Extruded mesh jitters when modifying its path spline's length
	[FIXED] Wrong name of Rasterize Path in the CG modules list
	[FIXED, except when building against .NET Core] Curvy doesn't detect custom CG modules and Shapes from other assemblies.
	[FIXED] The Curvy generator templates list is broken in .Net 4.6
	[FIXED] In the CG graph, the suggested list of modules to connect with the current module contains modules you can't connect to
	[FIXED] Spline to Mesh tool generated spline at the wrong position
	[FIXED] Pools get duplicated if the pooled object's class is in a different assembly from the pool
	[FIXED] Multiple pools of the same component crash the game
	[FIXED] Obsolete messages from Unity 2017.3 and newer
	[FIXED] WebGL application crashes when using a spline having Use Threading set to true
	Example scenes:
		[CHANGE] Set assets serialization mode to Text
		[CHANGE] Reduced Ressource images size
		[CHANGE] Various tweaks and enhancements
		[FIXED] Example scenes do not render properly on WebGL and Mobile
		
2.2.2
	Spline to Mesh:
		[CHANGE] Renamed the "Mesh Export" tool to "Spline to Mesh" to avoid confusion.
		[CHANGE] Spline to Mesh does no more require the input spline to be on the X/Y plane
		[FIXED] Spline to Mesh does not open in recent Unity version
		[FIXED] Spline to Mesh produces wrong values in UV2.y
		
	Spline Import/Export wizard:
		[NEW] Added link to documentation in the spline Import/Export wizard
		[CHANGE] Spline Import/Export wizard modified to make it more straight forward to use
		[CHANGE] Modified the field names in the exported spline JSON format, to make it similar to the spline field names in the inspector
		[FIXED] Spline Import/Export wizard does not open in recent Unity versions
		[FIXED] "String too long" error when displaying long text in the spline Import/Export wizard TextMeshGenerator Cutting off characters

	Others:
		[Change] Replaced the usage of the obsolete UnityEditor.EditorApplication.playmodeStateChanged method with UnityEditor.EditorApplication.playModeStateChanged and UnityEditor.EditorApplication.pauseStateChanged for Unity 2017.2 and above
		[FIXED] WebGL builds fail
		[FIXED] Builds that use code stripping fail on Unity 2017.1 and older
		[FIXED] When synchronizing connected spline handles, moving a handle can invert the others
		
2.2.1
	[CHANGE] Modified the UI of the CG module "Create Mesh" to avoid confusion regarding the "Make Static" option:
		- "Make Static" is now not editable in play mode, since its Unity equivalent (GameObject.IsStatic) is an editor only property.
		- When "Make Static" is true, the other options are not editable while in play mode. This is to reflect the behaviour of the "Create Mesh" module, which is to not update the mesh while under those conditions, to avoid overriding the optimizations Unity do to static game objects'meshs.
	[FIXED] When combining multiple Volumes having different values for the "Generate UV" setting, the created mesh has invalid UVs
	[FIXED] "Mesh.normals is out of bounds" error when Generating a mesh that has Caps while using the Combine option
	[FIXED] Convex property, in CG module Create Mesh, not applied on generated mesh collider
	[FIXED] Negative SwirlTurns are ignored
	[FIXED] Orientation interpolated the wrong way (Lerping instead of SLerping)
	[FIXED] Cross's "Reverse Normal" in "Shape Extrusion" module is ignored when a "Volume Hollow" is set
	[FIXED] Crash on IOS when using code stripping on Unity 2017.2 and above.
	[Optimization] Various optimizations, the most important ones are related to "Shape Extrusion"'s normals computations and Orientation computation
	[API] Added a new GetNearestPointTF overload that also returns the nearestSegment and the nearestSegmentF
	[API] Made CrossReverseNormals, HollowInset and HollowReverseNormals properties public in BuildShapeExtrusion
	
2.2.0
	[NEW] Addressed Unity 2017.3 incompatibilities
	[NEW] Added a RendererEnabled option to the CreateMesh CG module. Useful if you generate a mesh for collider purposes only.
	[FIXED] Error when using pooling with Unity 2017.2 and above
	[FIXED] Incompatibility with UWP10 build
	[FIXED] SceneSwitcher.cs causing issues with the global namespace of Scene Switcher being occupied by the PS4's SDK
	[FIXED] Curvy crashing when compiled with the -checked compiler option
	[FIXED] TRSShape CG module not updating properly the shape's normals
	[FIXED] ReverseNormals not reversing normals in some cases
	      Note: You might have ticked "Reverse Normals" in some of your Curvy Generators, but didn't notice it because of the bug. Now that the bug is fixed, those accidental "Reverse Normals" will get activated.
	[FIXED] Split meshes not having the correct normals
	[CHANGE] Replaced website, documentation and forum URLs with the new ones.
	[Optimization] Various optimizations, the most important ones are related to mesh generation (UVs, normals and tangents computation)
	
2.1.3
	[FIXED] TimeScale affects controller movement when Animate is off
	[FIXED] Reverse spline movement going wrong under some rare conditions
	
2.1.2
	[NEW] Added CreatePathLineRenderer CG module
	[NEW] Addressed Unity 5.5 incompatibilities
	[FIXED] SplineController.AdaptOnChange failing under some conditions
	[FIXED] Selecting a spline while the Shape wizard is open immediately changes it's shape
	[FIXED] ModifierMixShapes module not generating normals
	[CHANGE] Changed 20_CGPath example to showcase CreatePathLineRenderer module
	
2.1.1
	[NEW] Added CurvySplineBase.GetApproximationPoints
	[NEW] Added Offsetting and offset speed compensation to CurvyController
	[FIXED] ImportExport toolbar button ignoring ShowGlobalToolbar option
	[FIXED] Assigning CGDataReference to VolumeController.Volume and PathController.Path fails at runtime
	[CHANGE] OrientationModeEnum and OrientationAxisEnum moved from CurvyController to FluffyUnderware.Curvy namespace
	[CHANGE] ImportExport Wizard now cuts text and logs a warning if larger then allowed by Unity's TextArea
	
2.1.0
	[NEW] More options for the Mesh Triangulation wizard
	[NEW] Improved Spline2Mesh and SplinePolyLine classes for better triangulator support
	[NEW] BuildVolumeCaps performance heavily improved
	[NEW] Added preference option to hide _CurvyGlobal_ GameObject
	[NEW] Import/Export API & wizard for JSON serialization of Splines and Control Points (Catmull-Rom & Bezier)
	[NEW] Added 22_CGClonePrefabs example scene
	[NEW] Windows Store compatiblity (Universal 8.1, Universal 10)
	[FIXED] BuildVolumeMesh.KeepAspect not working properly
	[FIXED] CreateMesh.SaveToScene() not working properly
	[FIXED] NRE when using CreateMesh module's Mesh export option
	[FIXED] Spline layer always resets to default spline layer
	[FIXED] CurvySpline.TFToSegmentIndex returning wrong values
	[FIXED] SceneSwitcher helper script raise errors at some occasions
	[CHANGE] Setting CurvyController.Speed will only change movement direction if it had a value of 0 before
	[CHANGE] Dropped poly2tri in favor of LibTessDotNet for triangulation tasks
	[CHANGE] Removed all legacy components from Curvy 1.X
	[CHANGE] New Control Points now use the spline's layer
	
2.0.5
	[NEW] Added CurvyGenerator.FindModule<T>()
	[NEW] Added InputSplineShape.SetManagedShape()
	[NEW] Added 51_InfiniteTrack example scene
	[NEW] Added CurvyController.Pause()
	[NEW] Added CurvyController.Apply()
	[NEW] Added CurvyController.OnAnimationEnd event
	[NEW] Added option to select Connection GameObject to Control Point inspector
	[FIXED] UV2 calculation not working properly
	[FIXED] CurvyController.IsInitialized becoming true too early
	[FIXED] Controller Damping not working properly when moving backwards
	[FIXED] Control Point pool keeps invalid objects after scene load
	[FIXED] _CurvyGlobal_ frequently causes errors in editor when switching scenes
	[FIXED] Curve Gizmo drawing allocating memory unnecessarily
	[FIXED] SplineController allocates memory at some occasions
	[FIXED] CurvyDefaultEventHandler.UseFollowUp causing Stack Overflow/Unity crashing
	[FIXED] _CurvyGlobal_ GameObject disappearing by DontDestroyOnLoad bug introduced by Unity 5.3
	[CHANGE] UITextSplineController resets state when you disable it
	[CHANGE] CurvyGenerator.OnRefresh() now returns the first changed module in CGEventArgs.Module
	[CHANGE] Renamed CurvyControlPointEventArgs.AddMode to ModeEnum, changed content to "AddBefore","AddAfter","Delete","None"
	
2.0.4
	[FIXED] Added full Unity 5.3 compatibility
	
2.0.3
	[NEW] Added Pooling example scene
	[NEW] Added CurvyGLRenderer.Add() and CurvyGLRenderer.Delete()
	[FIXED] CG graph not refreshing properly
	[FIXED] CG module window background rendering transparent under Unity 5.2 at some occasions
	[FIXED] Precise Movement over connections causing position warps
	[FIXED] Fixed Curvy values resetting to default editor settings on upgrade
	[FIXED] Control Points not pooled when deleting spline
	[FIXED] Pushing Control Points to pool at runtime causing error
	[FIXED] Bezier orientation not updated at all occasions
	[FIXED] MetaCGOptions: Explicit U unable to influence faces on both sides of hard edges
	[FIXED] Changed UITextSplineController to use VertexHelper.Dispose() instead of VertexHelper.Clear()
	[FIXED] CurvySplineSegment.ConnectTo() fails at some occasions
	
2.0.2
	[NEW] Added range option to InputSplinePath / InputSplineShape modules
	[NEW] CG editor improvements
	[NEW] Added more Collider options to CreateMesh module
	[NEW] Added Renderer options to CreateMesh module
	[NEW] Added CurvySpline.IsPlanar(CurvyPlane) and CurvySpline.MakePlanar(CurvyPlane)
	[NEW] Added CurvyController.DampingDirection and CurvyController.DampingUp
	[FIXED] Shift ControlPoint Toolbar action fails with some Control Points
	[FIXED] IOS deployment code stripping (link.xml)
	[FIXED] Controller Inspector leaking textures
	[FIXED] Controllers refreshing when Speed==0
	[FIXED] VolumeController not using individual faces at all occasions
	[FIXED] Unity 5.2.1p1 silently introduced breaking changes in IMeshModifier
	[CHANGE] CurvyController.OrientationDamping now obsolete!
	
2.0.1
	[NEW] CG path rasterization now has a dedicated angle threshold
	[NEW] Added CurvyController.ApplyTransformPosition() and CurvyController.ApplyTransformRotation()
	[FIXED] CG not refreshing as intended in the editor
	[FIXED] CG not refreshing when changing used splines
	[FIXED] Controllers resets when changing inspector while playing
	A few minor fixes and improvements
	
2.0.0 Initial Curvy 2 release