//
// Created by terraKote on 09.07.2026.
//

#include <stdio.h>
#include <stdlib.h>

#include "cli/parser.h"
#include "cli/command_registry.h"

int main(int argc, char *argv[]) {
    CliArguments args;

    if (!parseArgs(argc, argv, &args)) {
        printf("Failed to parse command line arguments\n");
        return EXIT_FAILURE;
    }

    // TODO: add check?
    actions[args.command](&args);

    return EXIT_SUCCESS;
}
