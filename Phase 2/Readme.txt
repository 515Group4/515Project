CSE 515 - Group 4 
Project 2 - Readme

This readme file will help illustrate how to accomplish all tasks in the project 2 specification.

Task 1
To index shapes using our custom 5 features, one must compile the ShapeIndexer project in the code folder.  In order for this program to run, ImageMagick .dll's must exist in C:\Data\Programs\ImageMagick\ and a C:\Data\Datasets directory must also exist.

To run the program simply run
ShapeExtractor.exe -k <number> -l <number> <folder>
Folder is the full path of a folder full of only image files in which to index.

This will write an index file at C:\Data\Datasets\reduced_test_images_index.txt which can then be consumed via indexer.

To index shapes using SIFT, simple compile The SiftExtractor project.  In order for this program to run, ImageMagick dlls, vl.dll and sift.exe (from the vlfeat package) must be present in the executables directory.

To run the program simply run:
SiftExtractor.exe <number k> <number l> [Option -F] <folder/file>
Use -F if you only want an individual file.  All output will be sent to
output.txt of the current directory.
