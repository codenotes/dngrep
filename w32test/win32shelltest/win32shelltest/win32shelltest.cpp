// win32shelltest.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "win32shelltest.h"
#include <string>
#include <Shtypes.h>
#include <Shlobj.h>
#include <assert.h>
#include <algorithm>
#include <thread>
#include <cstdlib>

#define MAX_LOADSTRING 100




static LPITEMIDLIST GetNextItemID(LPITEMIDLIST pidl)
{
	// Get the size of the specified item identifier.
	int cb = pidl->mkid.cb;

	// If the size is zero, it is the end of the list.
	if (cb == 0)
		return NULL;

	// Add cb to pidl (casting to increment by bytes).
	pidl = (LPITEMIDLIST)(((LPBYTE)pidl) + cb);

	// Return NULL if it is null-terminating or a pidl otherwise.
	return (pidl->mkid.cb == 0) ? NULL : pidl;
}


// CopyItemID - creates an item identifier list containing the first
//     item identifier in the specified list.
// Returns a PIDL if successful or NULL if out of memory.
static LPITEMIDLIST CopyItemID(LPITEMIDLIST pidl)
{
	LPMALLOC pMalloc;

	if (FAILED(SHGetMalloc(&pMalloc)))
		return NULL;

	// Get the size of the specified item identifier.
	int cb = pidl->mkid.cb;

	// Allocate a new item identifier list.
	LPITEMIDLIST pidlNew = (LPITEMIDLIST)pMalloc->Alloc(cb +
		sizeof(USHORT));
	if (pidlNew != NULL)

	{
		// Copy the specified item identifier.
		CopyMemory(pidlNew, pidl, cb);

		// Append a terminating zero.
		*((USHORT *)(((LPBYTE)pidlNew) + cb)) = 0;
	}

	pMalloc->Release();

	return pidlNew;
}

// return an IShellFolder given a pidl to the directory.
LPSHELLFOLDER GetIShellFolder(const LPITEMIDLIST pDirPidl)
{
	LPSHELLFOLDER pFolder;
	LPITEMIDLIST  pidl;
	LPMALLOC      pMalloc;


	if (pDirPidl == NULL)
		return NULL;

	if (FAILED(SHGetDesktopFolder(&pFolder)))
		return NULL;

	if (FAILED(SHGetMalloc(&pMalloc)))
	{
		pFolder->Release();
		return NULL;
	}

	// step thru the pidl, binding to each subdirectory in turn.
	// Process each item identifier in the list.
	for (pidl = pDirPidl; pidl != NULL; pidl = GetNextItemID(pidl))
	{
		LPSHELLFOLDER pSubFolder;
		LPITEMIDLIST pidlCopy;

		// Copy the item identifier to a list by itself.
		if ((pidlCopy = CopyItemID(pidl)) == NULL)
			break;

		// Bind to the subfolder.
		if (!SUCCEEDED(pFolder->BindToObject(
			pidlCopy, NULL,
			IID_IShellFolder, (void**)&pSubFolder)))
		{
			pMalloc->Free(pidlCopy);
			pFolder->Release();
			pFolder = NULL;
			break;
		}

		// Free the copy of the item identifier.
		pMalloc->Free(pidlCopy);

		// Release the parent folder and point to the
		// subfolder.
		pFolder->Release();
		pFolder = pSubFolder;
	}

	if (pMalloc)
		pMalloc->Release();

	// return  the last folder that was bound to.
	return pFolder;
}
// return an IShellFolder interface or null given the directory.
LPSHELLFOLDER GetIShellFolder(const char *pDirName)
{
	LPSHELLFOLDER pFolder = NULL;
	LPITEMIDLIST  DirPidl = NULL;
	LPMALLOC      pMalloc = NULL;
	ULONG eaten, attribs;
	USHORT wsz[MAX_PATH];

	// setup a shell malloc pointer for the helper funcs.
	if (FAILED(SHGetMalloc(&pMalloc)))
		return NULL;


	// the desktop folder.
	if (FAILED(SHGetDesktopFolder(&pFolder)))
	{
		pMalloc->Release();
		return NULL;
	}

	// Ensure that the string is Unicode.
	MultiByteToWideChar(CP_ACP, 0, pDirName, -1, wsz, MAX_PATH);

	// this should return a pidl to that directory.
	if (FAILED(pFolder->ParseDisplayName(NULL, 0, wsz, &eaten, &DirPidl,
		&attribs)))
	{
		pFolder->Release();
		pMalloc->Release();
		return NULL;
	}
	// done with the desktop folder.
	pFolder->Release();

	// get the folder we really want.
	pFolder = GetIShellFolder(DirPidl);

	// clean up
	pMalloc->Free(DirPidl);
	pMalloc->Release();


	// return  the last folder that was bound to.
	return pFolder;

}














// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];					// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];			// the main window class name

// Forward declarations of functions included in this code module:
ATOM				MyRegisterClass(HINSTANCE hInstance);
BOOL				InitInstance(HINSTANCE, int);
LRESULT CALLBACK	WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK	About(HWND, UINT, WPARAM, LPARAM);



//
//
// HRESULT WINAPI IUnknown_SetSite(
//        IUnknown *obj,        /* [in]   OLE object     */
//        IUnknown *site)       /* [in]   Site interface */
//{
//    HRESULT hr;
//    IObjectWithSite *iobjwithsite;
//    IInternetSecurityManager *isecmgr;
//
//    if (!obj) return E_FAIL;
//	IUnknown_QueryInterface
//    hr = IUnknown_QueryInterface(obj, &IID_IObjectWithSite, (LPVOID *)&iobjwithsite);
//   // TRACE("IID_IObjectWithSite QI ret=%08x, %p\n", hr, iobjwithsite);
//    if (SUCCEEDED(hr))
//    {
//    hr = IObjectWithSite_SetSite(iobjwithsite, site);
//    //TRACE("done IObjectWithSite_SetSite ret=%08x\n", hr);
//    IObjectWithSite_Release(iobjwithsite);
//    }
//    else
//    {
//    hr = IUnknown_QueryInterface(obj, &IID_IInternetSecurityManager, (LPVOID *)&isecmgr);
//    //TRACE("IID_IInternetSecurityManager QI ret=%08x, %p\n", hr, isecmgr);
//    if (FAILED(hr)) return hr;
//
//    //hr = IInternetSecurityManager_SetSecuritySite(isecmgr, (IInternetSecurityMgrSite *)site);
//    //TRACE("done IInternetSecurityManager_SetSecuritySite ret=%08x\n", hr);
//    //IInternetSecurityManager_Release(isecmgr);
//    }
//    return hr;
//}
//
//

