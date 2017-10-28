namespace Devops

[<AutoOpen>]
module Builders = 
    let (>>=) x f =  match x with
                     | Ok v, output -> f (v, output)
                     | Error e, output -> Error e, output

    type CommandResultBuilder() = 
        member __.Return(x) = Ok (fst x), snd x
        member __.ReturnFrom(x) = x
        member __.Bind(x, f) = x >>= f

    let commandResult = new CommandResultBuilder()


