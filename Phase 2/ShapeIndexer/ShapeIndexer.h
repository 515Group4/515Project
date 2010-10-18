typedef struct MyPoint_t
{
	int x;
	int y;
} MyPoint;

bool IsInBounds(int x, int y, int cols, int rows);
Color GetPxColor(const PixelPacket *px, int cols, int x, int y);
MyPoint GetPoint(int x, int y);
void VisitPixel(int x, int y, const PixelPacket *px, bool *pixelvisited, int cols, int rows, vector<MyPoint>& mystack, const Color& mycolor);
int CountSegments(const PixelPacket *pc, int cols, int rows);
bool FindUnvisitedPixel(const PixelPacket *pc, const bool *pixelvisited, int cols, int rows, int *px, int *py);