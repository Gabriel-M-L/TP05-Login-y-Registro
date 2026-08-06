function VerificarSesion() {
    var nombreUsuario = document.getElementById("nombreUsuario").value;
    var password = document.getElementById("password").value;
    var error = document.getElementById("error");
    var cuentaError = 0;

    if (nombreUsuario.length < 8) {
        error.innerHTML = "El nombre de usuario debe tener al menos 8 caracteres.";
        error.style.color = "red";
        cuentaError++;
    }   
    if (password.length < 8) {
        error.innerHTML = "La contraseña debe tener al menos 8 caracteres.";
        error.style.color = "red";
        cuentaError++;
    }
    if (cuentaError > 0) {
        return;
    }
}