const screens = [...document.querySelectorAll('[data-screen]')];
const toast = document.querySelector('.toast');
const menuToggle = document.querySelector('.menu-toggle');
const mobileMenu = document.querySelector('.mobile-menu');
let toastTimer;

function showToast(message) {
  toast.textContent = message;
  toast.classList.add('show');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
}

function navigate(route) {
  const target = document.querySelector(`[data-screen="${route}"]`);
  if (!target) return;
  screens.forEach((screen) => { screen.hidden = screen !== target; });
  document.title = `${target.querySelector('h1')?.textContent || 'prompt/market'} - prompt/market`;
  window.scrollTo({ top: 0, behavior: 'smooth' });
  mobileMenu.classList.remove('open');
  mobileMenu.setAttribute('aria-hidden', 'true');
  menuToggle.setAttribute('aria-expanded', 'false');
}

document.addEventListener('click', (event) => {
  const routeTarget = event.target.closest('[data-route]');
  if (routeTarget) {
    event.preventDefault();
    const route = routeTarget.dataset.route;
    window.history.replaceState({}, '', `#${route}`);
    navigate(route);
  }

  const filter = event.target.closest('.filter');
  if (filter) {
    filter.parentElement.querySelectorAll('.filter').forEach((item) => item.classList.remove('active'));
    filter.classList.add('active');
    showToast(`Showing ${filter.textContent.toLowerCase()}`);
  }

  if (event.target.closest('[data-action="publish"]')) {
    showToast('Prompt published to your storefront.');
  }

  if (event.target.closest('.menu-toggle')) {
    const open = mobileMenu.classList.toggle('open');
    mobileMenu.setAttribute('aria-hidden', String(!open));
    menuToggle.setAttribute('aria-expanded', String(open));
  }
});

document.querySelector('#payment-form').addEventListener('submit', (event) => {
  event.preventDefault();
  showToast('Payment complete. Your prompt is now in My library.');
  setTimeout(() => {
    window.history.replaceState({}, '', '#library');
    navigate('library');
  }, 650);
});

document.querySelector('#prompt-form').addEventListener('submit', (event) => event.preventDefault());

const initialRoute = window.location.hash.slice(1);
navigate(screens.some((screen) => screen.dataset.screen === initialRoute) ? initialRoute : 'home');
