#ifndef MORSECORE_COMPAT_H
#define MORSECORE_COMPAT_H

#include <stddef.h>
#include <stdint.h>
#include <stdbool.h>

/* --- 1. Checked Integer Arithmetic (stdckdint.h fallback) --- */
#if defined(__has_include)
#  if __has_include(<stdckdint.h>) && (!defined(__STDC_VERSION__) || __STDC_VERSION__ >= 202311L || defined(__clang__) || (defined(__GNUC__) && __GNUC__ >= 14))
#    include <stdckdint.h>
#    define MORSECORE_HAS_STDCKDINT 1
#  endif
#endif

#ifndef MORSECORE_HAS_STDCKDINT
#  if defined(__GNUC__) || defined(__clang__)
#    define ckd_add(result, a, b) __builtin_add_overflow((a), (b), (result))
#    define ckd_sub(result, a, b) __builtin_sub_overflow((a), (b), (result))
#    define ckd_mul(result, a, b) __builtin_mul_overflow((a), (b), (result))
#  else
/* Portable fallback for size_t overflow checking on MSVC or other ISO C compilers */
static inline int mc_ckd_add_size_t(size_t *result, size_t a, size_t b)
{
    *result = a + b;
    return *result < a;
}
static inline int mc_ckd_mul_size_t(size_t *result, size_t a, size_t b)
{
    if (a != 0 && b > SIZE_MAX / a) return 1;
    *result = a * b;
    return 0;
}
#    define ckd_add(result, a, b) mc_ckd_add_size_t((size_t *)(result), (size_t)(a), (size_t)(b))
#    define ckd_mul(result, a, b) mc_ckd_mul_size_t((size_t *)(result), (size_t)(a), (size_t)(b))
#  endif
#endif

/* --- 2. Thread-Local Storage --- */
#ifndef thread_local
#  if defined(__STDC_VERSION__) && __STDC_VERSION__ >= 202311L
     /* Native C23 keyword */
#  elif defined(_MSC_VER)
#    define thread_local __declspec(thread)
#  elif defined(__GNUC__) || defined(__clang__)
#    define thread_local __thread
#  elif defined(__STDC_VERSION__) && __STDC_VERSION__ >= 201112L && !defined(__STDC_NO_THREADS__)
#    include <threads.h>
#  else
#    define thread_local _Thread_local
#  endif
#endif

/* --- 3. Static Assertions (supporting 1 or 2 arguments) --- */
#include <assert.h>
#if !defined(__STDC_VERSION__) || __STDC_VERSION__ < 202311L
#  ifndef static_assert
#    define static_assert _Static_assert
#  endif
#  if defined(_MSC_VER) && !defined(__clang__)
     /* MSVC assert.h defines static_assert */
#  elif defined(__GNUC__) || defined(__clang__)
     /* Macro to support single-argument static_assert in pre-C23 compilers */
#    define _MC_ASSERT_1(cond) _Static_assert(cond, #cond)
#    define _MC_ASSERT_2(cond, msg) _Static_assert(cond, msg)
#    define _MC_ASSERT_SELECT(_1, _2, NAME, ...) NAME
#    undef static_assert
#    define static_assert(...) _MC_ASSERT_SELECT(__VA_ARGS__, _MC_ASSERT_2, _MC_ASSERT_1)(__VA_ARGS__)
#  endif
#endif

/* --- 4. nullptr Keyword --- */
#ifndef nullptr
#  define nullptr ((void *)0)
#endif

#endif /* MORSECORE_COMPAT_H */
