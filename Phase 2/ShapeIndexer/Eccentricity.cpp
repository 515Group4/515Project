#include "stdafx.h"

#define PI (3.141592653589793)

using namespace std;
using namespace Magick;

#include "Eccentricity.h"

// 2D Array to store the status (whether the pixel has been visited or not) of pixels of an image of size upto 1000x1000
// sushovan: copy the shapelayer to segArr, since it needs to be modified.
// I have changed the data type of segArr to char, so that it matches the sizeof bool and we can do a fast memcpy
char segArr [1000][1000];

double FindEccentricityDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y)
{
	int totalPixels = size;

	int imageLength = rows;
	int imageWidth = cols;

	double centerOfMass_X = 0;
	double centerOfMass_Y = 0;

	double X_Total = 0;
	double Y_Total = 0;

	double Periapsis = 0;
	double Apoapsis = 0;

	double iEccentricity = 0;
	double tempDistance = 0;
	int boundarySize = 0;
	bool firstTime = true;

	int i =0;
	
	for(int row = 0; row < imageLength; row++) //sushovan: flipped length and width here -- app was crashing
	{
		for(int col=0; col < imageWidth; col++)
		{
			if(shapelayer[row*cols + col])
			{
				Y_Total += row;
				X_Total += col;
			}
		}
	}
	
	centerOfMass_X = X_Total/totalPixels;
	centerOfMass_Y = Y_Total/totalPixels;

	//cout<<"Center of Mass = ("<<centerOfMass_X<<" , "<<centerOfMass_Y<<")"<<endl;


	//Calculation of Apoapsis and Periapsis
	boundarySize = perimeter(shapelayer, rows, cols, x, y);
	for(int row = 0; row < imageWidth; row++)
	{
		for(int col=0; col < imageLength; col++)
		{
			if(segArr[row][col] == 8)
			{
				tempDistance = sqrt( pow((centerOfMass_X-row),2) + pow((centerOfMass_Y-col),2));

				if(firstTime)
				{
					Periapsis = tempDistance;
					firstTime = false;
				}

				if(tempDistance > Apoapsis)
					Apoapsis = tempDistance;

				if(tempDistance < Periapsis)
					Periapsis = tempDistance;
			}
		}
	}

	//cout<<"Apoapsis = " <<Apoapsis<<endl;
	//cout<<"Periapsis = "<<Periapsis<<endl;
	
	iEccentricity = (Apoapsis - Periapsis) / (Apoapsis + Periapsis);

	//cout<<"\n\nEccentricity = "<<iEccentricity<<endl;

	return iEccentricity;
}

