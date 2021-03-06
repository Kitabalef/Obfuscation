#ifndef UNC_JUMP_H
#define UNC_JUMP_H

#include "threeadressinstruction.h"

using namespace std;

class Unc_jump : public CThreeAdressInstruction
{
    string label;
    int i;
	string target;
	COperand* ret;
public:
    Unc_jump(string s, int i, COperand* r = nullptr):label(s), i(i), ret(r) {}
    string gets() { return label; }
    int geti() { return i; }
	void seti ( int in ) { i = in; }
    bool isuncjmp() { return true; }
    void print(stringstream& s);
	
	string gettarget() {return target;}
	void settarget( string t ) { target = t; }
};

#endif // UNC_JUMP_H
