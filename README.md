# Escape from Darktoff
Escape from Darktoff es un extraction shooter multijugador en primera persona. Aquí se explicará como contribuir al proyecto de manera ordenada y sencilla.
## Como contribuir
Para contribuir al proyecto, tendrás que crear un nuevo branch, basado sobre el `main`, y este debe seguir la siguiente sintaxis:

`<nombre>/<tipo>/<descripcion>`

En el nombre tendremos que poner nuestro nombre personal o de usuario. En el tipo tendremos varias opciones a elegir, basados en que tipo de tarea vamos a realizar:
- **feature**: Se implementa una caracteristica nueva.
- **bugfix**: Se arregla algun bug.
- **refactor**: Se refactoriza (Se cambia la estructura del código sin cambiar la funcionalidad) parte del código.
- **patch**: Se cambia una parte del código menor (Que no sea un arreglo de bug).

En descripción pondremos una breve descripción que sirva de titulo, de la tarea que vallamos a realizar, tendrá que estar en Inglés y en imperativo.

Una vez creado el branch, añadiremos nuevos cambios a través de commits. Los commits también tendrán que estar en Inglés y en imperativo, además se aconseja que sean breves y sencillos de entender.

Cuando hayamos terminado nuestra tarea y queramos implementar el cambio al branch `main`, lo podremos hacer creando un nuevo ***Pull Request*** con el branch de nuestra implementación. Se aconseja que todo Pull Request tenga el título en Inglés y en imperativo, tendrá que tener una descripción que resuma todos los cambios (Esta descripción puede estar en Español). Cuando este listo, un administrador del repositorio revisará nuestra Pull Request y si todo está en orden la aceptará, cuando esté aceptada podremos completarla, y el branch con los cambios se mergeará al branch `main`. Cuando la Pull Request se haya completado, tenemos que eliminar el branch relacionado.
