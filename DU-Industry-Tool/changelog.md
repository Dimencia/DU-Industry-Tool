# Changelog

## v0.7.3

- Added display of Industry below Products with tier and size.
- Added "Industry" attribute to all 3203 recipes.
- Removed all "optical sensor" and several "lights" parts as these are not available in game.
- Code reviewed to have no more warnings in VS2019 IntelliSense list.

## v0.7.2

- Improved search in tree to prioritize exact match over partial match.
- Fixed version number handling in About dialogue
- Some code cleanup

## v0.7.1

- Switching between result tabs will now synchronize the tree to highlight the same item.
- Changing the amount selection box will now re-calculate a currently selected recipe with the selected amount of items.
- Removed forgotten sample code resources.

## v0.7.0

- Renamed solution and application (dashes instead of blanks)
- Added Github Actions for automatic binary release generation incl. assets
- Switched version tagging to SemVer (major/minor/patch)

## v0.640

- Byproducts are now linked as well
- Added About dialogue with links to discord and github repositories
- Plasmas are now also clickable links in ingredients
- Fixed interim bug with diverging amounts in top and bottom results for pures
- Fixed a couple of recipes where plasma was still missing (Bonsai, Warp Beacon)
- Some internal code cleanup

## v0.630

- Revised a lot of items' group assignments that were listed in wrong branches.
- Fixed Nickel pure and Silver pure not being used due to a previous renaming.
- Fixed: Pure Hydrogren is now being treated as a "pure" in the results.
- Listings in calculation results are now all prefixed by tier (T1,T2...) and sorted alphabetically.
- Added application icon and version number to EXE.
- For now disabled re-saving of recipes file upon application start.
- Known issues: there might appear a slight difference for pure amounts across top and bottom lists.

## v0.620

- Added even more missing recipes for variations of Vertical Boosters and Exotic Atmo/Space Engines
- Added missing Stasis Weapons and Stasis Ammo
- Added display of mass, volume and nanocraftable status
- Internal dev features extended to automatically convert dumps from du-lua.dev to Json, import the data and log cross-checks to debugging console for review to identify missing recipes
- Big thanks to Jericho1060 for help with items/recipes data via his website https://du-lua.dev/!

## v0.610

- Added several more missing recipes for variations of e.g. Hover Engines and Vertical Boosters.
- Tree members are now sorted by tier first, then by name, i.e. "uncommon" (T2) items are listed after "basic" (T1), but before "advanced" (T3) etc.
- Fixed group assignments of several atmo/space engines and some honeycomb.
- Fixed tier number for all industry (all were wrongly on T1).
- Fixed issues with broken layouts after resizing the application and then switching recipes.
- Fixed a couple of ammo group names to show size at end of their name.
- In factory breakdown, omitted formulas for Ore to avoid DIV0 errors.

## v0.600

- Updated a lot of recipes for patch v0.31 to fix wrong ID's and add more recipes, that may be available via the item API, but may not/no longer/not yet available as items to players. Those items are marked with "(!)" in their name in the tree.
- Calculation values for costs padded which results in better readability.
- Added display of list of all superior items that have the current one as ingredient. These are clickable and basically allow "drill up".
- Internal code added to convert an item dump lua file to an almost correct JSON (more a dev tool).
- Program now opens in approx. full HD size due to the data layout needing space. :)

## v0.500

- Updated and extended version to support recipes as of patch v0.30.
- Where appropriate, recipes have gotten a schematic cost assigned to them (T2+ ores, pures, products, elements, but not parts).
- Many recipes have been corrected or added (new containers, shield generators, plasmas).
- Plasma prices are editable in the ore price dialogue now.
- A new schematics dialogue allows to edit the cost per single schematic for each category.
- Calculation output tabulates schematic cost per item(s) and also offers a new total ingredients list.
- The UI has been changed to support tabbed display of multiple results, an amount dropbox for how many items the calculation is to be performed as well as a "back" button to go to the previous recipe.
- The talents dialogue will highlight the talents related to a selected recipe (if any).