//LRESULT TestOnContextMenu(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled)
// {
//     WORD                 x;
//     WORD                 y;
//     UINT                 uCommand;
//     DWORD                wFlags;
//     HMENU                hMenu;
//     BOOL                 fExplore;
//     HWND                 hwndTree;
//     CMINVOKECOMMANDINFO  cmi;
//     HRESULT              hResult;
// 
//     // for some reason I haven't figured out, we sometimes recurse into this method
//     if (pCM != NULL)
//         return 0;
// 
//     x = LOWORD(lParam);
//     y = HIWORD(lParam);
// 
//  //   TRACE("(%p)->(0x%08x 0x%08x) stub\n", this, x, y);
// 
//     fExplore = FALSE;
//     hwndTree = NULL;
// 
//     /* look, what's selected and create a context menu object of it*/
//     if (GetSelections())
//     {
//         pSFParent->GetUIObjectOf(hWndParent, cidl, (LPCITEMIDLIST*)apidl, IID_IContextMenu, NULL, (LPVOID *)&pCM);
// 
//         if (pCM)
//         {
//             TRACE("-- pContextMenu\n");
//             hMenu = CreatePopupMenu();
// 
//             if (hMenu)
//             {
//                 hResult = IUnknown_SetSite(pCM, (IShellView *)this);
// 
//                 /* See if we are in Explore or Open mode. If the browser's tree is present, we are in Explore mode.*/
//                 if (SUCCEEDED(pShellBrowser->GetControlWindow(FCW_TREE, &hwndTree)) && hwndTree)
//                 {
//                     TRACE("-- explore mode\n");
//                     fExplore = TRUE;
//                 }
// 
//                 /* build the flags depending on what we can do with the selected item */
//                 wFlags = CMF_NORMAL | (cidl != 1 ? 0 : CMF_CANRENAME) | (fExplore ? CMF_EXPLORE : 0);
// 
//                 /* let the ContextMenu merge its items in */
//                 if (SUCCEEDED(pCM->QueryContextMenu(hMenu, 0, FCIDM_SHVIEWFIRST, FCIDM_SHVIEWLAST, wFlags )))
//                 {
//                     if (FolderSettings.fFlags & FWF_DESKTOP)
//                         SetMenuDefaultItem(hMenu, FCIDM_SHVIEW_OPEN, MF_BYCOMMAND);
// 
//                     TRACE("-- track popup\n");
//                     uCommand = TrackPopupMenu(hMenu,
//                                               TPM_LEFTALIGN | TPM_RETURNCMD | TPM_LEFTBUTTON | TPM_RIGHTBUTTON,
//                                               x, y, 0, m_hWnd, NULL);
// 
//                     if (uCommand > 0)
//                     {
//                         TRACE("-- uCommand=%u\n", uCommand);
// 
//                         if (uCommand == FCIDM_SHVIEW_OPEN && pCommDlgBrowser.p != NULL)
//                         {
//                             TRACE("-- dlg: OnDefaultCommand\n");
//                             if (OnDefaultCommand() != S_OK)
//                                 OpenSelectedItems();
//                         }
//                         else
//                         {
//                             TRACE("-- explore -- invoke command\n");
//                             ZeroMemory(&cmi, sizeof(cmi));
//                             cmi.cbSize = sizeof(cmi);
//                             cmi.hwnd = hWndParent; /* this window has to answer CWM_GETISHELLBROWSER */
//                             cmi.lpVerb = (LPCSTR)MAKEINTRESOURCEA(uCommand);
//                             pCM->InvokeCommand(&cmi);
//                         }
//                     }
// 
//                     hResult = IUnknown_SetSite(pCM, NULL);
//                     DestroyMenu(hMenu);
//                 }
//             }
//             pCM.Release();
//         }
//     }
//     else    /* background context menu */
//     {
//         hMenu = CreatePopupMenu();
// 
//         CDefFolderMenu_Create2(NULL, NULL, cidl, (LPCITEMIDLIST*)apidl, pSFParent, NULL, 0, NULL, (IContextMenu**)&pCM);
//         pCM->QueryContextMenu(hMenu, 0, FCIDM_SHVIEWFIRST, FCIDM_SHVIEWLAST, 0);
// 
//         uCommand = TrackPopupMenu(hMenu,
//                                   TPM_LEFTALIGN | TPM_RETURNCMD | TPM_LEFTBUTTON | TPM_RIGHTBUTTON,
//                                   x, y, 0, m_hWnd, NULL);
//         DestroyMenu(hMenu);
// 
//         TRACE("-- (%p)->(uCommand=0x%08x )\n", this, uCommand);
// 
//         ZeroMemory(&cmi, sizeof(cmi));
//         cmi.cbSize = sizeof(cmi);
//         cmi.lpVerb = (LPCSTR)MAKEINTRESOURCEA(uCommand);
//         cmi.hwnd = hWndParent;
//         pCM->InvokeCommand(&cmi);
// 
//         pCM.Release();
//     }
// 
//     return 0;
// }
// 


//
//ypedef struct _browseinfoW {
//	HWND        hwndOwner;
//	PCIDLIST_ABSOLUTE pidlRoot;
//	LPWSTR       pszDisplayName;        // Return display name of item selected.
//	LPCWSTR      lpszTitle;                     // text to go in the banner over the tree.
//	UINT         ulFlags;                       // Flags that control the return stuff
//	BFFCALLBACK  lpfn;
//	LPARAM       lParam;                        // extra info that's passed back in callbacks
//	int          iImage;                        // output var: where to return the Image index.
//} BROWSEINFOW, *PBROWSEINFOW, *LPBROWSEINFOW;

