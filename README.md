## SlnMaker
Utility for creating .sln files for existing projects with automatic adding project references

#Usage
Pass following parameters to CLI:

-p <project path> or
-d <sln directory> -s <sln name> -p <project path> or
-d <sln directory> -s <sln name> -pn <project name>

When project name is specified, it's assumed, that project is stored to its folder with same name.
It's also assumed that by default project folder is located in solution folder.
For exit print :q
