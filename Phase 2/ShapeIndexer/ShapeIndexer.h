#ifndef _SHAPEINDEXER_H__
#define _SHAPEINDEXER_H__

typedef struct MyPoint_t
{
	int x;
	int y;
	int size;
} MyPoint;

void	IndexDirectory(const char *foldername, const char *indexFile);
bool	IsInBounds(int x, int y, int cols, int rows);
Color	GetPxColor(const PixelPacket *px, int cols, int x, int y);
MyPoint GetPoint(int x, int y);
bool	VisitPixel(int x, int y, const PixelPacket *px, bool *pixelvisited, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor);
int		CountSegments(const PixelPacket *pc, int cols, int rows);
bool	FindUnvisitedPixel(const PixelPacket *pc, const bool *pixelvisited, int cols, int rows, int *px, int *py);
void	VisitPixelAndMarkShape(int x, int y, const PixelPacket *px, bool *pixelvisited, bool *shapelayer, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor);
void	IterateThroughEachShape(const PixelPacket *pc, int cols, int rows, ofstream& indexFile, const char* filename);
int		FindOverlap(const PixelPacket* shapeLibrary, int libx, int liby, const PixelPacket* shape, int shpCols, int shpRows);
int		GetShapeLibraryByte(const PixelPacket* shapeLibrary, const PixelPacket* shape, int shapeCols, int shapeRows);
void	IndexImage(const char* foldername, const char* filename, ofstream& indexFile);
bool	compare_mypoints (MyPoint a, MyPoint b);

static unsigned int riffle(int byte1, int byte2, int byte3, int byte4);
static unsigned int color_riffle(int red, int green, int blue);
unsigned int		FindShapeLibraryDescriptor(const PixelPacket *pc, const bool *shapelayer, int rows, int cols, int x, int y);
unsigned int		FindColorDescriptor(const PixelPacket *pc, const bool *shapelayer, int size, int rows, int cols, int x, int y);
#endif //_SHAPEINDEXER_H__
