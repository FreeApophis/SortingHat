# SortingHat

SortingHat is a non traditional Document Management System for private users with the focus of organising Documents in non-hierarchical way.

## UI Program

The UI is in a barely usable state, please refer to the console Program

## Console Program

The console program is called `hat`. A file can be added with `hat tag-files <file>`, tags are assigned with `hat tag-files :tag <file>`. Tags can contain variables which are filled in from the file itself, the available variables depend on the installed plugins.

```
hat <command> [<arguments>] [<options>]:

Commands

  help           ?    This is the help command, it shows a list of the available commands.
  plugins             Lists the loaded plugins
  repair              Check each path locked in the database if the file still exists and is not corrupted / changed
  statistics     stat Shows global statistics
  version        v    Shows the current Version of this program
  exif                Reads exif information from files.
  identify       id   This command identifies the real file type (ignoring the file-extension)

  add-tags            This adds a tag without any associated files to the db.
  list-tags      tags Lists all available tags in hierarchical form
  move-tags           this moves a tag to another parent tag, if you want to move it to the root, use the empty tag ':'
  remove-tags         Removes a tag from database
  rename-tag          Renames a tag from database

  copy-files     cp   This command copies all files which match the search query to a specified folder location.
  duplicate      d    Find duplicate files in your dms
  file-info      info Shows all available information about the current file.
  find-files     ff   Finds all files matching the search query
  move-files     mv   This moves all files which match the search query to a specified folder location
  tag-files      tag  Is tagging the files the given tags
  untag-files         Remove tags from the indicated files
  scan           s    scans a folder and its content

  new-project         Creates a new project database with a given name.
  remove-project      Removes a project with all tags and files.
  export              Exports a project as a file.
  import              Imports a project file into the db.
  list-projects       Lists all the project databases on this machine.
  switch-project      Switches to an existing database with the given name.


Options

     --dry-run   Dry run. No changes are written to the database.
     --open      Opens the file with the associated program.
  -r --recursive Iterate through directories recursively
  -v --verbose   More verbose output!

Possible Tag Variables:

* Camera.Make
* Camera.Model
* Camera.Taken.<DatePart> Possible values: Year, Month, Day, DayOfWeek, Hour, Minute, Second
* CreatedAt.<DatePart> Possible values: Year, Month, Day, DayOfWeek, Hour, Minute, Second
* FileType.Category
* FileType.Extension
* FileType.Type
* Folder.FromRoot.<Integer> Can be any valid integer number: 0,1,2...

Examples:

  hat find-files (:tax:2018 or :tax:2017) and :bank
    This will output all files which are tagged as bank documents for tax in 2017 or 2018.

  hat tag-files :Files:{FileType.Category}:{Camera.Make} :Taken:{Camera.Taken.Year} *
    This will tag all files in the current directory, the possible automatic tags depend on your plugins.
```

## Build and publish

```sh
dotnet build
dotnet test
```

`Scripts/publish.ps1` (Windows) and `Scripts/publish.sh` publish `hat` with all plugins into `publish/`. Plugins are loaded from the `plugins` folder next to the executable.

# Plugins

## Exif

* Read exif information from images
* Use exif information to automatically tag images with information from the exif tags

## FileType

* Identify file types correctly according to content
* give information about file type
* Auto-tag according to file type

## ExtractRelevantText

not working yet

# Similar Products

## TMSU https://tmsu.org/ 
Source: https://github.com/oniony/TMSU
 
Written in GO, Licensed GPL3, uses SQLite

Console application, Virtual Filesystem (linux only), problems on windows on the console (asterix)

* tags: flat
* copies file: no
* search: simple
* duplicated detection: no
* db: SQlite

## Tabbles https://tabbles.net/de/

Program is freemium (max 5000 Files), only software rent on a yearly subscription basis, aimed at companies.

Clearly the best program of this type. UI looks like WPF. Filesystem integration, Tags URLs and Mails, Rule based tagging (not very intuitive), Comments on files

* tags: hierarchical
* copies file: no
* search: advanced
* duplicated detection: yes
* db: MSSQL

## TagFlow http://www.tagflow.ch

Commerical, Betaversion since 2017, looks like a failed company

* tags: flat
* copies file: no
* search: advanced
* duplicated detection: yes
* db: unknown

## TagSpaces https://www.tagspaces.org/ 
Source: https://github.com/tagspaces/tagspaces

Program is freemium and open sourced? modern UI (electron) 

* tags: flat
* copies file: no
* search: advanced
* duplicated detection: no
* db: changes the files
