#include <stdio.h>
#include <time.h>

#include "../cli/parser.h"
#include "xpk/library.h"

void command_read_header(CliArguments *arguments) {
    Header header;

    if (!read_header(arguments->input, &header)) {
        printf("Failed to read header\n");
        return;
    }

    struct tm time_info = {0};

    time_info.tm_year = header.year - 1900;
    time_info.tm_mon = header.month - 1;
    time_info.tm_mday = header.day;
    time_info.tm_hour = header.hour;
    time_info.tm_min = header.minute;
    time_info.tm_sec = header.second;

    char formatted_time[64];

    const size_t result = strftime(formatted_time, sizeof(formatted_time),
                                   "%Y-%m-%d %H:%M:%S", &time_info);

    if (result > 0) {
        printf("Compilation Date: %s\n", formatted_time);
    } else {
        printf("Error fetching compilation date\n");
    }
}
