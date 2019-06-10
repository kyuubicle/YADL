# What is this?
It's... well, yet another doom launcher. YADL. Yeah. *ahem*

It's tool to help keep track of and launch Doom Engine games and mods.  
It's based off of the source of another launcher: [Doomie.](https://forum.zdoom.org/viewtopic.php?f=44&t=61647)

It's mostly something I modifed/created for my own use, but others might find it useful!

# What's new from Doomie 1.7?
- When Creating a New Playlist or Opening a single playlist, that playlist will be automatically selected.
  - No playlist will be selected when *importing* a directory of playlists.
- Can launch a playlist by double-clicking
- Can now rename playlists (without saving it as a new file)
- Can now specify Categories tabs to help further organize Playlists.
  - Playlists can be a part of any number of categories.
  - Can reorder Categories tabs via drag/drop.
  - Categories can also be set via the Playlist context menu (helpful for adding a category to groups of playlists)
- Allows hiding of Load/Merge/Location fields in PWAD list.
  - Settings are saved to the program's config file
- Wads can be loaded/unloaded by double-clicking them.
- Wads can also be loaded/unloaded by pressing the spacebar or enter key while selected (useful for loading/unloading groups of items)
- Text indicates the status of the Pwad:
 	- Bold - Will be Loaded
  - Red and Italic - File missing
- Changed UI a bit:
  - Context Menus on Playlist list and Pwad list themselves (as well as the individual elements)
  -	Uses a Menu instead of buttons for basic Playlist controls
  -	Play Button is now above the PWAD list, below the port/iwad/category options.

# TODO
- [ ] Allow directly specifying an icon for a playlist
- [ ] Remember last category/playlist on startup
- [ ] Better looking Text Prompts
- [ ] Default pwad list to load with any playlist? A load-last list?
- [X] Add direct Config/Save Directory settings 
  - They are present, but they are only setup to work with GZDoom for now, so are disabled/hidden by default
    - Can enable by adding the following to your YADL.cfg:
      > [Hidden_Features]  
      > Config=yes  
      > SaveDir=yes  
- [X] Removing Categories.
  - When removing a Category, it will remove it from the UI only. Playlists will still keep the Category in their file in case the Category is added back later.
- [X] Renaming Categories
  - When renaming a Category, all currently loaded playlists with that category will be updated to use the new category name.
- [X] Better multi-select/deselect methods for the exit program 'Save Playlists' form.
  - Can now use the same Double-Click, Space Bar/Enter methods as the pwad list

# CREDITS
YADL is a fork of [Doomie 1.6 by buja-buja](https://forum.zdoom.org/viewtopic.php?f=44&t=61647)
  - Original Doomie can be downloaded [here](http://www.mediafire.com/file/oozoqoer362o6rq/Doomie+Release+1.7.zip)
  - Original Doomie 1.6 source can be downloaded [here](http://www.mediafire.com/file/o8ssed5gvumyamz/Doomie_Source_1.6.7z)

YADL also uses two libraries:
- [Extended WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit)
- [GongSolutions.WPF.DragDrop](https://github.com/punker76/gong-wpf-dragdrop)

# License
100% Freeware. 95% of this project is buja-buja's work. So, abide by their own rules first and foremost:
> 1. THOU SHALL NOT USE THE NAME DOOMIE OR SIMILAR (EXAMPLE: DOOMIE-NG) TO NAME YOUR PROJECT, 
> 2. THOU SHALL GIVE CREDIT WHERE CREDIT IS DUE AND INCLUDE A REFERENCE TO THE ORIGINAL DOOMIE NAME, ORIGINAL DOOMIE URL FOR DOWNLOAD, ANY > DOOMIE DLL FILES USED FOR SUPPORT, ANY DOOMIE CONTRIBUTORS & DOOMIE DEVELOPER AUTHOR ON YOUR DERIVATIVE VERSION, 
> 3. THOU SHALL RELEASE YOU VERSION UNDER A FREEWARE OR GPL LICENSE...

As for my own rules, do whatever you want with the source. Just give credit, geez.
