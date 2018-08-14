pipeline {
    agent none
    options { 
        disableConcurrentBuilds()
        skipDefaultCheckout() 
    }

    stages {
        stage('Build') {
            agent { label 'build' }
            steps {
                retry(5) {
                    checkout scm
                }

                bat("build.cmd")            
            }
            post {
                success {
                    archiveArtifacts 'GitLabNotifier/bin/Debug/**/*.*'
                    bat("git clean -fdx")
                }
            }        
        }
        stage('Deploy') {
            agent { label 'arek-deploy' }
            when {
                branch 'master'
            }
            steps {
                unarchive mapping: ['GitLabNotifier/': '.']

                bat("net stop GitLabNotifier")

                dir("GitLabNotifier/bin/Debug") {
                    bat("xcopy *.* ${env.ArekDir} /Y /E")
                }
                bat("net start GitLabNotifier")
            }
        }
    }
}