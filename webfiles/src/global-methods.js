const GlobalMethods = {
    install(Vue, options) {
        var globalMethods = {
            GetScreenWidth() {
                var sw = screen.width;

                if (sw >= 1280)
                    return 'xl';
                else if (sw >= 1024)
                    return 'lg';
                else if (sw >= 768)
                    return 'md';
                else if (sw >= 640)
                    return 'sm';

                return 'xs';
            },
            IsScreenWidth(swString) {
                var sw = screen.width;

                switch (swString) {
                    case 'xl':
                        return sw >= 1280;
                    case 'lg':
                        return sw >= 1024 && sw < 1280;
                    case 'md':
                        return sw >= 768 && sw < 1024;
                    case 'sm':
                        return sw >= 640 && sw < 768;
                    case 'xs':
                        return sw < 640;
                }

                return false;
            },
            IsMobile() {
                return this.IsScreenWidth('sm') || this.IsScreenWidth('xs');
            }
        }

        Vue.prototype.$globalMethods = globalMethods
    }
};

export default GlobalMethods;