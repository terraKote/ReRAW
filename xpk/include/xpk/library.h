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

typedef struct {
    uint16_t file_type;
    uint16_t string_size;
    uint32_t file_index;
    char *file_name;
} FileEntry;

typedef struct {
    uint8_t depth;
    int16_t parent;
    FileEntry *file;
    uint32_t pointer;
} TreeNode;

XPK_API bool read_header(const char *file_path, Header *header);

XPK_API bool get_xpkt_d_file_entries(const char *file_path, TreeNode **tree, uint16_t *entry_count);

XPK_API void clear_xpkt_d_file_entries(TreeNode **tree, uint16_t entry_count);


#endif // XPK_LIBRARY_H
