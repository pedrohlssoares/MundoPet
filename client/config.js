// Endereço da API. Se o cliente for aberto pela própria API (http://localhost:5080/app/) funciona como está.
// Se for servido por outro servidor (Docker/nginx em :5500, Live Server...), a API precisa estar neste endereço
// e ter a origem do cliente liberada em "Cors:Origins" (appsettings.json).
window.APP_CONFIG = {
  apiUrl: "http://localhost:5080"
};