LPITEMIDLIST bshell()
{
	BROWSEINFO bi = { 0 };

	bi.ulFlags = BIF_BROWSEINCLUDEFILES;
	bi.lpszTitle = _T("Pick a Directory");
	LPITEMIDLIST pidl = SHBrowseForFolder(&bi);
	if (pidl != 0)
	{
		// get the name of the folder
		TCHAR path[MAX_PATH];
		if (SHGetPathFromIDList(pidl, path))
		{
			_tprintf(_T("Selected Folder: %s\n"), path);
		}

		// free memory used
		IMalloc * imalloc = 0;
		if (SUCCEEDED(SHGetMalloc(&imalloc)))
		{
			imalloc->Free(pidl);
			imalloc->Release();
		}

		return pidl;

	}

	
}
//Below, this works, but doesn't include the contextmenuhandlers that are in explorer. No different than my other demos. 
bool openShellContextMenuForObject(const std::wstring &path, int xPos, int yPos, void * parentWindow)
{
	assert(parentWindow);
	ITEMIDLIST * id = 0;
	std::wstring windowsPath = path;
	std::replace(windowsPath.begin(), windowsPath.end(), '/', '\\');
	HRESULT result = SHParseDisplayName(windowsPath.c_str(), 0, &id, 0, 0);
	if (!SUCCEEDED(result) || !id)
		return false;
	//CItemIdListReleaser idReleaser(id);

	LPITEMIDLIST pa=0;
	LPITEMIDLIST pa2=0;
	IShellFolder * ifolder = 0;
	IShellFolder * idesktopfolder = 0;
	IContextMenu * imenu = 0;

	IObjectWithSite * isite = 0;
	//browser will ONLY return the pidl of the folder.  
	//pa=bshell();
	//pa2 = bshell();

	pa2 = pa;
	DWORD Eaten;
	LPITEMIDLIST idChild = 0;
	LPITEMIDLIST Pidl2;
	//ifolder gets initialized here.  

	//*************alternate test
#if 0
	LPITEMIDLIST ParentPidl;

	result= SHGetDesktopFolder(&idesktopfolder);
	//desktopfolder->BindToObject()
	result = idesktopfolder->ParseDisplayName(0, 0, (LPWSTR)windowsPath.c_str(), 0, &ParentPidl, 0);


	LPSHELLFOLDER ParentFolder;

	result = idesktopfolder->BindToObject(ParentPidl, 0, IID_IShellFolder, (void**)&ParentFolder);


	
	
	/*ParentFolder->ParseDisplayName(
		Handle, 0, Path, &Eaten, &Pidl, 0);
*/

	
	ParentFolder->ParseDisplayName(
		(HWND)parentWindow, 0, (LPWSTR)windowsPath.c_str(), 0, &Pidl2, 0);

	LPCONTEXTMENU CM;
	ParentFolder->GetUIObjectOf(
		(HWND)parentWindow, 1, (LPCITEMIDLIST*)&Pidl2,
		IID_IContextMenu, 0, (void**)&CM);


	HMENU hMenu2 = CreatePopupMenu();
	if (!hMenu2)
		return false;
	if (SUCCEEDED(CM->QueryContextMenu(hMenu2, 0, 1, 0x7FFF, CMF_NORMAL | CMF_EXPLORE)))
	{
		int iCmd = TrackPopupMenuEx(hMenu2, TPM_RETURNCMD, xPos, yPos, (HWND)parentWindow, NULL);
		if (iCmd > 0)
		{
			CMINVOKECOMMANDINFOEX info = { 0 };
			info.cbSize = sizeof(info);
			info.fMask = CMIC_MASK_UNICODE;
			info.hwnd = (HWND)parentWindow;
			info.lpVerb = MAKEINTRESOURCEA(iCmd - 1);
			info.lpVerbW = MAKEINTRESOURCEW(iCmd - 1);
			info.nShow = SW_SHOWNORMAL;
			CM->InvokeCommand((LPCMINVOKECOMMANDINFO)&info);
		}
	}
	DestroyMenu(hMenu2);

	return 0;
	//****************end alternate

#endif
	
	//old, working
	result = SHBindToParent(id, IID_IShellFolder, (void**)&ifolder, (LPCITEMIDLIST*)&idChild);
	//new test, failed
	//result = SHBindToParent(id, IID_IShellFolder, (void**)&ifolder,  (LPCITEMIDLIST*)&pa);
	//result = SHBindToParent(pa, IID_IShellFolder, (void**)&ifolder, (LPCITEMIDLIST*)&idChild);
	
	if (!SUCCEEDED(result))// || !ifolder)
		return false;
	//CComInterfaceReleaser ifolderReleaser(ifolder);

	//new test
	//result = ifolder->ParseDisplayName(
	//	(HWND)parentWindow, 0, (LPWSTR)windowsPath.c_str(), 0, &Pidl2, 0);
	//
	//result = SHBindToParent(Pidl2, IID_IShellFolder, (void**)&ifolder, (LPCITEMIDLIST*)&idChild);

		//end test
	
	
	//old,working
	result = ifolder->GetUIObjectOf((HWND)parentWindow, 1, (const ITEMIDLIST **)&idChild, IID_IContextMenu, 0, (void**)&imenu);
	//result = idesktopfolder->GetUIObjectOf((HWND)parentWindow, 1, (const ITEMIDLIST **)&id, IID_IContextMenu, 0, (void**)&imenu);
//	result = ifolder->GetUIObjectOf((HWND)parentWindow, 1, (const ITEMIDLIST **)&pa, IID_IContextMenu, 0, (void**)&imenu);
	
	
	if (!SUCCEEDED(result))// || !ifolder)
		return false;






	//CComInterfaceReleaser menuReleaser(imenu);

	HMENU hMenu = CreatePopupMenu();
	if (!hMenu)
		return false;
	if (SUCCEEDED(imenu->QueryContextMenu(hMenu, 0, 1, 0x7FFF,CMF_NORMAL| CMF_EXPLORE)))
	{
		int iCmd = TrackPopupMenuEx(hMenu, TPM_RETURNCMD, xPos, yPos, (HWND)parentWindow, NULL);
		if (iCmd > 0)
		{
			CMINVOKECOMMANDINFOEX info = { 0 };
			info.cbSize = sizeof(info);
			info.fMask = CMIC_MASK_UNICODE;
			info.hwnd = (HWND)parentWindow;
			info.lpVerb = MAKEINTRESOURCEA(iCmd - 1);
			info.lpVerbW = MAKEINTRESOURCEW(iCmd - 1);
			info.nShow = SW_SHOWNORMAL;
			imenu->InvokeCommand((LPCMINVOKECOMMANDINFO)&info);
		}
	}
	DestroyMenu(hMenu);

	return true;
}



