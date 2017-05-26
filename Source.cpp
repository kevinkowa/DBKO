#include <iostream>
#include <windows.h>
#include <TlHelp32.h> 
#include <iostream> 
#include <tchar.h> 
#include<string>
using namespace std;

LRESULT WINAPI WndProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam)
{
	if (Msg == WM_DESTROY)
		PostQuitMessage(0);
	return DefWindowProc(hWnd, Msg, wParam, lParam);
}

DWORD_PTR dwGetModuleBaseAddress(DWORD dwProcessIdentifier, TCHAR *szModuleName)
{
	DWORD_PTR dwModuleBaseAddress = 0;
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, dwProcessIdentifier);
	if (hSnapshot != INVALID_HANDLE_VALUE)
	{
		MODULEENTRY32 ModuleEntry32;
		ModuleEntry32.dwSize = sizeof(MODULEENTRY32);
		if (Module32First(hSnapshot, &ModuleEntry32))
		{
			do
			{
				if (_tcscmp(ModuleEntry32.szModule, szModuleName) == 0)
				{
					dwModuleBaseAddress = (DWORD_PTR)ModuleEntry32.modBaseAddr;
					break;
				}
			} while (Module32Next(hSnapshot, &ModuleEntry32));
		}
		CloseHandle(hSnapshot);
	}
	return dwModuleBaseAddress;
}
int main()
{

	HWND hand = ::FindWindowEx(0, 0, L"TibiaClient", 0);
	if (!hand) {
		printf("Window not found!\n");
		char f;
		cin >> f;
		return 0;
	}
	cout << hand << endl;
	DWORD pid = 0;
	DWORD p=GetWindowThreadProcessId(hand, &pid);
	HANDLE phandle = OpenProcess(PROCESS_VM_READ, 0, pid);
	cout << pid << endl;

	DWORD base_addr = dwGetModuleBaseAddress(pid, L"DBKO v6.0.exe");
	DWORD exp_addr = base_addr + 0x235F04;	
	DWORD mana_addr = base_addr + 0x235EF0;

	HWND edit = FindWindowEx(hand, NULL, _T("Edit"), NULL);

	int experience = 0;
	int mana = 0;
	ReadProcessMemory(phandle, (LPCVOID)(exp_addr), &experience, sizeof experience, 0);
	cout << experience << endl;
	string spell = "POWER DOWN";
	HINSTANCE hInst = GetModuleHandle(0);
	WNDCLASS cls = { CS_HREDRAW | CS_VREDRAW, WndProc, 0, 0, hInst, LoadIcon(hInst,MAKEINTRESOURCE(IDI_APPLICATION)),
		LoadCursor(hInst,MAKEINTRESOURCE(IDC_ARROW)), GetSysColorBrush(COLOR_WINDOW),0,(LPCWSTR)"Window" };
	RegisterClass(&cls);
	HWND window = CreateWindow(L"Window", L"Hello World", WS_OVERLAPPEDWINDOW | WS_VISIBLE, 64, 64, 640, 480, 0, 0, hInst, 0);

	while (1)
	{
		ReadProcessMemory(phandle, (LPCVOID)(mana_addr), &mana, sizeof mana, 0);
		cout << mana << endl;
		PostMessage(hand, WM_KEYDOWN, VK_CONTROL, 1);
		PostMessage(hand, WM_KEYDOWN, VK_LEFT, 1);
		PostMessage(hand, WM_KEYUP, VK_CONTROL, 1);
		PostMessage(hand, WM_KEYUP, VK_LEFT, 1);
		Sleep(1000);

		PostMessage(hand, WM_KEYDOWN, VK_CONTROL, 1);
		PostMessage(hand, WM_KEYDOWN, VK_RIGHT, 1);
		PostMessage(hand, WM_KEYUP, VK_CONTROL, 1);
		PostMessage(hand, WM_KEYUP, VK_RIGHT, 1);
		for (int i = 0; i < spell.size(); i++) {
			PostMessage(hand, WM_KEYDOWN, spell[i], 1);
		}
		PostMessage(hand, WM_KEYDOWN, VK_RETURN, 1);
		PostMessage(hand, WM_KEYUP, VK_RETURN, 1);

		Sleep(1000);

	}

	cin.get();
	return 0;
}
