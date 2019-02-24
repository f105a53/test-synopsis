pipeline {
      agent {
        docker { image 'microsoft/dotnet:sdk' }
    }
  stages {
    stage('Build') {
      steps {
        sh 'dotnet restore'
        sh 'dotnet build -c Release'
      }
    }
    stage('Benchmark') {
      steps {
        sh 'cd Benchmark; dotnet run -c Release'
      }
    }
  }
}