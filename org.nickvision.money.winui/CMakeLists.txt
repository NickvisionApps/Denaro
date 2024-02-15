if(WIN32)
    cmake_policy(SET CMP0091 NEW)

    configure_file("${CMAKE_CURRENT_SOURCE_DIR}/Directory.Build.props" "${CMAKE_CURRENT_BINARY_DIR}/Directory.Build.props" COPYONLY)
    configure_file("${CMAKE_CURRENT_SOURCE_DIR}/Directory.Build.targets" "${CMAKE_CURRENT_BINARY_DIR}/Directory.Build.targets" COPYONLY)

    add_executable(org.nickvision.money.winui WIN32 
        "org.nickvision.money.winui.manifest" "org.nickvision.money.winui.rc"
        "App.xaml.cpp" "App.xaml.h" "App.xaml" "App.idl"
        "Controls/SettingsRow.xaml.cpp" "Controls/SettingsRow.xaml.h" "Controls/SettingsRow.xaml" "Controls/SettingsRow.idl"
        "Controls/StatusPage.xaml.cpp" "Controls/StatusPage.xaml.h" "Controls/StatusPage.xaml" "Controls/StatusPage.idl"
        "Controls/ViewStack.xaml.cpp" "Controls/ViewStack.xaml.h" "Controls/ViewStack.xaml" "Controls/ViewStack.idl"
        "Controls/ViewStackPage.xaml.cpp" "Controls/ViewStackPage.xaml.h" "Controls/ViewStackPage.xaml" "Controls/ViewStackPage.idl"
        "Helpers/WinUI.cpp" "Helpers/WinUI.h"
        "MainWindow.xaml.cpp" "MainWindow.xaml.h" "MainWindow.xaml" "MainWindow.idl"
        "SettingsPage.xaml.cpp" "SettingsPage.xaml.h" "SettingsPage.xaml" "SettingsPage.idl")
    set_property(SOURCE "App.xaml" PROPERTY VS_XAML_TYPE "ApplicationDefinition")
    set_property(TARGET org.nickvision.money.winui PROPERTY VS_PACKAGE_REFERENCES
        "Microsoft.Windows.CppWinRT_2.0.240111.5"
        "Microsoft.WindowsAppSDK_1.4.240211001"
        "Microsoft.Windows.SDK.BuildTools_10.0.22621.2428"
        "Microsoft.Windows.ImplementationLibrary_1.0.240122.1"
    )
    target_include_directories(org.nickvision.money.winui PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}")
    target_precompile_headers(org.nickvision.money.winui PRIVATE "pch.h")
    set_target_properties(org.nickvision.money.winui PROPERTIES
        VS_GLOBAL_RootNamespace Nickvision.Money.WinUI
        VS_GLOBAL_AppContainerApplication false
        VS_GLOBAL_AppxPackage false
        VS_GLOBAL_CppWinRTOptimized true
        VS_GLOBAL_CppWinRTRootNamespaceAutoMerge true
        VS_GLOBAL_CppWinRTUsePrefixes false
        VS_GLOBAL_UseWinUI true
        VS_GLOBAL_ApplicationType "Windows Store"
        VS_GLOBAL_ApplicationTypeRevision 10.0
        VS_GLOBAL_WindowsTargetPlatformVersion 10.0
        VS_GLOBAL_WindowsTargetPlatformMinVersion "10.0.20348.0"
        VS_GLOBAL_WindowsPackageType None
        VS_GLOBAL_EnablePreviewMsixTooling true
        VS_GLOBAL_WindowsAppSDKSelfContained true
        VS_GLOBAL_WindowsAppSDKBootstrapAutoInitializeOptions_OnNoMatch_ShowUI true
        VS_GLOBAL_WindowsAppSDKBootstrapAutoInitializeOptions_OnPackageIdentity_NoOp true)
    target_link_libraries(org.nickvision.money.winui PRIVATE libdenaro)

    get_target_property(SOURCES org.nickvision.money.winui SOURCES)
    foreach(SOURCE ${SOURCES})
        cmake_path(GET SOURCE EXTENSION LAST_ONLY EXTENSION)
        if(NOT "${EXTENSION}" STREQUAL ".idl")
            continue()
        endif()
        set(IDL_SOURCE "${SOURCE}")
        cmake_path(REMOVE_EXTENSION SOURCE LAST_ONLY OUTPUT_VARIABLE BASENAME)
        set(XAML_SOURCE "${BASENAME}.xaml")
        if("${XAML_SOURCE}" IN_LIST SOURCES)
            set_property(SOURCE "${IDL_SOURCE}" PROPERTY VS_SETTINGS "SubType=Code" "DependentUpon=${XAML_SOURCE}")
        else()
            set_property(SOURCE "${IDL_SOURCE}" PROPERTY VS_SETTINGS "SubType=Code")
            set_property(SOURCE "${BASENAME}.h" PROPERTY VS_SETTINGS "DependentUpon=${IDL_SOURCE}")
            set_property(SOURCE "${BASENAME}.cpp" PROPERTY VS_SETTINGS "DependentUpon=${IDL_SOURCE}")
        endif()
    endforeach()

    add_custom_command(TARGET org.nickvision.money.winui POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/resources/org.nickvision.money.ico" "${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/resources/org.nickvision.money.ico")
    if(HAS_GETTEXT)
        foreach(LINGUA IN ITEMS ${LINGUAS})
            add_custom_command(TARGET org.nickvision.money.winui POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/${LINGUA}")
            add_custom_command(TARGET org.nickvision.money.winui POST_BUILD COMMAND "${GETTEXT_MSGFMT_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/resources/po/${LINGUA}.po" --output-file="${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/${LINGUA}/${SHORT_NAME}.mo")
        endforeach()
    endif()
endif()
