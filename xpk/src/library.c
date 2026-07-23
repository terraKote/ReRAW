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

bool get_xpkt_d_file_entries(const char *file_path, TreeNode ***tree, uint16_t *entry_count) {
    // Open file for reading
    FILE *file = fopen(file_path, "rb");

    if (file == NULL) {
        fprintf(stderr, "Error opening file: %s\n", file_path);
        return false;
    }

    // Read the initial file offset
    fseek(file, 0x0C, SEEK_SET);
    uint32_t file_offset = {0};

    if (fread(&file_offset, sizeof(uint32_t), 1, file) == 0) {
        fprintf(stderr, "Error reading file: %s\n", file_path);
        return false;
    }

    // Create the dynamic array to store the tree nodes
    uint16_t array_size = 0;
    uint16_t array_capacity = 3;
    TreeNode **nodes = malloc(array_capacity * sizeof(TreeNode *));

    // Read the entries recursively
    uint8_t depth = 0;
    bool isEndReached = false;

    while (!isEndReached) {
        FileEntry *file_entry = malloc(sizeof(FileEntry));

        if (file_entry == NULL) {
            fprintf(stderr, "Error allocating memory for file_entry\n");
            free(file_entry);
            break;
        }

        const uint8_t current_depth = depth;
        read_file_entry(file, file_offset, file_entry);

        // Check the entry type
        switch (file_entry->file_type) {
            // Empty data sentinel
            default:
            case 0x00:
                free(file_entry);
                depth--;

                uint32_t previous_offset = file_offset;

                for (int i = 0; i < array_size; i++) {
                    TreeNode *existing_node = nodes[i];

                    if (existing_node->parent != depth)
                        continue;

                    file_offset = existing_node->pointer;
                }

                if (previous_offset == file_offset) {
                    isEndReached = true;
                }

                continue;

            // File entry
            case 0x01:
                file_offset = ftell(file);
                break;

            // Directory entry
            case 0x02:
                depth++;
                file_offset = file_entry->file_index;
                break;
        }

        // Create the tree node
        TreeNode *node = malloc(sizeof(TreeNode));

        if (node == NULL) {
            fprintf(stderr, "Error allocating memory for node\n");
            free(node);
            break;
        }

        node->parent = current_depth;
        node->file = file_entry;
        node->pointer = ftell(file);

        if (array_size == array_capacity) {
            array_capacity *= 2;

            TreeNode **temp = realloc(nodes, array_capacity * sizeof(TreeNode *));

            if (temp == NULL) {
                fprintf(stderr, "Error reallocating memory for nodes\n");
                free(temp);
                return false;
            }

            nodes = temp;
        }

        nodes[array_size] = node;
        array_size++;
    }

    // Fill the buffers
    *entry_count = array_size;
    *tree = nodes;

    // Free the buffer
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
