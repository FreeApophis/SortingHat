# SortingHat

SorthingHat is a non traditional Document Management System for private users with the focus of organising Documents in non-hierarchical way.

## UI Program

The UI is in a barely usable state, please refer to the console Program

## Console Program

```
Sortinghat <command> [arguments]:

Commands

  help        ?    This is the help command, it shows a list of the available commands.
  init             Initializes the database, a new database is created
  plugins          Lists the loaded plugins
  repair           Check each path locked in the database if the file still exists and is not corrupted / changed
  statistics  stat Shows global statistics
  version     v    Shows the current Version of this program

  add-tags         This adds a tag without any associated files to the db.
  list-tags   tags Lists all avaialable tags in hierarchical form
  move-tags        this moves a tag to another parent tag, if you want to move it to the root, use the empty tag ':'
  remove-tags      Removes a tag from database
  rename-tag       Renames a tag from database

  copy-files  cp   This command copies all files which match the search query to a specified folder location.
  duplicate   d    Find duplicate files in your dms
  file-info   info Shows all available information about the current file.
  find-files  ff   Finds all files matching the search query
  move-files  mv   This moves all files which match the search query to a specified folder location
  tag-files   tag  Is tagging the files the given tags
  untag-files      Remove tags from the indicated files

  auto-tag    auto Automatically tags stuff ...


Examples:

  hat.exe find (:tax:2018 or :tax:2017) and :bank
    This will output all files which are tagged as bank documents for tax in 2017 or 2018.

  auto-tag :Files:{FileType.Category}:{CameraMake} :Taken:{Taken.Year} *
    This will tag all files in the current directory, the possible automatic tags depend on your plugins.
```

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
