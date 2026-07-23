cmake_minimum_required(VERSION 4.0)
project(xpk-cli C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)

add_executable(xpk-cli
        main.c
        commands/header.c
        commands/extract_file_table.c
        cli/parser.c
)

target_link_libraries(xpk-cli PRIVATE xpk_static)