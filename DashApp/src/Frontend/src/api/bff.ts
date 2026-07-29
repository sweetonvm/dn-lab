export const BFF = {
  login: () => window.location.href = '/login',

  logout: () => {
    const form = document.createElement('form');
    form.method = 'post';
    form.action = '/logout';

    document.body.appendChild(form);
    form.submit();
  },
  
  connect: (provider: string) => window.location.href = `/connect/${provider}`,

  unlink: async (provider: string) => {
    const response = await fetch(`/unlink/${provider}`, {
      method: 'POST',
      credentials: 'same-origin'
    })

    if (!response.ok) {
      throw new Error(`Unlink failed: ${response.status}`)
    }
  },
  
  session: async () => {
    const response = await fetch('/api/session', {
      credentials: 'same-origin'
    });

    if (response.status === 401) return null;

    if (!response.ok) throw new Error(`Request failed: ${response.status}`);

    return response.json();
  },
    
  dashboard: async () => {
    const response = await fetch("/api/dashboard", { credentials: 'same-origin' });

    if (!response.ok) {
      throw new Error(`Request failed: ${response.status}`);
    }
    
    return response.json();
  }
}