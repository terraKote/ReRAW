#include <stdio.h>
#include <stdlib.h>

#include "../include/xpk/library.h"

bool read_header(const char *file_path, Header *header) {
    FILE *file = fopen(file_path, "rb");

    if (file == NULL) {
        fprintf(stderr, "Error opening file: %s\n", file_path);
        return false;
    }

    fseek(file, 0, SEEK_SET);
    const size_t items = fread(header, sizeof(Header), 1, file);
    fclose(file);

    if (items > 0) {
        return true;
    }

    return false;
}
