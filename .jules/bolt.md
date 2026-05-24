## 2026-05-24 - DI Resolution Allocation Bottleneck
**Learning:** The DI container's internal resolution ( in ) heavily used LINQ (), which created excessive allocations via closures and iterators on the hottest path of dependency injection.
**Action:** Replaced LINQ with manual  loops and pre-allocated arrays, eliminating these allocations and drastically improving resolution performance.
## 2024-05-24 - DI Resolution Allocation Bottleneck
**Learning:** The DI container's internal resolution (`ResolveParameters` in `DiContainerInvokeExtensions`) heavily used LINQ (`.Select().ToArray()`), which created excessive allocations via closures and iterators on the hottest path of dependency injection.
**Action:** Replaced LINQ with manual `for` loops and pre-allocated arrays, eliminating these allocations and drastically improving resolution performance.
