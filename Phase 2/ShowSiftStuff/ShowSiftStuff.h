
// ShowSiftStuff.h : main header file for the ShowSiftStuff application
//
#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"       // main symbols


// CShowSiftStuffApp:
// See ShowSiftStuff.cpp for the implementation of this class
//

class CShowSiftStuffApp : public CWinApp
{
public:
	CShowSiftStuffApp();


// Overrides
public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();

// Implementation

public:
	afx_msg void OnAppAbout();
	DECLARE_MESSAGE_MAP()
};

extern CShowSiftStuffApp theApp;
