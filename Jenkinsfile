def deploy = {
    sh 'mkdir -p /home/sr/semantickernel'
    sh 'cp presentation/docker-compose.yml /home/sr/semantickernel/docker-compose.yml'
    withCredentials([string(credentialsId: 'cf94de35-0bd9-4cda-898c-b2ea4c72a4c0', variable: 'dockerKey')]) {
        sh 'echo ${dockerKey} | docker login ghcr.io -u joshendriks --password-stdin'
        dir("/home/sr/semantickernel") {
            sh 'docker compose pull'
            sh 'docker compose up -d'
        }
        sh 'docker logout'
    }					
    withCredentials([string(credentialsId: 'ntfy-basic-auth', variable: 'authString')]) {
        sh 'curl -H "Authorization: basic ${authString}" -d "semantic kernel deployed" https://notify.netwatwezoeken.nl/deploy'
    }
}

pipeline {
	agent none

    stages {
		stage('Deploy') {
			agent { label 'soyo' }
            steps {
                script {deploy ()}
            }			
        }
    }
}