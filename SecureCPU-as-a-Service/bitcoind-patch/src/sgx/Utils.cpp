#include "sgx_urts.h"
#include "sgx_uae_service.h"
#include "Enclave_u.h"
#include "Utils.h"
#include "pouw.h"

#include <stdio.h>
#include <string.h>
#include <assert.h>

#include <string>
#include <ctime>
#include <vector>
#include <stdexcept>
#include <Log.h>

#include <unistd.h>
#include <pwd.h>
#include "Error.h"

#define MAX_PATH FILENAME_MAX

/* Initialize the enclave:
 *   Step 1: try to retrieve the launch token saved by last transaction
 *   Step 2: call sgx_create_enclave to initialize an enclave instance
 *   Step 3: save the launch token if it is updated
 */
int initialize_enclave(const char* enclave_name, sgx_enclave_id_t* eid)
{
    char token_path[MAX_PATH] = {'\0'};
    sgx_launch_token_t token = {0};
    sgx_status_t ret = SGX_ERROR_UNEXPECTED;
    int updated = 0;

    /* Step 1: try to retrieve the launch token saved by last transaction
     *         if there is no token, then create a new one.
     */
    const char *home_dir = getpwuid(getuid())->pw_dir;

    if (home_dir != NULL &&
        (strlen(home_dir)+strlen("/")+sizeof(TOKEN_FILENAME)+1) <= MAX_PATH) {
        /* compose the token path */
        strncpy(token_path, home_dir, strlen(home_dir));
        strncat(token_path, "/", strlen("/"));
        strncat(token_path, TOKEN_FILENAME, sizeof(TOKEN_FILENAME)+1);
    } else {
        /* if token path is too long or $HOME is NULL */
        strncpy(token_path, TOKEN_FILENAME, sizeof(TOKEN_FILENAME));
    }

    FILE *fp = fopen(token_path, "rb");
    if (fp == NULL && (fp = fopen(token_path, "wb")) == NULL) {
        printf("Warning: Failed to create/open the launch token file \"%s\".\n", token_path);
    }

    if (fp != NULL) {
        /* read the token from saved file */
        size_t read_num = fread(token, 1, sizeof(sgx_launch_token_t), fp);
        if (read_num != 0 && read_num != sizeof(sgx_launch_token_t)) {
            /* if token is invalid, clear the buffer */
            memset(&token, 0x0, sizeof(sgx_launch_token_t));
            printf("Warning: Invalid launch token read from \"%s\".\n", token_path);
        }
    }
    /* Step 2: call sgx_create_enclave to initialize an enclave instance */
    /* Debug Support: set 2nd parameter to 1 */
    ret = sgx_create_enclave(enclave_name, SGX_DEBUG_FLAG, &token, &updated, eid, NULL);
    if (ret != SGX_SUCCESS) {
        LL_CRITICAL("Failed to create enclave: %s (%d)", sgx_error_message(ret), ret);
        if (fp != NULL) fclose(fp);
        return -1;
    }

    /* Step 3: save the launch token if it is updated */
    if (updated == 0 || fp == NULL) {
        /* if the token is not updated, or file handler is invalid, do not perform saving */
        if (fp != NULL) fclose(fp);
        return 0;
    }

    /* reopen the file with write capablity */
    fp = freopen(token_path, "wb", fp);
    if (fp == NULL) return 0;
    size_t write_num = fwrite(token, 1, sizeof(sgx_launch_token_t), fp);
    if (write_num != sizeof(sgx_launch_token_t))
        printf("Warning: Failed to save launch token to \"%s\".\n", token_path);
    fclose(fp);
    return 0;
}

static uint8_t char2int(char input)
{
  if(input >= '0' && input <= '9')
    return input - '0';
  if(input >= 'A' && input <= 'F')
    return input - 'A' + 10;
  if(input >= 'a' && input <= 'f')
    return input - 'a' + 10;
  throw std::invalid_argument("Invalid input string");
}

void fromHex(const char* src, std::vector<uint8_t> & out)
{
    if (strlen(src) % 2 != 0) 
        { LL_CRITICAL("Error: input is not of even len\n");}
    if (strncmp(src, "0x", 2) == 0) src += 2;
    while(*src && src[1])
    {
        out.push_back(char2int(*src)*16 + char2int(src[1]));
        src += 2;
    }
}

// This function assumes src to be a zero terminated sanitized string with
// an even number of [0-9a-f] characters, and target to be sufficiently large
/*
    convert a hex string to a byte array
    [in]  src 
    [out] target 
    [out] len: how many bytes get written to the target
*/
void fromHex(const char* src, uint8_t* target, unsigned* len)
{
    *len = 0;
    if ((strlen(src) % 2) != 0) 
        { LL_CRITICAL("Error: input is not of even len\n"); *len=0;}
    if (strncmp(src, "0x", 2) == 0) src += 2;
    while(*src && src[1])
    {
        try { *(target++) = char2int(*src)*16 + char2int(src[1]); } 
        catch (std::invalid_argument e)
            { printf("Error: can't convert %s to bytes\n", src); }
        src += 2; 
        *len = (*len)+1;
    }
    if (*len == 1 && *(target - *len) == 0) *len = 0;
}
