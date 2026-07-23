#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "../cli/parser.h"
#include "xpk/library.h"

void command_extract_file_table(CliArguments *arguments) {
    uint16_t node_count = 0;
    TreeNode *tree = NULL;

    if (get_xpkt_d_file_entries(arguments->input, &tree, &node_count)) {
        for (int i = 0; i < node_count; i++) {
            char pathBuffer[256] = {0};

            int16_t parent = tree[i].parent;

            while (parent != -1) {
                strcat(pathBuffer, tree[parent].file_name);
                parent = tree[parent].parent;
                strcat(pathBuffer, "/");
            }

            strcat(pathBuffer, tree[i].file_name);

            printf("[%d] Name: %s; Parent: %d; Full Path: %s\n", i, tree[i].file_name, tree[i].parent,
                   pathBuffer);
        }

        clear_xpkt_d_file_entries(&tree, node_count);
    }
}
