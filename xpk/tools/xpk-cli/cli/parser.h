#pragma once

#include <stdbool.h>

#include "command.h"

enum Verbosity {
    VERBOSE,
    INFO
};

typedef struct {
    enum Command command;
    char input[256];
    char output[256];
    enum Verbosity verbosity;
} CliArguments;

bool parseArgs(int argc, char *argv[], CliArguments *args);