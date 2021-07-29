pipeline {
    agent any
    environment{
        scannerHome = tool 'sonar_scanner_dotnet'
        registry = 'bhardwajakash/ecommerce'
        properties = null
        docker_port = 7100
        username = 'bhardwajakash'
		container_exist = "${bat(script:'docker ps -q -f name=c-bhardwajakash-master', returnStdout: true).trim().readLines().drop(1).join("")}"
    }
	options{
        timestamps()
        timeout(time:1,unit:'HOURS')
        skipDefaultCheckout()
        buildDiscarder(logRotator(
            numToKeepStr:'3',
            daysToKeepStr:'15'
        ))
    }
    stages {
        stage ('Start') {
          steps {
            git 'https://github.com/Akash0511/Ecommerce.git'
          }
        }
		stage('nuget restore'){
            steps{
                echo "Running build ${JOB_NAME} # ${BUILD_NUMBER} for ${properties['user.employeeid']} with docker as ${docker_port}"
                echo "Nuget Restore Step"
                bat "dotnet restore"
            }
        }
		stage('SonarQube code Analysis') {
			when {
                branch 'master'
            }
            steps{
                echo "Start sonarqube analysis step"
                withSonarQubeEnv('Test_Sonar'){
                    bat "dotnet ${scannerHome}\\SonarScanner.MSBuild.dll begin /k:sonar-akashbhardwaj /d:sonar.login=efd3f45d08383412de2d040b2c249fee91a921d6"
                }
            }
		}
		stage('Code build'){
            steps{
                echo "Clean Previous Build"
                bat "dotnet clean"
                echo "Code build"
				bat "dotnet build"
            }
        }
		stage('Stop sonarqube analysis'){
            when {
                branch 'master'
            }
            steps{
                echo "Stop sonarqube analysis"
                withSonarQubeEnv('Test_Sonar'){
                   bat "dotnet ${scannerHome}\\SonarScanner.MSBuild.dll end /d:sonar.login=efd3f45d08383412de2d040b2c249fee91a921d6"
                }
            }
        }
		stage('Release artifact'){
            when {
                branch 'develop'
            }
            steps{
                echo "Release artifact"
                bat "dotnet publish -c Release"
            }
        }
		
		stage('Create Docker Image'){
            steps{
                echo "Docker Image Step"
                bat "dotnet publish -c Release"
                bat "docker build -t i-${username}-master --no-cache -f Dockerfile ."
            }
        }
		stage('Containers') {
            steps{
                parallel(
                    "PrecontainerCheck": {
                        script {
                            echo "check if c-${username}-master already exist with container id = ${env.container_exist}"
                            if (env.container_exist != null) {
                                echo "deleting existing c-${username}-master container"
                                bat "docker stop c-${username}-master && docker rm c-${username}-master"
                            }
                        }
                    },
                    "Push to Docker Hub": {
                        script{
                            echo "Push to Docker Hub"
                            bat "docker tag i-${username}-master ${registry}:${BUILD_NUMBER}"
                            withDockerRegistry([credentialsId:'DockerHub',url:""]){
                                bat "docker push ${registry}:${BUILD_NUMBER}"
                            }
                        }
                    }
                )
            }
        }
        stage('Docker Deployment'){
            steps{
				echo "Docker Deployment"
				bat "docker run --name c-${username}-master -d -p 7100:80 ${registry}:${BUILD_NUMBER}"
            }
        }
    }
}