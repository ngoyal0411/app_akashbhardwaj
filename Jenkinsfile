pipeline {
    agent any
    environment{
        scannerHome = tool 'sonar_scanner_dotnet'
        registry = 'bhardwajakash/ecommerce'
        docker_port = null
        username = 'akashbhardwaj'
        container_name = "c-${username}-${BRANCH_NAME}"
		container_exist = "${bat(script:"docker ps -a -q -f name=${env.container_name}", returnStdout: true).trim().readLines().drop(1).join("")}"
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
        stage('Checkout'){
            steps{
                echo "Checkout from git repository for branch - ${BRANCH_NAME}"
                checkout scm
                script{
                    if (BRANCH_NAME == 'master') {
                        docker_port = 7200
                    } else {
                        docker_port = 7300
                    }
                }
            }
        }
		stage('nuget restore'){
            steps{
                echo "Running build ${JOB_NAME} # ${BUILD_NUMBER} with docker as ${docker_port}"
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
		
		stage('Docker Image'){
            steps{
                echo "Docker Image Step"
                bat "dotnet publish -c Release"
                bat "docker build -t i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} --no-cache -f Dockerfile ."
            }
        }
		stage('Containers') {
            steps{
                parallel(
                    "PrecontainerCheck": {
                        script {
                            echo "check if ${env.container_name} already exist with container id = ${env.container_exist}"
                            if (env.container_exist != null) {
                                echo "deleting existing ${env.container_name} container"
                                bat "docker stop ${env.container_name} && docker rm ${env.container_name}"
                            }
                        }
                    },
                    "Push to Docker Hub": {
                        script{
                            echo "Push to Docker Hub"
                            bat "docker tag i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} ${registry}:${BRANCH_NAME}-${BUILD_NUMBER}"
							bat "docker tag i-${username}-${BRANCH_NAME}:${BUILD_NUMBER} ${registry}:latest-${BRANCH_NAME}"
                            withDockerRegistry([credentialsId:'DockerHub',url:""]){
                                bat "docker push ${registry}:${BRANCH_NAME}-${BUILD_NUMBER}"
								bat "docker push ${registry}:latest-${BRANCH_NAME}"
                            }
                        }
                    }
                )
            }
        }
        stage('Docker Deployment'){
            steps{
				echo "Docker Deployment"
				bat "docker run --name ${env.container_name} -d -p ${docker_port}:80 ${registry}:${BRANCH_NAME}-${BUILD_NUMBER}"
            }
        }
		stage('Kubernetes Deployment'){
            steps{
                echo "Kubernetes Deployment"
                bat "kubectl apply -f deployment.yaml"
                bat "kubectl apply -f service.yaml"
            }
        }
    }
}