int perimeter(const bool *shapelayer, int imageLength, int imageWidth, int x, int y)
{
	// Two stacks to store the keep track of pixel positions of an image
	ColorYUV pixelColor;
	stack <int> stX;
	stack <int> stY;
	int row = x;
	int col = y;
	int segLabel = 1;

	int boundaryLabel = 8;

	for (int yy=0; yy<imageLength; yy++)
	{
		memcpy(segArr[yy], &shapelayer[yy*imageWidth], imageWidth*sizeof(bool));
	}

	int i = 0;	
	stX.push(x);
	stY.push(y);
	segArr[x][y] = segLabel;

	int checked = 9;
	while((!stX.empty()) || (!stY.empty()))// stack not empty
	{
		// Pop out the top pixel position
		int rr = stX.top();
		int cc = stY.top();	

		stX.pop();
		stY.pop();
		if((segLabel == segArr[rr][cc])/* && (boundaryLabel != segArr[rr][cc]) && (checked != segArr[rr][cc])*/)
		{
			////cout<<"Entered "<<i<<"th time"<<endl;
			//i++;
			//// Traverse all possible surrounding pixels (8 pixels based on same color and boundary conditions) in the image and push them into stacks
			if(((rr<(imageWidth-1)) && (segArr[rr+1][cc] == 0)) || (rr == (imageWidth-1))){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((rr<(imageWidth-1)) && (segArr[rr+1][cc] ==segLabel))
			{
				stX.push(rr+1);
				stY.push(cc);
			}

			if(((rr>0) && (segArr[rr-1][cc] == 0)) || (rr == 0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((rr>0) && (segArr[rr-1][cc] ==segLabel))
			{
				stX.push(rr-1);
				stY.push(cc);
			}

			if(((cc<imageLength-1) && (segArr[rr][cc+1] == 0)) || (cc == (imageLength -1))){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((cc<imageLength-1) && (segArr[rr][cc+1] ==segLabel))
			{
				stX.push(rr);
				stY.push(cc+1);
			}

			if(((cc>0) && (segArr[rr][cc-1] == 0)) || (cc ==0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((cc>0) && (segArr[rr][cc-1] ==segLabel))
			{
				stX.push(rr);
				stY.push(cc-1);
			}

			if((cc<imageLength-1) && (rr<imageWidth-1) && (segArr[rr+1][cc+1] == 0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((cc<imageLength-1) && (rr<imageWidth-1) && (segArr[rr+1][cc+1] ==segLabel))
			{
				stX.push(rr+1);
				stY.push(cc+1);
			}

			if(cc>0 && (rr<imageWidth-1) && (segArr[rr+1][cc-1] == 0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if(cc>0 && (rr<imageWidth-1) && (segArr[rr+1][cc-1] ==segLabel))
			{
				stX.push(rr+1);
				stY.push(cc-1);
			}

			if(cc>0 && rr>0 && (segArr[rr-1][cc-1] == 0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if(cc>0 && rr>0 && (segArr[rr-1][cc-1] ==segLabel))
			{
				stX.push(rr-1);
				stY.push(cc-1);
			}
			if((cc<imageLength-1) && rr>0 && (segArr[rr-1][cc+1] == 0)){
				
				segArr[rr][cc] = boundaryLabel;
			}
			else if((cc<imageLength-1) && rr>0 && (segArr[rr-1][cc+1] ==segLabel) )
			{
				stX.push(rr-1);
				stY.push(cc+1);
			}
			if(segLabel == segArr[rr][cc])
			{
				segArr[rr][cc] = checked;
			}
		}
		
	}

	//Count the number of boundary pixels in the segment
	int boundarySize = 0;
	//FILE* file2 = fopen("f2.txt","w");
	for(int row = 0; row < imageWidth; row++)
	{
		for(int col=0; col < imageLength; col++)
		{
			if(segArr[row][col] == boundaryLabel)
			{
				boundarySize++;
			}
			//fprintf(file2,"%d",segArr[row][col]);
		}
		//fprintf(file2,"\n");
	}
	//fclose(file2);

	//std::cout<<"Boundary Size = "<<boundarySize<<endl;

	return boundarySize;
}

double FindCentralityDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y)
{
	int iArea = 0;
	int iPerimeter = 0;
	double iCentrality = 0;

	iArea = size;
	iPerimeter = perimeter(shapelayer, rows, cols, x, y);

	iCentrality = (4 * PI * iArea) / (double)(iPerimeter * iPerimeter);

	//cout<<"Centrality = "<<iCentrality <<endl;

	return iCentrality;
}

unsigned int FindMomentDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y)
{
	unsigned int moment = 0;

	int Y_Total = 0;
	int X_Total = 0;
	int centerOfMass_X = 0;
	int centerOfMass_Y = 0;

	for(int row = 0; row < rows; row++) //sushovan: flipped length and width here -- app was crashing
	{
		for(int col=0; col < cols; col++)
		{
			if(shapelayer[row*cols + col])
			{
				Y_Total += row;
				X_Total += col;
			}
		}
	}
	
	centerOfMass_X = X_Total/size;
	centerOfMass_Y = Y_Total/size;

	for (size_t j=0; j<rows; j++) {
		for (size_t i=0; i<cols; i++) {
			if (shapelayer[j*cols + i]==1) {
				double x = i - centerOfMass_X;
				double y = j - centerOfMass_Y;
				int px = x*x;
				int qy = y<0 ? -y : y;
				moment+= px*qy;
			}
		}
	}

	return moment;
}