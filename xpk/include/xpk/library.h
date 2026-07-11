#ifndef XPK_LIBRARY_H
#define XPK_LIBRARY_H

#include <stdint.h>
#include <stdbool.h>

#include "export.h"

typedef struct {
    uint16_t year;
    uint16_t month;
    uint16_t day;
    uint16_t hour;
    uint16_t minute;
    uint16_t second;
} Header;

XPK_API bool read_header(const char* file_path, Header *header);

#endif // XPK_LIBRARY_H
