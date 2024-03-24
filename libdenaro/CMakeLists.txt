if(WIN32)
    set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
endif()

include(CTest)
add_library(libdenaro
    "src/controllers/accountviewcontroller.cpp"
    "src/controllers/dashboardviewcontroller.cpp"
    "src/controllers/mainwindowcontroller.cpp"
    "src/controllers/newaccountdialogcontroller.cpp"
    "src/controllers/preferencesviewcontroller.cpp"
    "src/helpers/currencyhelpers.cpp"
    "src/helpers/datehelpers.cpp"
    "src/models/account.cpp"
    "src/models/accountmetadata.cpp"
    "src/models/color.cpp"
    "src/models/configuration.cpp"
    "src/models/currency.cpp"
    "src/models/currencyconversion.cpp"
    "src/models/currencyconversionservice.cpp"
    "src/models/dashboardgroup.cpp"
    "src/models/group.cpp"
    "src/models/importresult.cpp"
    "src/models/receipt.cpp"
    "src/models/recentaccount.cpp"
    "src/models/transaction.cpp"
    "src/models/transfer.cpp")
target_include_directories(libdenaro PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
set_target_properties(libdenaro PROPERTIES VERSION "${PROJECT_VERSION}" SOVERSION "${PROJECT_VERSION}")
if(MSVC)
    target_compile_options(libdenaro PRIVATE /W4)
else()
    target_compile_options(libdenaro PRIVATE -Wall -Wextra -Wpedantic)
endif()

find_package(libnick CONFIG REQUIRED)
find_package(Boost REQUIRED COMPONENTS date_time)
find_package(Matplot++ CONFIG REQUIRED)
find_package(podofo CONFIG REQUIRED)
find_path(RAPIDCSV_INCLUDE_DIRS "rapidcsv.h")
target_link_libraries(libdenaro PUBLIC libnick::libnick Boost::boost Boost::date_time Matplot++::cimg Matplot++::matplot $<IF:$<TARGET_EXISTS:podofo_shared>,podofo_shared,podofo_static>)
target_include_directories(libdenaro PUBLIC ${RAPIDCSV_INCLUDE_DIRS})

if(HAS_GETTEXT)
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${GETTEXT_XGETTEXT_EXECUTABLE}" --from-code=utf-8 --add-comments --c++ --force-po --output="${POT_FILE}" --keyword=_ --keyword=_n:1,2 --keyword=_p:1c,2 --keyword=_pn:1c,2,3 --keyword=C_:1c,2 --width=80 ${TRANSLATE_FILES} WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}")
    foreach(LINGUA IN ITEMS ${LINGUAS})
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${GETTEXT_MSGMERGE_EXECUTABLE}" --backup=off -U "${CMAKE_SOURCE_DIR}/resources/po/${LINGUA}.po" "${POT_FILE}")
    endforeach()
endif()

if(HAS_GETTEXT AND ITSTOOL_EXECUTABLE AND YELP_BUILD_EXECUTABLE)
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${ITSTOOL_EXECUTABLE}" -o "docs/po/${SHORT_NAME}.pot" "docs/yelp/C/*.page" WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}")
    foreach(LINGUA IN ITEMS ${DOC_LINGUAS})
        # Update Po
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${GETTEXT_MSGMERGE_EXECUTABLE}" --backup=off -U "${CMAKE_SOURCE_DIR}/docs/po/${LINGUA}.po" "${DOC_POT_FILE}")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${GETTEXT_MSGFMT_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/docs/po/${LINGUA}.po" --output-file="${CMAKE_SOURCE_DIR}/docs/po/${LINGUA}.mo")
        # Generate Yelp Pages
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${ITSTOOL_EXECUTABLE}" -m "${CMAKE_SOURCE_DIR}/docs/po/${LINGUA}.mo" -o "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}/" "${CMAKE_SOURCE_DIR}/docs/yelp/C/*.page")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E rm "${CMAKE_SOURCE_DIR}/docs/po/${LINGUA}.mo")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}/figures")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_SOURCE_DIR}/docs/yelp/C/figures" "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}/figures")
        # Generate HTML Pages
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/html/${LINGUA}")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/html/${LINGUA}/figures")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}/figures" "${CMAKE_SOURCE_DIR}/docs/html/${LINGUA}/figures")
        add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${YELP_BUILD_EXECUTABLE}" html -o "${CMAKE_SOURCE_DIR}/docs/html/${LINGUA}/" "${CMAKE_SOURCE_DIR}/docs/yelp/${LINGUA}/")
    endforeach()
    # Generate English HTML
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/html/C")
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E make_directory "${CMAKE_SOURCE_DIR}/docs/html/C/figures")
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_SOURCE_DIR}/docs/yelp/C/figures" "${CMAKE_SOURCE_DIR}/docs/html/C/figures")
    add_custom_command(TARGET libdenaro POST_BUILD COMMAND "${YELP_BUILD_EXECUTABLE}" html -o "${CMAKE_SOURCE_DIR}/docs/html/C/" "${CMAKE_SOURCE_DIR}/docs/yelp/C/")
endif()

if (BUILD_TESTING)
    add_executable(libdenaro_test
        "tests/accounttests.cpp"
        "tests/colortests.cpp"
        "tests/currencyconversiontests.cpp"
        "tests/currencytests.cpp"
        "tests/datetests.cpp"
        "tests/main.cpp"
        "tests/receipttests.cpp")
    find_package(GTest CONFIG REQUIRED)
    target_link_libraries(libdenaro_test PRIVATE GTest::gtest GTest::gtest_main GTest::gmock GTest::gmock_main libdenaro)
    if(MSVC)
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount1.csv" "${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/DenaroTestAccount1.csv")
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount2.ofx" "${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/DenaroTestAccount2.ofx")
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount3.qif" "${CMAKE_CURRENT_BINARY_DIR}/$<CONFIG>/DenaroTestAccount3.qif")
    else()
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount1.csv" "${CMAKE_CURRENT_BINARY_DIR}/DenaroTestAccount1.csv")
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount2.ofx" "${CMAKE_CURRENT_BINARY_DIR}/DenaroTestAccount2.ofx")
        add_custom_command(TARGET libdenaro_test POST_BUILD COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_SOURCE_DIR}/libdenaro/tests/DenaroTestAccount3.qif" "${CMAKE_CURRENT_BINARY_DIR}/DenaroTestAccount3.qif")
    endif()
endif()