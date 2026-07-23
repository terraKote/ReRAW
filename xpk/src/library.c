#include <stdio.h>
#include <stdlib.h>

#include "../include/xpk/library.h"

#include <math.h>
#include <string.h>

void read_file_entry(FILE *file, uint32_t offset, FileEntry *entry) {
    fseek(file, (long) offset, SEEK_SET);
    fread(&entry->file_type, sizeof(entry->file_type), 1, file);

    if (entry->file_type == 0) {
        return;
    }

    fread(&entry->string_size, sizeof(entry->string_size), 1, file);
    fread(&entry->file_index, sizeof(entry->file_index), 1, file);

    const uint16_t string_length = entry->string_size + 1;

    char *file_name = malloc(string_length);
    if (file_name == NULL) {
        fprintf(stderr, "Error allocating memory for file_name\n");
        return;
    }

    size_t file_bytes_read = fread(file_name, string_length, 1, file);

    if (file_bytes_read != 1) {
        free(file_name);
        return;
    }

    entry->file_name = file_name;
}

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

bool get_xpkt_d_file_entries(const char *file_path, TreeNode **tree, uint16_t *entry_count) {
    // Open the file for reading
    // TODO: use buffers
    FILE *file = fopen(file_path, "rb");

    if (file == NULL) {
        fprintf(stderr, "Error opening file: %s\n", file_path);
        return false;
    }

    // Read the first file offset
    fseek(file, 0x0C, SEEK_SET);
    uint32_t offset = {0};

    if (fread(&offset, sizeof(uint32_t), 1, file) != 1) {
        fprintf(stderr, "Error reading file: %s\n", file_path);
        fclose(file);
        return false;
    }

    // Create the dynamic array
    uint16_t next_element_index = 0;
    uint16_t array_capacity = 3;
    TreeNode *nodes = malloc(sizeof(TreeNode) * array_capacity);

    if (nodes == NULL) {
        fprintf(stderr, "Error allocating memory for nodes\n");
        fclose(file);
        return false;
    }

    // Read the entries recursively
    int16_t parent_stack[32] = {0};

    uint8_t depth = 0;
    int16_t parent = -1;
    bool is_end_reached = false;

    while (!is_end_reached) {
        // Read the file entry
        FileEntry *file_entry = malloc(sizeof(*file_entry));

        if (file_entry == NULL) {
            fprintf(stderr, "Error allocating memory for file_entry\n");
            free(file_entry);
            break;
        }
        read_file_entry(file, offset, file_entry);

        // Copy depth
        const uint8_t current_depth = depth;

        parent_stack[depth] = parent;

        // Evaluate file type
        switch (file_entry->file_type) {
            // Empty data sentinel
            default:
            case 0x00:
                depth--;

                parent = parent_stack[depth];

                const uint32_t previous_offset = offset;

                for (int i = 0; i < next_element_index; i++) {
                    if (nodes[i].depth != depth)
                        continue;

                    offset = nodes[i].pointer;
                }

                if (previous_offset == offset) {
                    is_end_reached = true;
                }

                free(file_entry);
                continue;

            // File entry sentinel
            case 0x01:
                offset = ftell(file);
                break;

            // Directory entry sentinel
            case 0x02:
                depth++;
                parent = next_element_index;
                offset = file_entry->file_index;
                break;
        }

        // Check for array size
        if (next_element_index == array_capacity) {
            array_capacity *= 2;

            TreeNode *tempNodeBuffer = realloc(nodes, sizeof(TreeNode) * array_capacity);

            if (tempNodeBuffer == NULL) {
                fprintf(stderr, "Error allocating memory for nodes\n");
                free(file_entry);
                break;
            }

            nodes = tempNodeBuffer;
        }

        // Assign node
        nodes[next_element_index].depth = current_depth;
        nodes[next_element_index].file = file_entry;
        nodes[next_element_index].pointer = ftell(file);
        nodes[next_element_index].parent = parent_stack[current_depth];

        next_element_index++;
    }

    // Copy temp buffers
    *tree = malloc(sizeof(TreeNode) * next_element_index);

    if (*tree == NULL) {
        fprintf(stderr, "Error allocating memory for nodes\n");
        fclose(file);
        free(nodes);
        return false;
    }

    memcpy(*tree, nodes, sizeof(TreeNode) * next_element_index);
    *entry_count = next_element_index;

    // Clear temp buffers
    free(nodes);

    // Close the file
    fclose(file);
    return true;
}

void clear_xpkt_d_file_entries(TreeNode **tree, uint16_t entry_count) {
    for (int i = 0; i < entry_count; i++) {
        free(tree[i]->file);
        free(tree[i]);
    }
    free(tree);
}
