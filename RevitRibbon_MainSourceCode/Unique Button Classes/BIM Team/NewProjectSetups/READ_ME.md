### README: BIM Project Setup Ribbon

This README provides an overview of the buttons included in the "BIM Project Setup" ribbon tab. Each button's functionality is described below, following the order they appear on the ribbon.

#### 1. Create BIM Setup View
   - Class: Cmd_CreateBIMSetupView.cs
   - Function: Creates a BIM setup view that helps in organizing and managing BIM projects effectively.

#### 2. Scope Box Grid
   - Class: Cmd_ScopeBoxGrid.cs
   - Function: This button will allow you to create a grid of scope boxes based on the number of columns and rows required.
	You will first have to create a desired size scope box, then select it in order to create a Grid of Scope Boxes.

	STEP 1: Create first scope box
	STEP 2: Select scope box
	STEP 3: Click Scope Box Grid button
	STEP 4: Specify the amount of columns, rows and the horizontal, vertical overlapping distances

#### 3. Rename Scope Boxes
   - Class: Cmd_RenameScopeBoxes.cs
   - Function: This button will rename all selected ScopeBoxes.
	OPTIONS:
	1. You can either pre-select a group of scopeboxes before clicking the button
	 OR
	2. click the button first and select the individual scopeboxes then hit ESC to continue.

#### 4. Create Dependent Views
   - Class: Cmd_CreateDependentScopeView.cs
   - Function: This button will create dependent views of the Current Active view based on the number of Scope Boxes shown.

#### 5. Grid Dimensions
   - Class: Cmd_GridDimensions.cs
   - Function: This button will create dimensions for horizontal and vertical grid lines. Additionally, when scope boxes are selected, it will create linear dimensions above and to the right for all grids within each Scope Box. All dimensions created will use the GRID DIMENSIONS style to differentiate themselves from other standard dimension styles.

#### 6. Matchline Detail Lines
   - Class: Cmd_MatchlineDetailLines.cs
   - Function: This button will create intersecting detail lines between selected scope boxes. These detail lines can then be used as references when creating match lines using Revit’s native Match Line function. Note: Only use this in a non-plotting view to avoid any confusion.

#### 7. Place View References
   - Class: btn_CreateViewReferencesDuplicates.cs
   - Function: This button will create Duplicates of an existing View Reference and place them at the intersecting corners of scope boxes.

#### 8. Create Parent Views
   - Class: Cmd_CreateParentPlotViews.cs
   - Function: Creates parent views based on the level and view template selected by the user.

#### 9. Update Applied Dependent Views
   - Class: Cmd_UpdateAppliedDependentViews.cs
   - Function: Updates the applied dependent views selected by the user by adding the appropriate Scope Box and renaming each view accordingly.

#### 10. Cmd_CopyDimsToParentViews.cs
   - Class: CopyDimsToParentViews
   - Function: Copies dimensions from the 'BIM Setup View' to the parent views selected by the user, ensuring consistent dimensioning across views.

#### 11. Clean Dependent View Dims
   - Class: Cmd_CleanDependentViewDims.cs
   - Function: This button will go through the selected dependent views and hide any unnecessary dimensions that uses the 'GRID DIMENSIONS' style. Dimensions will be cleaned accordingly based on the selected dependent views’ crop boxes.