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
    stages {
        stage ('Clean workspace') {
          steps {
            cleanWs()
          }
        }
        stage ('Code checkout from GitHub') {
          steps {
            git 'https://github.com/Akash0511/Ecommerce.git'
          }
        }
		stage ('compile/build the code') {
          steps {
            bat "dotnet build"
          }
        }
		stage ('Execute Unit test cases') {
          steps {
            bat "dotnet test"
          }
        }
		stage('SonarQube code Analysis') {
			steps{
				withSonarQubeEnv('Test_Sonar') {
				  bat "dotnet ${scannerHome}\\SonarScanner.MSBuild.dll begin /k:\"sonar-akashbhardwaj\" /d:sonar.login="efd3f45d08383412de2d040b2c249fee91a921d6""
				  echo "clean previous build"
				  bat "dotnet clean"
				  bat "dotnet build"
				  bat "dotnet test"
				  bat "dotnet ${scannerHome}\\SonarScanner.MSBuild.dll end /d:sonar.login="efd3f45d08383412de2d040b2c249fee91a921d6""
				}
			}
		}
		stage('Create Docker Image'){
            steps{
                echo "Docker Image Step"
                bat "dotnet publish -c Release"
                bat "docker build -t i-${username}-master --no-cache -f Dockerfile ."
            }
        }
        stage('Push Docker Image to Docker Hub'){
            steps{
                echo "Move Image to docker hub"
                bat "docker tag i-${username}-master ${registry}:${BUILD_NUMBER}"
                withDockerRegistry([credentialsId: 'DockerHub',url:""]){
                    bat "docker push ${registry}:${BUILD_NUMBER}"
                }
            }
        }
        stage('Docker Deployment'){
            steps{
                script {
                    echo "c-${username}-master container already exist with container id = ${env.container_exist}"
                    if (env.container_exist != null) {
                        echo "Deleting existing c-${username}-master container"
                        bat "docker stop c-${username}-master && docker rm c-${username}-master"
                    }
                    echo "Docker Deployment"
                    bat "docker run --name c-${username}-master -d -p 7100:80 ${registry}:${BUILD_NUMBER}"
                }
            }
        }
    }
}