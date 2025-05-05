

The deployment is basic auth protected, see the labels in docker-compose.yml

- create the password using `htpasswd -nb user password | sed -e s/\\$/\\$\\$/g`
- create a .env file with a line `SECRET=<output of the above>`

