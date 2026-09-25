window.themeManager = {
    getTheme: function () {
        var match = document.cookie.match(new RegExp('(^| )app_theme=([^;]+)'));
        if (match && match[2]) {
            return match[2];
        }
        var current = document.documentElement.getAttribute('data-bs-theme');
        return current || 'dark';
    },
    setTheme: function (theme) {
        if (!theme || (theme !== 'light' && theme !== 'dark')) {
            theme = 'dark';
        }
        document.documentElement.setAttribute('data-bs-theme', theme);
        if (theme === 'dark') {
            document.body.classList.remove('bg-light', 'text-dark');
            document.body.classList.add('bg-dark', 'text-light');
        } else {
            document.body.classList.remove('bg-dark', 'text-light');
            document.body.classList.add('bg-light', 'text-dark');
        }
        var expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        document.cookie = 'app_theme=' + encodeURIComponent(theme) + '; path=/; expires=' + expires.toUTCString() + '; SameSite=Lax';
    }
};
