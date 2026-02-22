window.scrollToElement = (id) => {
    const el = document.getElementById(id);
    if (!el) return;

    const header = document.querySelector('.mud-appbar');
    const headerHeight = header ? header.offsetHeight : 0;
    const gap = 12;
    const top = el.getBoundingClientRect().top + window.scrollY - headerHeight - gap;

    const highlight = () => {
        el.classList.remove('scroll-highlight');
        void el.offsetWidth; // force reflow so re-adding the class restarts the animation
        el.classList.add('scroll-highlight');
        el.addEventListener('animationend', () => el.classList.remove('scroll-highlight'), { once: true });
    };

    if ('onscrollend' in window) {
        window.addEventListener('scrollend', highlight, { once: true });
    } else {
        setTimeout(highlight, 500);
    }

    window.scrollTo({ top, behavior: 'smooth' });
};