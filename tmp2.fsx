let rec retry count asyncF = 
    async { 
        try 
            return! asyncF
        with _ when count > 0 -> return! retry (count - 1) asyncF
    }

let calcTask i =
  new System.Threading.Tasks.Task<int>(fun _ -> i + 5)

let calcAsync i =
  async {
    let r = Async.AwaitTask <| (calcTask i)
    return r
    } |> retry 5

let a = calcAsync 2

Async.RunSynchronously a
