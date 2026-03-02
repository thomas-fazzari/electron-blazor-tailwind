globalThis.themeInterop = {
  apply(isDark) {
    document.documentElement.classList.add("no-transitions");
    document.documentElement.classList.toggle("dark", isDark);
    try {
      localStorage.setItem("theme", isDark ? "dark" : "light");
    } catch {}
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        document.documentElement.classList.remove("no-transitions");
      });
    });
  },
  prefersColorSchemeDark() {
    return globalThis.matchMedia("(prefers-color-scheme: dark)").matches;
  },
  onSystemThemeChange(dotnetHelper) {
    const mql = globalThis.matchMedia("(prefers-color-scheme: dark)");
    const handler = (e) => {
      dotnetHelper.invokeMethodAsync("OnSystemThemeChanged", e.matches);
    };
    mql.addEventListener("change", handler);
    return {
      dispose() {
        mql.removeEventListener("change", handler);
      },
    };
  },
};
