# Latest Changes

## v1.0.1
- Fix for market order reading regex (thanks to Ferry)

## v1.0.0
- Major revamp of calculation output as a tree layout 
- Double-click an ore, pure, product or part in tree to drill-down.
- Ores will display additional details, like refiner input/output values and time to refine, taking talents into account.
- Related talents for ores and pures are shown and can instantly be changed/corrected.
- Updated recipe base with current data from release thanks to du-lua.dev!
- Added "Recalculate" button on the results page (for both regular items as well as Production List).
- Added "[tier] Pure Refinery Efficiency" talents for Pures to allow for correct production batch sizes and times.
- Added display of number of entries in the recipe tree's nodes
- Added nanocraft filter checkbox above recipe tree
- Added checkbox option in ribbon bar to restore window position/size.
- Added checkbox option in ribbon bar to load last opened Production List file on startup.
- Moved setup-related items from the "File" menu (Ore prices,Talents,Schematics) to new Setup group under Tools.
- Added multiple ribbon buttons for managing a Production List (add/remove/clear).
- Added "F+"/"F-" buttons on top line of results window to (temporarily) increase/decrease font size (will be improved later).
- Added "Load Config"/"Save Config" buttons on top line of results window to save or restore grid column sizes.
- Added a combobox to pick a file from all recently opened production list files.
- Added hotkey CTRL+W to close current tab
- Added hotkeys CTRL+O to open and CTRL+S to save a currently open Production List.
- Search "quantity" box now allows entering of manual numeric values.
- Name of currently open Production List is shown in window title.
- Tabs now each have their own close button.
- Several more fixes of schematics data and their calculation.
- Internal refactoring, especially of the IndustryManager class.
- NOTE: Options/settings in the ribbon bar are all stored in file "DU-Industry-Tool.usersettings.json" (where the app is located).
- NOTE: windows restore **currently** only supports 1 screen (will be improved later).
- NOTE: not all ore/pures/products talents are yet implemented!
