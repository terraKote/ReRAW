#ifndef COMMAND_REGISTRY_H
#define COMMAND_REGISTRY_H

#include "command.h"

void command_read_header(CliArguments *arguments);
void command_extract_file_table(CliArguments *arguments);

typedef void (*ActionFunc)(CliArguments*);

static const ActionFunc actions[COMMAND_COUNT] = {
    [COMMAND_HEADER] = command_read_header,
    [COMMAND_EXTRACT_FILE_TABLE] = command_extract_file_table,
};

#endif // COMMAND_REGISTRY_H
