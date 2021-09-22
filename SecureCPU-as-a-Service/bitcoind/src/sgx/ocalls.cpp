#include <iostream>
#include <string>
#include <ctime>
#include "Enclave_u.h"

#ifdef _WIN32
#include "windows.h"
#endif

#include "ocalls.h"
#include <Log.h>
#include <unistd.h>

std::string current_datetime()
{
    time_t now = time(0);
    char buf[sizeof "2011-10-08T07:07:09Z"];
    strftime(buf, sizeof buf, "%Y-%m-%dT%H:%M:%SZ", gmtime(&now));
    return buf;
}

long long rdtsc()
{
#ifdef _WIN32
    return __rdtsc();
#else
    unsigned long long int x;
    __asm__ volatile (".byte 0x0f, 0x31" : "=A" (x));
    return x;
#endif
}

void ocall_sleep(int milisec)
{
    LL_NOTICE("Waiting for %d", milisec/1000);
    for (int i = 0; i < milisec / 1000; i++)
    {
        printf(".");
#ifdef _WIN32
        Sleep(1000);
#else
        sleep(1);
#endif
    }
    printf("\n");
}

time_t ocall_time()
{
    return time(NULL);
}

int ocall_print_string(const char *str)
{
    /* Proxy/Bridge will check the length and null-terminate
     * the input string to prevent buffer overflow.
     */
    int ret = printf("%s", str);
    fflush(stdout);
    return ret;
}