//
// Created by terraKote on 09.07.2026.
//

#ifndef XPK_OBJECTS_EXPORT_H
#define XPK_OBJECTS_EXPORT_H

#if defined(_WIN32)

    #if defined(XPK_STATIC)
        #define XPK_API

    #elif defined(XPK_EXPORTS)
        #define XPK_API __declspec(dllexport)

    #else
        #define XPK_API __declspec(dllimport)

    #endif

#else

    #define XPK_API

#endif

#endif //XPK_OBJECTS_EXPORT_H