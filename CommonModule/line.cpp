#include "line.h"

void Line::print(stringstream& s)
{
    s << "<Instruction PolyRequired=\"false\" ID=\"ID_";
	s << getid() << "\" StatementType=\"";
	s << "Unknown" << "\">";
	s << name;
    s << "</Instruction>"<< std::endl;
}
