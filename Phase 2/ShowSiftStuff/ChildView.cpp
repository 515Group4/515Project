
// ChildView.cpp : implementation of the CChildView class
//

#include "stdafx.h"
#include "ShowSiftStuff.h"
#include "ChildView.h"
#include <iostream>
#include <fstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CChildView
CButton *pBtn1 = NULL;
CButton *pBtnLoadSift = NULL;
Gdiplus::Bitmap *pMainImage = NULL;
Gdiplus::Bitmap *pSiftImage = NULL;

CChildView::CChildView()
{
	pBtn1 = new CButton();
	pBtnLoadSift = new CButton();
}

CChildView::~CChildView()
{
	if (pBtn1) pBtn1->DestroyWindow();
	delete pBtn1;
	delete pMainImage;
	delete pSiftImage;
}


BEGIN_MESSAGE_MAP(CChildView, CWnd)
	ON_WM_PAINT()
	ON_WM_CREATE()
	ON_BN_CLICKED(1, LoadImageClicked)
	ON_BN_CLICKED(2, LoadSiftClicked)
END_MESSAGE_MAP()



// CChildView message handlers

BOOL CChildView::PreCreateWindow(CREATESTRUCT& cs) 
{
	if (!CWnd::PreCreateWindow(cs))
		return FALSE;

	cs.dwExStyle |= WS_EX_CLIENTEDGE;
	cs.style &= ~WS_BORDER;
	cs.lpszClass = AfxRegisterWndClass(CS_HREDRAW|CS_VREDRAW|CS_DBLCLKS, 
		::LoadCursor(NULL, IDC_ARROW), reinterpret_cast<HBRUSH>(COLOR_WINDOW+1), NULL);

	return TRUE;
}

void CChildView::OnPaint() 
{
	CPaintDC dc(this); // device context for painting
	Gdiplus::Graphics g(dc.GetSafeHdc());
	if (pMainImage)
	{
		g.DrawImage(pMainImage, 100, 100, pMainImage->GetWidth(), pMainImage->GetHeight());
	}
	if (pSiftImage)
	{
		g.DrawImage(pSiftImage, 100, 100, pSiftImage->GetWidth(), pSiftImage->GetHeight());
	}
}

int CChildView::OnCreate(LPCREATESTRUCT createStruct)
{
	// Add the "Load Image" button
	pBtn1->Create(L"Load Image", WS_CHILD|WS_VISIBLE|WS_TABSTOP|BS_PUSHBUTTON, CRect(10, 10, 85, 33), this, 1);
	if (pBtn1->GetSafeHwnd())
	{
		// Deprecated - can use SystemParametersInfo with SPI_GETNONCLIENTMETRICS followed by a CreateFontIndirectW on the LOGFONT
		HFONT hFont = (HFONT)GetStockObject(DEFAULT_GUI_FONT);
		pBtn1->SendMessage(WM_SETFONT, (WPARAM)hFont, MAKELPARAM(1, 0));
	}

	// Add the "Load Sift" button
	pBtnLoadSift->Create(L"Load Sift", WS_CHILD|WS_VISIBLE|WS_TABSTOP|BS_PUSHBUTTON, CRect(91, 10, 91+75, 33), this, 2);
	if (pBtn1->GetSafeHwnd())
	{
		// Deprecated - can use SystemParametersInfo with SPI_GETNONCLIENTMETRICS followed by a CreateFontIndirectW on the LOGFONT
		HFONT hFont = (HFONT)GetStockObject(DEFAULT_GUI_FONT);
		pBtnLoadSift->SendMessage(WM_SETFONT, (WPARAM)hFont, MAKELPARAM(1, 0));
	}
	
	return 0;
}

void CChildView::LoadImageClicked()
{
	pMainImage = Gdiplus::Bitmap::FromFile(L"C:\\Data\\Code\\Cplusplus\\ImageMagicTry1\\imagemagick_tests\\mytest.png");
	InvalidateRect(NULL, TRUE);
	UpdateWindow();
}

void CChildView::LoadSiftClicked()
{
	if (pMainImage)
	{
		// create a render target
		pSiftImage = new Gdiplus::Bitmap(pMainImage->GetWidth(), pMainImage->GetHeight(), PixelFormat32bppARGB);
		Gdiplus::Graphics g(pSiftImage);
		Gdiplus::Pen p(Gdiplus::Color::Pink);
		Gdiplus::SolidBrush b(Gdiplus::Color::Blue);

		// load the sift file
		std::ifstream siftfile(L"C:\\Data\\Code\\Cplusplus\\ImageMagicTry1\\imagemagick_tests\\mytest.txt");
		if (siftfile.is_open())
		{
			float x, y, radius, angle;
			int temp;

			while(!siftfile.eof())
			{
				siftfile >> x >> y >> radius >> angle;
				for (int i=0; i<128; i++) siftfile >> temp;

				g.DrawEllipse(&p, x-radius, y-radius, 2*radius, 2*radius);
				g.FillEllipse(&b, x-1, y-1, 2.0F, 2.0F);
			}

			siftfile.close();
		}

		InvalidateRect(NULL, TRUE);
		UpdateWindow();
	}
}
