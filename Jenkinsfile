node {
  stage('CheckOut'){
    checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/southernwind/HomeDashboardBackend']]])
  }

  stage('Configuration'){
    configFileProvider([configFile(fileId: '626675d6-7479-4537-88d9-5dc14eb34765', targetLocation: 'Back/appsettings.json')]) {}
  }

  stage('Build'){
    dotnetBuild configuration: 'Release', project: 'HomeServer.sln', runtime: 'linux-x64', sdk: '.NET5', unstableIfWarnings: true
  }

  withCredentials( \
      bindings: [sshUserPrivateKey( \
        credentialsId: 'ac005f9d-9b4b-496f-873c-1c610df01c03', \
        keyFileVariable: 'SSH_KEY', \
        usernameVariable: 'SSH_USER')]) {

    stage('Deploy'){
      sh 'scp -pr -i ${SSH_KEY} ./Back/bin/Release/net5/linux-x64/* ${SSH_USER}@home-server.localnet: /opt/back-end-api-service'
    }

    stage('Restart'){
      sh 'ssh home-server.localnet -t -l ${SSH_USER} -i ${SSH_KEY} sudo service back_end_api restart'
    }
  }

  stage('Notify Slack'){
    sh 'curl -X POST --data-urlencode "payload={\\"channel\\": \\"#jenkins-deploy\\", \\"username\\": \\"jenkins\\", \\"text\\": \\"ダッシュボード(BackEnd)のデプロイが完了しました。\\nBuild:${BUILD_URL}\\"}" ${WEBHOOK_URL}'
  }
}