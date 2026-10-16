# Activar la licencia Personal de Unity para GitHub Actions

GameCI necesita tres secretos en el repo. Unity ya no permite la activación "manual" por archivo `.alf`,
así que el archivo de licencia `.ulf` se obtiene una sola vez con **Unity Hub** (solo el Hub, sin instalar
el editor; ocupa unos cientos de MB y se puede desinstalar después).

1. Descarga e instala [Unity Hub](https://unity.com/download) y entra con tu cuenta de Unity.
2. En el Hub: **Preferences (engranaje) > Licenses > Add > "Get a free personal license"** y acepta.
   Espera a que la licencia aparezca en la lista.
3. Busca el archivo de licencia (carpeta oculta; activa "ver archivos ocultos"):
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - Mac: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. Ábrelo con el Bloc de notas, copia TODO su contenido y, en el repo, **Settings > Secrets and variables > Actions > New repository secret**:
   - `UNITY_LICENSE` = el contenido del `.ulf`
   - `UNITY_EMAIL` = correo de tu cuenta de Unity
   - `UNITY_PASSWORD` = contraseña de tu cuenta de Unity
5. Vuelve a lanzar el workflow **WebGL** (Actions > WebGL > Re-run jobs). Si compila, GitHub Pages queda publicado.

Si Unity cambia de versión en `ProjectSettings/ProjectVersion.txt`, la licencia sigue sirviendo; no hay que repetir nada.
Fuente: [GameCI: Activation](https://game.ci/docs/github/activation).
