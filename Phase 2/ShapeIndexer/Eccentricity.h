#ifndef _ECCENTRICITY_H__
#define _ECCENTRICITY_H__

double	FindEccentricityDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y);
double	FindCentralityDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y);
unsigned int FindMomentDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y);


int perimeter(const bool *shapelayer, int imageLength, int imageWidth, int x, int y);

#endif //_ECCENTRICITY_H__
