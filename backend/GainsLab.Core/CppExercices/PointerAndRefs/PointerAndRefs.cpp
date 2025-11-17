
#include <iostream>
#include <vector>

int main(int argc, char* argv[])
{

    int num{10};
    int num2 = 10;
    int a = 100;
    int b = 5;
    
    //initialize empty
    int *int_prt = {};
    int *int_ptr2 = {nullptr};

    //initialize with adresses of a and b
    int_prt = &a;
    int_ptr2 = &b;

    std::cout<< "int_ptr:"<< int_prt<< "\n"; //should be adress of int_prt - 000000212350F4F4
    std::cout<< "int_ptr2:"<< *int_ptr2<<"\n"; //should be 5
    
    *int_prt =1;
    *int_ptr2 = 25;

    
    int v=5;
    int* v_ptr = &v;
    

    std::cout<< "&a:"<< &a<< '\n'; //should be adress of int_prt
    std::cout<< "&int_ptr2:"<< &int_ptr2<< "\n"; //should be adress of 25 - same as int_ptr2
    std::cout<< "*int_ptr2:"<< *int_ptr2<< "\n"; //should be 25
    std::cout<< "v_ptr:"<< v_ptr<< "\n"; //should be adress of v
    std::cout<< "&v:"<< &v<< "\n"; //should be 5
    std::cout<< "*v_ptr:"<< *v_ptr<< "\n"; //should be 5
    std::cout<< "size of *v_ptr:"<< sizeof *v_ptr<< "\n"; //should be 4 bytes
    std::cout<< "size of v_ptr:"<< sizeof v_ptr<< "\n"; //should be ?? bytes


    std::vector<int> test = {*int_prt};
    std::cout<< "size of test:"<< sizeof test << "\n"; //should be ?? bytes

    
    return 0;
}
