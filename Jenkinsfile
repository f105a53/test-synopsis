pipeline {
  agent any
  stages {
    stage('Build') {
      steps {
        sh 'dotnet restore'
        sh 'dotnet build DLS-Case-Search.sln --configuration Release'
      }
    }

    stage('Unit Tests') {
      steps {
        dir(path: 'PreviewService.UnitTests') {
          sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=previewService_unit.trx" --results-directory TestResults/'
        }

        dir(path: 'SearchService.UnitTests') {
          sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=searchService_unit.trx" --results-directory TestResults/'
        }

        dir(path: 'SpellCheckService.UnitTests') {
          sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=spellCheck_unit.trx" --results-directory TestResults/'
        }

      }
    }

  }
}