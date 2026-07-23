#include "parser.h"

#include <stdio.h>
#include <string.h>

bool parseArgs(int argc, char *argv[], CliArguments *args) {
    if (argc < 2) {
        fprintf(stderr, "Error: Missing file path argument.\n");
        fprintf(stderr, "Usage: %s <path_to_binary_file>\n", argv[0]);
        return false;
    }

    char *command = argv[1];
    char *path = argv[2];

    strcpy(args->input, path);

    if (strcmp(command, "header") == 0) {
        args->command = COMMAND_HEADER;
        return true;
    }

    if (strcmp(command, "extract_file_table") == 0) {
        args->command = COMMAND_EXTRACT_FILE_TABLE;

        return true;
    }

    return false;
}
