#include <iostream>

#include "hello.hpp"

int main()
{
    std::cout << MyCoolLibrary::get_hello_cxx_string()
        << "\nThis is: C++ application\n" << std::flush;
    return 0;
}
