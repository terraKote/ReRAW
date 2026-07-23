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
            printf("%s\n", tree[i].file->file_name);
        }

        //clear_xpkt_d_file_entries(tree, node_count);
    }
}