int APIENTRY _tWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPTSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	CoInitialize(0);

 	// TODO: Place code here.
	MSG msg;
	HACCEL hAccelTable;

	// Initialize global strings
	LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
	LoadString(hInstance, IDC_WIN32SHELLTEST, szWindowClass, MAX_LOADSTRING);
	MyRegisterClass(hInstance);

	// Perform application initialization:
	if (!InitInstance (hInstance, nCmdShow))
	{
		return FALSE;
	}

	hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WIN32SHELLTEST));

	// Main message loop:
	while (GetMessage(&msg, NULL, 0, 0))
	{
		if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
	}

	return (int) msg.wParam;
}



//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
	WNDCLASSEX wcex;

	wcex.cbSize = sizeof(WNDCLASSEX);

	wcex.style			= CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc	= WndProc;
	wcex.cbClsExtra		= 0;
	wcex.cbWndExtra		= 0;
	wcex.hInstance		= hInstance;
	wcex.hIcon			= LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WIN32SHELLTEST));
	wcex.hCursor		= LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground	= (HBRUSH)(COLOR_WINDOW+1);
	wcex.lpszMenuName	= MAKEINTRESOURCE(IDC_WIN32SHELLTEST);
	wcex.lpszClassName	= szWindowClass;
	wcex.hIconSm		= LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

	return RegisterClassEx(&wcex);
}

//
//   FUNCTION: InitInstance(HINSTANCE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   HWND hWnd;

   hInst = hInstance; // Store instance handle in our global variable

   hWnd = CreateWindow(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, NULL, NULL, hInstance, NULL);

   if (!hWnd)
   {
      return FALSE;
   }

   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   return TRUE;
}

//
//  FUNCTION: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND	- process the application menu
//  WM_PAINT	- Paint the main window
//  WM_DESTROY	- post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	int wmId, wmEvent;
	PAINTSTRUCT ps;
	HDC hdc;

	switch (message)
	{
	case WM_COMMAND:
		wmId    = LOWORD(wParam);
		wmEvent = HIWORD(wParam);
		// Parse the menu selections:
		switch (wmId)
		{
		case IDM_ABOUT:
			//DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
			openShellContextMenuForObject(L"c:\\temp\\test.xml",30,30,hWnd);
	//		openShellContextMenuForObject(L"c:\\temp", 30, 30, hWnd);
			break;
		case IDM_EXIT:
			DestroyWindow(hWnd);
			break;
		default:
			return DefWindowProc(hWnd, message, wParam, lParam);
		}
		break;
	case WM_PAINT:
		hdc = BeginPaint(hWnd, &ps);
		// TODO: Add any drawing code here...
		EndPaint(hWnd, &ps);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hWnd, message, wParam, lParam);
	}
	return 0;
}

// Message handler for about box.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	UNREFERENCED_PARAMETER(lParam);
	switch (message)
	{
	case WM_INITDIALOG:
		return (INT_PTR)TRUE;

	case WM_COMMAND:
		if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
		{
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}
