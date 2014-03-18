﻿// Copyright (C) by Housemarque, Inc.

namespace Hopac.Extra

open Hopac
open Hopac.Extra.Alt.Infixes
open Hopac.Alt.Infixes
open Hopac.Job.Infixes

module Stream =
  type In<'x> = Alt<'x>
  type Out<'x> = 'x -> Alt<unit>

  let inline imp (mk: Out<_> -> Job<unit>) = Job.delay <| fun () ->
    let ch = Ch.Now.create ()
    mk (Ch.Alt.give ch) >>% Ch.Alt.take ch

  let inline forever x = Job.forever x |> Job.server
  let inline iterate x x2xJ = Job.iterate x x2xJ |> Job.server

  let filterFun x2b (xIn: In<_>) (xOut: Out<_>) =
    forever (xIn >>= fun x -> if x2b x then xOut x :> Job<_> else Job.unit)

  let filterJob (x2bJ: 'x -> Job<_>) (xIn: In<_>) (xOut: Out<_>) =
    forever (xIn >>= fun x ->
             x2bJ x >>= fun b ->
             if b then xOut x :> Job<_> else Job.unit)

  let iterateFun x x2x (xOut: Out<_>) =
    iterate x (fun x -> xOut x >>% x2x x)

  let iterateJob x (x2xJ: _ -> Job<_>) (xOut: Out<_>) =
    iterate x (fun x -> xOut x >>. x2xJ x)

  let mapFun x2y (xIn: In<_>) (yOut: Out<_>) =
    forever (xIn >>= fun x -> yOut (x2y x) :> Job<_>)

  let mapJob (x2yJ: _ -> Job<_>) (xIn: In<_>) (yOut: Out<_>) =
    forever (xIn >>= fun x -> x2yJ x >>= fun y -> yOut y :> Job<_>)

  let sumWithFun xy2z (xIn: In<_>) (yIn: In<_>) (zOut: Out<_>) =
    forever (xIn <+> yIn >>= fun (x, y) -> zOut (xy2z x y) :> Job<_>)

  let sumWithJob (xy2zJ: _ -> _ -> Job<_>) xIn yIn (zOut: Out<_>) =
    forever (xIn <+> yIn >>= fun (x, y) ->
             xy2zJ x y >>= fun z ->
             zOut z :> Job<_>)
