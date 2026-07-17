cmake_minimum_required(VERSION 3.20)
project(crypto_core LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# liboqs is not packaged on current Ubuntu, so it is fetched and built from source,
# pinned to a release tag for reproducibility. Only the two algorithms the platform
# uses (SR-1: ML-KEM-768 key exchange, SR-3: ML-DSA-65 signatures) are compiled,
# which keeps the build to a fraction of full liboqs. OpenSSL is disabled so the
# core has zero system library dependencies beyond libc.
include(FetchContent)
set(OQS_BUILD_ONLY_LIB ON CACHE BOOL "" FORCE)
set(OQS_MINIMAL_BUILD "KEM_ml_kem_768;SIG_ml_dsa_65" CACHE STRING "" FORCE)
set(OQS_USE_OPENSSL OFF CACHE BOOL "" FORCE)
FetchContent_Declare(
    liboqs
    GIT_REPOSITORY https://github.com/open-quantum-safe/liboqs.git
    GIT_TAG 0.16.0
    GIT_SHALLOW ON
)
FetchContent_MakeAvailable(liboqs)

add_library(securelearning_pqc STATIC src/pqc.cpp)
target_include_directories(securelearning_pqc PUBLIC include)
target_link_libraries(securelearning_pqc PUBLIC oqs)

enable_testing()
add_executable(test_pqc tests/test_pqc.cpp)
target_link_libraries(test_pqc PRIVATE securelearning_pqc)
add_test(NAME pqc_roundtrip COMMAND test_pqc)
