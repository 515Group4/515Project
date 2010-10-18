#include "stdafx.h"

using namespace Magick;
using namespace std;

#include "ShapeIndexer.h"

bool FindUnvisitedPixel(const PixelPacket *pc, const bool *pixelvisited, int cols, int rows, int *px, int *py);

int main(int argc, char* argv[])
{
    string s;

    Magick::InitializeMagick("E:\\Programs\\Dev\\ImageMagick\\");

    if (argc > 1)
    {
        s = string(argv[1]);
    }
    else
    {
        s = "button.gif";
    }

    Image img(s.c_str());
    img.quantizeColorSpace(YUVColorspace);
	img.segment(1.0, 1.5); // segments the image with the parameter values 1.0 and 1.5
	img.modifyImage(); // ensures that no other references to this image exist

	int cols = img.columns();
	int rows = img.rows();
	const PixelPacket *pc = img.getConstPixels(0, 0, cols, rows); // gets a pointer to the pixels of the image

	int numSegments = CountSegments(pc, cols, rows); // counts the number of segments
	cout << "Number of segments is: " << numSegments << "\r\n"; // prints to the console

	return 0;
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
	return pt;
}

void VisitPixel(int x, int y, const PixelPacket *px, bool *pixelvisited, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor)
{
	// if the pixel is within bounds, hasn't been visited yet, and is the same color as me
	if (IsInBounds(x, y, cols, rows) && (!pixelvisited[y*cols+x]) && mycolor == GetPxColor(px, cols, x, y))
	{
		mystack.push_back(GetPoint(x, y)); // add to the stack
		pixelvisited[y*cols + x] = true; // mark as visited
	}
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