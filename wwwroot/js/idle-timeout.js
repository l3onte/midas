(() => {
 const IDLE_LIMIT_MS = 2 * 60 * 1000;
 const PING_EVERY_MS = 45 * 1000;
 let idleTimer;
 let lastPing = 0;
 const logoutForm = document.getElementById('logoutForm');
 function scheduleLogout() {
 clearTimeout(idleTimer);
 idleTimer = setTimeout(() => {
 if (logoutForm) {
 logoutForm.submit();
 } else {
 window.location.href = '/Cuenta/Login';
 }
 }, IDLE_LIMIT_MS);
 }
 async function registerActivity() {
 scheduleLogout();
 const now = Date.now();
 if (now - lastPing >= PING_EVERY_MS) {
 lastPing = now;
 try {
 await fetch('/Cuenta/Ping', {
 method: 'GET',
 credentials: 'same-origin',
 cache: 'no-store'
 });
 } catch {
 // Si el servidor no responde, la próxima navegación
 // comprobará el estado real de la autenticación.
 }
 }
 }
 ['click', 'keydown', 'mousemove', 'scroll', 'touchstart']
 .forEach(eventName =>
 document.addEventListener(
 eventName,
 registerActivity,
 { passive: true }
 )
 );
 scheduleLogout();
})();