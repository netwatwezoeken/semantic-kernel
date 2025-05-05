# Use a lightweight base image
FROM nginx:alpine

# Copy the static website files to the Nginx document root
COPY index.html /usr/share/nginx/html
COPY plugin /usr/share/nginx/html/plugin
COPY dist /usr/share/nginx/html/dist
COPY assets /usr/share/nginx/html/assets