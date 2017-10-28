namespace  Devops

module DevopsScripts = 
    open Command
    open Git

    let createBranche brancheName (repoPath: string) = 
        let seqName = sprintf "Create branch '%s' in '%s' repo" brancheName (FileSystem.getLeafDir repoPath)
        let createBrancheAction() = commandResult {
            let! a = repoPath |> git "stash"
            let! b = repoPath |> git "checkout master"
            let! c = repoPath |> git "pull"
            let! d = repoPath |> git ("checkout -b " + brancheName)
            return!  repoPath |> git ("push --set-upstream origin " + brancheName)
        }
        executeCommandSeq seqName createBrancheAction

