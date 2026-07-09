cmake_minimum_required(VERSION 4.0)
project(xpk C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)

set(XPK_SOURCES
        src/library.c
)

add_library(xpk_static STATIC ${XPK_SOURCES})
add_library(xpk_shared SHARED ${XPK_SOURCES})

foreach (target xpk_static xpk_shared)
    target_include_directories(${target} PUBLIC include)
    target_compile_features(${target} PUBLIC c_std_11)
endforeach ()

target_compile_definitions(xpk_shared PRIVATE XPK_EXPORTS)
target_compile_definitions(xpk_static PUBLIC XPK_STATIC)

add_subdirectory(tools/xpk-cli)