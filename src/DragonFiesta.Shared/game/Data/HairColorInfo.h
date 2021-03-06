#ifndef HAIRCOLORINFO_H
#define HAIRCOLORINFO_H

#include "../Enum/BodyShopGrade.h"

class HairColorInfo
{

public:
    int8 GetID()
    {
        return ID;
    }
    std::string GetIndex()
    {
        return index;
    }
    std::string GetName()
    {
        return Name;
    }

    BodyShopGrade GetGrade()
    {
        return Grade;
    }

private :
    int8 ID;
    std::string index;
    std::string Name;
    BodyShopGrade Grade;
};

#endif // HAIRCOLORINFO_H




