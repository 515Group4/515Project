#include "stdafx.h"

using namespace Magick;
using namespace std;

#include "ShapeIndexer.h"
#include "Eccentricity.h"

bool FindUnvisitedPixel(const PixelPacket *pc, const bool *pixelvisited, int cols, int rows, int *px, int *py);

static int foo = 0;
static Image *shapeLibrary1 = NULL;
static Image *shapeLibrary2 = NULL;
static Image *shapeLibrary3 = NULL;
static Image *shapeLibrary4 = NULL;

static const int NumImportantShapes = 10;


int main(int argc, char* argv[])
{
    string s;

	// C:\\Data\\Programs\\ImageMagick\\
	// E:\\Programs\\Dev\\ImageMagick\\

    Magick::InitializeMagick("C:\\Data\\Programs\\ImageMagick\\");

    if (argc > 1)
    {
        s = string(argv[1]);
    }
    else
    {
        s = "button.gif";
    }

	IndexDirectory("C:\\Data\\Datasets\\reducedtestimages\\", "C:\\Data\\Datasets\\reduced_test_images_index.txt");
	return 0;
}

void IndexDirectory(const char *foldername, const char *indexFile)
{
	WIN32_FIND_DATAA findData;
	ofstream indexWriter(indexFile);
	cout << "Indexing directory: " << foldername << endl;

	char findPattern[MAX_PATH];
	sprintf(findPattern, "%s*", foldername);
	HANDLE hFind = FindFirstFileA(findPattern, &findData);

	if (hFind != INVALID_HANDLE_VALUE)
	{
		do{
			if (!(findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
			{
				cout << endl << "Now indexing: " << findData.cFileName << "\n";
				IndexImage(foldername, findData.cFileName, indexWriter);
			}
		} while(FindNextFileA(hFind, &findData));

		FindClose(hFind);
	}

	indexWriter.flush();
	indexWriter.close();
}

void IndexImage(const char* foldername, const char* filename, ofstream& indexFile)
{
	char fullFilePath[MAX_PATH];
	sprintf(fullFilePath, "%s%s", foldername, filename);
	Image img(fullFilePath);
	//img.colorSpace(ColorspaceType::YUVColorspace);
	img.segment(1.0, 1.5);
	img.write("segmented-image.png");
	int cols = img.columns();
	int rows = img.rows();
	const PixelPacket *pc = img.getConstPixels(0, 0, cols, rows);
	IterateThroughEachShape(pc, cols, rows, indexFile, filename);
}

void IterateThroughEachShape(const PixelPacket *px, int cols, int rows, ofstream& indexFile, const char* filename)
{
	int x = 0, y = 0, shapecount=0;
	vector<MyPoint> mystack; // a stack to keep track of pixels in the same segment
	bool *pixelvisited = new bool[cols*rows]; // creates an array to keep track of which pixels have been visited
	bool *shapelayer = new bool[cols*rows]; // a bool array that marks the positions corresponding to the current shape

	memset(pixelvisited, 0, cols*rows*sizeof(bool));
	vector<MyPoint> importantPoints; // this list contains the top 12 shapes. top shape(s) will be discarded.

	// go through all the shapes, discard the top 1 and anything that covers more than 20% of the image
	// and then take the top 10.
	while(FindUnvisitedPixel(px, pixelvisited, cols, rows, &x, &y))
	{
		shapecount++;
		MyPoint pt1 = GetPoint(x, y); // this also sets the size to 1
		mystack.push_back(pt1); // push the starting point into the stack

		pixelvisited[y*cols + x] = true; // mark the starting pixel as visited
		Color mycolor = GetPxColor(px, cols, x, y);

		while(!mystack.empty())
		{
			MyPoint pt = mystack.back(); mystack.pop_back(); // pop out an element from the stack
			int x = pt.x;
			int y = pt.y;
		
			if (VisitPixel(x,   y-1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // north
			if (VisitPixel(x+1, y-1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // north-east
			if (VisitPixel(x+1, y,   px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // east
			if (VisitPixel(x+1, y+1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // south-east
			if (VisitPixel(x,   y+1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // south
			if (VisitPixel(x-1, y+1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // south-west
			if (VisitPixel(x-1, y,   px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // west
			if (VisitPixel(x-1, y-1, px, pixelvisited, cols, rows, mystack, mycolor)) { pt1.size++; } // north-west
		}

		if (importantPoints.size() < NumImportantShapes) // if the vector doesn't have enough shapes yet
		{
			importantPoints.push_back(pt1);
			sort(importantPoints.begin(), importantPoints.end(), compare_mypoints);
		}
		else
		{
			if (pt1.size > importantPoints.back().size) // if our current shape size is larger
			{
				importantPoints.pop_back();
				importantPoints.push_back(pt1);
				sort(importantPoints.begin(), importantPoints.end(), compare_mypoints);
			}
		}
	}	

	memset(pixelvisited, 0, cols*rows*sizeof(bool));

	for (unsigned int i=0; i<importantPoints.size(); i++)
	//while(FindUnvisitedPixel(px, pixelvisited, cols, rows, &x, &y))
	{
		int x = importantPoints[i].x;
		int y = importantPoints[i].y;

		MyPoint pt1 = GetPoint(x, y);
		mystack.push_back(pt1); // push the starting point into the stack

		memset(shapelayer, 0, cols*rows*sizeof(bool));

		pixelvisited[y*cols + x] = true; // mark the starting pixel as visited
		shapelayer[y*cols + x] = true;
		Color mycolor = GetPxColor(px, cols, x, y);

		while(!mystack.empty())
		{
			MyPoint pt = mystack.back(); mystack.pop_back(); // pop out an element from the stack
			int x = pt.x;
			int y = pt.y;
		
			VisitPixelAndMarkShape(x,   y-1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // north
			VisitPixelAndMarkShape(x+1, y-1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // north-east
			VisitPixelAndMarkShape(x+1, y,   px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // east
			VisitPixelAndMarkShape(x+1, y+1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // south-east
			VisitPixelAndMarkShape(x,   y+1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // south
			VisitPixelAndMarkShape(x-1, y+1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // south-west
			VisitPixelAndMarkShape(x-1, y,   px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // west
			VisitPixelAndMarkShape(x-1, y-1, px, pixelvisited, shapelayer, cols, rows, mystack, mycolor); // north-west
		}

		// at this point, the shape is ready.
		unsigned int descriptor1 = FindColorDescriptor(px, shapelayer, importantPoints[i].size, rows, cols, x, y);
		unsigned int descriptor2 = (unsigned int)(FindEccentricityDescriptor(px, shapelayer, importantPoints[i].size, rows, cols, x, y) * 65536); // arbit
		unsigned int descriptor3 = (unsigned int)FindCentralityDescriptor(px, shapelayer, importantPoints[i].size, rows, cols, x, y);
		unsigned int descriptor4 = FindMomentDescriptor(px, shapelayer, importantPoints[i].size, rows, cols, x, y);
		unsigned int descriptor5 = FindShapeLibraryDescriptor(px, shapelayer, rows, cols, x, y);

		cout << "------------------------" << endl;
		cout << "Color Descr:   \t" << descriptor1 << endl;
		cout << "Eccentricity:  \t" << descriptor2 << endl;
		cout << "Centrality:    \t" << descriptor3 << endl;
		cout << "Moment(2, 1):  \t" << descriptor4 << endl;
		cout << "Shape Library: \t" << descriptor5 << endl;

		indexFile << filename << "," << descriptor1 << "," << descriptor2 << "," << descriptor3 << "," << descriptor4 << "," << descriptor5 << endl; 
	}



	delete[] pixelvisited;
	delete[] shapelayer;
}
unsigned int FindColorDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y)
{
	double redComponent = 0.0;
	double greenComponent = 0.0;
	double blueComponent = 0.0;

	ColorRGB pixelColorOrig;

	for(int row = 0; row < rows; row++)
	{
      	for(int col=0; col < cols; col++)
		{
			if(shapelayer[row*cols + col])
			{
				// Here, insert the code to calculate the average of color pixels in the original image
				pixelColorOrig = pc[row*cols + col];
				redComponent += pixelColorOrig.red();
				greenComponent += pixelColorOrig.green();
				blueComponent += pixelColorOrig.blue();
			}
		}
	}
	redComponent   = redComponent/size;
	greenComponent = greenComponent/size;
	blueComponent  = blueComponent/size;

	int redn = (int)(redComponent*(1<<16));
	int greenn = (int)(greenComponent*(1<<8));
	int bluen = (int)(blueComponent*(1<<8));
	
	return color_riffle(redn, greenn, bluen);
}

static unsigned int color_riffle(int red, int green, int blue)
{
	int descriptor = 0;

	descriptor |= (red   & 0x8000) ? 0x80000000 : 0;
	descriptor |= (red   & 0x4000) ? 0x40000000 : 0;
	descriptor |= (green & 0x80)   ? 0x20000000 : 0;
	descriptor |= (blue  & 0x80)   ? 0x10000000 : 0;
	
	descriptor |= (red   & 0x2000) ? 0x08000000 : 0;
	descriptor |= (red   & 0x1000) ? 0x04000000 : 0;
	descriptor |= (green & 0x40)   ? 0x02000000 : 0;
	descriptor |= (blue  & 0x40)   ? 0x01000000 : 0;
	
	descriptor |= (red   & 0x0800) ? 0x00800000 : 0;
	descriptor |= (red   & 0x0400) ? 0x00400000 : 0;
	descriptor |= (green & 0x20)   ? 0x00200000 : 0;
	descriptor |= (blue  & 0x20)   ? 0x00100000 : 0;
	
	descriptor |= (red   & 0x0200) ? 0x00080000 : 0;
	descriptor |= (red   & 0x0100) ? 0x00040000 : 0;
	descriptor |= (green & 0x10)   ? 0x00020000 : 0;
	descriptor |= (blue  & 0x10)   ? 0x00010000 : 0;
	
	descriptor |= (red   & 0x0080) ? 0x00008000 : 0;
	descriptor |= (red   & 0x0040) ? 0x00004000 : 0;
	descriptor |= (green & 0x08)   ? 0x00002000 : 0;
	descriptor |= (blue  & 0x08)   ? 0x00001000 : 0;
	
	descriptor |= (red   & 0x0020) ? 0x00000800 : 0;
	descriptor |= (red   & 0x0010) ? 0x00000400 : 0;
	descriptor |= (green & 0x04)   ? 0x00000200 : 0;
	descriptor |= (blue  & 0x04)   ? 0x00000100 : 0;
	
	descriptor |= (red   & 0x0008) ? 0x00000080 : 0;
	descriptor |= (red   & 0x0004) ? 0x00000040 : 0;
	descriptor |= (green & 0x02)   ? 0x00000020 : 0;
	descriptor |= (blue  & 0x02)   ? 0x00000010 : 0;
	
	descriptor |= (red   & 0x0002) ? 0x00000008 : 0;
	descriptor |= (red   & 0x0001) ? 0x00000004 : 0;
	descriptor |= (green & 0x01)   ? 0x00000002 : 0;
	descriptor |= (blue  & 0x01)   ? 0x00000001 : 0;

	return descriptor;
}


unsigned int FindShapeLibraryDescriptor(const PixelPacket *pc, const bool *shapelayer, int rows, int cols, int xx, int yy)
{
	// step 1: find the bounds of the shape
	int shapeminx = INT_MAX, shapeminy = INT_MAX, shapemaxx = INT_MIN, shapemaxy = INT_MIN; 
	for (int y=0; y<rows; y++)
	{
		for (int x=0; x<cols; x++)
		{
			if (*(shapelayer + y*cols + x))
			{
				shapeminx = min(shapeminx, x);
				shapemaxx = max(shapemaxx, x);
				shapeminy = min(shapeminy, y);
				shapemaxy = max(shapemaxy, y);
			}
		}
	}

	// step 2: create an image and blt it to that
	int shpwidth = shapemaxx - shapeminx;
	int shpheight = shapemaxy - shapeminy;
	Image shp(Geometry(shpwidth, shpheight), ColorMono(false));
	shp.modifyImage();
	PixelPacket *shpPix = shp.getPixels(0, 0, shpwidth, shpheight);
	for (int y=0; y<shpheight; y++)
	{
		for (int x=0; x<shpwidth; x++)
		{
			if ((*(shapelayer + (y+shapeminy)*cols + (x+shapeminx))))
			{
				*(shpPix +y*shpwidth + x)= ColorMono(true);
			}
		}
	}
	
	// step 3: resize the image to standard size
	shp.transform(Geometry(32, 32));
	// bug ImageMagick ignores the gravity parameter
	//shp.extent(Geometry(32, 32), ColorMono(false), GravityType::SouthGravity);

	Image shp2(Geometry(32, 32), ColorMono(false));
	shp2.composite(shp, GravityType::CenterGravity, CompositeOperator::OverCompositeOp);

	// step 2a: debug ... write it to a file so we can see how it looks
	char szFilename[200];
	sprintf(szFilename, "MyShape_%d.png", foo);
	foo++;
	//shp2.write(szFilename);

	// step 4: go through the shape library, trying to find max overlap
	if (shapeLibrary1 == NULL){	shapeLibrary1 = new Image("shapeLibrary1.png");	}
	if (shapeLibrary2 == NULL){	shapeLibrary2 = new Image("shapeLibrary2.png");	}
	if (shapeLibrary3 == NULL){	shapeLibrary3 = new Image("shapeLibrary3.png");	}
	if (shapeLibrary4 == NULL){	shapeLibrary4 = new Image("shapeLibrary4.png");	}

	const PixelPacket *shplib1 = shapeLibrary1->getConstPixels(0, 0, shapeLibrary1->columns(), shapeLibrary1->rows());
	const PixelPacket *shplib2 = shapeLibrary2->getConstPixels(0, 0, shapeLibrary2->columns(), shapeLibrary2->rows());
	const PixelPacket *shplib3 = shapeLibrary3->getConstPixels(0, 0, shapeLibrary3->columns(), shapeLibrary3->rows());
	const PixelPacket *shplib4 = shapeLibrary4->getConstPixels(0, 0, shapeLibrary4->columns(), shapeLibrary4->rows());

	const PixelPacket *shppx = shp2.getConstPixels(0, 0, shp2.columns(), shp2.rows());

	int byte1 = GetShapeLibraryByte(shplib1, shppx, shp2.columns(), shp2.rows());
	int byte2 = GetShapeLibraryByte(shplib2, shppx, shp2.columns(), shp2.rows());
	int byte3 = GetShapeLibraryByte(shplib3, shppx, shp2.columns(), shp2.rows());
	int byte4 = GetShapeLibraryByte(shplib4, shppx, shp2.columns(), shp2.rows());
	
	//printf("the best match was: (%d, %d, %d, %d)\n", byte1, byte2, byte3, byte4);

	return riffle(byte1, byte2, byte3, byte4);
}

int GetShapeLibraryByte(const PixelPacket* shapeLibrary, const PixelPacket* shape, int shapeCols, int shapeRows)
{
	int libmaxX = 0;
	int libmaxY = 0;
	int libcounterMax = 0;

	for (int liby=0; liby<8; liby++)
	{
		for (int libx=0; libx<8; libx++)
		{
			int overlap = FindOverlap(shapeLibrary, libx, liby, shape, shapeCols, shapeRows);

			if (overlap > libcounterMax)
			{
				libmaxX = libx;
				libmaxY = liby;
				libcounterMax = overlap;
			}
		}
	}

	//printf("inside byte: %d, %d\n", libmaxX, libmaxY);
	return (libmaxX << 4) | libmaxY;
}

int FindOverlap(const PixelPacket* shapeLibrary, int libx, int liby, const PixelPacket* shape, int shpCols, int shpRows)
{
	int startx = (32 - shpCols)/2;
	int starty = (32 - shpRows)/2;
	int counter = 0;

	for (int y=0; y<shpRows; y++)
	{
		for (int x=0; x<shpCols; x++)
		{
			if (((shapeLibrary + (liby*32 + starty + y)*256 + (libx*32 + startx + x))->opacity <= 0)) // shape library
			{
				if((shape + y*shpCols + x)->red > 0) // sort-of XOR with shape (dont count if shapeLibrary is false)
				{
					counter++;
					//*(ptix + y*32 + x) = ColorMono(false);
				}
				else
				{
					counter--;
					//*(ptix + y*32 + x) = ColorMono(true);
				}
			}
			else if((shape +  y*shpCols + x)->red > 0) // negate if shape isnt but shape is
			{
				counter--;
			}

		}
	}

	//char szShapeMatch[200];
	//sprintf(szShapeMatch, "Shape(%d)_Lib(%d, %d).png", foo, libx, liby);
	//tim.write(szShapeMatch);

	return counter;
}

bool FindUnvisitedPixel(const PixelPacket *pc, const bool *pixelvisited, int cols, int rows, int *px, int *py)
{
	int x, y;
	bool reply = false;

	// iterate through the whole image till we find a pixel that has not been visited yet.
	for (y=0; (!reply) && y<rows; y++)
	{
		for (x=0; (!reply) && x<cols; x++)
		{
			if (!pixelvisited[y*cols + x])
			{
				reply = true;
				*px = x;
				*py = y;
			}
		}
	}
	
	return reply;
}

bool IsInBounds(int x, int y, int cols, int rows)
{
	return ( (x >= 0) && (x<cols) && (y >= 0) && (y<rows) );
}

Color GetPxColor(const PixelPacket *px, int cols, int x, int y)
{
	return *(px + y*cols + x);
}

MyPoint GetPoint(int x, int y)
{
	MyPoint pt;
	pt.x = x;
	pt.y = y;
	pt.size = 1;
	return pt;
}

void VisitPixelAndMarkShape(int x, int y, const PixelPacket *px, bool *pixelvisited, bool *shapelayer, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor)
{
	// if the pixel is within bounds, hasn't been visited yet, and is the same color as me
	if (IsInBounds(x, y, cols, rows) && (!pixelvisited[y*cols+x]) && mycolor == GetPxColor(px, cols, x, y))
	{
		mystack.push_back(GetPoint(x, y)); // add to the stack
		pixelvisited[y*cols + x] = true; // mark as visited
		shapelayer[y*cols + x] = true;
	}
}

bool VisitPixel(int x, int y, const PixelPacket *px, bool *pixelvisited, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor)
{
	// if the pixel is within bounds, hasn't been visited yet, and is the same color as me
	if (IsInBounds(x, y, cols, rows) && (!pixelvisited[y*cols+x]) && mycolor == GetPxColor(px, cols, x, y))
	{
		mystack.push_back(GetPoint(x, y)); // add to the stack
		pixelvisited[y*cols + x] = true; // mark as visited
		return true;
	}

	return false;
}

void FloodFill(const PixelPacket *px, bool *pixelvisited, int cols, int rows, int x, int y)
{
	vector<MyPoint> mystack; // a stack to keep track of pixels in the same segment
	MyPoint pt1 = GetPoint(x, y);
	mystack.push_back(pt1);
	pixelvisited[y*cols + x] = true; // mark the starting pixel as visited
	Color mycolor = GetPxColor(px, cols, x, y);

	while(!mystack.empty())
	{
		MyPoint pt = mystack.back(); mystack.pop_back(); // pop out an element from the stack
		int x = pt.x;
		int y = pt.y;
		
		VisitPixel(x, y-1, px, pixelvisited, cols, rows, mystack, mycolor);   // north
		VisitPixel(x+1, y-1, px, pixelvisited, cols, rows, mystack, mycolor); // north-east
		VisitPixel(x+1, y, px, pixelvisited, cols, rows, mystack, mycolor);   // east
		VisitPixel(x+1, y+1, px, pixelvisited, cols, rows, mystack, mycolor); // south-east
		VisitPixel(x, y+1, px, pixelvisited, cols, rows, mystack, mycolor);   // south
		VisitPixel(x-1, y+1, px, pixelvisited, cols, rows, mystack, mycolor); // south-west
		VisitPixel(x-1, y, px, pixelvisited, cols, rows, mystack, mycolor);   // west
		VisitPixel(x-1, y-1, px, pixelvisited, cols, rows, mystack, mycolor); // north-west
	}
}

int CountSegments(const PixelPacket *pc, int cols, int rows)
{
	int x=0, y=0;
	bool *pixelvisited = new bool[cols*rows]; // creates an array to keep track of which pixels have been visited
	memset(pixelvisited, 0, cols*rows*sizeof(bool));
	int count = 0;

	// till no pixels are unvisited
	while(FindUnvisitedPixel(pc, pixelvisited, cols, rows, &x, &y))
	{
		count++;
		FloodFill(pc, pixelvisited, cols, rows, x, y); // mark that pixel and all pixels in the same segment as visited
	}

	return count;
}

bool compare_mypoints (MyPoint a, MyPoint b)
{
	return a.size > b.size;
}

static unsigned int riffle(int byte1, int byte2, int byte3, int byte4)
{
	int descriptor = 0;

	descriptor |= (byte1 & 0x80) ? 0x80000000 : 0;
	descriptor |= (byte2 & 0x80) ? 0x40000000 : 0;
	descriptor |= (byte3 & 0x80) ? 0x20000000 : 0;
	descriptor |= (byte4 & 0x80) ? 0x10000000 : 0;
	
	descriptor |= (byte1 & 0x40) ? 0x08000000 : 0;
	descriptor |= (byte2 & 0x40) ? 0x04000000 : 0;
	descriptor |= (byte3 & 0x40) ? 0x02000000 : 0;
	descriptor |= (byte4 & 0x40) ? 0x01000000 : 0;
	
	descriptor |= (byte1 & 0x20) ? 0x00800000 : 0;
	descriptor |= (byte2 & 0x20) ? 0x00400000 : 0;
	descriptor |= (byte3 & 0x20) ? 0x00200000 : 0;
	descriptor |= (byte4 & 0x20) ? 0x00100000 : 0;
	
	descriptor |= (byte1 & 0x10) ? 0x00080000 : 0;
	descriptor |= (byte2 & 0x10) ? 0x00040000 : 0;
	descriptor |= (byte3 & 0x10) ? 0x00020000 : 0;
	descriptor |= (byte4 & 0x10) ? 0x00010000 : 0;
	
	descriptor |= (byte1 & 0x08) ? 0x00008000 : 0;
	descriptor |= (byte2 & 0x08) ? 0x00004000 : 0;
	descriptor |= (byte3 & 0x08) ? 0x00002000 : 0;
	descriptor |= (byte4 & 0x08) ? 0x00001000 : 0;
	
	descriptor |= (byte1 & 0x04) ? 0x00000800 : 0;
	descriptor |= (byte2 & 0x04) ? 0x00000400 : 0;
	descriptor |= (byte3 & 0x04) ? 0x00000200 : 0;
	descriptor |= (byte4 & 0x04) ? 0x00000100 : 0;
	
	descriptor |= (byte1 & 0x02) ? 0x00000080 : 0;
	descriptor |= (byte2 & 0x02) ? 0x00000040 : 0;
	descriptor |= (byte3 & 0x02) ? 0x00000020 : 0;
	descriptor |= (byte4 & 0x02) ? 0x00000010 : 0;
	
	descriptor |= (byte1 & 0x01) ? 0x00000008 : 0;
	descriptor |= (byte2 & 0x01) ? 0x00000004 : 0;
	descriptor |= (byte3 & 0x01) ? 0x00000002 : 0;
	descriptor |= (byte4 & 0x01) ? 0x00000001 : 0;

	return descriptor;
}
