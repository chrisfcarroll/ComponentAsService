#
#.SYNOPSIS
#Limbo runs something like:   dotnet test  ; if($?){ git commit }else{ git reset --hard }
#.DESCRIPTION
#Run tests and then commit or reset hard depending on pass/fail
#

# This file has a bash section followed by a powershell section, as well as shared sections.
echo @'
' > /dev/null
#vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
# Bash Start -----------------------------------------------------------

[[ -x clip.exe ]] && clip=clip.exe || clip=pbcopy

dotnet test --no-restore  ./ComponentAsService2.Specs

if [[ $? -eq 0 ]] ; then 

    git add :/
    git diff --cached -U1 --stat | $clip
    echo "Enter your commit message in the editor. $(git diff --cached -U1 --stat) is on the clipboard"
    git commit 

else

  git reset --hard 

fi

# Bash End -------------------------------------------------------------
# ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
echo > /dev/null <<"out-null" ###
'@ | out-null
#vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
# Powershell Start -----------------------------------------------------

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

# Powershell End -------------------------------------------------------
# ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
out-null

echo "Some lines work in both bash and powershell. Calculating scriptdir=$scriptdir, requires separate sections."
