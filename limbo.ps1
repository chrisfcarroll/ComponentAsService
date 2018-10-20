<#
.SYNOPSIS
Limbo runs something like:   dotnet test  ; if($?){ git commit }else{ git reset --hard }
.DESCRIPTION
Run tests and then commit or reset hard depending on pass/fail
.NOTES
No parameters. Just do it.
#>

function summarisePendingChange {   return ($raw= $(git diff --cached --stat -U1))  }

dotnet test --no-restore  .\ComponentAsService2.Specs\ 

if($?){ 

    git add :/
    git diff --cached -U1 --stat | clip.exe
    Write-Host "Enter your commit message in the editor. `$(git diff --cached -U1 --stat) is on the clipboard"
    git commit 

}else{

  git reset --hard 

}