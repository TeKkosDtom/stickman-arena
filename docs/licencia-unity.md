# Activar la licencia Personal de Unity para GitHub Actions

GameCI necesita tres secretos en el repo. Pasos (una sola vez, unos 10 minutos):

1. Entra a **Settings > Secrets and variables > Actions** del repo y crea `UNITY_EMAIL` y `UNITY_PASSWORD` con el correo y la contraseña de tu cuenta de Unity.
2. Ejecuta a mano el workflow **Activación** (pestaña Actions > "Activación de licencia Unity" > Run workflow). Al terminar, descarga el artefacto `Unity_v*.alf`.
3. Sube ese archivo `.alf` en https://license.unity3d.com/manual, elige **Unity Personal Edition** y descarga el archivo `.ulf`.
4. Abre el `.ulf` con un editor de texto, copia TODO su contenido y créalo como secreto `UNITY_LICENSE`.
5. Vuelve a lanzar el workflow **WebGL**. Si compila, GitHub Pages queda publicado.

Si Unity cambia de versión (archivo `ProjectSettings/ProjectVersion.txt`), la licencia sigue sirviendo; no hay que repetir nada.
