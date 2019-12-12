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
	    dir ('PreviewService.UnitTests') {
		   sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=unit.trx" --results-directory TestResults/'
		}
		dir ('SearchService.UnitTests') {
		   sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=unit.trx" --results-directory TestResults/'
		}
		dir ('SpellCheckService.UnitTests') {
		   sh 'dotnet test --configuration Release /p:AltCover=true  /p:AltCoverForce=true --logger "trx;logfilename=unit.trx" --results-directory TestResults/'
		}
	  }
	}
  }
}