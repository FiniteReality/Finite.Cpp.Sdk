extern "C"
{
#include "hello.h"
}

#include "hello.hpp"

namespace MyCoolLibrary
{
    __attribute__((visibility("default"))) std::string get_hello_cxx_string()
    {
        return std::string(get_hello_string()) + "\nThis is: C++ library";
    }
}
