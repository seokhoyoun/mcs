# Base Image for development
FROM node:18-alpine

# Set working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json to leverage Docker cache
# The path is relative to the build context (project root)
COPY src/apps/Nexus.Portal/nexus.portal/package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the app's source code. This will be overwritten by the volume mount,
# but it's good practice to have it here for building the image without mounting.
COPY src/apps/Nexus.Portal/nexus.portal/. .

# Expose the port Next.js runs on
EXPOSE 3001

# Command to run the development server
CMD ["npm", "run", "dev"]
