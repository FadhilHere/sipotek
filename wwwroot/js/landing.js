/**
 * SIPOTEK Landing Page JavaScript
 * Author: SIPOTEK Team
 * Version: 1.0
 * Description: Interactive features for the landing page
 */

class LandingPage {
    constructor() {
        this.init();
    }

    init() {
        this.bindEvents();
        this.initAnimations();
        this.initNavbar();
        this.initLoading();
    }

    bindEvents() {
        document.addEventListener('DOMContentLoaded', () => {
            this.setupSmoothScrolling();
            this.setupMobileMenu();
            this.setupCounterAnimation();
        });

        window.addEventListener('scroll', () => {
            this.handleNavbarScroll();
            this.handleScrollAnimations();
        });

        window.addEventListener('load', () => {
            this.hideLoading();
        });

        // Handle page visibility change
        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'visible') {
                this.refreshAnimations();
            }
        });
    }

    initLoading() {
        const spinner = document.getElementById('loading-spinner');
        const mainContent = document.getElementById('main-content');

        if (spinner && mainContent) {
            // Show content after a minimum delay for smooth UX
            setTimeout(() => {
                this.hideLoading();
            }, 1000);
        }
    }

    hideLoading() {
        const spinner = document.getElementById('loading-spinner');
        const mainContent = document.getElementById('main-content');

        if (spinner && mainContent) {
            spinner.classList.add('hidden');
            mainContent.classList.add('loaded');

            // Remove spinner from DOM after animation
            setTimeout(() => {
                if (spinner.parentNode) {
                    spinner.parentNode.removeChild(spinner);
                }
            }, 500);
        }
    }

    initNavbar() {
        const navbar = document.querySelector('.navbar-custom');
        if (navbar) {
            // Add initial class if page is already scrolled
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
            }
        }
    }

    setupSmoothScrolling() {
        // Enhanced smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                e.preventDefault();
                const target = document.querySelector(anchor.getAttribute('href'));

                if (target) {
                    const offsetTop = target.offsetTop - 80; // Account for fixed navbar
                    const startPosition = window.pageYOffset;
                    const distance = offsetTop - startPosition;
                    const duration = Math.min(Math.abs(distance) / 2, 1000); // Dynamic duration

                    this.smoothScrollTo(startPosition, distance, duration);

                    // Update active nav link
                    this.updateActiveNavLink(anchor.getAttribute('href'));
                }
            });
        });
    }

    smoothScrollTo(startPosition, distance, duration) {
        let start = null;

        const easeInOutCubic = (t) => {
            return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        };

        const animateScroll = (currentTime) => {
            if (start === null) start = currentTime;
            const timeElapsed = currentTime - start;
            const progress = Math.min(timeElapsed / duration, 1);
            const ease = easeInOutCubic(progress);

            window.scrollTo(0, startPosition + distance * ease);

            if (timeElapsed < duration) {
                requestAnimationFrame(animateScroll);
            }
        };

        requestAnimationFrame(animateScroll);
    }

    updateActiveNavLink(href) {
        document.querySelectorAll('.nav-link-custom').forEach(link => {
            link.classList.remove('active');
        });

        const activeLink = document.querySelector(`a[href="${href}"]`);
        if (activeLink && activeLink.classList.contains('nav-link-custom')) {
            activeLink.classList.add('active');
        }
    }

    handleNavbarScroll() {
        const navbar = document.querySelector('.navbar-custom');
        if (navbar) {
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        }
    }

    setupMobileMenu() {
        const mobileLinks = document.querySelectorAll('#mobileMenu .nav-link-custom');
        const mobileMenu = document.querySelector('#mobileMenu');

        mobileLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (mobileMenu && window.bootstrap) {
                    const bsCollapse = new bootstrap.Collapse(mobileMenu, {
                        toggle: false
                    });
                    bsCollapse.hide();
                }
            });
        });
    }

    initAnimations() {
        // Intersection Observer for scroll animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        this.observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.// Landing Page JavaScript
                    document.addEventListener('DOMContentLoaded', function () {

                        // Smooth scrolling for anchor links
                        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
                            anchor.addEventListener('click', function (e) {
                                e.preventDefault();
                                const target = document.querySelector(this.getAttribute('href'));
                                if (target) {
                                    const offsetTop = target.offsetTop - 80; // Account for fixed navbar
                                    window.scrollTo({
                                        top: offsetTop,
                                        behavior: 'smooth'
                                    });
                                }
                            });
                        });

                        // Navbar background change on scroll
                        const navbar = document.querySelector('.navbar-custom');
                        window.addEventListener('scroll', function () {
                            if (window.scrollY > 50) {
                                navbar.style.background = 'rgba(255, 255, 255, 0.98)';
                                navbar.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.1)';
                            } else {
                                navbar.style.background = 'rgba(255, 255, 255, 0.95)';
                                navbar.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.05)';
                            }
                        });

                        // Intersection Observer for animations
                        const observerOptions = {
                            threshold: 0.1,
                            rootMargin: '0px 0px -50px 0px'
                        };

                        const observer = new IntersectionObserver(function (entries) {
                            entries.forEach(entry => {
                                if (entry.isIntersecting) {
                                    entry.target.classList.add('active');
                                }
                            });
                        }, observerOptions);

                        // Observe all elements with fade-in-up class
                        document.querySelectorAll('.fade-in-up').forEach(el => {
                            observer.observe(el);
                        });

                        // Show first few elements immediately
                        setTimeout(() => {
                            document.querySelectorAll('.fade-in-up').forEach((el, index) => {
                                if (index < 3) {
                                    el.classList.add('active');
                                }
                            });
                        }, 100);

                        // Counter animation for stats
                        function animateCounter(element, target, duration = 2000) {
                            const start = 0;
                            const increment = target / (duration / 16);
                            let current = start;

                            const timer = setInterval(() => {
                                current += increment;
                                if (current >= target) {
                                    element.textContent = target;
                                    clearInterval(timer);
                                } else {
                                    element.textContent = Math.floor(current);
                                }
                            }, 16);
                        }

                        // Stats counter animation when section is visible
                        const statsSection = document.querySelector('.stats-section');
                        if (statsSection) {
                            const statsObserver = new IntersectionObserver(function (entries) {
                                entries.forEach(entry => {
                                    if (entry.isIntersecting) {
                                        const counters = entry.target.querySelectorAll('.stat-number');
                                        counters.forEach(counter => {
                                            const text = counter.textContent;
                                            const target = parseInt(text.replace(/[^0-9]/g, '')) || 0;
                                            if (target > 0) {
                                                counter.textContent = '0';
                                                animateCounter(counter, target);
                                            }
                                        });
                                        statsObserver.unobserve(entry.target);
                                    }
                                });
                            }, { threshold: 0.5 });

                            statsObserver.observe(statsSection);
                        }

                        // Mobile menu close on link click
                        const mobileLinks = document.querySelectorAll('#mobileMenu .nav-link-custom');
                        const mobileMenu = document.querySelector('#mobileMenu');

                        mobileLinks.forEach(link => {
                            link.addEventListener('click', () => {
                                const bsCollapse = new bootstrap.Collapse(mobileMenu, {
                                    toggle: false
                                });
                                bsCollapse.hide();
                            });
                        });

                        // Parallax effect for hero section
                        const hero = document.querySelector('.hero-section');
                        if (hero) {
                            window.addEventListener('scroll', () => {
                                const scrolled = window.pageYOffset;
                                const rate = scrolled * -0.5;
                                hero.style.transform = `translateY(${rate}px)`;
                            });
                        }

                        // Add loading animation
                        window.addEventListener('load', function () {
                            document.body.classList.add('loaded');
                        });
                    });